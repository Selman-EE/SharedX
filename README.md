cat > README.md << 'EOF'
# SharedX

[![NuGet](https://img.shields.io/nuget/v/SharedX.Core.svg)](https://www.nuget.org/packages/SharedX.Core/)
[![Build Status](https://github.com/Selman-EE/SharedX/workflows/CI/badge.svg)](https://github.com/Selman-EE/SharedX/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A comprehensive shared library for .NET microservices, providing common utilities, extensions, and abstractions.

## 📦 Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| **SharedX.Core** | Core utilities, models, and constants | [![NuGet](https://img.shields.io/nuget/v/SharedX.Core.svg)](https://www.nuget.org/packages/SharedX.Core/) |
| **SharedX.Extensions** | Extension methods for common types | [![NuGet](https://img.shields.io/nuget/v/SharedX.Extensions.svg)](https://www.nuget.org/packages/SharedX.Extensions/) |
| **SharedX.Abstractions** | Interfaces for caching, messaging, persistence | [![NuGet](https://img.shields.io/nuget/v/SharedX.Abstractions.svg)](https://www.nuget.org/packages/SharedX.Abstractions/) |

## 🚀 Features

- **Multi-targeting**: Supports .NET Standard 2.0, .NET 6.0, and .NET 8.0
- **Serilog Integration**: Structured logging with OpenTelemetry support
- **Production Ready**: Comprehensive testing and CI/CD pipeline
- **Well Organized**: Clear separation of concerns with logical namespaces

## 📖 Documentation

See [docs/architecture.md](docs/architecture.md) for detailed architecture information.

## 🛠️ Installation
```bash
dotnet add package SharedX.Core
dotnet add package SharedX.Extensions
dotnet add package SharedX.Abstractions
```

## 💡 Quick Start
```csharp
using SharedX.Core;
using SharedX.Extensions;

// Coming soon...
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
EOF