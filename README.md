# Common.Bootstrap

Modulares Dependency-Injection-Framework für .NET 8 mit automatischer Service-Registrierung, Bootstrap-Wrapper-Pattern und EqualityComparer-Management.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🎯 Überblick

**Common.Bootstrap** strukturiert Ihre Dependency-Injection-Konfiguration durch modulare Service-Registrierung und automatisches Assembly-Scanning.

### Kernfeatures

- 📦 **IServiceModule** – Modulare DI-Registrierung
- 🔄 **IBootstrapWrapper** – Erweiterbare Bootstrap-Pipeline mit Decorator-Pattern
- ⚖️ **EqualityComparer-Management** – Automatisches Scanning mit Fallback
- ✅ **Idempotenz** – Sichere Mehrfach-Registrierungen

## 🚀 Schnellstart

```bash
dotnet add package Common.Bootstrap
```

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();
```

## 📚 Dokumentation

Die vollständige Dokumentation finden Sie hier:

- **[📖 Projekt-README](Common.BootStrap/README.md)** – Vollständige Anleitung und Beispiele
- **[📋 API-Referenz](Common.BootStrap/Docs/API-Referenz.md)** – Alphabetisch sortierte API-Dokumentation

## 📄 Lizenz

Dieses Projekt ist unter der MIT-Lizenz lizenziert – siehe [LICENSE](LICENSE).

## 💬 Support

Bei Fragen erstellen Sie bitte ein [Issue](https://github.com/ReneRose1971/Common.Bootstrap/issues).

---

**Happy Coding! 🚀**
