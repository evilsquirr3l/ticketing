using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Migrations
{
    /// <inheritdoc />
    public partial class AddOffersCheckConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "offer_section_seat_check",
                table: "Offers",
                sql: "(\"SectionId\" IS NOT NULL AND \"SeatId\" IS NULL) OR (\"SectionId\" IS NULL AND \"SeatId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "offer_section_seat_check",
                table: "Offers");
        }
    }
}
