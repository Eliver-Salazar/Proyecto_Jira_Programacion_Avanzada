
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. VALIDAR DEPENDENCIAS
    -- ==========================================

    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.Issues', 'U') IS NULL
    BEGIN
        THROW 50100, 'Faltan las tablas Projects o Issues.', 1;
    END;

    IF OBJECT_ID('dbo.Labels', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.IssueLabels', 'U') IS NOT NULL
    BEGIN
        THROW 50101, 'Una o varias tablas de etiquetas ya existen.', 1;
    END;

    -- La clave compuesta debe existir para asegurar
    -- que cada tarea pertenece al proyecto indicado.
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.key_constraints kc
        WHERE kc.parent_object_id = OBJECT_ID('dbo.Issues')
          AND kc.name = 'UQ_Issues_Project_Issue'
          AND kc.type = 'UQ'
    )
    BEGIN
        THROW 50102,
              'Falta UQ_Issues_Project_Issue en Issues.',
              1;
    END;


    -- ==========================================
    -- 2. TABLA LABELS
    -- ==========================================

    CREATE TABLE dbo.Labels
    (
        LabelId INT IDENTITY(1,1) NOT NULL,

        ProjectId INT NOT NULL,

        Name NVARCHAR(100) NOT NULL,

        Color CHAR(7) NOT NULL
            CONSTRAINT DF_Labels_Color
            DEFAULT ('#3B82F6'),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Labels_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        IsActive BIT NOT NULL
            CONSTRAINT DF_Labels_IsActive
            DEFAULT (1),

        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_Labels
            PRIMARY KEY (LabelId),

        -- Clave alternativa para relaciones por proyecto.
        CONSTRAINT UQ_Labels_Project_Label
            UNIQUE (ProjectId, LabelId),

        -- Evita etiquetas duplicadas en un proyecto.
        CONSTRAINT UQ_Labels_Project_Name
            UNIQUE (ProjectId, Name),

        CONSTRAINT CK_Labels_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

        -- Formato hexadecimal: #RRGGBB.
        CONSTRAINT CK_Labels_Color
            CHECK
            (
                Color COLLATE Latin1_General_BIN2
                    LIKE '#[0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f][0-9A-Fa-f]'
            ),

        CONSTRAINT FK_Labels_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId)
    );


    -- ==========================================
    -- 3. TABLA ISSUELABELS
    -- ==========================================

    CREATE TABLE dbo.IssueLabels
    (
        IssueLabelId BIGINT IDENTITY(1,1) NOT NULL,

        ProjectId INT NOT NULL,

        IssueId BIGINT NOT NULL,

        LabelId INT NOT NULL,

        AssignedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueLabels_AssignedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IssueLabels
            PRIMARY KEY (IssueLabelId),

        -- Una tarea no puede tener dos veces la misma etiqueta.
        CONSTRAINT UQ_IssueLabels_Issue_Label
            UNIQUE (IssueId, LabelId),

        -- Garantiza que la tarea pertenece al proyecto.
        CONSTRAINT FK_IssueLabels_Issues
            FOREIGN KEY (ProjectId, IssueId)
            REFERENCES dbo.Issues(ProjectId, IssueId),

        -- Garantiza que la etiqueta pertenece
        -- exactamente al mismo proyecto.
        CONSTRAINT FK_IssueLabels_Labels
            FOREIGN KEY (ProjectId, LabelId)
            REFERENCES dbo.Labels(ProjectId, LabelId)
    );


    -- ==========================================
    -- 4. INDICES
    -- ==========================================

    -- Optimiza la consulta de etiquetas por proyecto.
    CREATE INDEX IX_Labels_Project_Active
        ON dbo.Labels(ProjectId, Name)
        WHERE IsActive = 1;

    -- Optimiza la busqueda de tareas por etiqueta.
    CREATE INDEX IX_IssueLabels_Label
        ON dbo.IssueLabels(LabelId, IssueId);


    -- ==========================================
    -- 5. CONFIRMAR
    -- ==========================================

    COMMIT TRANSACTION;

    PRINT 'Modulo Labels creado correctamente.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
