START TRANSACTION;

ALTER TABLE "StoreItems" ADD "EndDate" timestamp with time zone;

ALTER TABLE "StoreItems" ADD "StartDate" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251123073803_AddDatesToStoreItems', '8.0.4');

COMMIT;

