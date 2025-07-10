# Aurora Coinly API

Aurora Coinly is a personal finance management application designed to help users track incomes, expenses, budgets, and wallet operations efficiently and securely.

## Table of Contents

- [Technologies](#technologies)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Project Structure](#project-structure)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)
- [Acknowledgements](#acknowledgements)

## Technologies

Aurora Coinly leverages the following technologies and tools:

- **.NET 9**: Modern, high-performance backend framework.
- **C# 13**: Latest language features, including records and DateOnly.
- **Entity Framework Core**: ORM for data access and persistence.
- **PostgreSQL**: Robust, open-source relational database.
- **MediatR**: Implements the mediator pattern for decoupled business logic.
- **Swagger/OpenAPI**: Interactive API documentation and testing.
- **Clean Architecture**: Clear separation of concerns (Domain, Application, Infrastructure, API).
- **Dependency Injection**: Flexible and decoupled configuration.
- **Automated Migrations**: EF Core migrations to keep the database schema up to date.

## Features

- **Wallet management** with operation history.
- **Transaction tracking** (income, expenses, payments, reversals).
- **Budget management** and budget periods.
- **Payment methods** and custom categories.
- **Automatic monthly summaries** of financial activity.
- Robust business validations and consistent error handling.
- RESTful API, fully documented and easy to consume.
- Ready for authentication and authorization integration.
- Extensible structure for future integrations (frontend, users, etc).

## Installation

To set up the Aurora Coinly API locally, follow these steps:

1. **Clone the repository**:

   ```bash
   git clone https://github.com/gerardogarnica/aurora-coinly-api.git
   ```

2. **Navigate to the project directory**:

   ```bash
   cd aurora-coinly-api
   ```

3. **Restore dependencies**:

   ```bash
   dotnet restore
   ```

4. **Update the database**:

   ```bash
   dotnet ef database update
   ```

5. **Run the application**:

   ```bash
   dotnet run
   ```

## Usage

Once the application is running, you can explore the API endpoints using Swagger:

1. Open your web browser and navigate to `http://localhost:5000/swagger`.
2. Use the interactive interface to test various API endpoints.

## Project Structure

Aurora Coinly follows the Clean Architecture principles, with the following layers:

- **Domain**: Contains core entities, value objects, and business rules.
- **Application**: Includes use cases, DTOs, and application-specific logic.
- **Infrastructure**: Handles data access, external services, and configurations.
- **API**: Exposes the RESTful endpoints and integrates the application layer.

## Roadmap

Planned features and improvements include:

- [x] Implement Wallet domain model
- [x] Add Budget entity with DateRange value object
- [x] Integrate BudgetTransaction entity
- [x] Implement MonthlySummary updates based on paid transactions
- [ ] Add user authentication and authorization
- [ ] Develop dashboard API

## Contributing

Contributions are welcome! To contribute:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature/YourFeature`).
3. Commit your changes (`git commit -m 'Add some feature'`).
4. Push to the branch (`git push origin feature/YourFeature`).
5. Open a Pull Request.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.

## Contact

For questions or feedback:

- **Author**: Gerardo Garnica
- **Email**: [gerardo.garnica@gmail.com](mailto:gerardo.garnica@gmail.com)
- **GitHub**: [@gerardogarnica](https://github.com/gerardogarnica)

## Acknowledgements

Special thanks to:

- [othneildrew's Best-README-Template](https://github.com/othneildrew/Best-README-Template) for inspiration.
- The open-source community for continuous support and contributions.
