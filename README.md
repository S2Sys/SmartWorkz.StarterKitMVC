# SmartWorkz.StarterKitMVC

A boilerplate/starter kit for web applications using ASP.NET Core MVC (.NET 8.0 LTS).

## Features

- ASP.NET Core MVC 8.0 (LTS)
- MVC Architecture (Model-View-Controller)
- Bootstrap 5 UI Framework
- Structured logging configuration
- Development/Production environment separation
- HTTPS redirection and HSTS enabled
- Static file serving
- Error handling with custom error pages

## Project Structure

```
SmartWorkz.StarterKitMVC/
├── src/
│   └── SmartWorkz.StarterKitMVC/          # Main web application
│       ├── Controllers/                    # MVC Controllers
│       ├── Models/                         # View Models and Data Models
│       ├── Views/                          # Razor Views
│       │   ├── Home/                       # Home controller views
│       │   └── Shared/                     # Shared layouts and partials
│       ├── wwwroot/                        # Static files (CSS, JS, images)
│       ├── Properties/                     # Launch settings
│       ├── appsettings.json               # Application configuration
│       ├── appsettings.Development.json   # Development configuration
│       └── Program.cs                      # Application entry point
├── SmartWorkz.StarterKitMVC.sln           # Solution file
└── README.md                               # This file
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/S2Sys/SmartWorkz.StarterKitMVC.git
cd SmartWorkz.StarterKitMVC
```

### Build the Solution

```bash
dotnet build
```

### Run the Application

```bash
cd src/SmartWorkz.StarterKitMVC
dotnet run
```

The application will start and be available at:
- HTTPS: https://localhost:5001
- HTTP: http://localhost:5000

### Run in Development Mode

```bash
dotnet run --environment Development
```

## Configuration

### Application Settings

Configuration is managed through `appsettings.json` files:

- `appsettings.json` - Base configuration for all environments
- `appsettings.Development.json` - Development-specific settings

### Environment Variables

The application supports standard ASP.NET Core environment variables:
- `ASPNETCORE_ENVIRONMENT` - Set to `Development`, `Staging`, or `Production`

## Development

### Adding a New Controller

1. Create a new controller in `Controllers/` folder
2. Inherit from `Controller` base class
3. Create corresponding views in `Views/{ControllerName}/` folder

### Adding Views

1. Create `.cshtml` files in the appropriate `Views/` subfolder
2. Use `_Layout.cshtml` for consistent page structure
3. Use `_ViewImports.cshtml` for shared using statements

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

This project is available under the MIT License.