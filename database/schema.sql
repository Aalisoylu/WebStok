CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

BEGIN TRANSACTION;
CREATE TABLE "Categories" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Categories" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "ParentId" INTEGER NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_Categories_Categories_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Warehouses" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Warehouses" PRIMARY KEY AUTOINCREMENT,
    "Code" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Description" TEXT NULL,
    "IsActive" INTEGER NOT NULL
);

CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Code" TEXT NOT NULL,
    "Barcode" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "CategoryId" INTEGER NOT NULL,
    "Unit" TEXT NOT NULL,
    "PurchasePrice" TEXT NOT NULL,
    "SalePrice" TEXT NOT NULL,
    "MinimumStockLevel" TEXT NOT NULL,
    "IssueMethod" INTEGER NOT NULL,
    "IsActive" INTEGER NOT NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "Lots" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Lots" PRIMARY KEY AUTOINCREMENT,
    "ProductId" INTEGER NOT NULL,
    "LotNumber" TEXT NOT NULL,
    "ProductionDate" TEXT NULL,
    "ExpiryDate" TEXT NULL,
    "WarrantyEndDate" TEXT NULL,
    "CreatedAtUtc" TEXT NOT NULL,
    CONSTRAINT "FK_Lots_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "StockMovements" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_StockMovements" PRIMARY KEY AUTOINCREMENT,
    "OperationId" TEXT NOT NULL,
    "WarehouseId" INTEGER NOT NULL,
    "LotId" INTEGER NOT NULL,
    "Type" INTEGER NOT NULL,
    "Quantity" TEXT NOT NULL,
    "Description" TEXT NOT NULL,
    "PerformedByUserId" INTEGER NOT NULL,
    "CreatedAtUtc" TEXT NOT NULL,
    CONSTRAINT "FK_StockMovements_Lots_LotId" FOREIGN KEY ("LotId") REFERENCES "Lots" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_StockMovements_Warehouses_WarehouseId" FOREIGN KEY ("WarehouseId") REFERENCES "Warehouses" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_Categories_ParentId" ON "Categories" ("ParentId");

CREATE UNIQUE INDEX "IX_Lots_ProductId_LotNumber" ON "Lots" ("ProductId", "LotNumber");

CREATE UNIQUE INDEX "IX_Products_Barcode" ON "Products" ("Barcode");

CREATE INDEX "IX_Products_CategoryId" ON "Products" ("CategoryId");

CREATE UNIQUE INDEX "IX_Products_Code" ON "Products" ("Code");

CREATE INDEX "IX_StockMovements_LotId" ON "StockMovements" ("LotId");

CREATE INDEX "IX_StockMovements_OperationId" ON "StockMovements" ("OperationId");

CREATE INDEX "IX_StockMovements_WarehouseId_LotId" ON "StockMovements" ("WarehouseId", "LotId");

CREATE UNIQUE INDEX "IX_Warehouses_Code" ON "Warehouses" ("Code");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260920135400_InitialCreate', '10.0.12');

COMMIT;

