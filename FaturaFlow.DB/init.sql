-- Create the database
CREATE DATABASE IF NOT EXISTS faturaflow_db
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE faturaflow_db;

-- Customers Table
CREATE TABLE IF NOT EXISTS Customers (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    NIF CHAR(9) NOT NULL UNIQUE, 
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address VARCHAR(255),
    City VARCHAR(100),
    ZipCode VARCHAR(10),
    IsActive BOOLEAN NOT NULL DEFAULT True
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Suppliers Table
CREATE TABLE IF NOT EXISTS Suppliers (
    Id CHAR(36) PRIMARY KEY,
    CompanyName VARCHAR(150) NOT NULL,
    NIPC CHAR(9) NOT NULL UNIQUE, 
    RepresentativeName VARCHAR(150),
    Phone VARCHAR(20),
    Email VARCHAR(100),
    Address VARCHAR(255),
    City VARCHAR(100),
    ZipCode VARCHAR(10),
    IsActive BOOLEAN NOT NULL DEFAULT True
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Products Table
CREATE TABLE IF NOT EXISTS Products (
    Id CHAR(36) PRIMARY KEY,
    SupplierId CHAR(36) NOT NULL,
    Name VARCHAR(150) NOT NULL,
    Reference VARCHAR(50) UNIQUE,
    Description TEXT,
    PurchasePrice DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    VatIncluded BOOLEAN NOT NULL DEFAULT False,
    PriceWithVat DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    SalePrice DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    UnitOfMeasure VARCHAR(20),
    VatRate DECIMAL(5, 2) NOT NULL DEFAULT 0.00,
    StockQuantity DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    IsActive BOOLEAN NOT NULL DEFAULT True,
    CONSTRAINT FK_Products_Suppliers FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Invoices Table (Header)
CREATE TABLE IF NOT EXISTS Invoices (
    Id CHAR(36) PRIMARY KEY,
    CustomerId CHAR(36) NOT NULL,
    InvoiceNumber VARCHAR(50) NOT NULL UNIQUE,
    IssueDate DATETIME NOT NULL,
    TotalNet DECIMAL(10, 2) NOT NULL,
    TotalVat DECIMAL(10, 2) NOT NULL,
    TotalPayable DECIMAL(10, 2) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Issued',
    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Invoice Lines Table (Details)
CREATE TABLE IF NOT EXISTS InvoiceLines (
    Id CHAR(36) PRIMARY KEY,
    InvoiceId CHAR(36) NOT NULL,
    ProductId CHAR(36) NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10, 2) NOT NULL,
    VatRate DECIMAL(5, 2) NOT NULL,
    Subtotal DECIMAL(10, 2) NOT NULL,
    VatAmount DECIMAL(18,2) NOT NULL,

    CONSTRAINT FK_Lines_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lines_Products FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Users Table
CREATE TABLE IF NOT EXISTS Users (
    Id CHAR(36) PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE, 
    Password VARCHAR(255) NOT NULL,
    Email VARCHAR(100),
    RecoveryCode VARCHAR(10)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Company Table
CREATE TABLE IF NOT EXISTS Companies (
    Id CHAR(36) PRIMARY KEY,
    Name VARCHAR(150) NOT NULL,
    NIF CHAR(9) NOT NULL UNIQUE, 
    Address VARCHAR(255),
    City VARCHAR(100),
    ZipCode VARCHAR(10),
    Phone VARCHAR(20),
    Email VARCHAR(100)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert Admin User (Notice the GUID format)
INSERT IGNORE INTO Users (Id, Username, Password) 
VALUES ('00000000-0000-0000-0000-000000000001', 'admin', '$2a$11$CsrJEPekAXnp1R.dATa8defDBnDnQoKlo4S7LhVuUJ7eSOgLI1Tuq'); -- Password is "admin" hashed with bcrypt
-- Insert Default Customer
INSERT IGNORE INTO Customers (Id, Name, NIF)
VALUES ('00000000-0000-0000-0000-000000000001', 'Consumidor Final', '999999990'); -- NIF 999999990 is commonly used for final consumers in Portugal
-- Insert Default Company
INSERT IGNORE INTO Companies (Id, Name, NIF)
VALUES ('00000000-0000-0000-0000-000000000001', 'Sua Empresa', '123456789'); -- Replace with actual company details or keep as placeholder

-- Indices for performance
CREATE INDEX idx_taxid_customer ON Customers(NIF);
CREATE INDEX idx_taxid_supplier ON Suppliers(NIPC);
CREATE INDEX idx_ref_product ON Products(Reference);