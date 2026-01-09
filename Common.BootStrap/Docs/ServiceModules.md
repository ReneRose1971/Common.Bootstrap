# IServiceModule verstehen

Vollständiger Leitfaden zum modularen DI-Registrierungskonzept in Common.Bootstrap.

## 📑 Inhaltsverzeichnis

- [Was ist ein IServiceModule?](#was-ist-ein-iservicemodule)
- [Modulare Service-Registrierung](#modulare-service-registrierung)
- [Anatomie eines Service-Moduls](#anatomie-eines-service-moduls)
- [Lebenszyklus](#lebenszyklus)
- [Best Practices](#best-practices)
- [Erweiterte Szenarien](#erweiterte-szenarien)

---

## Was ist ein IServiceModule?

`IServiceModule` ist eine Schnittstelle für modulare Dependency-Injection-Registrierungen:

```csharp
public interface IServiceModule
{
    void Register(IServiceCollection services);
}
```

### Grundprinzip

Jedes Modul kapselt seine eigenen Service-Registrierungen:

```csharp
public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
        services.AddScoped<IMyRepository, MyRepository>();
    }
}
```

### Automatische Erkennung

Module werden automatisch vom `DefaultBootstrapWrapper` erkannt und ausgeführt:

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
// InfrastructureModule.Register wird automatisch aufgerufen
```

---

## Modulare Service-Registrierung

Module ermöglichen eine strukturierte Organisation von DI-Registrierungen nach Verantwortungsbereichen.

### Bootstrap in Program.cs

```csharp
var builder = Host.CreateApplicationBuilder(args);

var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();
```

### Modul-Beispiele

```csharp
// DatabaseModule.cs
public sealed class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
    }
}

// MessagingModule.cs
public sealed class MessagingModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMessageBus, RabbitMQBus>();
        services.AddScoped<IEventPublisher, EventPublisher>();
        services.AddScoped<ICommandHandler, CommandHandler>();
    }
}

// ValidationModule.cs
public sealed class ValidationModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<IValidator<Customer>, CustomerValidator>();
        services.AddScoped<IValidator<Order>, OrderValidator>();
    }
}
```

### Vorteile

- Klare Trennung nach Verantwortung
- Wiederverwendbar in anderen Projekten
- Isoliert testbar
- Vermeidung von Merge-Konflikten im Team
- Übersichtliche Code-Organisation

---

## Anatomie eines Service-Moduls

### Minimales Modul

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Infrastructure;

public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}
```

### Konventionen

#### Sealed Class
```csharp
public sealed class MyModule : IServiceModule
//     ^^^^^^
```
Module sind normalerweise `sealed`, da keine Vererbung notwendig ist.

#### Namespace-Organisation
```csharp
namespace MyApp.Infrastructure;  // Fachliche Schicht
namespace MyApp.Core;            // Core-Services
namespace MyApp.Persistence;     // Datenbank
```

#### Dateinamen-Konvention
```
DatabaseModule.cs
MessagingModule.cs
ValidationModule.cs
```

### Lebenszyklus-Auswahl

Module registrieren Services mit verschiedenen Lifecycles:

```csharp
public void Register(IServiceCollection services)
{
    // Singleton: Eine Instanz für gesamte Anwendung
    services.AddSingleton<IConfiguration, AppConfiguration>();
    
    // Scoped: Eine Instanz pro Request/Scope
    services.AddScoped<IUnitOfWork, UnitOfWork>();
    
    // Transient: Jedes Mal neue Instanz
    services.AddTransient<IValidator, CustomerValidator>();
}
```

---

## Lebenszyklus

### Phase 1: Assembly-Scanning

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
```

Der `DefaultBootstrapWrapper`:
1. Scannt die angegebenen Assemblies
2. Sucht nach Klassen, die `IServiceModule` implementieren
3. Erstellt Instanzen der gefundenen Module
4. Ruft `Register` auf jedem Modul auf

### Phase 2: Modul-Instanziierung

Jedes Modul muss einen **öffentlichen parameterlosen Konstruktor** haben:

```csharp
public sealed class MyModule : IServiceModule
{
    public MyModule() { }  // Explizit oder implizit
    
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}
```

**Begründung:** Module werden vor dem DI-Container instanziiert, daher kein Zugriff auf DI zur Modul-Erzeugung.

### Phase 3: Registrierung

Jedes Modul wird genau einmal pro Bootstrap-Aufruf ausgeführt:

```csharp
public void Register(IServiceCollection services)
{
    services.AddSingleton<IMyService, MyService>();
}
```

---

## Best Practices

### 1. Ein Modul pro Verantwortungsbereich

Module sollten fokussiert sein und jeweils einen klar abgegrenzten Verantwortungsbereich abdecken:

```csharp
public sealed class DatabaseModule : IServiceModule { }
public sealed class MessagingModule : IServiceModule { }
public sealed class ValidationModule : IServiceModule { }
```

### 2. Idempotente Registrierungen

Verwenden Sie `TryAdd*`-Methoden für idempotente Registrierungen:

```csharp
public void Register(IServiceCollection services)
{
    services.TryAddSingleton<IMyService, MyService>();
}
```

**Begründung:** Module können mehrfach aufgerufen werden (verschiedene Assemblies, Tests, konsumierende Projekte).

### 3. Keine Seiteneffekte

Die `Register`-Methode sollte ausschließlich Registrierungen durchführen:

```csharp
public void Register(IServiceCollection services)
{
    services.AddSingleton<IMyService, MyService>();
}
```

Keine I/O-Operationen oder globale Zustandsänderungen in `Register`.

### 4. Unabhängige Module

Module sollten unabhängig voneinander sein. Abhängigkeiten zwischen Services werden über DI zur Laufzeit aufgelöst.

### 5. Fokussierte Modulgrößen

**Faustregel:** Ein Modul sollte < 20 Registrierungen haben. Bei größeren Modulen Aufteilung in mehrere Module erwägen.

```csharp
public sealed class ValidationModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddScoped<IValidator<Customer>, CustomerValidator>();
        services.AddScoped<IValidator<Order>, OrderValidator>();
        services.AddScoped<IValidator<Product>, ProductValidator>();
        services.AddScoped<IValidationEngine, ValidationEngine>();
    }
}
```

### 6. Fehlerbehandlung bei optionalen Dependencies

```csharp
public void Register(IServiceCollection services)
{
    // Optionale Abhängigkeit: Nicht crashen, wenn nicht verfügbar
    try
    {
        services.AddSingleton<IOptionalService, OptionalService>();
    }
    catch (FileNotFoundException)
    {
        // Optional: Logging, aber nicht crashen
    }
    
    // Pflicht-Abhängigkeit: Fehler propagieren
    services.AddSingleton<IRequiredService, RequiredService>();
}
```

### 7. Konfiguration über IConfiguration

Konfigurationswerte sollten über `IConfiguration` bezogen werden:

```csharp
public sealed class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionString);
        });
    }
}
```

### 8. Stateless Module

Module sollten keinen Zustand halten. Sie sind Registrierungs-Logik, keine Services.

---

## Erweiterte Szenarien

### Konditionale Registrierungen

```csharp
public sealed class DatabaseModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Registriere nur, wenn noch nicht vorhanden
        if (!services.Any(sd => sd.ServiceType == typeof(IMyService)))
        {
            services.AddSingleton<IMyService, MyService>();
        }
        
        // Oder: TryAdd (empfohlen)
        services.TryAddSingleton<IMyService, MyService>();
    }
}
```

### Environment-spezifische Registrierungen

Factory-Pattern für environment-spezifische Implementierungen:

```csharp
public void Register(IServiceCollection services)
{
    services.AddSingleton<IEmailService>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var env = config["Environment"];
        
        return env == "Development"
            ? new FakeEmailService()
            : new SmtpEmailService(config);
    });
}
```

### Multi-Assembly-Module

Ein Modul kann Services aus mehreren Assemblies registrieren:

```csharp
public sealed class CoreModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        // Aus dieser Assembly
        services.AddScoped<IMyService, MyService>();
        
        // Aus referenzierter Assembly
        services.AddScoped<IExternalService, ExternalService>();
    }
}
```

### Decorator-Pattern für Module

Module können andere Module dekorieren:

```csharp
public sealed class ExtendedDatabaseModule : IServiceModule
{
    private readonly DatabaseModule _baseModule = new();
    
    public void Register(IServiceCollection services)
    {
        // Basis-Registrierungen
        _baseModule.Register(services);
        
        // Erweiterte Registrierungen
        services.AddScoped<ICachingLayer, RedisCachingLayer>();
    }
}
```

**Hinweis:** Dies ist selten notwendig. Normalerweise einfach ein neues Modul erstellen.

---

## Zusammenfassung

### Kernprinzipien

1. **Ein Modul = Ein Verantwortungsbereich**
2. **Idempotent**: TryAdd verwenden
3. **Keine Seiteneffekte**: Nur Registrierungen
4. **Kein Zustand**: Module sind stateless
5. **Parameterloser Konstruktor**: Module brauchen keine Dependencies

### Anwendungsbereiche

Module eignen sich für:
- Projekte mit mehreren Bibliotheken, die jeweils ihre eigenen Services registrieren
- Klare Trennung der DI-Konfiguration
- Wiederverwendbare Service-Konfigurationen über Projekte hinweg
- Team-Projekte zur Vermeidung von Merge-Konflikten

### Verwandte Dokumentation

- [Modulare Registrierung](Modulare-Registrierung.md) – Assembly-Scanning im Detail
- [API-Referenz](API-Referenz.md) – IServiceModule API-Dokumentation
- [Projekt-README](../README.md) – Schnellstart und Überblick

---

**[← Zurück zur Übersicht](../README.md)**
