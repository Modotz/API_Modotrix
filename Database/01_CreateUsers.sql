IF DB_ID('ERP_NET9') IS NULL
BEGIN
    CREATE DATABASE ERP_NET9;
END
GO

USE ERP_NET9;
GO

IF OBJECT_ID('dbo.users', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.users
    (
        id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        uid UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_users_uid DEFAULT NEWID(),
        role_id BIGINT NOT NULL,
        name VARCHAR(100) NOT NULL,
        username VARCHAR(50) NOT NULL,
        email VARCHAR(191) NOT NULL,
        password VARCHAR(255) NOT NULL,
        photo VARCHAR(191) NULL,
        signature VARCHAR(MAX) NULL,
        is_active BIT NOT NULL CONSTRAINT DF_users_is_active DEFAULT 1,
        last_login DATETIME2 NULL,
        created_at DATETIME2 NOT NULL CONSTRAINT DF_users_created_at DEFAULT GETDATE(),
        updated_at DATETIME2 NULL,
        deleted_at DATETIME2 NULL
    );
END
GO
