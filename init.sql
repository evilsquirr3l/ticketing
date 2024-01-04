-- dotnet ef migrations script --idempotent -p Ticketing/Ticketing.csproj -s Ticketing/Ticketing.csproj -o init.sql
CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Customers" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "Email" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Customers" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Events" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Events" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Manifests" (
        "Id" uuid NOT NULL,
        "Map" text NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Manifests" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Payments" (
        "Id" uuid NOT NULL,
        "Amount" numeric NOT NULL,
        "PaymentDate" timestamp with time zone NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Prices" (
        "Id" uuid NOT NULL,
        "Amount" numeric NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Prices" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Carts" (
        "Id" uuid NOT NULL,
        "CustomerId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Carts" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Carts_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Sections" (
        "Id" uuid NOT NULL,
        "Name" text NOT NULL,
        "ManifestId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Sections" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Sections_Manifests_ManifestId" FOREIGN KEY ("ManifestId") REFERENCES "Manifests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Venues" (
        "Id" uuid NOT NULL,
        "Location" text NOT NULL,
        "EventId" uuid NOT NULL,
        "ManifestId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Venues" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Venues_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Venues_Manifests_ManifestId" FOREIGN KEY ("ManifestId") REFERENCES "Manifests" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Rows" (
        "Id" uuid NOT NULL,
        "Number" text NOT NULL,
        "SectionId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Rows" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Rows_Sections_SectionId" FOREIGN KEY ("SectionId") REFERENCES "Sections" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Seats" (
        "Id" uuid NOT NULL,
        "SeatNumber" text NOT NULL,
        "RowId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Seats" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_Seats_Rows_RowId" FOREIGN KEY ("RowId") REFERENCES "Rows" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "Offers" (
        "Id" uuid NOT NULL,
        "OfferType" text NOT NULL,
        "SeatId" uuid,
        "SectionId" uuid,
        "EventId" uuid NOT NULL,
        "PaymentId" uuid,
        "PriceId" uuid,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_Offers" PRIMARY KEY ("Id"),
        CONSTRAINT offer_section_seat_check CHECK (("SectionId" IS NOT NULL AND "SeatId" IS NULL) OR ("SectionId" IS NULL AND "SeatId" IS NOT NULL)),
        CONSTRAINT "FK_Offers_Events_EventId" FOREIGN KEY ("EventId") REFERENCES "Events" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_Offers_Payments_PaymentId" FOREIGN KEY ("PaymentId") REFERENCES "Payments" ("Id"),
        CONSTRAINT "FK_Offers_Prices_PriceId" FOREIGN KEY ("PriceId") REFERENCES "Prices" ("Id"),
        CONSTRAINT "FK_Offers_Seats_SeatId" FOREIGN KEY ("SeatId") REFERENCES "Seats" ("Id"),
        CONSTRAINT "FK_Offers_Sections_SectionId" FOREIGN KEY ("SectionId") REFERENCES "Sections" ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE TABLE "CartItems" (
        "Id" uuid NOT NULL,
        "CartId" uuid NOT NULL,
        "OfferId" uuid NOT NULL,
        "IsDeleted" boolean NOT NULL,
        CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_CartItems_Carts_CartId" FOREIGN KEY ("CartId") REFERENCES "Carts" ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_CartItems_Offers_OfferId" FOREIGN KEY ("OfferId") REFERENCES "Offers" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_CartItems_CartId" ON "CartItems" ("CartId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_CartItems_OfferId" ON "CartItems" ("OfferId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Carts_CustomerId" ON "Carts" ("CustomerId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Offers_EventId" ON "Offers" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE UNIQUE INDEX "IX_Offers_PaymentId" ON "Offers" ("PaymentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Offers_PriceId" ON "Offers" ("PriceId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Offers_SeatId" ON "Offers" ("SeatId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Offers_SectionId" ON "Offers" ("SectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Rows_SectionId" ON "Rows" ("SectionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Seats_RowId" ON "Seats" ("RowId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE INDEX "IX_Sections_ManifestId" ON "Sections" ("ManifestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE UNIQUE INDEX "IX_Venues_EventId" ON "Venues" ("EventId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    CREATE UNIQUE INDEX "IX_Venues_ManifestId" ON "Venues" ("ManifestId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126184527_InitialMigration') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231126184527_InitialMigration', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126193711_UpdateEvent') THEN
    ALTER TABLE "Events" ADD "Date" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126193711_UpdateEvent') THEN
    ALTER TABLE "Events" ADD "Description" text NOT NULL DEFAULT '';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126193711_UpdateEvent') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231126193711_UpdateEvent', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126211430_AddPriceToOffer') THEN
    ALTER TABLE "Offers" DROP CONSTRAINT "FK_Offers_Prices_PriceId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126211430_AddPriceToOffer') THEN
    UPDATE "Offers" SET "PriceId" = '00000000-0000-0000-0000-000000000000' WHERE "PriceId" IS NULL;
    ALTER TABLE "Offers" ALTER COLUMN "PriceId" SET NOT NULL;
    ALTER TABLE "Offers" ALTER COLUMN "PriceId" SET DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126211430_AddPriceToOffer') THEN
    ALTER TABLE "Offers" ADD CONSTRAINT "FK_Offers_Prices_PriceId" FOREIGN KEY ("PriceId") REFERENCES "Prices" ("Id") ON DELETE CASCADE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231126211430_AddPriceToOffer') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231126211430_AddPriceToOffer', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231127175145_AddPropertyToSeat') THEN
    ALTER TABLE "Seats" ADD "IsReserved" boolean NOT NULL DEFAULT FALSE;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231127175145_AddPropertyToSeat') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231127175145_AddPropertyToSeat', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231127182752_MakePaymentDateNullable') THEN
    ALTER TABLE "Payments" ALTER COLUMN "PaymentDate" DROP NOT NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231127182752_MakePaymentDateNullable') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231127182752_MakePaymentDateNullable', '8.0.0');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231210204007_AddUniqueConstraintToCartItem') THEN
    DROP INDEX "IX_CartItems_CartId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231210204007_AddUniqueConstraintToCartItem') THEN
    CREATE UNIQUE INDEX "IX_CartItems_CartId_OfferId" ON "CartItems" ("CartId", "OfferId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20231210204007_AddUniqueConstraintToCartItem') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20231210204007_AddUniqueConstraintToCartItem', '8.0.0');
    END IF;
END $EF$;
COMMIT;

