using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentMaster.Migrations
{
    /// <inheritdoc />
    public partial class Updateschema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "address_division",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<string>(type: "text", nullable: true),
                    IsDeprecated = table.Column<bool>(type: "boolean", nullable: false),
                    DeprecatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousUnitCodes = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_address_division", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "admin",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    Gmail = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "apartment_rooms",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    ApartmentUid = table.Column<Guid>(type: "uuid", nullable: false),
                    LandlordUid = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AreaLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AreaWidth = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MetaData = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Images = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apartment_rooms", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "consumer",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    Gmail = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consumer", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "landlord",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false),
                    Gmail = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    Avatar = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_landlord", x => x.Uid);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PartnerCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PayType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "apartments",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    LandlordUid = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Pid = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AddressDivisionUid = table.Column<Guid>(type: "uuid", nullable: true),
                    AreaLength = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    AreaWidth = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    ProvinceDivisionUid = table.Column<Guid>(type: "uuid", nullable: true),
                    WardDivisionUid = table.Column<Guid>(type: "uuid", nullable: true),
                    StreetUid = table.Column<Guid>(type: "uuid", nullable: true),
                    MetaData = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: true),
                    TotalFloors = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<string>(type: "varchar(50)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", nullable: false),
                    Images = table.Column<List<string>>(type: "text[]", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apartments", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_apartments_address_division_ProvinceDivisionUid",
                        column: x => x.ProvinceDivisionUid,
                        principalTable: "address_division",
                        principalColumn: "Uid");
                    table.ForeignKey(
                        name: "FK_apartments_address_division_StreetUid",
                        column: x => x.StreetUid,
                        principalTable: "address_division",
                        principalColumn: "Uid");
                    table.ForeignKey(
                        name: "FK_apartments_address_division_WardDivisionUid",
                        column: x => x.WardDivisionUid,
                        principalTable: "address_division",
                        principalColumn: "Uid");
                });

            migrationBuilder.CreateTable(
                name: "ConsumerContacts",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    Consumer_Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    Landlord_Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    Apartment_UID = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerContacts", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_ConsumerContacts_consumer_Consumer_Uid",
                        column: x => x.Consumer_Uid,
                        principalTable: "consumer",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumerContacts_landlord_Landlord_Uid",
                        column: x => x.Landlord_Uid,
                        principalTable: "landlord",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    Uid = table.Column<Guid>(type: "uuid", nullable: false),
                    UserUid = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerUid = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ApartmentUid = table.Column<Guid>(type: "uuid", nullable: true),
                    ApartmentRoomUid = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DepositAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MonthlyRent = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Note = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.Uid);
                    table.ForeignKey(
                        name: "FK_tenants_apartment_rooms_ApartmentRoomUid",
                        column: x => x.ApartmentRoomUid,
                        principalTable: "apartment_rooms",
                        principalColumn: "Uid");
                    table.ForeignKey(
                        name: "FK_tenants_apartments_ApartmentUid",
                        column: x => x.ApartmentUid,
                        principalTable: "apartments",
                        principalColumn: "Uid");
                    table.ForeignKey(
                        name: "FK_tenants_consumer_UserUid",
                        column: x => x.UserUid,
                        principalTable: "consumer",
                        principalColumn: "Uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_apartments_ProvinceDivisionUid",
                table: "apartments",
                column: "ProvinceDivisionUid");

            migrationBuilder.CreateIndex(
                name: "IX_apartments_StreetUid",
                table: "apartments",
                column: "StreetUid");

            migrationBuilder.CreateIndex(
                name: "IX_apartments_WardDivisionUid",
                table: "apartments",
                column: "WardDivisionUid");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerContacts_Consumer_Uid",
                table: "ConsumerContacts",
                column: "Consumer_Uid");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerContacts_Landlord_Uid",
                table: "ConsumerContacts",
                column: "Landlord_Uid");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_ApartmentRoomUid",
                table: "tenants",
                column: "ApartmentRoomUid");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_ApartmentUid",
                table: "tenants",
                column: "ApartmentUid");

            migrationBuilder.CreateIndex(
                name: "IX_tenants_UserUid",
                table: "tenants",
                column: "UserUid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin");

            migrationBuilder.DropTable(
                name: "ConsumerContacts");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DropTable(
                name: "landlord");

            migrationBuilder.DropTable(
                name: "apartment_rooms");

            migrationBuilder.DropTable(
                name: "apartments");

            migrationBuilder.DropTable(
                name: "consumer");

            migrationBuilder.DropTable(
                name: "address_division");
        }
    }
}
