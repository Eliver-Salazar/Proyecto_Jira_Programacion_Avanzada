
USE JiraCloneDB;
GO

-- Revierte automáticamente la transacción ante errores.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- =====================================================
    -- 1. VALIDAR DEPENDENCIAS
    -- =====================================================

    -- Estas tablas deben existir antes de crear tableros.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.Issues', 'U') IS NULL
       OR OBJECT_ID('dbo.WorkflowStatuses', 'U') IS NULL
    BEGIN
        THROW 50050, 'Faltan tablas requeridas.', 1;
    END;

    -- Evita crear nuevamente las tablas del modulo.
    IF OBJECT_ID('dbo.Boards', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.BoardColumns', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.BoardColumnStatuses', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.IssueBoardPositions', 'U') IS NOT NULL
    BEGIN
        THROW 50051, 'Una o varias tablas de tableros ya existen.', 1;
    END;


    -- =====================================================
    -- 2. TABLA BOARDS
    -- =====================================================

    -- Almacena los tableros Kanban o Scrum de cada proyecto.
    CREATE TABLE dbo.Boards
    (
        BoardId INT IDENTITY(1,1) NOT NULL,
        ProjectId INT NOT NULL,

        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(500) NULL,

        -- Tipo de tablero permitido.
        BoardType NVARCHAR(20) NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_Boards_IsActive DEFAULT (1),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Boards_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        -- Permite controlar modificaciones concurrentes.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_Boards
            PRIMARY KEY (BoardId),

        -- Identifica el tablero dentro de su proyecto.
        CONSTRAINT UQ_Boards_Project_Board
            UNIQUE (ProjectId, BoardId),

        -- Impide nombres duplicados en un mismo proyecto.
        CONSTRAINT UQ_Boards_Project_Name
            UNIQUE (ProjectId, Name),

        -- Evita nombres vacios.
        CONSTRAINT CK_Boards_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

        -- Solo permite tableros Kanban o Scrum.
        CONSTRAINT CK_Boards_Type
            CHECK (BoardType IN ('KANBAN', 'SCRUM')),

        -- Relaciona el tablero con su proyecto.
        CONSTRAINT FK_Boards_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId)
    );


    -- =====================================================
    -- 3. TABLA BOARDCOLUMNS
    -- =====================================================

    -- Define las columnas visibles dentro de cada tablero.
    CREATE TABLE dbo.BoardColumns
    (
        BoardColumnId INT IDENTITY(1,1) NOT NULL,
        BoardId INT NOT NULL,

        Name NVARCHAR(100) NOT NULL,

        -- Orden visual de la columna.
        Position INT NOT NULL,

        -- Cantidad maxima opcional de tareas.
        WipLimit INT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_BoardColumns_IsActive DEFAULT (1),

        CONSTRAINT PK_BoardColumns
            PRIMARY KEY (BoardColumnId),

        -- Identifica una columna dentro de su tablero.
        CONSTRAINT UQ_BoardColumns_Board_Column
            UNIQUE (BoardId, BoardColumnId),

        -- Evita nombres repetidos en un tablero.
        CONSTRAINT UQ_BoardColumns_Board_Name
            UNIQUE (BoardId, Name),

        -- Evita posiciones repetidas.
        CONSTRAINT UQ_BoardColumns_Board_Position
            UNIQUE (BoardId, Position),

        CONSTRAINT CK_BoardColumns_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0),

        -- Las posiciones no pueden ser negativas.
        CONSTRAINT CK_BoardColumns_Position
            CHECK (Position >= 0),

        -- El limite WIP debe ser positivo si se define.
        CONSTRAINT CK_BoardColumns_WipLimit
            CHECK (WipLimit IS NULL OR WipLimit > 0),

        CONSTRAINT FK_BoardColumns_Boards
            FOREIGN KEY (BoardId)
            REFERENCES dbo.Boards(BoardId)
    );


    -- =====================================================
    -- 4. TABLA BOARDCOLUMNSTATUSES
    -- =====================================================

    -- Relaciona los estados del flujo con las columnas.
    CREATE TABLE dbo.BoardColumnStatuses
    (
        BoardColumnStatusId INT IDENTITY(1,1) NOT NULL,

        BoardId INT NOT NULL,
        BoardColumnId INT NOT NULL,
        WorkflowStatusId INT NOT NULL,

        CONSTRAINT PK_BoardColumnStatuses
            PRIMARY KEY (BoardColumnStatusId),

        -- Un estado solo puede aparecer una vez por tablero.
        CONSTRAINT UQ_BoardColumnStatuses_Board_Status
            UNIQUE (BoardId, WorkflowStatusId),

        -- Garantiza que la columna pertenezca al tablero.
        CONSTRAINT FK_BoardColumnStatuses_BoardColumns
            FOREIGN KEY (BoardId, BoardColumnId)
            REFERENCES dbo.BoardColumns(BoardId, BoardColumnId),

        -- Garantiza que el estado exista.
        CONSTRAINT FK_BoardColumnStatuses_WorkflowStatuses
            FOREIGN KEY (WorkflowStatusId)
            REFERENCES dbo.WorkflowStatuses(WorkflowStatusId)
    );


    -- =====================================================
    -- 5. VALIDAR CLAVE COMPUESTA DE ISSUES
    -- =====================================================

    -- Permite relacionar una tarea con su proyecto.
    -- Solo crea la restriccion cuando no existe.
    IF NOT EXISTS
    (
        SELECT 1
        FROM sys.key_constraints
        WHERE name = 'UQ_Issues_Project_Issue'
          AND parent_object_id = OBJECT_ID('dbo.Issues')
    )
    BEGIN
        ALTER TABLE dbo.Issues
        ADD CONSTRAINT UQ_Issues_Project_Issue
            UNIQUE (ProjectId, IssueId);
    END;


    -- =====================================================
    -- 6. TABLA ISSUEBOARDPOSITIONS
    -- =====================================================

    -- Almacena la posicion de cada tarea en cada tablero.
    CREATE TABLE dbo.IssueBoardPositions
    (
        IssueBoardPositionId BIGINT IDENTITY(1,1) NOT NULL,

        BoardId INT NOT NULL,
        ProjectId INT NOT NULL,
        IssueId BIGINT NOT NULL,

        -- Permite ordenar tareas con posiciones decimales.
        Position DECIMAL(19,6) NOT NULL,

        UpdatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueBoardPositions_UpdatedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IssueBoardPositions
            PRIMARY KEY (IssueBoardPositionId),

        -- Una tarea solo tiene una posicion por tablero.
        CONSTRAINT UQ_IssueBoardPositions_Board_Issue
            UNIQUE (BoardId, IssueId),

        -- Impide posiciones negativas.
        CONSTRAINT CK_IssueBoardPositions_Position
            CHECK (Position >= 0),

        -- Valida que el tablero pertenezca al proyecto.
        CONSTRAINT FK_IssueBoardPositions_Boards
            FOREIGN KEY (ProjectId, BoardId)
            REFERENCES dbo.Boards(ProjectId, BoardId),

        -- Valida que la tarea pertenezca al mismo proyecto.
        CONSTRAINT FK_IssueBoardPositions_Issues
            FOREIGN KEY (ProjectId, IssueId)
            REFERENCES dbo.Issues(ProjectId, IssueId)
    );


    -- =====================================================
    -- 7. INDICES PARA MEJORAR CONSULTAS
    -- =====================================================

    -- Facilita buscar columnas por tablero.
    CREATE INDEX IX_BoardColumns_Board
        ON dbo.BoardColumns(BoardId);

    -- Facilita consultar estados por columna.
    CREATE INDEX IX_BoardColumnStatuses_Column
        ON dbo.BoardColumnStatuses(BoardColumnId);

    -- Facilita ordenar tareas dentro de un tablero.
    CREATE INDEX IX_IssueBoardPositions_Board_Position
        ON dbo.IssueBoardPositions(BoardId, Position);


    -- =====================================================
    -- 8. CONFIRMAR TRANSACCION
    -- =====================================================

    -- Guarda todos los cambios si no hubo errores.
    COMMIT TRANSACTION;

    PRINT 'Modulo de tableros creado correctamente.';

END TRY
BEGIN CATCH

    -- Deshace los cambios si alguna instruccion falla.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    -- Muestra el error original.
    THROW;

END CATCH;
GO
