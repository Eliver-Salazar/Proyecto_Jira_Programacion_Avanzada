
USE JiraCloneDB;
GO

-- Revierte automáticamente la transacción ante errores.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Verifica las tablas necesarias.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.WorkflowStatuses', 'U') IS NULL
        THROW 50030, 'Faltan tablas requeridas.', 1;

    -- Impide crear nuevamente la tabla.
    IF OBJECT_ID('dbo.WorkflowTransitions', 'U') IS NOT NULL
        THROW 50031, 'WorkflowTransitions ya existe.', 1;

    -- Define los cambios de estado permitidos por proyecto.
    CREATE TABLE dbo.WorkflowTransitions
    (
        WorkflowTransitionId INT IDENTITY(1,1) NOT NULL,

        -- Proyecto al que pertenece esta regla.
        ProjectId INT NOT NULL,

        -- Estado de origen y estado de destino.
        FromStatusId INT NOT NULL,
        ToStatusId INT NOT NULL,

        -- Nombre visible de la transición.
        Name NVARCHAR(100) NOT NULL,

        -- Permite desactivar una transición sin eliminarla.
        IsActive BIT NOT NULL
            CONSTRAINT DF_WorkflowTransitions_IsActive
            DEFAULT (1),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_WorkflowTransitions_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_WorkflowTransitions
            PRIMARY KEY (WorkflowTransitionId),

        -- Evita duplicar una transición dentro del proyecto.
        CONSTRAINT UQ_WorkflowTransitions_Route
            UNIQUE (ProjectId, FromStatusId, ToStatusId),

        -- Impide transiciones de un estado hacia sí mismo.
        CONSTRAINT CK_WorkflowTransitions_Different
            CHECK (FromStatusId <> ToStatusId),

        CONSTRAINT CK_WorkflowTransitions_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

        -- Relación con el proyecto.
        CONSTRAINT FK_WorkflowTransitions_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        -- Relación con el estado de origen.
        CONSTRAINT FK_WorkflowTransitions_FromStatus
            FOREIGN KEY (FromStatusId)
            REFERENCES dbo.WorkflowStatuses(WorkflowStatusId),

        -- Relación con el estado de destino.
        CONSTRAINT FK_WorkflowTransitions_ToStatus
            FOREIGN KEY (ToStatusId)
            REFERENCES dbo.WorkflowStatuses(WorkflowStatusId)
    );

    -- Optimiza la búsqueda de transiciones desde un estado.
    CREATE INDEX IX_WorkflowTransitions_Project_From
        ON dbo.WorkflowTransitions(ProjectId, FromStatusId)
        INCLUDE (ToStatusId, IsActive);

    COMMIT TRANSACTION;

    PRINT 'WorkflowTransitions creada correctamente.';
END TRY
BEGIN CATCH
    -- Deshace los cambios si falla alguna instrucción.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
