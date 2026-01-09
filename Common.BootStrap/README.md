# Common.Bootstrap

Modulares Dependency-Injection-Framework für .NET 8 mit automatischer Service-Registrierung und EqualityComparer-Management.

## 📑 Inhaltsverzeichnis

- [Überblick](#überblick)
- [Installation](#installation)
- [Schnellstart](#schnellstart)
- [Hauptfeatures](#hauptfeatures)
- [Bootstrap-Wrapper Pattern](#bootstrap-wrapper-pattern)
- [EqualityComparer-Management](#equalitycomparer-management)
- [Best Practices](#best-practices)
- [Weiterführende Dokumentation](#weiterführende-dokumentation)

## Überblick

**Common.Bootstrap** bietet eine strukturierte Lösung für die Dependency-Injection-Konfiguration in .NET-Anwendungen durch:

- 📦 **Modulares Design**: Organisieren Sie DI-Registrierungen in wiederverwendbaren `IServiceModule`-Implementierungen
- 🔄 **Bootstrap-Wrapper**: Erweiterbare Registrierungs-Pipeline mit Decorator-Pattern
- 🔍 **Assembly-Scanning**: Automatische Erkennung und Registrierung von Services
- ⚖️ **EqualityComparer-Management**: Vereinfachte Registrierung von `IEqualityComparer<T>`-Implementierungen
- ✅ **Idempotenz**: Sichere Mehrfach-Registrierungen ohne Konflikte

### Wann Common.Bootstrap verwenden?

✅ **Ideal für:**
- Projekte mit mehreren Bibliotheken, die jeweils ihre eigenen Services registrieren müssen
- Anwendungen, die eine klare Trennung der DI-Konfiguration benötigen
- Teams, die wiederverwendbare Service-Module über Projekte hinweg nutzen möchten
- Konsumierende Libraries, die den Bootstrap-Prozess erweitern möchten (Decorator-Pattern)

❌ **Nicht geeignet für:**
- Sehr kleine Projekte mit wenigen Services (Overhead nicht gerechtfertigt)
- Szenarien, wo direkte `IServiceCollection`-Registrierung ausreicht

## Installation

### NuGet Package
```bash
dotnet add package Common.Bootstrap
```

### Lokale Entwicklung
```bash
git clone https://github.com/ReneRose1971/Common.Bootstrap.git
cd Common.Bootstrap
dotnet build
```

## Schnellstart

### 1. Erstellen Sie ein Service-Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
        services.AddScoped<IMyRepository, MyRepository>();
    }
}
```

### 2. Bootstrap mit DefaultBootstrapWrapper

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

// Bootstrap-Wrapper registriert automatisch:
// - IServiceModule-Implementierungen
// - IEqualityComparer<T>-Implementierungen
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### 3. Nutzen Sie Ihre Services

```csharp
public class MyApplication
{
    private readonly IMyService _service;

    public MyApplication(IMyService service)
    {
        _service = service;  // Automatisch injiziert
    }

    public void Run()
    {
        _service.DoSomething();
    }
}
```

## Hauptfeatures

### Modulare Service-Registrierung

Organisieren Sie komplexe DI-Konfigurationen in unabhängige, testbare Module:

```csharp
public class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}

public class MessagingModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMessageBus, RabbitMQBus>();
        services.AddScoped<IEventPublisher, EventPublisher>();
    }
}
```

## Bootstrap-Wrapper Pattern

### DefaultBootstrapWrapper

Die Standard-Implementierung führt folgende Registrierungen durch:

1. Scannt nach `IServiceModule`-Implementierungen und ruft deren `Register` auf
2. Scannt nach `IEqualityComparer<T>`-Implementierungen und registriert sie als Singleton

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly
);
```

### Decorator-Pattern für eigene Extensions

Erweitern Sie den Bootstrap-Prozess für konsumierende Libraries:

```csharp
public class MyLibraryBootstrapWrapper : IBootstrapWrapper
{
    private readonly IBootstrapWrapper _innerWrapper;
    
    public MyLibraryBootstrapWrapper(IBootstrapWrapper innerWrapper)
    {
        _innerWrapper = innerWrapper;
    }
    
    public void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
    {
        // Basis-Registrierungen
        _innerWrapper.RegisterServices(services, assemblies);
        
        // Eigene erweiterte Scans
        services.AddMyValidatorsFromAssemblies(assemblies);
        services.AddMyMappersFromAssemblies(assemblies);
    }
}

// Verwendung:
var bootstrap = new MyLibraryBootstrapWrapper(new DefaultBootstrapWrapper());
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
```

## EqualityComparer-Management

### Automatisches Scanning

EqualityComparer werden automatisch vom `DefaultBootstrapWrapper` registriert:

```csharp
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y) => x?.Id == y?.Id;
    public int GetHashCode(Customer obj) => obj.Id.GetHashCode();
}

// Wird automatisch gefunden und registriert
```

### Verwendung mit automatischem Fallback

```csharp
using Common.Extensions;

public class MyRepository
{
    private readonly IEqualityComparer<Customer> _comparer;
    
    public MyRepository(IServiceProvider serviceProvider)
    {
        // Holt registrierten Comparer oder fällt auf EqualityComparer<T>.Default zurück
        _comparer = serviceProvider.GetEqualityComparer<Customer>();
    }
    
    public bool Contains(Customer customer)
    {
        return _collection.Any(c => _comparer.Equals(c, customer));
    }
}
```

## Best Practices

### ✅ Do's

- **Ein Modul pro Verantwortungsbereich**: Trennen Sie z.B. Datenbank, Messaging, Validation
- **Nutzen Sie DefaultBootstrapWrapper**: Konsistenter Bootstrap-Prozess
- **Decorator für Extensions**: Erweitern Sie den Bootstrap für eigene Scans
- **Idempotente Registrierungen**: Nutzen Sie `TryAdd*`-Methoden
- **Assembly-spezifisches Scanning**: Scannen Sie nur Ihre eigenen Assemblies

### ❌ Don'ts

- **Keine zirkulären Abhängigkeiten**: Module sollten unabhängig voneinander sein
- **Nicht alles in ein Modul**: Halten Sie Module fokussiert und klein
- **Kein Scanning von System-Assemblies**: Performance-Problem

## Weiterführende Dokumentation

- [📋 API-Referenz](Docs/API-Referenz.md) – Vollständige alphabetisch sortierte API-Dokumentation

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Common.Bootstrap](https://github.com/ReneRose1971/Common.Bootstrap)
- **Lizenz**: [MIT License](../LICENSE)

## Support & Beiträge

Bei Fragen oder Problemen erstellen Sie bitte ein [Issue](https://github.com/ReneRose1971/Common.Bootstrap/issues).

Beiträge sind willkommen! Lesen Sie unseren [Contributing Guide](../CONTRIBUTING.md) für weitere Informationen.

---

**Made with ❤️ using .NET 8**
