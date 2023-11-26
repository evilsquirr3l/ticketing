using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDataModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_SeatPrices_SeatPriceId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Carts_CartId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Venues_VenueId",
                table: "Sections");

            migrationBuilder.DropTable(
                name: "SeatPrices");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CartId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "VenueId",
                table: "Sections",
                newName: "ManifestId");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_VenueId",
                table: "Sections",
                newName: "IX_Sections_ManifestId");

            migrationBuilder.RenameColumn(
                name: "SeatPriceId",
                table: "CartItems",
                newName: "SeatId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_SeatPriceId",
                table: "CartItems",
                newName: "IX_CartItems_SeatId");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Venues",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ManifestId",
                table: "Venues",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Sections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Seats",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Rows",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Prices",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Carts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CartItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Manifests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Map = table.Column<string>(type: "text", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferType = table.Column<string>(type: "text", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: true),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PriceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Offers_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Offers_Prices_PriceId",
                        column: x => x.PriceId,
                        principalTable: "Prices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Offers_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Offers_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventId",
                table: "Venues",
                column: "EventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues",
                column: "ManifestId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_EventId",
                table: "Offers",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_PaymentId",
                table: "Offers",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Offers_PriceId",
                table: "Offers",
                column: "PriceId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SeatId",
                table: "Offers",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_SectionId",
                table: "Offers",
                column: "SectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Seats_SeatId",
                table: "CartItems",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Manifests_ManifestId",
                table: "Sections",
                column: "ManifestId",
                principalTable: "Manifests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_Manifests_ManifestId",
                table: "Venues",
                column: "ManifestId",
                principalTable: "Manifests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Seats_SeatId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sections_Manifests_ManifestId",
                table: "Sections");

            migrationBuilder.DropForeignKey(
                name: "FK_Venues_Manifests_ManifestId",
                table: "Venues");

            migrationBuilder.DropTable(
                name: "Manifests");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropIndex(
                name: "IX_Venues_EventId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ManifestId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Rows");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Prices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "ManifestId",
                table: "Sections",
                newName: "VenueId");

            migrationBuilder.RenameIndex(
                name: "IX_Sections_ManifestId",
                table: "Sections",
                newName: "IX_Sections_VenueId");

            migrationBuilder.RenameColumn(
                name: "SeatId",
                table: "CartItems",
                newName: "SeatPriceId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_SeatId",
                table: "CartItems",
                newName: "IX_CartItems_SeatPriceId");

            migrationBuilder.AddColumn<Guid>(
                name: "CartId",
                table: "Payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SeatPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    PriceType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeatPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeatPrices_Prices_PriceId",
                        column: x => x.PriceId,
                        principalTable: "Prices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeatPrices_Seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "Seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venues_EventId",
                table: "Venues",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CartId",
                table: "Payments",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatPrices_PriceId",
                table: "SeatPrices",
                column: "PriceId");

            migrationBuilder.CreateIndex(
                name: "IX_SeatPrices_SeatId",
                table: "SeatPrices",
                column: "SeatId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_SeatPrices_SeatPriceId",
                table: "CartItems",
                column: "SeatPriceId",
                principalTable: "SeatPrices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Carts_CartId",
                table: "Payments",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Sections_Venues_VenueId",
                table: "Sections",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
