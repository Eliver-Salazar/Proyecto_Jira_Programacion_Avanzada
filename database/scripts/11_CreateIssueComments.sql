
USE JiraCloneDB;
GO

-- Revierte automaticamente la transaccion ante errores.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- ==========================================
    -- 1. VALIDAR DEPENDENCIAS
    -- ==========================================

    -- Las tareas y los usuarios deben existir.
    IF OBJECT_ID('dbo.Issues', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
    BEGIN
        THROW 50070, 'Faltan tablas requeridas.', 1;
    END;

    -- Evita crear nuevamente la tabla.
    IF OBJECT_ID('dbo.IssueComments', 'U') IS NOT NULL
    BEGIN
        THROW 50071, 'La tabla IssueComments ya existe.', 1;
    END;

    -- ==========================================
    -- 2. CREAR TABLA ISSUECOMMENTS
    -- ==========================================

    -- Almacena los comentarios realizados en las tareas.
    CREATE TABLE dbo.IssueComments
    (
        IssueCommentId BIGINT IDENTITY(1,1) NOT NULL,

        -- Tarea a la que pertenece el comentario.
        IssueId BIGINT NOT NULL,

        -- Usuario que escribio el comentario.
        AuthorUserId NVARCHAR(128) NOT NULL,

        -- Contenido del comentario.
        Content NVARCHAR(MAX) NOT NULL,

        -- Fecha de creacion en horario UTC.
        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueComments_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        -- Fecha de ultima modificacion.
        UpdatedAt DATETIME2(3) NULL,

        -- Indica si el comentario fue editado.
        IsEdited BIT NOT NULL
            CONSTRAINT DF_IssueComments_IsEdited
            DEFAULT (0),

        -- Eliminacion logica: conserva el registro.
        IsDeleted BIT NOT NULL
            CONSTRAINT DF_IssueComments_IsDeleted
            DEFAULT (0),

        -- Fecha de eliminacion logica.
        DeletedAt DATETIME2(3) NULL,

        -- Control de concurrencia.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_IssueComments
            PRIMARY KEY (IssueCommentId),

        -- No permite comentarios vacios.
        CONSTRAINT CK_IssueComments_Content
            CHECK (
                LEN(LTRIM(RTRIM(Content))) > 0
            ),

        -- Si esta eliminado, debe tener fecha.
        CONSTRAINT CK_IssueComments_Deleted
            CHECK (
                (IsDeleted = 0 AND DeletedAt IS NULL)
                OR
                (IsDeleted = 1 AND DeletedAt IS NOT NULL)
            ),

        -- Si esta editado, debe tener fecha.
        CONSTRAINT CK_IssueComments_Edited
            CHECK (
                IsEdited = 0
                OR UpdatedAt IS NOT NULL
            ),

        -- Relacion con la tarea.
        CONSTRAINT FK_IssueComments_Issues
            FOREIGN KEY (IssueId)
            REFERENCES dbo.Issues(IssueId),

        -- Relacion con el autor.
        CONSTRAINT FK_IssueComments_AspNetUsers
            FOREIGN KEY (AuthorUserId)
            REFERENCES dbo.AspNetUsers(Id)
    );

    -- ==========================================
    -- 3. CREAR INDICES
    -- ==========================================

    -- Mejora la consulta de comentarios por tarea.
    CREATE INDEX IX_IssueComments_Issue_CreatedAt
        ON dbo.IssueComments(IssueId, CreatedAt);

    -- Mejora la consulta de comentarios por usuario.
    CREATE INDEX IX_IssueComments_Author
        ON dbo.IssueComments(AuthorUserId);

    -- ==========================================
    -- 4. CONFIRMAR TRANSACCION
    -- ==========================================

    COMMIT TRANSACTION;

    PRINT 'Modulo IssueComments creado correctamente.';

END TRY
BEGIN CATCH

    -- Deshace los cambios ante cualquier error.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO