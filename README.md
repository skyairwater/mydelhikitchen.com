# EcommerceStore

A dynamic E-commerce application built with ASP.NET Core, featuring a robust admin dashboard, product management, and Google Maps integration for delivery addresses.

## Features

- **Storefront**: Browse products and categories.
- **Admin Dashboard**: Manage products, categories, and view orders.
- **Authentication**: Role-based access (via claims) using ASP.NET Core Identity.
- **Address Autocomplete**: Integrated Google Maps API for smooth delivery address entry.
- **PostgreSQL Integration**: High-performance database management.

## Tech Stack

- **Framework**: ASP.NET Core (net10.0)
- **Database**: PostgreSQL
- **ORM**: Entity Framework Core
- **Frontend**: Bootstrap 5, Razor Pages/MVC
- **Maps**: Google Maps JavaScript API

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL](https://www.postgresql.org/download/)

### Setup

1. **Clone the repository**:
   ```bash
   git clone <repository-url>
   cd EcommerceStore
   ```

2. **Configure Database**:
   Update the connection string in `appsettings.json` or use User Secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=EcommerceStoreDB;Username=postgres;Password=YOUR_PASSWORD"
   ```

3. **Configure Admin Credentials & API Keys**:
   To avoid hardcoding sensitive data, set these using User Secrets:
   ```bash
   dotnet user-secrets set "AdminUser:Email" "admin@example.com"
   dotnet user-secrets set "AdminUser:Password" "A_Very_Strong_Password_123!"
   dotnet user-secrets set "GoogleMaps:ApiKey" "YOUR_GOOGLE_MAPS_API_KEY"
   ```

4. **Apply Migrations**:
   ```bash
   dotnet ef database update
   ```

5. **Run the Application**:
   ```bash
   dotnet run
   ```

## Security Recommendation

> [!IMPORTANT]
> **Never commit your real secrets to GitHub.** Use `appsettings.json` only for placeholders. For production deployments, use **Environment Variables** (e.g., `AdminUser__Email`, `GoogleMaps__ApiKey`).

## Admin Access

The application automatically seeds a default admin user on the first run if the `AdminUser` configuration is provided. Once logged in as admin, you will see management links for Products and Categories in the top navigation bar.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
