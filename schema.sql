CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetRoles" PRIMARY KEY,
    "Name" TEXT NULL,
    "NormalizedName" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL
);


CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL CONSTRAINT "PK_AspNetUsers" PRIMARY KEY,
    "FirstName" TEXT NULL,
    "LastName" TEXT NULL,
    "OrganizationRole" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "LastLoginAt" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "UserName" TEXT NULL,
    "NormalizedUserName" TEXT NULL,
    "Email" TEXT NULL,
    "NormalizedEmail" TEXT NULL,
    "EmailConfirmed" INTEGER NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" INTEGER NOT NULL,
    "TwoFactorEnabled" INTEGER NOT NULL,
    "LockoutEnd" TEXT NULL,
    "LockoutEnabled" INTEGER NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL
);


CREATE TABLE "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Color" TEXT NULL,
    "Icon" TEXT NULL,
    "Type" INTEGER NOT NULL,
    "BudgetLimit" TEXT NULL,
    "IsArchived" INTEGER NOT NULL,
    "SortOrder" INTEGER NOT NULL,
    "ParentId" INTEGER NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    CONSTRAINT "FK_Categories_Categories_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Donors" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Donors" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Type" INTEGER NOT NULL,
    "Email" TEXT NULL,
    "Phone" TEXT NULL,
    "Address" TEXT NULL,
    "Notes" TEXT NULL,
    "TotalContributions" TEXT NOT NULL,
    "FirstContributionDate" TEXT NULL,
    "LastContributionDate" TEXT NULL,
    "IsAnonymous" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL
);


CREATE TABLE "Funds" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Funds" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Type" INTEGER NOT NULL,
    "Balance" TEXT NOT NULL,
    "Description" TEXT NULL,
    "TargetBalance" TEXT NULL,
    "IsActive" INTEGER NOT NULL,
    "RestrictionExpiryDate" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL
);


CREATE TABLE "Grants" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Grants" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "GrantorName" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "AmountUsed" TEXT NOT NULL,
    "StartDate" TEXT NOT NULL,
    "EndDate" TEXT NULL,
    "ApplicationDate" TEXT NULL,
    "Status" INTEGER NOT NULL,
    "Restrictions" TEXT NULL,
    "Notes" TEXT NULL,
    "GrantNumber" TEXT NULL,
    "ContactPerson" TEXT NULL,
    "ContactEmail" TEXT NULL,
    "ReportingRequirements" TEXT NULL,
    "NextReportDueDate" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL
);


CREATE TABLE "AspNetRoleClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY AUTOINCREMENT,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE
);


CREATE TABLE "AspNetUserClaims" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY AUTOINCREMENT,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Transactions" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Transactions" PRIMARY KEY AUTOINCREMENT,
    "Date" TEXT NOT NULL,
    "Amount" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Type" INTEGER NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "FundType" INTEGER NOT NULL,
    "FundId" INTEGER NULL,
    "DonorId" INTEGER NULL,
    "GrantId" INTEGER NULL,
    "Payee" TEXT NULL,
    "Tags" TEXT NULL,
    "ReferenceNumber" TEXT NULL,
    "ReceiptPath" TEXT NULL,
    "IsRecurring" INTEGER NOT NULL,
    "RecurrencePattern" TEXT NULL,
    "NextRecurrenceDate" TEXT NULL,
    "ExternalId" TEXT NULL,
    "IsReconciled" INTEGER NOT NULL,
    "CreatedAt" TEXT NOT NULL,
    "UpdatedAt" TEXT NULL,
    CONSTRAINT "FK_Transactions_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Transactions_Donors_DonorId" FOREIGN KEY ("DonorId") REFERENCES "Donors" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Transactions_Funds_FundId" FOREIGN KEY ("FundId") REFERENCES "Funds" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Transactions_Grants_GrantId" FOREIGN KEY ("GrantId") REFERENCES "Grants" ("Id") ON DELETE SET NULL
);


CREATE TABLE "Documents" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Documents" PRIMARY KEY AUTOINCREMENT,
    "FileName" TEXT NOT NULL,
    "OriginalFileName" TEXT NOT NULL,
    "ContentType" TEXT NOT NULL,
    "FileSize" INTEGER NOT NULL,
    "StoragePath" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Type" INTEGER NOT NULL,
    "GrantId" INTEGER NULL,
    "DonorId" INTEGER NULL,
    "TransactionId" INTEGER NULL,
    "UploadedAt" TEXT NOT NULL,
    "UploadedBy" TEXT NULL,
    "Tags" TEXT NULL,
    "IsArchived" INTEGER NOT NULL,
    CONSTRAINT "FK_Documents_Donors_DonorId" FOREIGN KEY ("DonorId") REFERENCES "Donors" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Documents_Grants_GrantId" FOREIGN KEY ("GrantId") REFERENCES "Grants" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Documents_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE SET NULL
);


CREATE TABLE "TransactionSplits" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_TransactionSplits" PRIMARY KEY AUTOINCREMENT,
    "TransactionId" INTEGER NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "Amount" TEXT NOT NULL,
    "Description" TEXT NULL,
    "Percentage" TEXT NULL,
    "CreatedAt" TEXT NOT NULL,
    CONSTRAINT "FK_TransactionSplits_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_TransactionSplits_Transactions_TransactionId" FOREIGN KEY ("TransactionId") REFERENCES "Transactions" ("Id") ON DELETE CASCADE
);


CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");


CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");


CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");


CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");


CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");


CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");


CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");


CREATE UNIQUE INDEX "IX_Categories_ParentId_Name" ON "Categories" ("ParentId", "Name");


CREATE INDEX "IX_Documents_DonorId" ON "Documents" ("DonorId");


CREATE INDEX "IX_Documents_GrantId" ON "Documents" ("GrantId");


CREATE INDEX "IX_Documents_TransactionId" ON "Documents" ("TransactionId");


CREATE INDEX "IX_Transactions_CategoryId" ON "Transactions" ("CategoryId");


CREATE INDEX "IX_Transactions_DonorId" ON "Transactions" ("DonorId");


CREATE INDEX "IX_Transactions_FundId" ON "Transactions" ("FundId");


CREATE INDEX "IX_Transactions_GrantId" ON "Transactions" ("GrantId");


CREATE INDEX "IX_TransactionSplits_CategoryId" ON "TransactionSplits" ("CategoryId");


CREATE INDEX "IX_TransactionSplits_TransactionId" ON "TransactionSplits" ("TransactionId");


