
USE JiraCloneDB;
GO

SET NOCOUNT ON;

-- ============================================
-- 1. RESUMEN GENERAL DE LA BASE DE DATOS
-- ============================================

SELECT
    DB_NAME() AS BaseDatos,

    (SELECT COUNT(*)
     FROM sys.tables
     WHERE is_ms_shipped = 0) AS TotalTablas,

    (SELECT COUNT(*)
     FROM sys.foreign_keys) AS TotalRelaciones,

    (SELECT COUNT(*)
     FROM sys.check_constraints) AS TotalChecks,

    (SELECT COUNT(*)
     FROM sys.key_constraints
     WHERE type = 'UQ') AS TotalUnique,

    (SELECT COUNT(*)
     FROM sys.procedures
     WHERE is_ms_shipped = 0) AS TotalProcedimientos;


-- ============================================
-- 2. INVENTARIO COMPLETO DE TABLAS
-- ============================================

SELECT
    s.name AS Esquema,
    t.name AS NombreTabla,
    (SELECT COUNT(*)
     FROM sys.columns c
     WHERE c.object_id = t.object_id) AS TotalColumnas
FROM sys.tables t
INNER JOIN sys.schemas s
    ON t.schema_id = s.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY t.name;


-- ============================================
-- 3. INVENTARIO DE RELACIONES
-- ============================================

SELECT
    fk.name AS NombreRelacion,
    OBJECT_NAME(fk.parent_object_id) AS TablaOrigen,
    OBJECT_NAME(fk.referenced_object_id) AS TablaDestino,
    fk.is_disabled AS Deshabilitada,
    fk.is_not_trusted AS NoValidada
FROM sys.foreign_keys fk
ORDER BY TablaOrigen, NombreRelacion;


-- ============================================
-- 4. RESTRICCIONES CHECK
-- ============================================

SELECT
    OBJECT_NAME(parent_object_id) AS Tabla,
    name AS Restriccion,
    definition AS Condicion,
    is_disabled AS Deshabilitada,
    is_not_trusted AS NoValidada
FROM sys.check_constraints
ORDER BY Tabla, Restriccion;


-- ============================================
-- 5. RESTRICCIONES UNIQUE
-- ============================================

SELECT
    OBJECT_NAME(parent_object_id) AS Tabla,
    name AS Restriccion
FROM sys.key_constraints
WHERE type = 'UQ'
ORDER BY Tabla, Restriccion;


-- ============================================
-- 6. INDICES DE LA BASE DE DATOS
-- ============================================

SELECT
    OBJECT_NAME(i.object_id) AS Tabla,
    i.name AS NombreIndice,
    i.is_unique AS EsUnico,
    i.has_filter AS TieneFiltro,
    i.filter_definition AS Filtro
FROM sys.indexes i
INNER JOIN sys.tables t
    ON i.object_id = t.object_id
WHERE i.name IS NOT NULL
  AND t.is_ms_shipped = 0
ORDER BY Tabla, NombreIndice;


-- ============================================
-- 7. PROCEDIMIENTOS ALMACENADOS
-- ============================================

SELECT
    name AS Procedimiento,
    create_date AS FechaCreacion,
    modify_date AS UltimaModificacion
FROM sys.procedures
WHERE is_ms_shipped = 0
ORDER BY name;


-- ============================================
-- 8. TABLAS PENDIENTES DEL MODELO PLANIFICADO
-- ============================================

DECLARE @TablasEsperadas TABLE
(
    NombreTabla SYSNAME PRIMARY KEY
);

INSERT INTO @TablasEsperadas (NombreTabla)
VALUES
    ('AspNetUsers'),
    ('AspNetRoles'),
    ('AspNetUserRoles'),
    ('AspNetUserClaims'),
    ('AspNetUserLogins'),
    ('Projects'),
    ('ProjectMembers'),
    ('ProjectSettings'),
    ('ProjectInvitations'),
    ('Issues'),
    ('IssueTypes'),
    ('IssuePriorities'),
    ('IssueRelations'),
    ('ProjectIssueCounters'),
    ('WorkflowStatuses'),
    ('WorkflowTransitions'),
    ('Boards'),
    ('BoardColumns'),
    ('BoardColumnStatuses'),
    ('IssueBoardPositions'),
    ('Sprints'),
    ('SprintIssues'),
    ('IssueComments'),
    ('IssueAttachments'),
    ('Labels'),
    ('IssueLabels'),
    ('IssueHistory'),
    ('Notifications'),
    ('IssueWatchers'),
    ('ProjectActivity'),
    ('IssueWorkLogs');

SELECT
    e.NombreTabla AS TablaPendiente
FROM @TablasEsperadas e
WHERE OBJECT_ID(
    N'dbo.' + e.NombreTabla, 'U'
) IS NULL
ORDER BY e.NombreTabla;


-- ============================================
-- 9. VALIDAR CLAVES PRIMARIAS
-- ============================================

SELECT
    t.name AS TablaSinPrimaryKey
FROM sys.tables t
WHERE t.is_ms_shipped = 0
  AND NOT EXISTS
  (
      SELECT 1
      FROM sys.key_constraints kc
      WHERE kc.parent_object_id = t.object_id
        AND kc.type = 'PK'
  )
ORDER BY t.name;


-- ============================================
-- 10. VALIDAR INTEGRIDAD DE LAS RELACIONES
-- ============================================

SELECT
    name AS RelacionConProblema,
    OBJECT_NAME(parent_object_id) AS Tabla
FROM sys.foreign_keys
WHERE is_disabled = 1
   OR is_not_trusted = 1;


-- ============================================
-- 11. RESUMEN FINAL
-- ============================================

PRINT 'Auditoria finalizada correctamente.';
PRINT 'Revisar especialmente las consultas 8, 9 y 10.';
GO
