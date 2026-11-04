
USE JiraCloneDB;
GO

-- Evita dejar cambios parciales si ocurre un error.
SET XACT_ABORT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- Verificamos que exista la tabla de usuarios de Identity 2.
    IF OBJECT_ID('dbo.AspNetUsers', 'U') IS NULL
        THROW 50001, 'No existe AspNetUsers.', 1;

    -- Evitamos ejecutar nuevamente este bloque sobre tablas existentes.
    IF OBJECT_ID('dbo.Projects', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.ProjectMembers', 'U') IS NOT NULL
       OR OBJECT_ID('dbo.ProjectSettings', 'U') IS NOT NULL
        THROW 50002, 'Las tablas de proyectos ya existen.', 1;

    -- Tabla principal de proyectos.
    CREATE TABLE dbo.Projects
    (
        ProjectId INT IDENTITY(1,1) NOT NULL,
        ProjectKey NVARCHAR(20) NOT NULL,
        Name NVARCHAR(150) NOT NULL,
        Description NVARCHAR(1000) NULL,

        -- Usuario que creó el proyecto.
        OwnerUserId NVARCHAR(128) NOT NULL,

        IsActive BIT NOT NULL
            CONSTRAINT DF_Projects_IsActive DEFAULT (1),

        CreatedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_Projects_CreatedAt DEFAULT (SYSUTCDATETIME()),

        UpdatedAt DATETIME2(3) NULL,

        -- Permite detectar modificaciones simultáneas.
        RowVersion ROWVERSION NOT NULL,

        CONSTRAINT PK_Projects PRIMARY KEY (ProjectId),

        -- Evita repetir la clave de un proyecto.
        CONSTRAINT UQ_Projects_ProjectKey UNIQUE (ProjectKey),

        -- Relaciona el propietario con Identity 2.
        CONSTRAINT FK_Projects_AspNetUsers
            FOREIGN KEY (OwnerUserId)
            REFERENCES dbo.AspNetUsers(Id),

        -- Impide nombres o claves vacíos.
        CONSTRAINT CK_Projects_Key
            CHECK (LEN(LTRIM(RTRIM(ProjectKey))) > 0),

        CONSTRAINT CK_Projects_Name
            CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );

    -- Permite buscar proyectos por propietario.
    CREATE INDEX IX_Projects_OwnerUserId
        ON dbo.Projects(OwnerUserId);

    -- Tabla que relaciona usuarios con proyectos.
    CREATE TABLE dbo.ProjectMembers
    (
        ProjectMemberId INT IDENTITY(1,1) NOT NULL,
        ProjectId INT NOT NULL,
        UserId NVARCHAR(128) NOT NULL,

        -- Rol del usuario dentro del proyecto.
        ProjectRole NVARCHAR(30) NOT NULL,

        JoinedAt DATETIME2(3) NOT NULL
            CONSTRAINT DF_ProjectMembers_JoinedAt
            DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_ProjectMembers
            PRIMARY KEY (ProjectMemberId),

        -- Un usuario no puede pertenecer dos veces al mismo proyecto.
        CONSTRAINT UQ_ProjectMembers_Project_User
            UNIQUE (ProjectId, UserId),

        CONSTRAINT FK_ProjectMembers_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId),

        CONSTRAINT FK_ProjectMembers_AspNetUsers
            FOREIGN KEY (UserId)
            REFERENCES dbo.AspNetUsers(Id),

        -- Roles permitidos dentro de un proyecto.
        CONSTRAINT CK_ProjectMembers_ProjectRole
            CHECK (ProjectRole IN ('ADMIN', 'MEMBER', 'VIEWER'))
    );

    -- Facilita consultar los proyectos de un usuario.
    CREATE INDEX IX_ProjectMembers_UserId
        ON dbo.ProjectMembers(UserId);

    -- Configuración individual de cada proyecto.
    CREATE TABLE dbo.ProjectSettings
    (
        ProjectId INT NOT NULL,

        -- Permite o impide crear tareas en el proyecto.
        AllowIssueCreation BIT NOT NULL
            CONSTRAINT DF_ProjectSettings_AllowIssueCreation DEFAULT (1),

        -- Controla si se permiten archivos adjuntos.
        AllowAttachments BIT NOT NULL
            CONSTRAINT DF_ProjectSettings_AllowAttachments DEFAULT (1),

        -- Controla la disponibilidad de sprints.
        EnableSprints BIT NOT NULL
            CONSTRAINT DF_ProjectSettings_EnableSprints DEFAULT (1),

        UpdatedAt DATETIME2(3) NULL,

        -- Cada proyecto tendrá una sola configuración.
        CONSTRAINT PK_ProjectSettings
            PRIMARY KEY (ProjectId),

        CONSTRAINT FK_ProjectSettings_Projects
            FOREIGN KEY (ProjectId)
            REFERENCES dbo.Projects(ProjectId)
    );

    COMMIT TRANSACTION;

    PRINT 'Tablas de proyectos creadas correctamente.';
END TRY
BEGIN CATCH
    -- Si ocurre un error, deshacemos los cambios.
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
