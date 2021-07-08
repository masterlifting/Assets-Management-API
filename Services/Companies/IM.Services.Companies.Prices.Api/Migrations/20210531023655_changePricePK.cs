using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IM.Services.Companies.Prices.Api.Migrations
{
    public partial class changePricePK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Tickers_TickerName",
                table: "Prices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prices",
                table: "Prices");

            migrationBuilder.DropIndex(
                name: "IX_Prices_Date",
                table: "Prices");

            migrationBuilder.DropIndex(
                name: "IX_Prices_TickerName",
                table: "Prices");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Prices");

            migrationBuilder.AlterColumn<string>(
                name: "TickerName",
                table: "Prices",
                type: "character varying(10)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prices",
                table: "Prices",
                columns: new[] { "TickerName", "Date" });

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Tickers_TickerName",
                table: "Prices",
                column: "TickerName",
                principalTable: "Tickers",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prices_Tickers_TickerName",
                table: "Prices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Prices",
                table: "Prices");

            migrationBuilder.AlterColumn<string>(
                name: "TickerName",
                table: "Prices",
                type: "character varying(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)");

            migrationBuilder.AddColumn<long>(
                name: "Id",
                table: "Prices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Prices",
                table: "Prices",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_Date",
                table: "Prices",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Prices_TickerName",
                table: "Prices",
                column: "TickerName");

            migrationBuilder.AddForeignKey(
                name: "FK_Prices_Tickers_TickerName",
                table: "Prices",
                column: "TickerName",
                principalTable: "Tickers",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
