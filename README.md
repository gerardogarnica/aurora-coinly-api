# Aurora Coinly API

Aurora Coinly is a personal finance management API that helps users track incomes, expenses, budgets, wallets, and payment methods efficiently and securely.

## Table of Contents

- [Technologies](#technologies)
- [Features](#features)
  - [Authentication and Users](#authentication-and-users)
  - [Wallets](#wallets)
  - [Transactions](#transactions)
  - [Budgets](#budgets)
  - [Categories](#categories)
  - [Payment Methods](#payment-methods)
  - [Dashboard and Insights](#dashboard-and-insights)
  - [User Profile](#user-profile)
  - [Architecture and Quality](#architecture-and-quality)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Roadmap and Upcoming Releases](#roadmap-and-upcoming-releases)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)
- [Acknowledgements](#acknowledgements)

## Technologies

- **.NET 10**: Modern, high-performance backend framework.
- **C# 14**: Latest language features (records, `DateOnly`, etc.).
- **Entity Framework Core**: ORM for data access and persistence with PostgreSQL.
- **PostgreSQL**: Robust, open-source relational database.
- **FluentValidation**: Command validation and consistent error handling.
- **JWT**: Authentication with access and refresh tokens.
- **Swagger/OpenAPI**: Interactive API documentation.
- **Clean Architecture**: Clear separation of concerns (Domain, Application, Infrastructure, API).
- **Outbox pattern**: Domain messages for async processing and consistency.
- **OpenTelemetry**: Traces, metrics, and logs (OTLP in development, Azure Monitor in production).
- **Docker & Docker Compose**: API, PostgreSQL, and Seq for observability.
- **Automated migrations**: EF Core to keep the database schema up to date.

## Features

### Authentication and Users

- **Register** (`POST /aurora/coinly/auth/register`): Create new user accounts.
- **Login** (`POST /aurora/coinly/auth/login`): Sign in with JWT.
- **Refresh token** (`POST /aurora/coinly/auth/refresh`): Renew access tokens.
- **Change password** (`PUT /aurora/coinly/auth/change-password`): Securely update password.
- Ready for role-based authorization integration.

### Wallets

- **Wallet CRUD**: Create, list, get by ID, update, and delete (soft delete).
- **Wallet types**: Bank, Cash, EMoney.
- **Operation history**: Query history per wallet (`GET /wallets/{id}/history`).
- **Transfers between wallets**: Move funds between wallets for the same user.
- **Assign to available / savings**: Allocate balance to “available” or “savings” within a wallet.
- Separate balances: available vs savings; total calculated automatically.

### Transactions

- **Income** (`POST /aurora/coinly/transactions/income`): Record income with category, payment method, and dates.
- **Expense** (`POST /aurora/coinly/transactions/expense`): Record expenses with category, payment method, and max payment date.
- **Statuses**: Pending, Paid, Removed.
- **Pay transactions** (`PUT /aurora/coinly/transactions/pay`): Mark transactions as paid.
- **Undo payment** (`PUT /aurora/coinly/transactions/{id}/undopayment`): Revert a transaction payment.
- **Remove** (`DELETE /aurora/coinly/transactions/{id}`): Delete a transaction.
- **Filtered listing**: By date range, status, category, payment method, and date type (transaction or payment).
- **Automatic monthly summary**: Paid income and expenses automatically update monthly totals (MonthlySummary).

### Budgets

- **Budget CRUD**: Create, list by year, get by ID, and update.
- **Frequencies**: Weekly, Biweekly, Monthly, Quarterly, SemiAnnual, Annual.
- **Budget periods**: Period management and association with transactions.
- Query budgets by year: `GET /aurora/coinly/budgets/year/{year}`.

### Categories

- **Category CRUD**: Create, list, get by ID, update, and delete.
- **Predefined groups**: Clothing, Education, Entertainment, Finances, FoodAndDining, Groceries, Health, Housing, Income, Insurance, Miscellaneous, Other, PersonalCare, Pets, Savings, Subscriptions, Transportation, Travel, Utilities, Vehicle.
- **Category type**: Income or Expense.
- **Color**: Per-category customization for UI.

### Payment Methods

- **Payment method CRUD**: Create, list, get by ID, update, and delete.
- **Default method**: Endpoint to set a method as default (`PUT /aurora/coinly/methods/{id}/default`).
- Used in transactions to link expenses/income to a payment method.

### Dashboard and Insights

- **Unified dashboard** (`GET /aurora/coinly/dashboard`): Summary for the main UI.
  - Total balance.
  - Summary cards: current vs previous month income, expenses, and savings (with % change).
  - Monthly trends: last 12 months (income, expenses, savings).
  - Expenses by category (top categories for the month).
  - Expenses by category group (top groups for the month).
  - Recent transactions.
  - Upcoming pending payments.
  - List of active wallets with balances (available, savings, total).

### User Profile

- **Get profile** (`GET /aurora/coinly/me`): Authenticated user data.
- **Update profile** (`PUT /aurora/coinly/me`): Update name, email, etc.

### Architecture and Quality

- **Business validations**: Rules in domain and application layers.
- **Error handling**: `Result<T>`, problem details, and global exception handlers.
- **Domain events**: For side effects (e.g. summary updates when assigning to savings).
- **RESTful API** documented with Swagger and easy to consume.
- **Observability**: OpenTelemetry (traces, metrics, logs) with OTLP (Seq in Docker) or Azure Monitor.

## Installation

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL (local or container)
- Optional: Docker and Docker Compose to run the full stack

### Steps (local development)

1. **Clone the repository**

   ```bash
   git clone https://github.com/gerardogarnica/aurora-coinly-api.git
   cd aurora-coinly-api
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Configure database**

   Ensure PostgreSQL is running and the connection string is set in `appsettings.Development.json` (or environment variables). Then apply migrations:

   ```bash
   dotnet ef database update --project src/Aurora.Coinly.Infrastructure --startup-project src/Aurora.Coinly.Api
   ```

4. **Run the API**

   ```bash
   dotnet run --project src/Aurora.Coinly.Api
   ```

### With Docker Compose

```bash
docker compose up -d
```

The API will be available at `http://localhost:5000` (and optionally `https://localhost:5001` depending on configuration). PostgreSQL on port 5432 and Seq on 8080/5341 for observability.

## Usage

1. Open in your browser: **http://localhost:5000/swagger** (or the URL configured in your environment).
2. Use the Swagger documentation to explore and test endpoints under the `aurora/coinly/` prefix.
3. For protected endpoints, use `auth/login` or `auth/register` first, then include the token in `Authorization: Bearer <token>`.

## Project Structure

Aurora Coinly follows Clean Architecture:

| Layer | Contents |
|-------|----------|
| **Domain** | Entities, value objects, business rules, domain events, enums (TransactionType, TransactionStatus, BudgetFrequency, CategoryGroup, WalletType, etc.). |
| **Application** | Use cases, DTOs, FluentValidation validators, infrastructure interfaces. |
| **Infrastructure** | DbContext, EF configurations, migrations, JWT, password hashing, Outbox implementation, time and encryption services. |
| **API** | Minimal endpoints, middlewares, Swagger, observability, global exception handling. |

## Roadmap and Upcoming Releases

Planned features and improvements for future versions:

### Upcoming releases

- **Extended dashboard API**: More charts and metrics (period comparisons, projections).
- **AI integration**: Category suggestions, spending pattern detection, natural-language summaries.
- **Transaction templates**: Recurring templates for income and expenses (subscriptions, payroll, etc.).
- **Data export**: CSV/Excel export of transactions and summaries by date range.
- **Multi-currency**: Explicit support for multiple currencies and conversion in summaries.
- **Reminders and notifications**: API or integration for pending payment reminders and budget alerts.
- **Savings goals**: Goals per wallet or global with progress and target dates.
- **Bank reconciliation**: Import movements (CSV/OFX) and reconcile with transactions.
- **Versioned public API**: Explicit versioning (v1, v2) and breaking-change documentation.
- **E2E tests**: End-to-end test suite against the API for regression and contracts.

### Under consideration

- Support for multiple users per “household” or shared accounts.
- Bank or aggregator integrations (Open Banking) when the ecosystem allows.

## Contributing

Contributions are welcome:

1. Fork the repository.
2. Create a branch (`git checkout -b feature/your-feature-name`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/your-feature-name`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact

- **Author**: Gerardo Garnica  
- **Email**: [gerardo.garnica@gmail.com](mailto:gerardo.garnica@gmail.com)  
- **GitHub**: [@gerardogarnica](https://github.com/gerardogarnica)

## Acknowledgements

- [othneildrew's Best-README-Template](https://github.com/othneildrew/Best-README-Template) for inspiration.
- The open-source community for continuous support and contributions.
