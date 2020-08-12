using Microsoft.EntityFrameworkCore.Migrations;

namespace Restaurant_Website.Migrations.Order
{
    public partial class Order2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CartBuffer",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartBuffer",
                table: "Orders");
        }
    }
}
