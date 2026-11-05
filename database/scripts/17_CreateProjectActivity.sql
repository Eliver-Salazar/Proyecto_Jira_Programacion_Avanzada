
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Validar dependencias.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
       OR OBJECT_ID('dbo.Issues', 'U') IS NULL
    BEGIN
        THROW 50090,
            'Faltan tablas requeridas para ProjectActivity.', 1;
    END;

    -- 2. Evitar ejecuciones duplicadas.
    IF OBJECT_ID('dbo.ProjectActivity', 'U') IS NOT NULL
    BEGIN
        THROW 50091,
            'La tabla ProjectActivity ya existe.', 1;
    END;

    -- 3. Crear tabla de actividad.
    CREATE TABLE dbo.ProjectActivity
    (
        ProjectActivityId BIGINT IDENTITY(1,1) NOT NULL,

        ProjectId INT NOT NULL,

        -- Usuario que realizó la actividad.
        UserId NVARCHAR(128) NOT NULL,

        -- Tarea relacionada, cuando corresponda.
        IssueId BIGINT NULL,

        -- Tipo de actividad.
        ActivityType NVARCHAR(50) NOT NULL,

        -- Descripción de la actividad.
        Description NVARCHAR(1000) NOT NULL,

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_ProjectActivity_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProjectActivity
            PRIMARY KEY (ProjectActivityId),

        CONSTRAINT CK_ProjectActivity_Type
            CHECK (
                LEN(LTRIM(RTRIM(ActivityType))) > 0
            ),

        CONSTRAINT CK_ProjectActivity_Description
            CHECK (
                LEN(LTRIM(RTRIM(Description))) > 0
            ),

        -- Relación con el proyecto.
        CONSTRAINT FK_ProjectActivity_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        -- Relación con el usuario.
        CONSTRAINT FK_ProjectActivity_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id),

        -- Garantiza que la tarea pertenezca
        -- al mismo proyecto de la actividad.
        CONSTRAINT FK_ProjectActivity_Issues
            FOREIGN KEY (ProjectId, IssueId)
            REFERENCES dbo.Issues(ProjectId, IssueId)
    );

    -- 4. Consultar actividad reciente por proyecto.
    CREATE INDEX IX_ProjectActivity_Project_CreatedAt
        ON dbo.ProjectActivity
        (
            ProjectId,
            CreatedAt DESC
        );

    -- 5. Consultar actividad por usuario.
    CREATE INDEX IX_ProjectActivity_User
        ON dbo.ProjectActivity(UserId);

    -- 6. Consultar actividad por tarea.
    CREATE INDEX IX_ProjectActivity_Issue
        ON dbo.ProjectActivity(IssueId)
        WHERE IssueId IS NOT NULL;

    COMMIT TRANSACTION;

    PRINT 'Script 17 ejecutado correctamente.';
    PRINT 'Tabla ProjectActivity creada.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
