
USE JiraCloneDB;
GO

-- Cancela los cambios si ocurre un error.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Verifica que exista la tabla principal de proyectos.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
        THROW 50010, 'La tabla Projects no existe.', 1;

    -- Evita crear nuevamente la tabla.
    IF OBJECT_ID('dbo.ProjectIssueCounters', 'U') IS NOT NULL
        THROW 50011, 'ProjectIssueCounters ya existe.', 1;

    -- Almacena el último número de tarea de cada proyecto.
    CREATE TABLE dbo.ProjectIssueCounters
    (
        -- Es clave primaria y también referencia a Projects.
        ProjectId INT NOT NULL,

        -- El consecutivo comienza en cero.
        LastIssueNumber INT NOT NULL
            CONSTRAINT DF_ProjectIssueCounters_Last
            DEFAULT (0),

        -- Fecha de la última actualización.
        UpdatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_ProjectIssueCounters_Updated
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProjectIssueCounters
            PRIMARY KEY (ProjectId),

        CONSTRAINT FK_ProjectIssueCounters_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        -- Evita consecutivos negativos.
        CONSTRAINT CK_ProjectIssueCounters_Last
            CHECK (LastIssueNumber >= 0)
    );

    COMMIT TRANSACTION;

    PRINT 'Tabla ProjectIssueCounters creada correctamente.';
END TRY
BEGIN CATCH
    -- Revierte los cambios si ocurre algún error.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

-- Inicializa el contador de proyectos que ya existan.
INSERT INTO dbo.ProjectIssueCounters
    (ProjectId, LastIssueNumber)
SELECT
    p.ProjectId,
    ISNULL(MAX(i.IssueNumber), 0)
FROM dbo.Projects AS p
LEFT JOIN dbo.Issues AS i
    ON i.ProjectId = p.ProjectId
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.ProjectIssueCounters AS c
    WHERE c.ProjectId = p.ProjectId
)
GROUP BY p.ProjectId;
GO
