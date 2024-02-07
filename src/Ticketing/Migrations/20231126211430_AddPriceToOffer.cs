using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceToOffer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Prices_PriceId",
                table: "Offers");

            migrationBuilder.AlterColumn<Guid>(
                name: "PriceId",
                table: "Offers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Prices_PriceId",
                table: "Offers",
                column: "PriceId",
                principalTable: "Prices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Offers_Prices_PriceId",
                table: "Offers");

            migrationBuilder.AlterColumn<Guid>(
                name: "PriceId",
                table: "Offers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Offers_Prices_PriceId",
                table: "Offers",
                column: "PriceId",
                principalTable: "Prices",
                principalColumn: "Id");
        }
    }
}
