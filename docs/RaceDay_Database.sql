/* ============================================================
   RaceDay Database — SQL Server DDL + seed data
   Corresponds exactly to the EF Core model in RaceDay.Infrastructure.
   Run against a clean SQL Server / LocalDB instance:

       sqlcmd -S (localdb)\mssqllocaldb -i RaceDay_Database.sql

   NOTE: this script is provided for reference / manual execution and
   mirrors what `dotnet ef database update` will generate from the
   EF Core migrations. It has not been executed in this environment
   (no SQL Server instance available here) — run it yourself and
   report back if anything fails; see README "Verification" section.
   ============================================================ */

IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO

-- ---------- Profiles ----------
IF OBJECT_ID('dbo.Profiles', 'U') IS NOT NULL DROP TABLE dbo.Profiles;
GO
CREATE TABLE dbo.Profiles (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    FullName        NVARCHAR(200)    NOT NULL,
    Email           NVARCHAR(256)    NOT NULL,
    Role            NVARCHAR(20)     NOT NULL CHECK (Role IN ('Participant','Organiser')),
    AvatarUrl       NVARCHAR(1000)   NULL,
    PasswordHash    NVARCHAR(MAX)    NOT NULL,
    SecurityStamp   NVARCHAR(64)     NOT NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Profiles_Email UNIQUE (Email)
);
GO

-- ---------- Events ----------
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
GO
CREATE TABLE dbo.Events (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    OrganiserId     UNIQUEIDENTIFIER NOT NULL,
    Name            NVARCHAR(200)    NOT NULL,
    Description     NVARCHAR(4000)   NULL,
    EventDate       DATE             NOT NULL,
    Location        NVARCHAR(300)    NOT NULL,
    DistanceKm      DECIMAL(6,2)     NOT NULL,
    EventType       NVARCHAR(10)     NOT NULL CHECK (EventType IN ('Run','Walk','Cycle')),
    BannerUrl       NVARCHAR(1000)   NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Events_Profiles FOREIGN KEY (OrganiserId) REFERENCES dbo.Profiles(Id) ON DELETE CASCADE,
    CONSTRAINT CK_Events_DistanceKm CHECK (DistanceKm > 0)
);
GO
CREATE INDEX IX_Events_OrganiserId ON dbo.Events(OrganiserId);
CREATE INDEX IX_Events_EventDate ON dbo.Events(EventDate);
CREATE INDEX IX_Events_EventType ON dbo.Events(EventType);
GO

-- ---------- Categories ----------
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
GO
CREATE TABLE dbo.Categories (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    EventId         UNIQUEIDENTIFIER NOT NULL,
    Name            NVARCHAR(150)    NOT NULL,
    Description     NVARCHAR(1000)   NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Categories_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE
);
GO
CREATE INDEX IX_Categories_EventId ON dbo.Categories(EventId);
GO

-- ---------- Enrolments ----------
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
GO
CREATE TABLE dbo.Enrolments (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    EventId         UNIQUEIDENTIFIER NOT NULL,
    CategoryId      UNIQUEIDENTIFIER NOT NULL,
    ParticipantId   UNIQUEIDENTIFIER NOT NULL,
    Status          NVARCHAR(20)     NOT NULL DEFAULT 'Registered'
                     CHECK (Status IN ('Registered','Attended','Completed','Withdrawn')),
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Enrolments_Events FOREIGN KEY (EventId) REFERENCES dbo.Events(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Enrolments_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Enrolments_Profiles FOREIGN KEY (ParticipantId) REFERENCES dbo.Profiles(Id) ON DELETE CASCADE,
    CONSTRAINT UQ_Enrolments_Event_Participant UNIQUE (EventId, ParticipantId)
);
GO
CREATE INDEX IX_Enrolments_ParticipantId ON dbo.Enrolments(ParticipantId);
GO

-- ---------- Results ----------
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
GO
CREATE TABLE dbo.Results (
    Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    EnrolmentId     UNIQUEIDENTIFIER NOT NULL,
    FinishTime      TIME             NULL,
    Position        INT              NULL,
    RecordedBy      UNIQUEIDENTIFIER NOT NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Results_Enrolments FOREIGN KEY (EnrolmentId) REFERENCES dbo.Enrolments(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Results_Profiles FOREIGN KEY (RecordedBy) REFERENCES dbo.Profiles(Id) ON DELETE NO ACTION,
    CONSTRAINT UQ_Results_EnrolmentId UNIQUE (EnrolmentId)
);
GO

/* ============================================================
   SEED DATA
   Passwords below are bcrypt/ASP.NET-Identity-style hashes are NOT
   pre-computed here (they must come from IPasswordHasher<Profile> at
   runtime). For a script-only seed, this placeholder hash forces a
   password reset flow; do not rely on it for login. Prefer seeding
   through the API's /api/auth/register endpoint or an EF Core seed
   method that calls PasswordHasher<Profile> in-process, so real,
   verifiable hashes are produced. See README "Seeding" section.
   ============================================================ */

DECLARE @Organiser1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Organiser2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Participant1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Participant2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Profiles (Id, FullName, Email, Role, PasswordHash, SecurityStamp)
VALUES
    (@Organiser1,  'Amara Ndlovu',  'amara.organiser@raceday.dev',  'Organiser',  'SEED-PLACEHOLDER-HASH', NEWID()),
    (@Organiser2,  'Peter van Wyk', 'peter.organiser@raceday.dev',  'Organiser',  'SEED-PLACEHOLDER-HASH', NEWID()),
    (@Participant1,'Lindiwe Khumalo','lindiwe.participant@raceday.dev','Participant','SEED-PLACEHOLDER-HASH', NEWID()),
    (@Participant2,'Sipho Dube',    'sipho.participant@raceday.dev','Participant','SEED-PLACEHOLDER-HASH', NEWID());

DECLARE @Event1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Event2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Event3 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Events (Id, OrganiserId, Name, Description, EventDate, Location, DistanceKm, EventType)
VALUES
    (@Event1, @Organiser1, 'Johannesburg City 10K', 'A fast, flat 10K through the CBD.', DATEADD(day, 30, CAST(GETDATE() AS DATE)), 'Johannesburg, South Africa', 10.00, 'Run'),
    (@Event2, @Organiser1, 'Sunset Park Walk', 'A relaxed 5K family walk.', DATEADD(day, 14, CAST(GETDATE() AS DATE)), 'Pretoria, South Africa', 5.00, 'Walk'),
    (@Event3, @Organiser2, 'Highveld Gran Fondo', 'A challenging 60K cycle route.', DATEADD(day, 60, CAST(GETDATE() AS DATE)), 'Benoni, South Africa', 60.00, 'Cycle');

DECLARE @Cat1a UNIQUEIDENTIFIER = NEWID();
DECLARE @Cat1b UNIQUEIDENTIFIER = NEWID();
DECLARE @Cat2a UNIQUEIDENTIFIER = NEWID();
DECLARE @Cat3a UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Categories (Id, EventId, Name, Description)
VALUES
    (@Cat1a, @Event1, 'Open', 'No age restriction.'),
    (@Cat1b, @Event1, 'Masters (40+)', 'For participants aged 40 and over.'),
    (@Cat2a, @Event2, 'Family', 'Walk with the whole family.'),
    (@Cat3a, @Event3, 'Open', 'No age restriction.');

DECLARE @Enrol1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Enrol2 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.Enrolments (Id, EventId, CategoryId, ParticipantId, Status)
VALUES
    (@Enrol1, @Event1, @Cat1a, @Participant1, 'Registered'),
    (@Enrol2, @Event2, @Cat2a, @Participant2, 'Registered');

-- Sample result: Event2 is in the past-tense demo below is illustrative only —
-- in a live system results are recorded after the event has taken place.
INSERT INTO dbo.Results (Id, EnrolmentId, FinishTime, Position, RecordedBy)
VALUES (NEWID(), @Enrol1, '00:48:32', 1, @Organiser1);
GO
