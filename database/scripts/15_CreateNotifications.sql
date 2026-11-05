
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Validar dependencias.
    IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
       OR OBJECT_ID('dbo.Issues', 'U') IS NULL
    BEGIN
        THROW 50070,
            'Faltan tablas requeridas para Notifications.', 1;
    END;

    -- 2. Evitar creación duplicada.
    IF OBJECT_ID('dbo.Notifications', 'U') IS NOT NULL
    BEGIN
        THROW 50071,
            'La tabla Notifications ya existe.', 1;
    END;

    -- 3. Crear tabla de notificaciones.
    CREATE TABLE dbo.Notifications
    (
        NotificationId BIGINT IDENTITY(1,1) NOT NULL,

        -- Usuario destinatario.
        UserId NVARCHAR(128) NOT NULL,

        -- Tarea asociada (opcional).
        IssueId BIGINT NULL,

        -- Tipo de notificación.
        NotificationType NVARCHAR(40) NOT NULL,

        Title NVARCHAR(200) NOT NULL,
        Message NVARCHAR(1000) NULL,

        -- Estado de lectura.
        IsRead BIT NOT NULL
            CONSTRAINT DF_Notifications_IsRead DEFAULT (0),

        ReadAt DATETIME2(3) NULL,

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Notifications_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Notifications
            PRIMARY KEY (NotificationId),

        -- Validaciones de contenido.
        CONSTRAINT CK_Notifications_Type
            CHECK (
                NotificationType IN
                (
                    'ISSUE_ASSIGNED',
                    'ISSUE_UPDATED',
                    'ISSUE_COMMENTED',
                    'ISSUE_STATUS_CHANGED',
                    'PROJECT_INVITATION',
                    'SPRINT_STARTED',
                    'SPRINT_COMPLETED',
                    'GENERAL'
                )
            ),

        CONSTRAINT CK_Notifications_Title
            CHECK (LEN(LTRIM(RTRIM(Title))) > 0),

        -- Coherencia entre lectura y fecha.
        CONSTRAINT CK_Notifications_ReadState
            CHECK (
                (IsRead = 0 AND ReadAt IS NULL)
                OR
                (IsRead = 1 AND ReadAt IS NOT NULL)
            ),

        -- Destinatario válido.
        CONSTRAINT FK_Notifications_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id),

        -- Tarea válida, cuando exista.
        CONSTRAINT FK_Notifications_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId)
    );

    -- 4. Optimizar bandeja de notificaciones.
    CREATE INDEX IX_Notifications_User_Read_CreatedAt
        ON dbo.Notifications
        (
            UserId,
            IsRead,
            CreatedAt DESC
        );

    -- 5. Consultar notificaciones por tarea.
    CREATE INDEX IX_Notifications_Issue
        ON dbo.Notifications(IssueId)
        WHERE IssueId IS NOT NULL;

    COMMIT TRANSACTION;

    PRINT 'Script 15 ejecutado correctamente.';
    PRINT 'Tabla Notifications creada.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
