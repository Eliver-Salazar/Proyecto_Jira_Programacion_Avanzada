
USE JiraCloneDB;
GO

-- Inserta tipos de tareas solo si todavía no existen.
INSERT INTO dbo.IssueTypes (Name, Description, Icon)
SELECT v.Name, v.Description, v.Icon
FROM (VALUES
    (N'Tarea', N'Trabajo general del proyecto', N'check-square'),
    (N'Historia', N'Funcionalidad solicitada por un usuario', N'book-open'),
    (N'Error', N'Problema que debe corregirse', N'bug'),
    (N'Epica', N'Conjunto de tareas relacionadas', N'layers')
) AS v(Name, Description, Icon)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.IssueTypes AS t
    WHERE t.Name = v.Name
);

-- Inserta los niveles de prioridad.
INSERT INTO dbo.IssuePriorities (Name, PriorityLevel, Color)
SELECT v.Name, v.PriorityLevel, v.Color
FROM (VALUES
    (N'Baja', 1, N'#22C55E'),
    (N'Media', 2, N'#3B82F6'),
    (N'Alta', 3, N'#F97316'),
    (N'Critica', 4, N'#EF4444')
) AS v(Name, PriorityLevel, Color)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.IssuePriorities AS p
    WHERE p.Name = v.Name
);

-- Inserta los estados iniciales del flujo de trabajo.
INSERT INTO dbo.WorkflowStatuses
    (Name, Description, StatusCategory, Color)
SELECT v.Name, v.Description, v.StatusCategory, v.Color
FROM (VALUES
    (N'Pendiente', N'Tarea sin iniciar', N'TODO', N'#64748B'),
    (N'En progreso', N'Tarea en desarrollo', N'IN_PROGRESS', N'#3B82F6'),
    (N'En revision', N'Tarea pendiente de validacion', N'IN_PROGRESS', N'#F59E0B'),
    (N'Finalizada', N'Tarea completada', N'DONE', N'#22C55E')
) AS v(Name, Description, StatusCategory, Color)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.WorkflowStatuses AS s
    WHERE s.Name = v.Name
);
GO
