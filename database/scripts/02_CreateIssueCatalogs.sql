
USE JiraCloneDB;
GO

-- Cancela la transacción si ocurre un error.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Evita crear nuevamente tablas existentes.
    IF OBJECT_ID('dbo.IssueTypes', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.IssuePriorities', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.WorkflowStatuses', 'U') IS NOT NULL
        THROW 50003, 'Uno o varios catalogos ya existen.', 1;

    -- 1. Tipos de tareas: Historia, Error, Tarea, etc.
    CREATE TABLE dbo.IssueTypes
    (
        IssueTypeId INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(60) NOT NULL,
        Description NVARCHAR(250) NULL,
        Icon NVARCHAR(100) NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_IssueTypes_IsActive DEFAULT (1),

        CONSTRAINT PK_IssueTypes PRIMARY KEY (IssueTypeId),

        -- Impide registrar dos tipos con el mismo nombre.
        CONSTRAINT UQ_IssueTypes_Name UNIQUE (Name),

        CONSTRAINT CK_IssueTypes_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );

    -- 2. Prioridades para clasificar la urgencia.
    CREATE TABLE dbo.IssuePriorities
    (
        IssuePriorityId INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(60) NOT NULL,

        -- Permite ordenar prioridades de menor a mayor.
        PriorityLevel INT NOT NULL,

        Color NVARCHAR(20) NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_IssuePriorities_IsActive DEFAULT (1),

        CONSTRAINT PK_IssuePriorities PRIMARY KEY (IssuePriorityId),

        CONSTRAINT UQ_IssuePriorities_Name UNIQUE (Name),

        CONSTRAINT UQ_IssuePriorities_Level UNIQUE (PriorityLevel),

        CONSTRAINT CK_IssuePriorities_Level
            CHECK (PriorityLevel > 0)
    );

    -- 3. Estados del flujo de trabajo.
    CREATE TABLE dbo.WorkflowStatuses
    (
        WorkflowStatusId INT IDENTITY(1,1) NOT NULL,
        Name NVARCHAR(60) NOT NULL,
        Description NVARCHAR(250) NULL,

        -- Categoría general para agrupar estados.
        StatusCategory NVARCHAR(20) NOT NULL,

        Color NVARCHAR(20) NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_WorkflowStatuses_IsActive DEFAULT (1),

        CONSTRAINT PK_WorkflowStatuses
            PRIMARY KEY (WorkflowStatusId),

        CONSTRAINT UQ_WorkflowStatuses_Name UNIQUE (Name),

        -- Limita las categorías a valores definidos.
        CONSTRAINT CK_WorkflowStatuses_Category
            CHECK (StatusCategory IN
                ('TODO', 'IN_PROGRESS', 'DONE'))
    );

    COMMIT TRANSACTION;

    PRINT 'Catalogos de tareas creados correctamente.';
END TRY
BEGIN CATCH
    -- Deshace los cambios si alguna instrucción falla.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
