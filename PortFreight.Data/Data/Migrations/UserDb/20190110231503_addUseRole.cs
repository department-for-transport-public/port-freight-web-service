using Microsoft.EntityFrameworkCore.Migrations;

namespace PortFreight.Data.Migrations
{
    public partial class addUseRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PortFreightUserId",
                table: "AspNetUserRoles",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_PortFreightUserId",
                table: "AspNetUserRoles",
                column: "PortFreightUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUserRoles_PortFreightUsers_PortFreightUserId",
                table: "AspNetUserRoles",
                column: "PortFreightUserId",
                principalTable: "PortFreightUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUserRoles_PortFreightUsers_PortFreightUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_PortFreightUserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropColumn(
                name: "PortFreightUserId",
                table: "AspNetUserRoles");
        }
    }
}
