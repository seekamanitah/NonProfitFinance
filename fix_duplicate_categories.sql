-- Find Duplicate Categories
-- This script identifies duplicate category names in the database

SELECT 
    Name,
    COUNT(*) as DuplicateCount,
    GROUP_CONCAT(Id) as CategoryIds
FROM Categories
GROUP BY LOWER(Name)
HAVING COUNT(*) > 1
ORDER BY DuplicateCount DESC;

-- Fix: Keep first occurrence, update references, delete duplicates
-- Run this after reviewing the duplicates above

-- Step 1: Show what will be kept vs deleted
WITH DuplicateGroups AS (
    SELECT 
        Name,
        Id,
        ROW_NUMBER() OVER (PARTITION BY LOWER(Name) ORDER BY Id) as RowNum
    FROM Categories
)
SELECT 
    Name,
    Id,
    CASE WHEN RowNum = 1 THEN 'KEEP' ELSE 'DELETE' END as Action
FROM DuplicateGroups
WHERE EXISTS (
    SELECT 1 
    FROM DuplicateGroups dg2 
    WHERE LOWER(dg2.Name) = LOWER(DuplicateGroups.Name) 
    GROUP BY LOWER(dg2.Name) 
    HAVING COUNT(*) > 1
)
ORDER BY LOWER(Name), RowNum;

-- Step 2: Update Transactions to point to first occurrence
UPDATE Transactions
SET CategoryId = (
    SELECT MIN(Id) 
    FROM Categories c2 
    WHERE LOWER(c2.Name) = LOWER((SELECT Name FROM Categories WHERE Id = Transactions.CategoryId))
)
WHERE CategoryId IN (
    SELECT Id FROM Categories c
    WHERE EXISTS (
        SELECT 1 FROM Categories c3 
        WHERE LOWER(c3.Name) = LOWER(c.Name) AND c3.Id < c.Id
    )
);

-- Step 3: Update Budget line items
UPDATE BudgetLineItems
SET CategoryId = (
    SELECT MIN(Id) 
    FROM Categories c2 
    WHERE LOWER(c2.Name) = LOWER((SELECT Name FROM Categories WHERE Id = BudgetLineItems.CategoryId))
)
WHERE CategoryId IN (
    SELECT Id FROM Categories c
    WHERE EXISTS (
        SELECT 1 FROM Categories c3 
        WHERE LOWER(c3.Name) = LOWER(c.Name) AND c3.Id < c.Id
    )
);

-- Step 4: Update CategorizationRules
UPDATE CategorizationRules
SET CategoryId = (
    SELECT MIN(Id) 
    FROM Categories c2 
    WHERE LOWER(c2.Name) = LOWER((SELECT Name FROM Categories WHERE Id = CategorizationRules.CategoryId))
)
WHERE CategoryId IN (
    SELECT Id FROM Categories c
    WHERE EXISTS (
        SELECT 1 FROM Categories c3 
        WHERE LOWER(c3.Name) = LOWER(c.Name) AND c3.Id < c.Id
    )
);

-- Step 5: Delete duplicate categories (keep first occurrence)
DELETE FROM Categories
WHERE Id IN (
    SELECT Id FROM Categories c
    WHERE EXISTS (
        SELECT 1 FROM Categories c3 
        WHERE LOWER(c3.Name) = LOWER(c.Name) AND c3.Id < c.Id
    )
);

-- Step 6: Verify no duplicates remain
SELECT 
    Name,
    COUNT(*) as Count
FROM Categories
GROUP BY LOWER(Name)
HAVING COUNT(*) > 1;
-- Should return no rows
