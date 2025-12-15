using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentMaster.Migrations
{
    /// <inheritdoc />
    public partial class UpdateParticipantUidsToList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantUidsJson",
                table: "rental_contracts");

            migrationBuilder.AddColumn<List<Guid>>(
                name: "ParticipantUids",
                table: "rental_contracts",
                type: "uuid[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParticipantUids",
                table: "rental_contracts");

            migrationBuilder.AddColumn<string>(
                name: "ParticipantUidsJson",
                table: "rental_contracts",
                type: "text",
                nullable: true);
        }
    }
}
