
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Validar dependencias.
    IF OBJECT_ID('dbo.Issues', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
    BEGIN
        THROW 50100,
            'Faltan tablas requeridas para IssueWorkLogs.', 1;
    END;

    -- 2. Evitar crear nuevamente la tabla.
    IF OBJECT_ID('dbo.IssueWorkLogs', 'U') IS NOT NULL
    BEGIN
        THROW 50101,
            'La tabla IssueWorkLogs ya existe.', 1;
    END;

    -- 3. Crear registros de tiempo trabajado.
    CREATE TABLE dbo.IssueWorkLogs
    (
        IssueWorkLogId BIGINT IDENTITY(1,1) NOT NULL,

        -- Tarea sobre la que se trabajó.
        IssueId BIGINT NOT NULL,

        -- Usuario que realizó el trabajo.
        UserId NVARCHAR(128) NOT NULL,

        -- Tiempo invertido, expresado en minutos.
        TimeSpentMinutes INT NOT NULL,

        -- Descripción opcional del trabajo.
        Description NVARCHAR(2000) NULL,

        -- Fecha y hora del trabajo realizado.
        WorkDate DATETIME2(3) NOT NULL,

        -- Fecha de creación del registro.
        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueWorkLogs_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        -- Fecha de última modificación.
        UpdatedAt DATETIME2(3) NULL,

        -- Control de concurrencia.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_IssueWorkLogs
            PRIMARY KEY (IssueWorkLogId),

        -- No permitir registros de tiempo negativos o cero.
        CONSTRAINT CK_IssueWorkLogs_TimeSpent
            CHECK (TimeSpentMinutes > 0),

        -- Evitar fechas de actualización anteriores
        -- a la creación del registro.
        CONSTRAINT CK_IssueWorkLogs_UpdateDate
            CHECK (
                UpdatedAt IS NULL
                OR UpdatedAt >= CreatedAt
            ),

        -- La tarea debe existir.
        CONSTRAINT FK_IssueWorkLogs_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId),

        -- El usuario debe existir.
        CONSTRAINT FK_IssueWorkLogs_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id)
    );

    -- 4. Consultar tiempo registrado por tarea.
    CREATE INDEX IX_IssueWorkLogs_Issue_WorkDate
        ON dbo.IssueWorkLogs
        (
            IssueId,
            WorkDate DESC
        );

    -- 5. Consultar tiempo registrado por usuario.
    CREATE INDEX IX_IssueWorkLogs_User_WorkDate
        ON dbo.IssueWorkLogs
        (
            UserId,
            WorkDate DESC
        );

    COMMIT TRANSACTION;

    PRINT 'Script 18 ejecutado correctamente.';
    PRINT 'Tabla IssueWorkLogs creada.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
