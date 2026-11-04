
USE JiraCloneDB;
GO

-- Crea o actualiza el procedimiento.
CREATE OR ALTER PROCEDURE dbo.usp_ChangeIssueStatus
    @IssueId BIGINT,
    @NewStatusId INT,
    @ChangedByUserId NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ProjectId INT;
    DECLARE @CurrentStatusId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Bloquea la tarea mientras se procesa el cambio.
        SELECT
            @ProjectId = ProjectId,
            @CurrentStatusId = WorkflowStatusId
        FROM dbo.Issues WITH (UPDLOCK, HOLDLOCK)
        WHERE IssueId = @IssueId;

        -- Comprueba que exista la tarea.
        IF @ProjectId IS NULL
            THROW 50040, 'La tarea no existe.', 1;

        -- Comprueba que el proyecto esté activo.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Projects
            WHERE ProjectId = @ProjectId
              AND IsActive = 1
        )
            THROW 50041, 'El proyecto esta inactivo.', 1;

        -- Comprueba que el usuario tenga permisos de edición.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.ProjectMembers
            WHERE ProjectId = @ProjectId
              AND UserId = @ChangedByUserId
              AND ProjectRole IN ('ADMIN', 'MEMBER')
        )
            THROW 50042, 'El usuario no tiene permisos.', 1;

        -- Impide cambiar al mismo estado.
        IF @CurrentStatusId = @NewStatusId
            THROW 50043, 'La tarea ya tiene ese estado.', 1;

        -- Comprueba que exista una transición activa.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.WorkflowTransitions
            WHERE ProjectId = @ProjectId
              AND FromStatusId = @CurrentStatusId
              AND ToStatusId = @NewStatusId
              AND IsActive = 1
        )
            THROW 50044, 'La transicion no esta permitida.', 1;

        -- Actualiza el estado y la fecha de modificación.
        UPDATE dbo.Issues
        SET WorkflowStatusId = @NewStatusId,
            UpdatedAt = SYSUTCDATETIME(),

            -- Registra la finalización al entrar en DONE.
            CompletedAt =
                CASE
                    WHEN EXISTS
                    (
                        SELECT 1
                        FROM dbo.WorkflowStatuses
                        WHERE WorkflowStatusId = @NewStatusId
                          AND StatusCategory = 'DONE'
                    )
                    THEN SYSUTCDATETIME()
                    ELSE NULL
                END
        WHERE IssueId = @IssueId;

        COMMIT TRANSACTION;

        -- Devuelve la tarea actualizada.
        SELECT
            i.IssueId,
            i.Title,
            s.Name AS CurrentStatus,
            i.UpdatedAt,
            i.CompletedAt
        FROM dbo.Issues AS i
        INNER JOIN dbo.WorkflowStatuses AS s
            ON s.WorkflowStatusId = i.WorkflowStatusId
        WHERE i.IssueId = @IssueId;
    END TRY
    BEGIN CATCH
        -- Revierte los cambios si ocurre un error.
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
