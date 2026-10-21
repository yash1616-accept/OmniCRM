-- ========================================================
-- OmniCRM Database Migration Script
-- Creates the OmniCrmDb database, relational tables,
-- foreign keys, unique indexes, and initial seed records.
-- Compatible with SQL Server 2017, 2019, 2022, and SQL Express.
-- ========================================================

-- 1. Create Database if it does not already exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'OmniCrmDb')
BEGIN
    CREATE DATABASE [OmniCrmDb];
    PRINT 'Database OmniCrmDb created successfully.';
END
GO

USE [OmniCrmDb];
GO

-- 2. Create Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE [Customers] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Name] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(120) NOT NULL,
        [Phone] NVARCHAR(25) NULL,
        [CompanyName] NVARCHAR(100) NULL,
        [Address] NVARCHAR(250) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Customers_Email] ON [Customers]([Email]);
    CREATE NONCLUSTERED INDEX [IX_Customers_Name] ON [Customers]([Name]);
    PRINT 'Customers table created with unique email index.';
END
GO

-- 3. Create Leads Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leads')
BEGIN
    CREATE TABLE [Leads] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Title] NVARCHAR(150) NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [EstimatedValue] DECIMAL(18,2) NOT NULL DEFAULT 0.00,
        [Notes] NVARCHAR(500) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CustomerId] INT NOT NULL,
        CONSTRAINT [FK_Leads_Customers_CustomerId] FOREIGN KEY ([CustomerId]) 
            REFERENCES [Customers]([Id]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_Leads_CustomerId] ON [Leads]([CustomerId]);
    CREATE NONCLUSTERED INDEX [IX_Leads_Status] ON [Leads]([Status]);
    PRINT 'Leads table created with cascade delete foreign key.';
END
GO

-- 4. Create FollowUps Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FollowUps')
BEGIN
    CREATE TABLE [FollowUps] (
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [CustomerId] INT NOT NULL,
        [ScheduledDate] DATETIME2 NOT NULL,
        [Notes] NVARCHAR(1000) NOT NULL,
        [IsCompleted] BIT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [FK_FollowUps_Customers_CustomerId] FOREIGN KEY ([CustomerId]) 
            REFERENCES [Customers]([Id]) ON DELETE CASCADE
    );
    CREATE NONCLUSTERED INDEX [IX_FollowUps_CustomerId] ON [FollowUps]([CustomerId]);
    CREATE NONCLUSTERED INDEX [IX_FollowUps_ScheduledDate] ON [FollowUps]([ScheduledDate]);
    CREATE NONCLUSTERED INDEX [IX_FollowUps_IsCompleted] ON [FollowUps]([IsCompleted]);
    PRINT 'FollowUps table created with scheduled date index.';
END
GO

-- 5. Seed Enterprise Demonstration Data if table is empty
IF NOT EXISTS (SELECT 1 FROM [Customers])
BEGIN
    INSERT INTO [Customers] ([Name], [Email], [Phone], [CompanyName], [Address], [CreatedAt])
    VALUES 
    ('John Doe', 'john.doe@apexindustries.com', '+1 (555) 234-5678', 'Apex Industries Ltd', '100 Industrial Parkway, Chicago, IL', DATEADD(DAY, -30, GETUTCDATE())),
    ('Sophia Martinez', 'smartinez@nexustech.io', '+1 (555) 876-5432', 'Nexus Tech Solutions', '450 Silicon Blvd, Austin, TX', DATEADD(DAY, -20, GETUTCDATE())),
    ('Vikram Halnure', 'vikram@halnuredistributors.com', '+91 98230 12345', 'Halnure Global Distributors', 'Plot 42, Sector 18, Pune, MH', DATEADD(DAY, -10, GETUTCDATE()));

    DECLARE @Cust1 INT = (SELECT Id FROM [Customers] WHERE Email = 'john.doe@apexindustries.com');
    DECLARE @Cust2 INT = (SELECT Id FROM [Customers] WHERE Email = 'smartinez@nexustech.io');
    DECLARE @Cust3 INT = (SELECT Id FROM [Customers] WHERE Email = 'vikram@halnuredistributors.com');

    INSERT INTO [Leads] ([Title], [CustomerId], [Status], [EstimatedValue], [Notes], [CreatedAt])
    VALUES
    ('Enterprise ERP Software License', @Cust1, 2, 45000.00, 'Client requested detailed quote for 50 workstation seats.', DATEADD(DAY, -15, GETUTCDATE())),
    ('Cloud Infrastructure Migration', @Cust2, 1, 28000.00, 'Introductory technical briefing scheduled for next Monday.', DATEADD(DAY, -8, GETUTCDATE())),
    ('Annual Maintenance & Support Contract', @Cust3, 3, 15000.00, 'Contract signed and purchase order issued.', DATEADD(DAY, -5, GETUTCDATE())),
    ('Custom Mobile Sales Companion App', @Cust1, 0, 32000.00, 'Inbound inquiry via website contact form.', DATEADD(DAY, -2, GETUTCDATE()));

    INSERT INTO [FollowUps] ([CustomerId], [ScheduledDate], [Notes], [IsCompleted], [CreatedAt])
    VALUES
    (@Cust1, DATEADD(DAY, 2, GETUTCDATE()), 'Deliver finalized SLA proposal document and discuss deployment timeline.', 0, DATEADD(DAY, -2, GETUTCDATE())),
    (@Cust2, DATEADD(DAY, 5, GETUTCDATE()), 'Review AWS vs Azure cost comparison chart with VP of Engineering.', 0, DATEADD(DAY, -1, GETUTCDATE())),
    (@Cust3, DATEADD(DAY, -1, GETUTCDATE()), 'Kickoff onboarding meeting with operations leadership.', 1, DATEADD(DAY, -4, GETUTCDATE()));

    PRINT 'Initial demonstration customer, lead, and follow-up records seeded successfully.';
END
GO
