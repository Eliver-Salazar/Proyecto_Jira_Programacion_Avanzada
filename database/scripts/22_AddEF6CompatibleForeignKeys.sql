USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    ALTER TABLE dbo.Sprints WITH CHECK
    ADD CONSTRAINT FK_Sprints_Boards_EF6
        FOREIGN KEY (BoardId)
        REFERENCES dbo.Boards (BoardId);

    ALTER TABLE dbo.IssueBoardPositions WITH CHECK
    ADD CONSTRAINT FK_IssueBoardPositions_Issues_EF6
        FOREIGN KEY (IssueId)
        REFERENCES dbo.Issues (IssueId);

    ALTER TABLE dbo.IssueBoardPositions WITH CHECK
    ADD CONSTRAINT FK_IssueBoardPositions_Boards_EF6
        FOREIGN KEY (BoardId)
        REFERENCES dbo.Boards (BoardId);

    ALTER TABLE dbo.SprintIssues WITH CHECK
    ADD CONSTRAINT FK_SprintIssues_Sprints_EF6
        FOREIGN KEY (SprintId)
        REFERENCES dbo.Sprints (SprintId);

    ALTER TABLE dbo.SprintIssues WITH CHECK
    ADD CONSTRAINT FK_SprintIssues_Issues_EF6
        FOREIGN KEY (IssueId)
        REFERENCES dbo.Issues (IssueId);

    ALTER TABLE dbo.BoardColumnStatuses WITH CHECK
    ADD CONSTRAINT FK_BoardColumnStatuses_BoardColumns_EF6
        FOREIGN KEY (BoardColumnId)
        REFERENCES dbo.BoardColumns (BoardColumnId);

    ALTER TABLE dbo.ProjectActivity WITH CHECK
    ADD CONSTRAINT FK_ProjectActivity_Issues_EF6
        FOREIGN KEY (IssueId)
        REFERENCES dbo.Issues (IssueId);

    ALTER TABLE dbo.IssueLabels WITH CHECK
    ADD CONSTRAINT FK_IssueLabels_Labels_EF6
        FOREIGN KEY (LabelId)
        REFERENCES dbo.Labels (LabelId);

    ALTER TABLE dbo.IssueLabels WITH CHECK
    ADD CONSTRAINT FK_IssueLabels_Issues_EF6
        FOREIGN KEY (IssueId)
        REFERENCES dbo.Issues (IssueId);

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO

-- Resultado esperado: nueve filas, todas con ambos indicadores en 0.
SELECT
    fk.name AS ForeignKeyName,
    OBJECT_NAME(fk.parent_object_id) AS DependentTable,
    OBJECT_NAME(fk.referenced_object_id) AS PrincipalTable,
    fk.is_disabled,
    fk.is_not_trusted
FROM sys.foreign_keys AS fk
WHERE fk.name IN
(
    N'FK_Sprints_Boards_EF6',
    N'FK_IssueBoardPositions_Issues_EF6',
    N'FK_IssueBoardPositions_Boards_EF6',
    N'FK_SprintIssues_Sprints_EF6',
    N'FK_SprintIssues_Issues_EF6',
    N'FK_BoardColumnStatuses_BoardColumns_EF6',
    N'FK_ProjectActivity_Issues_EF6',
    N'FK_IssueLabels_Labels_EF6',
    N'FK_IssueLabels_Issues_EF6'
)
ORDER BY DependentTable, ForeignKeyName;
GO