using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShowSphere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShowIdToBookingSeat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingSeats_SeatId",
                table: "BookingSeats");

            migrationBuilder.AddColumn<Guid>(
                name: "ShowId",
                table: "BookingSeats",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeat_Unique_Active_Seat_Per_Show",
                table: "BookingSeats",
                columns: new[] { "SeatId", "ShowId" },
                unique: true,
                filter: "\"Status\" IN (0, 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookingSeat_Unique_Active_Seat_Per_Show",
                table: "BookingSeats");

            migrationBuilder.DropColumn(
                name: "ShowId",
                table: "BookingSeats");

            migrationBuilder.CreateIndex(
                name: "IX_BookingSeats_SeatId",
                table: "BookingSeats",
                column: "SeatId");
        }
    }
}
