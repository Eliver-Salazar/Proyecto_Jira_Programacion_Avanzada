
USE JiraCloneDB;
GO

-- Revierte la transaccion automaticamente si ocurre un error.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. VALIDAR DEPENDENCIAS
    -- ==========================================

    -- Verificamos que existan las tablas relacionadas.
    IF OBJECT_ID('dbo.Issues', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
    BEGIN
        THROW 50080, 'Faltan las tablas Issues o AspNetUsers.', 1;
    END;

    -- Evitamos crear nuevamente la tabla.
    IF OBJECT_ID('dbo.IssueAttachments', 'U') IS NOT NULL
    BEGIN
        THROW 50081, 'La tabla IssueAttachments ya existe.', 1;
    END;


    -- ==========================================
    -- 2. CREAR TABLA ISSUEATTACHMENTS
    -- ==========================================

    -- Guarda la informacion de los archivos adjuntos.
    CREATE TABLE dbo.IssueAttachments
    (
        IssueAttachmentId BIGINT IDENTITY(1,1) NOT NULL,

        -- Tarea a la que pertenece el archivo.
        IssueId BIGINT NOT NULL,

        -- Usuario que subio el archivo.
        UploadedByUserId NVARCHAR(128) NOT NULL,

        -- Nombre original visible para el usuario.
        OriginalFileName NVARCHAR(255) NOT NULL,

        -- Identificador interno unico del archivo.
        StoredFileName NVARCHAR(255) NOT NULL,

        -- Ubicacion interna del archivo.
        StoragePath NVARCHAR(1000) NOT NULL,

        -- Tipo MIME, por ejemplo application/pdf.
        ContentType NVARCHAR(150) NOT NULL,

        -- Tamano del archivo expresado en bytes.
        FileSizeBytes BIGINT NOT NULL,

        -- Huella SHA-256 opcional para verificar integridad.
        FileHashSHA256 CHAR(64) NULL,

        -- Fecha en la que se adjunto el archivo.
        UploadedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueAttachments_UploadedAt
            DEFAULT (SYSUTCDATETIME()),

        -- Permite eliminar logicamente el adjunto.
        IsDeleted BIT NOT NULL
            CONSTRAINT DF_IssueAttachments_IsDeleted
            DEFAULT (0),

        -- Fecha de eliminacion logica.
        DeletedAt DATETIME2(3) NULL,

        -- Control de concurrencia.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_IssueAttachments
            PRIMARY KEY (IssueAttachmentId),

        -- Evita repetir el identificador de almacenamiento.
        CONSTRAINT UQ_IssueAttachments_StoredFileName
            UNIQUE (StoredFileName),

        -- Impide nombres originales vacios.
        CONSTRAINT CK_IssueAttachments_OriginalFileName
            CHECK (LEN(LTRIM(RTRIM(OriginalFileName))) > 0),

        -- Impide identificadores internos vacios.
        CONSTRAINT CK_IssueAttachments_StoredFileName
            CHECK (LEN(LTRIM(RTRIM(StoredFileName))) > 0),

        -- Impide rutas vacias.
        CONSTRAINT CK_IssueAttachments_StoragePath
            CHECK (LEN(LTRIM(RTRIM(StoragePath))) > 0),

        -- Impide tipos de contenido vacios.
        CONSTRAINT CK_IssueAttachments_ContentType
            CHECK (LEN(LTRIM(RTRIM(ContentType))) > 0),

        -- Solo permite archivos con tamano positivo.
        CONSTRAINT CK_IssueAttachments_FileSize
            CHECK (FileSizeBytes > 0),

        -- Comprueba que la eliminacion sea consistente.
        CONSTRAINT CK_IssueAttachments_Deleted
            CHECK (
                (IsDeleted = 0 AND DeletedAt IS NULL)
                OR
                (IsDeleted = 1 AND DeletedAt IS NOT NULL)
            ),

        -- Comprueba el formato hexadecimal del SHA-256.
        CONSTRAINT CK_IssueAttachments_SHA256
            CHECK (
                FileHashSHA256 IS NULL
                OR FileHashSHA256 COLLATE Latin1_General_BIN2
                    NOT LIKE '%[^0-9A-Fa-f]%'
            ),

        -- Relacion con la tarea.
        CONSTRAINT FK_IssueAttachments_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId),

        -- Relacion con el usuario que subio el archivo.
        CONSTRAINT FK_IssueAttachments_AspNetUsers
            FOREIGN KEY (UploadedByUserId)
            REFERENCES dbo.AspNetUsers(Id)
    );


    -- ==========================================
    -- 3. CREAR INDICES
    -- ==========================================

    -- Optimiza la consulta de adjuntos activos por tarea.
    CREATE INDEX IX_IssueAttachments_Issue_Active
        ON dbo.IssueAttachments(IssueId, UploadedAt)
        WHERE IsDeleted = 0;

    -- Optimiza la consulta de adjuntos por usuario.
    CREATE INDEX IX_IssueAttachments_Uploader
        ON dbo.IssueAttachments(UploadedByUserId);


    -- ==========================================
    -- 4. CONFIRMAR TRANSACCION
    -- ==========================================

    COMMIT TRANSACTION;

    PRINT 'Modulo IssueAttachments creado correctamente.';

END TRY
BEGIN CATCH

    -- Deshace los cambios si ocurre algun error.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO
