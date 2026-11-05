
USE JiraCloneDB;
GO

SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Validar tablas requeridas.
    IF OBJECT_ID('dbo.Projects', 'U') IS NULL
       OR OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
    BEGIN
        THROW 50060,
            'Faltan las tablas Projects o AspNetUsers.',
            1;
    END;

    -- Evitar ejecuciones duplicadas.
    IF OBJECT_ID('dbo.ProjectInvitations', 'U') IS NOT NULL
    BEGIN
        THROW 50061,
            'La tabla ProjectInvitations ya existe.',
            1;
    END;

    CREATE TABLE dbo.ProjectInvitations
    (
        ProjectInvitationId BIGINT IDENTITY(1,1) NOT NULL,

        ProjectId INT NOT NULL,

        Email NVARCHAR(256) NOT NULL,

        InvitedByUserId NVARCHAR(128) NOT NULL,

        InvitationToken UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_ProjectInvitations_Token
            DEFAULT (NEWID()),

        Status NVARCHAR(20) NOT NULL
            CONSTRAINT DF_ProjectInvitations_Status
            DEFAULT ('PENDING'),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_ProjectInvitations_CreatedAt
            DEFAULT (SYSUTCDATETIME()),

        ExpiresAt DATETIME2(3) NOT NULL,

        RespondedAt DATETIME2(3) NULL,

        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_ProjectInvitations
            PRIMARY KEY (ProjectInvitationId),

        CONSTRAINT FK_ProjectInvitations_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        CONSTRAINT FK_ProjectInvitations_InvitedBy
            FOREIGN KEY (InvitedByUserId)
            REFERENCES dbo.AspNetUsers(Id),

        CONSTRAINT CK_ProjectInvitations_Email
            CHECK (
                LEN(LTRIM(RTRIM(Email))) > 3
                AND Email LIKE '%_@_%._%'
            ),

        CONSTRAINT CK_ProjectInvitations_Status
            CHECK (
                Status IN (
                    'PENDING',
                    'ACCEPTED',
                    'REJECTED',
                    'CANCELLED',
                    'EXPIRED'
                )
            ),

        CONSTRAINT CK_ProjectInvitations_Expiration
            CHECK (ExpiresAt > CreatedAt),

        CONSTRAINT CK_ProjectInvitations_Response
            CHECK (
                (Status = 'PENDING' AND RespondedAt IS NULL)
                OR
                (Status <> 'PENDING')
            )
    );

    -- Cada invitación debe tener un token único.
    CREATE UNIQUE INDEX UX_ProjectInvitations_Token
        ON dbo.ProjectInvitations(InvitationToken);

    -- Evitar múltiples invitaciones pendientes
    -- para el mismo correo dentro de un proyecto.
    CREATE UNIQUE INDEX UX_ProjectInvitations_Project_Email_Pending
        ON dbo.ProjectInvitations(ProjectId, Email)
        WHERE Status = 'PENDING';

    -- Optimizar consultas por proyecto y estado.
    CREATE INDEX IX_ProjectInvitations_Project_Status
        ON dbo.ProjectInvitations(ProjectId, Status);

    -- Optimizar búsqueda de invitaciones por correo.
    CREATE INDEX IX_ProjectInvitations_Email
        ON dbo.ProjectInvitations(Email);

    COMMIT TRANSACTION;

    PRINT 'ProjectInvitations creada correctamente.';

END TRY
BEGIN CATCH

    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;

END CATCH;
GO