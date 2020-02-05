using Microsoft.EntityFrameworkCore.Migrations;

namespace PortFreight.Data.Migrations
{
    public partial class removeProfileComplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileComplete",
                table: "PortFreightUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProfileComplete",
                table: "PortFreightUsers",
                nullable: false,
                defaultValue: false);
        }
    }
}
