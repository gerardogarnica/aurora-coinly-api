using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aurora.Coinly.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(
            name: "coinly");

        migrationBuilder.CreateTable(
            name: "categories",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                max_days_to_reverse = table.Column<int>(type: "integer", nullable: false),
                color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                icon = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_categories", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "monthly_summaries",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                year = table.Column<int>(type: "integer", nullable: false),
                month = table.Column<int>(type: "integer", nullable: false),
                currency_code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                total_income_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                total_income_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                total_expense_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                total_expense_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_monthly_summaries", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "outbox_messages",
            schema: "coinly",
            columns: table => new
            {
                outbox_id = table.Column<Guid>(type: "uuid", nullable: false),
                type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                is_processed = table.Column<bool>(type: "boolean", nullable: false),
                processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                error = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_outbox_messages", x => x.outbox_id);
            });

        migrationBuilder.CreateTable(
            name: "wallets",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                available_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                available_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                savings_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                savings_amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                allow_negative = table.Column<bool>(type: "boolean", nullable: false),
                color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_wallets", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "budgets",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                year = table.Column<int>(type: "integer", nullable: false),
                frequency = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_budgets", x => x.id);
                table.ForeignKey(
                    name: "fk_budgets_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "coinly",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "payment_methods",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                is_default = table.Column<bool>(type: "boolean", nullable: false),
                allow_recurring = table.Column<bool>(type: "boolean", nullable: false),
                auto_mark_as_paid = table.Column<bool>(type: "boolean", nullable: false),
                wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                max_days_to_reverse = table.Column<int>(type: "integer", nullable: false),
                suggested_payment_day = table.Column<int>(type: "integer", nullable: true),
                statement_cutoff_day = table.Column<int>(type: "integer", nullable: true),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                deleted_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_payment_methods", x => x.id);
                table.ForeignKey(
                    name: "fk_payment_methods_wallets_wallet_id",
                    column: x => x.wallet_id,
                    principalSchema: "coinly",
                    principalTable: "wallets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "wallet_history",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                date = table.Column<DateOnly>(type: "date", nullable: false),
                amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                available_balance_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                available_balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                savings_balance_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                savings_balance_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_wallet_history", x => x.id);
                table.ForeignKey(
                    name: "fk_wallet_history_wallets_wallet_id",
                    column: x => x.wallet_id,
                    principalSchema: "coinly",
                    principalTable: "wallets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "budget_periods",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                period_start = table.Column<DateOnly>(type: "date", nullable: false),
                period_end = table.Column<DateOnly>(type: "date", nullable: false),
                limit_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                limit_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_budget_periods", x => x.id);
                table.ForeignKey(
                    name: "fk_budget_periods_budgets_budget_id",
                    column: x => x.budget_id,
                    principalSchema: "coinly",
                    principalTable: "budgets",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "transactions",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                category_id = table.Column<Guid>(type: "uuid", nullable: false),
                transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                max_payment_date = table.Column<DateOnly>(type: "date", nullable: false),
                payment_date = table.Column<DateOnly>(type: "date", nullable: true),
                amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                payment_method_id = table.Column<Guid>(type: "uuid", nullable: true),
                wallet_id = table.Column<Guid>(type: "uuid", nullable: true),
                status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                installment_number = table.Column<int>(type: "integer", nullable: false),
                notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                paid_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                removed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_transactions", x => x.id);
                table.ForeignKey(
                    name: "fk_transactions_categories_category_id",
                    column: x => x.category_id,
                    principalSchema: "coinly",
                    principalTable: "categories",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "fk_transactions_payment_methods_payment_method_id",
                    column: x => x.payment_method_id,
                    principalSchema: "coinly",
                    principalTable: "payment_methods",
                    principalColumn: "id");
                table.ForeignKey(
                    name: "fk_transactions_wallets_wallet_id",
                    column: x => x.wallet_id,
                    principalSchema: "coinly",
                    principalTable: "wallets",
                    principalColumn: "id");
            });

        migrationBuilder.CreateTable(
            name: "budget_transactions",
            schema: "coinly",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                budget_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                transaction_date = table.Column<DateOnly>(type: "date", nullable: false),
                amount_amount = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                amount_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_budget_transactions", x => x.id);
                table.ForeignKey(
                    name: "fk_budget_transactions_budget_periods_budget_period_id",
                    column: x => x.budget_period_id,
                    principalSchema: "coinly",
                    principalTable: "budget_periods",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "ix_budget_periods_budget_id",
            schema: "coinly",
            table: "budget_periods",
            column: "budget_id");

        migrationBuilder.CreateIndex(
            name: "ix_budget_transactions_budget_period_id",
            schema: "coinly",
            table: "budget_transactions",
            column: "budget_period_id");

        migrationBuilder.CreateIndex(
            name: "ix_budgets_category_id",
            schema: "coinly",
            table: "budgets",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_payment_methods_wallet_id",
            schema: "coinly",
            table: "payment_methods",
            column: "wallet_id");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_category_id",
            schema: "coinly",
            table: "transactions",
            column: "category_id");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_payment_method_id",
            schema: "coinly",
            table: "transactions",
            column: "payment_method_id");

        migrationBuilder.CreateIndex(
            name: "ix_transactions_wallet_id",
            schema: "coinly",
            table: "transactions",
            column: "wallet_id");

        migrationBuilder.CreateIndex(
            name: "ix_wallet_history_wallet_id",
            schema: "coinly",
            table: "wallet_history",
            column: "wallet_id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "budget_transactions",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "monthly_summaries",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "outbox_messages",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "transactions",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "wallet_history",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "budget_periods",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "payment_methods",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "budgets",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "wallets",
            schema: "coinly");

        migrationBuilder.DropTable(
            name: "categories",
            schema: "coinly");
    }
}
