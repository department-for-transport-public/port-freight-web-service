using Microsoft.EntityFrameworkCore.Migrations;

namespace PortFreight.Data.Migrations
{
    public partial class profilecomplete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ProfileComplete",
                table: "PortFreightUsers",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfileComplete",
                table: "PortFreightUsers");
        }
    }
}
