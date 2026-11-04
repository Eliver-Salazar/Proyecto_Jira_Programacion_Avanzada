
USE JiraCloneDB;
GO

-- Si ocurre un error, SQL Server revierte la transacción.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Comprobamos que existan las tablas necesarias.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.IssueTypes', 'U') IS NULL
       OR OBJECT_ID('dbo.IssuePriorities', 'U') IS NULL
       OR OBJECT_ID('dbo.WorkflowStatuses', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
        THROW 50004, 'Faltan tablas necesarias para crear Issues.', 1;

    -- Evitamos crear la tabla dos veces.
    IF OBJECT_ID('dbo.Issues', 'U') IS NOT NULL
        THROW 50005, 'La tabla Issues ya existe.', 1;

    -- Tabla principal de tareas.
    CREATE TABLE dbo.Issues
    (
        IssueId BIGINT IDENTITY(1,1) NOT NULL,

        -- Proyecto al que pertenece la tarea.
        ProjectId INT NOT NULL,

        -- Número consecutivo dentro del proyecto.
        IssueNumber INT NOT NULL,

        -- Clasificación, prioridad y estado.
        IssueTypeId INT NOT NULL,
        IssuePriorityId INT NOT NULL,
        WorkflowStatusId INT NOT NULL,

        -- Información principal de la tarea.
        Title NVARCHAR(250) NOT NULL,
        Description NVARCHAR(MAX) NULL,

        -- Usuario que creó la tarea.
        ReporterUserId NVARCHAR(128) NOT NULL,

        -- Usuario responsable; puede quedar sin asignar.
        AssigneeUserId NVARCHAR(128) NULL,

        -- Fechas de seguimiento.
        DueDate DATE NULL,

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Issues_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        UpdatedAt DATETIME2(3) NULL,

        CompletedAt DATETIME2(3) NULL,

        -- Permite detectar actualizaciones simultáneas.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_Issues
            PRIMARY KEY (IssueId),

        -- Impide repetir el número de tarea en un proyecto.
        CONSTRAINT UQ_Issues_Project_Number
            UNIQUE (ProjectId, IssueNumber),

        CONSTRAINT CK_Issues_Number
            CHECK (IssueNumber > 0),

        CONSTRAINT CK_Issues_Title
            CHECK (LEN(LTRIM(RTRIM(Title))) > 0),

        -- Relaciones con los catálogos y el proyecto.
        CONSTRAINT FK_Issues_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        CONSTRAINT FK_Issues_IssueTypes
            FOREIGN KEY (IssueTypeId)
            REFERENCES dbo.IssueTypes(IssueTypeId),

        CONSTRAINT FK_Issues_IssuePriorities
            FOREIGN KEY (IssuePriorityId)
            REFERENCES dbo.IssuePriorities(IssuePriorityId),

        CONSTRAINT FK_Issues_WorkflowStatuses
            FOREIGN KEY (WorkflowStatusId)
            REFERENCES dbo.WorkflowStatuses(WorkflowStatusId),

        -- Relaciones con los usuarios de Identity 2.
        CONSTRAINT FK_Issues_Reporter
            FOREIGN KEY (ReporterUserId)
            REFERENCES dbo.AspNetUsers(Id),

        CONSTRAINT FK_Issues_Assignee
            FOREIGN KEY (AssigneeUserId)
            REFERENCES dbo.AspNetUsers(Id)
    );

    -- Facilita consultar tareas por responsable.
    CREATE INDEX IX_Issues_Assignee
        ON dbo.Issues(AssigneeUserId);

    -- Facilita consultar tareas por estado y proyecto.
    CREATE INDEX IX_Issues_Project_Status
        ON dbo.Issues(ProjectId, WorkflowStatusId);

    -- Facilita consultar tareas por creador.
    CREATE INDEX IX_Issues_Reporter
        ON dbo.Issues(ReporterUserId);

    -- Facilita consultar tareas por prioridad.
    CREATE INDEX IX_Issues_Priority
        ON dbo.Issues(IssuePriorityId);

    COMMIT TRANSACTION;

    PRINT 'Tabla Issues creada correctamente.';
END TRY
BEGIN CATCH
    -- Deshace los cambios si ocurre algún error.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
