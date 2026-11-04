
USE JiraCloneDB;
GO

-- Crea o actualiza el procedimiento almacenado.
CREATE OR ALTER PROCEDURE dbo.usp_CreateIssue
    @ProjectId INT,
    @IssueTypeId INT,
    @IssuePriorityId INT,
    @WorkflowStatusId INT,
    @Title NVARCHAR(250),
    @Description NVARCHAR(MAX) = NULL,
    @ReporterUserId NVARCHAR(128),
    @AssigneeUserId NVARCHAR(128) = NULL,
    @DueDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @IssueNumber INT;
    DECLARE @IssueId BIGINT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Comprueba que el proyecto exista y esté activo.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Projects WITH (UPDLOCK, HOLDLOCK)
            WHERE ProjectId = @ProjectId
              AND IsActive = 1
        )
            THROW 50020, 'El proyecto no existe o esta inactivo.', 1;

        -- Comprueba que el creador pertenezca al proyecto.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.ProjectMembers
            WHERE ProjectId = @ProjectId
              AND UserId = @ReporterUserId
        )
            THROW 50021, 'El creador no pertenece al proyecto.', 1;

        -- Si hay responsable, verifica su pertenencia.
        IF @AssigneeUserId IS NOT NULL
           AND NOT EXISTS
           (
               SELECT 1
               FROM dbo.ProjectMembers
               WHERE ProjectId = @ProjectId
                 AND UserId = @AssigneeUserId
           )
            THROW 50022, 'El responsable no pertenece al proyecto.', 1;

        -- Inicializa el contador si todavía no existe.
        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.ProjectIssueCounters WITH (UPDLOCK, HOLDLOCK)
            WHERE ProjectId = @ProjectId
        )
        BEGIN
            INSERT INTO dbo.ProjectIssueCounters
                (ProjectId, LastIssueNumber)
            VALUES (@ProjectId, 0);
        END;

        -- Incrementa el contador y captura el nuevo número.
        DECLARE @NewNumber TABLE (IssueNumber INT);

        UPDATE dbo.ProjectIssueCounters
        SET LastIssueNumber = LastIssueNumber + 1,
            UpdatedAt = SYSUTCDATETIME()
        OUTPUT inserted.LastIssueNumber
            INTO @NewNumber(IssueNumber)
        WHERE ProjectId = @ProjectId;

        SELECT @IssueNumber = IssueNumber
        FROM @NewNumber;

        -- Registra la tarea con el consecutivo obtenido.
        INSERT INTO dbo.Issues
        (
            ProjectId,
            IssueNumber,
            IssueTypeId,
            IssuePriorityId,
            WorkflowStatusId,
            Title,
            Description,
            ReporterUserId,
            AssigneeUserId,
            DueDate
        )
        VALUES
        (
            @ProjectId,
            @IssueNumber,
            @IssueTypeId,
            @IssuePriorityId,
            @WorkflowStatusId,
            @Title,
            @Description,
            @ReporterUserId,
            @AssigneeUserId,
            @DueDate
        );

        -- Recupera el identificador interno de la nueva tarea.
        SET @IssueId = CONVERT(BIGINT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        -- Devuelve los datos de la tarea creada.
        SELECT
            i.IssueId,
            p.ProjectKey + '-' +
                CONVERT(VARCHAR(20), i.IssueNumber) AS IssueKey,
            i.Title
        FROM dbo.Issues AS i
        INNER JOIN dbo.Projects AS p
            ON p.ProjectId = i.ProjectId
        WHERE i.IssueId = @IssueId;
    END TRY
    BEGIN CATCH
        -- Si falla el registro, también revierte el consecutivo.
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
