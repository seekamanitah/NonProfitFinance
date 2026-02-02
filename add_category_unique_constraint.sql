-- Add Case-Insensitive Unique Constraint for Categories
-- SQLite specific - prevents duplicate category names (case-insensitive)

-- Step 1: Create a unique index on LOWER(Name) within same parent
-- This replaces the existing index with a case-insensitive one

-- First, drop the existing unique index (if it exists)
DROP INDEX IF EXISTS IX_Categories_ParentId_Name;

-- Create new case-insensitive unique index
-- For root categories (ParentId IS NULL), ensures unique names
-- For child categories, ensures unique names within same parent
CREATE UNIQUE INDEX IX_Categories_Name_Lower 
ON Categories (LOWER(Name), COALESCE(ParentId, -1));

-- Explanation:
-- - LOWER(Name) makes it case-insensitive
-- - COALESCE(ParentId, -1) handles NULL values (SQLite treats NULL != NULL)
-- - This allows "Income" and "income" to be considered duplicates
-- - Prevents: Category "Dinners" appearing twice at same level

-- Test queries to verify:

-- Should fail with unique constraint violation:
-- INSERT INTO Categories (Name, Type, ParentId) VALUES ('DINNERS', 'Expense', NULL);

-- Should succeed (different parent):
-- INSERT INTO Categories (Name, Type, ParentId) VALUES ('Supplies', 'Expense', 1);
-- INSERT INTO Categories (Name, Type, ParentId) VALUES ('Supplies', 'Expense', 2);
