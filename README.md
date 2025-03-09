# Aurora Coinly API

Aurora Coinly is a personal finance management application designed to help users track incomes, expenses, and budgets efficiently.

## Table of Contents

- [Technologies](#technologies)
- [Installation](#installation)
- [Usage](#usage)
- [Roadmap](#roadmap)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)
- [Acknowledgements](#acknowledgements)

## Technologies
Aurora Coinly leverages the following technologies:

- **Clean Architecture**: Ensures separation of concerns, promoting maintainability and testability.
- **.NET 9**: The latest version of Microsoft's framework for building scalable and high-performance applications.
- **PostgreSQL**: A powerful, open-source relational database system.
- **MediatR**: Implements the mediator pattern, facilitating clean and decoupled code.
- **Entity Framework Core**: An object-relational mapper (ORM) that simplifies data access.
- **Swagger**: Provides interactive API documentation and testing capabilities.

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

## Roadmap

Planned features and improvements include:

- [x] Implement Wallet domain model
- [x] Add Budget entity with DateRange value object
- [x] Integrate BudgetTransaction entity
- [ ] Implement MonthlySummary updates based on paid transactions
- [ ] Add user authentication and authorization
- [ ] Develop front-end interface

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
