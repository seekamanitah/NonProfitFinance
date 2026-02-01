-- Fix Documents table column name
-- Run this SQL if you want to keep existing data

-- SQLite doesn't support ALTER COLUMN, so we need to recreate the table
ALTER TABLE Documents RENAME TO Documents_old;

CREATE TABLE Documents (
    Id INTEGER NOT NULL CONSTRAINT PK_Documents PRIMARY KEY AUTOINCREMENT,
    FileName TEXT NOT NULL,
    OriginalFileName TEXT NOT NULL,
    ContentType TEXT NOT NULL,
    StoragePath TEXT NOT NULL,
    FileSize INTEGER NOT NULL,
    Description TEXT,
    Tags TEXT,
    Type INTEGER NOT NULL,
    UploadedAt TEXT NOT NULL,
    UploadedBy TEXT,
    GrantId INTEGER,
    DonorId INTEGER,
    TransactionId INTEGER,
    IsArchived INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    CONSTRAINT FK_Documents_Donors_DonorId FOREIGN KEY (DonorId) REFERENCES Donors (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Documents_Grants_GrantId FOREIGN KEY (GrantId) REFERENCES Grants (Id) ON DELETE SET NULL,
    CONSTRAINT FK_Documents_Transactions_TransactionId FOREIGN KEY (TransactionId) REFERENCES Transactions (Id) ON DELETE SET NULL
);

-- Copy data from old table
INSERT INTO Documents SELECT 
    Id, FileName, OriginalFileName, ContentType, StoragePath, FileSize,
    Description, Tags, Type, 
    UploadDate as UploadedAt,  -- Rename column here
    UploadedBy, GrantId, DonorId, TransactionId, IsArchived, CreatedAt, UpdatedAt
FROM Documents_old;

-- Drop old table
DROP TABLE Documents_old;

-- Recreate indexes
CREATE INDEX IF NOT EXISTS IX_Documents_DonorId ON Documents (DonorId);
CREATE INDEX IF NOT EXISTS IX_Documents_GrantId ON Documents (GrantId);
CREATE INDEX IF NOT EXISTS IX_Documents_TransactionId ON Documents (TransactionId);
