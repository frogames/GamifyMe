CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251115151105_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251115151105_InitialCreate', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251120134205_FixOrderAndValidationModels') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251120134205_FixOrderAndValidationModels', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123073803_AddDatesToStoreItems') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123073803_AddDatesToStoreItems', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251123081547_AddFirstNameToUsers') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251123081547_AddFirstNameToUsers', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125075125_AddDisplayDatesToObjective') THEN

                    DO $$ 
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Objectives' AND column_name='DisplayEndDate') THEN
                            ALTER TABLE "Objectives" ADD COLUMN "DisplayEndDate" timestamp with time zone;
                        END IF;

                        IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='Objectives' AND column_name='DisplayStartDate') THEN
                            ALTER TABLE "Objectives" ADD COLUMN "DisplayStartDate" timestamp with time zone;
                        END IF;
                    END $$;
                
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125075125_AddDisplayDatesToObjective') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251125075125_AddDisplayDatesToObjective', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125101048_AddFrequencyHoursToObjective') THEN
    ALTER TABLE "Objectives" ADD "FrequencyHours" integer;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125101048_AddFrequencyHoursToObjective') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251125101048_AddFrequencyHoursToObjective', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    ALTER TABLE "Users" ADD "GroupId" uuid;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    CREATE TABLE "Groups" (
        "Id" uuid NOT NULL,
        "EstablishmentId" uuid NOT NULL,
        "Name" text NOT NULL,
        "Description" text NOT NULL,
        "IconName" text NOT NULL,
        "TotalXp" integer NOT NULL,
        CONSTRAINT "PK_Groups" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Groups_Establishments_EstablishmentId" FOREIGN KEY ("EstablishmentId") REFERENCES "Establishments" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    CREATE INDEX "IX_Users_GroupId" ON "Users" ("GroupId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    CREATE INDEX "IX_Groups_EstablishmentId" ON "Groups" ("EstablishmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Groups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "Groups" ("Id");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125102437_AddGroups') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251125102437_AddGroups', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125175126_AddBonusPeriods') THEN
    CREATE TABLE "BonusPeriods" (
        "Id" uuid NOT NULL,
        "EstablishmentId" uuid NOT NULL,
        "Name" text NOT NULL,
        "StartDate" timestamp with time zone NOT NULL,
        "EndDate" timestamp with time zone NOT NULL,
        "Type" integer NOT NULL,
        "Multiplier" double precision NOT NULL,
        "IsActive" boolean NOT NULL,
        CONSTRAINT "PK_BonusPeriods" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_BonusPeriods_Establishments_EstablishmentId" FOREIGN KEY ("EstablishmentId") REFERENCES "Establishments" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125175126_AddBonusPeriods') THEN
    CREATE INDEX "IX_BonusPeriods_EstablishmentId" ON "BonusPeriods" ("EstablishmentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125175126_AddBonusPeriods') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251125175126_AddBonusPeriods', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125181436_AddDigitalAssetUrl') THEN
    ALTER TABLE "StoreItems" ADD "DigitalAssetUrl" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251125181436_AddDigitalAssetUrl') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251125181436_AddDigitalAssetUrl', '8.0.4');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251127102022_AddIsUniqueToStoreItem') THEN
    ALTER TABLE "StoreItems" ADD "IsUnique" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251127102022_AddIsUniqueToStoreItem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251127102022_AddIsUniqueToStoreItem', '8.0.4');
    END IF;
END $EF$;
COMMIT;

