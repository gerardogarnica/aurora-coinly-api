using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Coinly.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class IncreaseColorMaxLength : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "color",
            schema: "coinly",
            table: "wallets",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(7)",
            oldMaxLength: 7);

        migrationBuilder.AlterColumn<string>(
            name: "color",
            schema: "coinly",
            table: "categories",
            type: "character varying(20)",
            maxLength: 20,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(7)",
            oldMaxLength: 7);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "color",
            schema: "coinly",
            table: "wallets",
            type: "character varying(7)",
            maxLength: 7,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(20)",
            oldMaxLength: 20);

        migrationBuilder.AlterColumn<string>(
            name: "color",
            schema: "coinly",
            table: "categories",
            type: "character varying(7)",
            maxLength: 7,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(20)",
            oldMaxLength: 20);
    }
}
