using System;
using Microsoft.EntityFrameworkCore.Migrations;
using specmatic_uuid_api.Models.Entity;

#nullable disable

namespace specmatic_uuid_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:UuidType", "business,enterprise,premium,regular");

            migrationBuilder.CreateTable(
                name: "uuid",
                columns: table => new
                {
                    uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    uuid_type = table.Column<UuidType>(type: "\"UuidType\"", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uuid", x => x.uuid);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "uuid");
        }
    }
}
