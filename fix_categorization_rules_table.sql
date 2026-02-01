-- =====================================================
-- Create CategorizationRules Table
-- For Auto-Categorization Feature
-- =====================================================

-- Create CategorizationRules table
CREATE TABLE IF NOT EXISTS CategorizationRules (
    Id INTEGER NOT NULL CONSTRAINT PK_CategorizationRules PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    MatchType INTEGER NOT NULL,
    MatchPattern TEXT NOT NULL,
    CaseSensitive INTEGER NOT NULL DEFAULT 0,
    CategoryId INTEGER NOT NULL,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Priority INTEGER NOT NULL DEFAULT 50,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT,
    CONSTRAINT FK_CategorizationRules_Categories_CategoryId 
        FOREIGN KEY (CategoryId) REFERENCES Categories (Id) ON DELETE RESTRICT
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS IX_CategorizationRules_CategoryId 
    ON CategorizationRules (CategoryId);

CREATE INDEX IF NOT EXISTS IX_CategorizationRules_IsActive_Priority 
    ON CategorizationRules (IsActive, Priority);

-- Verify table was created
SELECT 'CategorizationRules table created successfully' AS Status;

-- Show table structure
PRAGMA table_info(CategorizationRules);
