
USE JiraCloneDB;
GO

-- Revierte la transaccion si ocurre un error.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. VALIDAR TABLAS EXISTENTES
    -- ==========================================

    -- Los sprints necesitan proyectos, tableros y tareas.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.Boards', 'U') IS NULL
       OR OBJECT_ID('dbo.Issues', 'U') IS NULL
    BEGIN
        THROW 50060, 'Faltan tablas requeridas.', 1;
    END;

    -- Evita ejecutar dos veces este script.
    IF OBJECT_ID('dbo.Sprints', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.SprintIssues', 'U') IS NOT NULL
    BEGIN
        THROW 50061, 'Las tablas de sprints ya existen.', 1;
    END;

    -- ==========================================
    -- 2. TABLA SPRINTS
    -- ==========================================

    -- Almacena los ciclos de trabajo Scrum.
    CREATE TABLE dbo.Sprints
    (
        SprintId INT IDENTITY(1,1) NOT NULL,
        ProjectId INT NOT NULL,
        BoardId INT NOT NULL,

        Name NVARCHAR(150) NOT NULL,
        Goal NVARCHAR(1000) NULL,

        -- Fechas planificadas del sprint.
        StartDate DATETIME2(3) NULL,
        EndDate DATETIME2(3) NULL,

        -- Fechas reales de ejecucion.
        StartedAt DATETIME2(3) NULL,
        CompletedAt DATETIME2(3) NULL,

        -- Ciclo de vida del sprint.
        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Sprints_Status
            DEFAULT ('PLANNED'),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Sprints_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        -- Control de concurrencia.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_Sprints
            PRIMARY KEY (SprintId),

        -- Permite validar sprint y proyecto conjuntamente.
        CONSTRAINT UQ_Sprints_Project_Sprint
            UNIQUE (ProjectId, SprintId),

        -- Evita nombres repetidos dentro del tablero.
        CONSTRAINT UQ_Sprints_Board_Name
            UNIQUE (BoardId, Name),

        CONSTRAINT CK_Sprints_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

        -- Estados permitidos.
        CONSTRAINT CK_Sprints_Status
            CHECK (Status IN (
                'PLANNED',
                'ACTIVE',
                'COMPLETED',
                'CANCELLED'
            )),

        -- La fecha final no puede ser anterior a la inicial.
        CONSTRAINT CK_Sprints_Dates
            CHECK (
                StartDate IS NULL
                OR EndDate IS NULL
                OR EndDate > StartDate
            ),

        -- La finalizacion real no puede preceder al inicio.
        CONSTRAINT CK_Sprints_ActualDates
            CHECK (
                CompletedAt IS NULL
                OR (
                    StartedAt IS NOT NULL
                    AND CompletedAt >= StartedAt
                )
            ),

        -- Un sprint activo debe tener fecha real de inicio.
        CONSTRAINT CK_Sprints_ActiveStart
            CHECK (
                Status <> 'ACTIVE'
                OR (
                    StartedAt IS NOT NULL
                    AND CompletedAt IS NULL
                )
            ),

        -- Un sprint completado debe tener ambas fechas.
        CONSTRAINT CK_Sprints_Completed
            CHECK (
                Status <> 'COMPLETED'
                OR (
                    StartedAt IS NOT NULL
                    AND CompletedAt IS NOT NULL
                )
            ),

        -- Valida que el tablero pertenezca al proyecto.
        CONSTRAINT FK_Sprints_Boards
            FOREIGN KEY (ProjectId, BoardId)
            REFERENCES dbo.Boards(ProjectId, BoardId)
    );

    -- Solo permite un sprint activo por tablero.
    CREATE UNIQUE INDEX UX_Sprints_OneActivePerBoard
        ON dbo.Sprints(BoardId)
        WHERE Status = 'ACTIVE';


    -- ==========================================
    -- 3. TABLA SPRINTISSUES
    -- ==========================================

    -- Relaciona las tareas con los sprints.
    CREATE TABLE dbo.SprintIssues
    (
        SprintIssueId BIGINT IDENTITY(1,1) NOT NULL,

        ProjectId INT NOT NULL,
        SprintId INT NOT NULL,
        IssueId BIGINT NOT NULL,

        -- Fecha de incorporacion de la tarea.
        AddedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_SprintIssues_AddedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_SprintIssues
            PRIMARY KEY (SprintIssueId),

        -- Impide agregar la misma tarea dos veces al sprint.
        CONSTRAINT UQ_SprintIssues_Sprint_Issue
            UNIQUE (SprintId, IssueId),

        -- El sprint debe pertenecer al proyecto indicado.
        CONSTRAINT FK_SprintIssues_Sprints
            FOREIGN KEY (ProjectId, SprintId)
            REFERENCES dbo.Sprints(ProjectId, SprintId),

        -- La tarea debe pertenecer al mismo proyecto.
        CONSTRAINT FK_SprintIssues_Issues
            FOREIGN KEY (ProjectId, IssueId)
            REFERENCES dbo.Issues(ProjectId, IssueId)
    );


    -- ==========================================
    -- 4. INDICES DE CONSULTA
    -- ==========================================

    -- Facilita consultar sprints de un proyecto.
    CREATE INDEX IX_Sprints_Project_Status
        ON dbo.Sprints(ProjectId, Status);

    -- Facilita buscar sprints por tablero.
    CREATE INDEX IX_Sprints_Board
        ON dbo.Sprints(BoardId);

    -- Facilita buscar los sprints de una tarea.
    CREATE INDEX IX_SprintIssues_Issue
        ON dbo.SprintIssues(IssueId);

    -- ==========================================
    -- 5. CONFIRMAR CAMBIOS
    -- ==========================================

    COMMIT TRANSACTION;

    PRINT 'Modulo Sprints creado correctamente.';

END TRY
BEGIN CATCH

    -- Revierte todos los cambios ante un error.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO