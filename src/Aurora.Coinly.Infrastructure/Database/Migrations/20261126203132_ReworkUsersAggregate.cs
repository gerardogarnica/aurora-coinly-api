using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Coinly.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class ReworkUsersAggregate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing tokens store the raw tokens being dropped and can't be rebuilt; their default
            // refresh_token_hash ('') would also break the unique index below.
            migrationBuilder.Sql("DELETE FROM coinly.user_tokens;");

            migrationBuilder.DropForeignKey(
                name: "fk_user_roles_roles_roles_name",
                schema: "coinly",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "coinly");

            migrationBuilder.DropIndex(
                name: "ix_users_identity_id",
                schema: "coinly",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_roles",
                schema: "coinly",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "ix_user_roles_user_id",
                schema: "coinly",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "identity_id",
                schema: "coinly",
                table: "users");

            migrationBuilder.DropColumn(
                name: "access_token",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "refresh_token",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.RenameColumn(
                name: "role_name",
                schema: "coinly",
                table: "user_roles",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "coinly",
                table: "user_tokens",
                newName: "user_token_id");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "coinly",
                table: "users",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                schema: "coinly",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "coinly",
                table: "users",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddColumn<string>(
                name: "access_token_id",
                schema: "coinly",
                table: "user_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                schema: "coinly",
                table: "user_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "refresh_token_hash",
                schema: "coinly",
                table: "user_tokens",
                type: "character(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                schema: "coinly",
                table: "user_tokens",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_roles",
                schema: "coinly",
                table: "user_roles",
                columns: new[] { "user_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_user_tokens_refresh_token_expires_on_utc",
                schema: "coinly",
                table: "user_tokens",
                column: "refresh_token_expires_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_user_tokens_refresh_token_hash",
                schema: "coinly",
                table: "user_tokens",
                column: "refresh_token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_tokens_refresh_token_expires_on_utc",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropIndex(
                name: "ix_user_tokens_refresh_token_hash",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_roles",
                schema: "coinly",
                table: "user_roles");

            migrationBuilder.DropColumn(
                name: "is_active",
                schema: "coinly",
                table: "users");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "coinly",
                table: "users");

            migrationBuilder.DropColumn(
                name: "access_token_id",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "refresh_token_hash",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.DropColumn(
                name: "xmin",
                schema: "coinly",
                table: "user_tokens");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "coinly",
                table: "user_roles",
                newName: "role_name");

            migrationBuilder.RenameColumn(
                name: "user_token_id",
                schema: "coinly",
                table: "user_tokens",
                newName: "id");

            migrationBuilder.AlterColumn<string>(
                name: "password_hash",
                schema: "coinly",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "identity_id",
                schema: "coinly",
                table: "users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "access_token",
                schema: "coinly",
                table: "user_tokens",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "refresh_token",
                schema: "coinly",
                table: "user_tokens",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_roles",
                schema: "coinly",
                table: "user_roles",
                columns: new[] { "role_name", "user_id" });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "coinly",
                columns: table => new
                {
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.name);
                });

            migrationBuilder.InsertData(
                schema: "coinly",
                table: "roles",
                column: "name",
                values: new object[]
                {
                    "Administrator",
                    "Member"
                });

            migrationBuilder.Sql("UPDATE coinly.users SET identity_id = id::text;");

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                schema: "coinly",
                table: "users",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_roles_user_id",
                schema: "coinly",
                table: "user_roles",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_roles_roles_roles_name",
                schema: "coinly",
                table: "user_roles",
                column: "role_name",
                principalSchema: "coinly",
                principalTable: "roles",
                principalColumn: "name",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
