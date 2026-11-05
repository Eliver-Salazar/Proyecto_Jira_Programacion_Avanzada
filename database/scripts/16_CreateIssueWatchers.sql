
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Validar tablas requeridas.
    IF OBJECT_ID('dbo.Issues', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
    BEGIN
        THROW 50080,
            'Faltan tablas requeridas para IssueWatchers.', 1;
    END;

    -- 2. Evitar crear nuevamente la tabla.
    IF OBJECT_ID('dbo.IssueWatchers', 'U') IS NOT NULL
    BEGIN
        THROW 50081,
            'La tabla IssueWatchers ya existe.', 1;
    END;

    -- 3. Crear seguidores de tareas.
    CREATE TABLE dbo.IssueWatchers
    (
        IssueWatcherId BIGINT IDENTITY(1,1) NOT NULL,

        -- Tarea que está siendo seguida.
        IssueId BIGINT NOT NULL,

        -- Usuario que sigue la tarea.
        UserId NVARCHAR(128) NOT NULL,

        -- Fecha de suscripción.
        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueWatchers_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IssueWatchers
            PRIMARY KEY (IssueWatcherId),

        -- Impide duplicar seguidores en una tarea.
        CONSTRAINT UQ_IssueWatchers_Issue_User
            UNIQUE (IssueId, UserId),

        -- La tarea debe existir.
        CONSTRAINT FK_IssueWatchers_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId),

        -- El usuario debe existir.
        CONSTRAINT FK_IssueWatchers_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id)
    );

    -- 4. Optimizar consultas por usuario.
    CREATE INDEX IX_IssueWatchers_User
        ON dbo.IssueWatchers(UserId);

    COMMIT TRANSACTION;

    PRINT 'Script 16 ejecutado correctamente.';
    PRINT 'Tabla IssueWatchers creada.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
