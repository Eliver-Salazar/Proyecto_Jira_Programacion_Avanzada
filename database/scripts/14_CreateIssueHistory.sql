
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
        THROW 50060,
            'Faltan tablas requeridas para IssueHistory.', 1;
    END;

    -- 2. Evitar ejecuciones duplicadas.
    IF OBJECT_ID('dbo.IssueHistory', 'U') IS NOT NULL
    BEGIN
        THROW 50061,
            'La tabla IssueHistory ya existe.', 1;
    END;

    -- 3. Crear historial de cambios.
    CREATE TABLE dbo.IssueHistory
    (
        IssueHistoryId BIGINT IDENTITY(1,1) NOT NULL,

        IssueId BIGINT NOT NULL,

        -- Usuario que realizó la modificación.
        ChangedByUserId NVARCHAR(128) NOT NULL,

        -- Campo que sufrió el cambio.
        FieldName NVARCHAR(100) NOT NULL,

        -- Valores antes y después del cambio.
        OldValue NVARCHAR(MAX) NULL,
        NewValue NVARCHAR(MAX) NULL,

        -- Fecha y hora en UTC.
        ChangedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueHistory_ChangedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_IssueHistory
            PRIMARY KEY (IssueHistoryId),

        -- Evitar nombres de campos vacíos.
        CONSTRAINT CK_IssueHistory_FieldName
            CHECK (LEN(LTRIM(RTRIM(FieldName))) > 0),

        -- Relación con la tarea.
        CONSTRAINT FK_IssueHistory_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId),

        -- Relación con el usuario.
        CONSTRAINT FK_IssueHistory_AspNetUsers
            FOREIGN KEY (ChangedByUserId)
            REFERENCES dbo.AspNetUsers(Id)
    );

    -- 4. Índice para consultar el historial
    -- de una tarea cronológicamente.
    CREATE INDEX IX_IssueHistory_Issue_ChangedAt
        ON dbo.IssueHistory
        (
            IssueId,
            ChangedAt DESC,
            IssueHistoryId DESC
        );

    -- 5. Índice para consultar cambios por usuario.
    CREATE INDEX IX_IssueHistory_User
        ON dbo.IssueHistory(ChangedByUserId);

    COMMIT TRANSACTION;

    PRINT 'Script 14 ejecutado correctamente.';
    PRINT 'Tabla IssueHistory creada.';
END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
