# Vehicle Rental Management System

A web-based vehicle rental management system developed using **C# and ASP.NET Core MVC (.NET 8)**. The application provides separate experiences for customers and administrators, supporting vehicle browsing, rental requests, customer management, and rental administration.

## Overview

The Vehicle Rental Management System aims to simplify the vehicle rental process by providing a centralised platform for customers to explore available vehicles and manage their rental activities, while administrators can manage vehicles, customers, and rental records.

The application follows the Model-View-Controller (MVC) architecture and uses Entity Framework Core for database operations.

## Technologies Used

| Category | Technologies |
|---|---|
| Language | C# |
| Framework | ASP.NET Core MVC (.NET 8) |
| ORM | Entity Framework Core |
| Database | SQL Server (LocalDB) |
| Authentication | ASP.NET Core Identity |
| Development Environment | Visual Studio |

## Key Features

### Customer Features
- Browse available vehicles.
- View detailed vehicle information.
- Submit vehicle rental requests.
- View personal rental records and rental history.
- Access and update customer profile information.
- View rental payment information.

### Administrator Features
- Access an administrative dashboard.
- Manage vehicle records.
- Manage customer information.
- Manage rental transactions and their statuses.
- Maintain rental and payment records.

## Technical Highlights

- **MVC Architecture:** Separates application logic, data handling, and user interface components.
- **Entity Framework Core:** Uses EF Core for database access and entity relationships.
- **Role-Based Access Control:** Implements ASP.NET Core Identity with separate Admin and Customer roles.
- **Vehicle Inheritance:** Uses a base `Vehicle` model with specialised `Car`, `Bike`, and `Truck` models.
- **Rental Management:** Maintains rental information, including start dates, end dates, total costs, rental statuses, and payment statuses.
- **Customer Portal:** Provides customer-oriented pages for browsing vehicles and managing rental activities.

## Project Structure

The project is organised into MVC components, with dedicated controllers and models for the main application features.

```text
VehicleRental/
├── Controllers/
│   ├── AccountController.cs
│   ├── CustomerController.cs
│   ├── CustomerPortalController.cs
│   ├── CustomerRentalController.cs
│   ├── RentalController.cs
│   ├── VehicleController.cs
│   └── AdminDashboardController.cs
│
├── Models/
│   ├── ApplicationUser.cs
│   ├── Customer.cs
│   ├── Vehicle.cs
│   ├── Car.cs
│   ├── Bike.cs
│   ├── Truck.cs
│   └── Rental.cs
│
├── Views/
├── Data/
├── wwwroot/
├── appsettings.json
└── Program.cs
```

*The structure above is illustrative. Adjust the folder and file names to match the actual repository.*

## Getting Started

### Prerequisites

- Visual Studio 2022 or a compatible IDE.
- .NET 8 SDK.
- SQL Server LocalDB.
- Entity Framework Core tools.

### Installation

1. Clone the repository:

   ```bash
   git clone <YOUR_REPOSITORY_URL>
   ```

2. Navigate to the project directory:

   ```bash
   cd <PROJECT_DIRECTORY>
   ```

3. Restore NuGet packages:

   ```bash
   dotnet restore
   ```

4. Configure the database connection string in `appsettings.json` to match your local SQL Server LocalDB configuration.

5. Apply Entity Framework Core migrations:

   ```bash
   dotnet ef database update
   ```

6. Run the application:

   ```bash
   dotnet run
   ```

7. Open the local URL displayed in the terminal or Visual Studio output.

## Database Design

The application uses a relational database with Entity Framework Core.

Core entities include:

- **ApplicationUser:** Stores application identity and user-related information.
- **Customer:** Represents customer records associated with application accounts.
- **Vehicle:** Base entity for different vehicle types.
- **Car, Bike, Truck:** Specialised vehicle entities inheriting from the base Vehicle model.
- **Rental:** Stores rental transactions, dates, costs, rental status, and payment status.

## Project Purpose

This project was developed to practise full-stack web application development with C#, ASP.NET Core MVC, database management, authentication, and object-oriented programming.

It demonstrates the application of MVC architecture, inheritance, relational data modelling, and role-based application design in a practical vehicle rental scenario.
