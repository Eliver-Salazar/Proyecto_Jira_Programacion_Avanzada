
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Verificar dependencias.
    IF OBJECT_ID('dbo.Issues', 'U') IS NULL
        THROW 50070, 'La tabla Issues no existe.', 1;

    IF OBJECT_ID('dbo.IssueRelations', 'U') IS NOT NULL
        THROW 50071, 'La tabla IssueRelations ya existe.', 1;

    -- 2. Crear tabla de relaciones.
    CREATE TABLE dbo.IssueRelations
    (
        IssueRelationId BIGINT IDENTITY(1,1) NOT NULL,

        SourceIssueId BIGINT NOT NULL,
        TargetIssueId BIGINT NOT NULL,

        RelationType NVARCHAR(30) NOT NULL,

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_IssueRelations_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        CreatedByUserId NVARCHAR(128) NOT NULL,

        CONSTRAINT PK_IssueRelations
            PRIMARY KEY (IssueRelationId),

        -- La tarea de origen debe existir.
        CONSTRAINT FK_IssueRelations_SourceIssue
            FOREIGN KEY (SourceIssueId)
            REFERENCES dbo.Issues(IssueId),

        -- La tarea de destino debe existir.
        CONSTRAINT FK_IssueRelations_TargetIssue
            FOREIGN KEY (TargetIssueId)
            REFERENCES dbo.Issues(IssueId),

        -- El usuario que creó la relación debe existir.
        CONSTRAINT FK_IssueRelations_CreatedBy
            FOREIGN KEY (CreatedByUserId)
            REFERENCES dbo.AspNetUsers(Id),

        -- Impedir que una tarea se relacione consigo misma.
        CONSTRAINT CK_IssueRelations_DifferentIssues
            CHECK (SourceIssueId <> TargetIssueId),

        -- Tipos de relaciones permitidos.
        CONSTRAINT CK_IssueRelations_Type
            CHECK (
                RelationType IN (
                    'BLOCKS',
                    'RELATES_TO',
                    'DUPLICATES',
                    'CLONES'
                )
            ),

        -- Evitar relaciones idénticas duplicadas.
        CONSTRAINT UQ_IssueRelations_Source_Target_Type
            UNIQUE (
                SourceIssueId,
                TargetIssueId,
                RelationType
            )
    );

    -- 3. Índices para mejorar las consultas.
    CREATE INDEX IX_IssueRelations_Source
        ON dbo.IssueRelations(SourceIssueId);

    CREATE INDEX IX_IssueRelations_Target
        ON dbo.IssueRelations(TargetIssueId);

    CREATE INDEX IX_IssueRelations_CreatedBy
        ON dbo.IssueRelations(CreatedByUserId);

    COMMIT TRANSACTION;

    PRINT 'IssueRelations creada correctamente.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO

-- 4. Verificar tabla.
SELECT name AS NombreTabla
FROM sys.tables
WHERE name = 'IssueRelations';

-- 5. Verificar relaciones.
SELECT
    fk.name AS NombreRelacion,
    OBJECT_NAME(fk.parent_object_id) AS TablaOrigen,
    OBJECT_NAME(fk.referenced_object_id) AS TablaDestino
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID('dbo.IssueRelations');

-- 6. Verificar índices.
SELECT name AS NombreIndice
FROM sys.indexes
WHERE object_id = OBJECT_ID('dbo.IssueRelations')
  AND name IS NOT NULL;

-- 7. Verificar restricciones.
SELECT name AS NombreRestriccion
FROM sys.check_constraints
WHERE parent_object_id = OBJECT_ID('dbo.IssueRelations');
GO
