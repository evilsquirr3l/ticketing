using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Migrations
{
    /// <inheritdoc />
    public partial class ChangeCartItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Seats_SeatId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues");

            migrationBuilder.RenameColumn(
                name: "SeatId",
                table: "CartItems",
                newName: "OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_SeatId",
                table: "CartItems",
                newName: "IX_CartItems_OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues",
                column: "ManifestId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Offers_OfferId",
                table: "CartItems",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Offers_OfferId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues");

            migrationBuilder.RenameColumn(
                name: "OfferId",
                table: "CartItems",
                newName: "SeatId");

            migrationBuilder.RenameIndex(
                name: "IX_CartItems_OfferId",
                table: "CartItems",
                newName: "IX_CartItems_SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ManifestId",
                table: "Venues",
                column: "ManifestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Seats_SeatId",
                table: "CartItems",
                column: "SeatId",
                principalTable: "Seats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
