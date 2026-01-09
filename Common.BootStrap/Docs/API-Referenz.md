# Common.Bootstrap – API-Referenz

Vollständige alphabetisch sortierte API-Dokumentation für alle öffentlichen Schnittstellen und Klassen.

## 📑 Inhaltsverzeichnis

- [DefaultBootstrapWrapper](#defaultbootstrapwrapper)
- [IBootstrapWrapper](#ibootstrapwrapper)
- [IServiceModule](#iservicemodule)
- [ServiceCollectionEqualityComparerExtensions](#servicecollectionequalitycomparerextensions)
- [ServiceCollectionModuleExtensions](#servicecollectionmoduleextensions)
- [ServiceProviderEqualityComparerExtensions](#serviceproviderequal­itycomparerextensions)

---

## DefaultBootstrapWrapper

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll  
**Implementiert:** [`IBootstrapWrapper`](#ibootstrapwrapper)

### Beschreibung

Standard-Implementierung des Bootstrap-Wrappers für Common.Bootstrap. Registriert `IServiceModule` und `IEqualityComparer<T>` aus Assemblies.

### Verhalten

Diese Implementierung führt folgende Registrierungen durch:

1. Scannt nach `IServiceModule`-Implementierungen und ruft deren `Register` auf
2. Scannt nach `IEqualityComparer<T>`-Implementierungen und registriert sie als Singleton

### Methoden

#### `virtual void RegisterServices(IServiceCollection services, params Assembly[] assemblies)`

Registriert Services aus den angegebenen Assemblies. Führt Module- und EqualityComparer-Scanning durch.

**Parameter:**
- `services` (`IServiceCollection`): Die Service-Collection.
- `assemblies` (`params Assembly[]`): Die zu scannenden Assemblies.

**Exceptions:**
- `ArgumentNullException`: Wenn `services` oder `assemblies` null ist.

**Erweiterbarkeit:**

Diese Methode ist `virtual` und kann in abgeleiteten Klassen überschrieben werden, um zusätzliche Registrierungen durchzuführen.

### Verwendungsbeispiel

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly
);

var app = builder.Build();
await app.RunAsync();
```

### Erweiterung durch Ableitung

```csharp
public class CustomBootstrapWrapper : DefaultBootstrapWrapper
{
    public override void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
    {
        // Basis-Registrierungen
        base.RegisterServices(services, assemblies);
        
        // Zusätzliche Registrierungen
        services.AddSingleton<IMyCustomService, MyCustomService>();
    }
}
```

### Siehe auch

- [`IBootstrapWrapper`](#ibootstrapwrapper) – Basis-Interface
- [`IServiceModule`](#iservicemodule) – Modul-Interface
- [Modulare Registrierung](Modulare-Registrierung.md) – Vollständiger Leitfaden

---

## IBootstrapWrapper

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Schnittstelle für Bootstrap-Wrapper, die Assembly-basierte Registrierungen kapseln. Ermöglicht Decorator-Pattern für erweiterbare Registrierungs-Pipelines.

### Design-Philosophie

Das Bootstrap-Wrapper-Pattern ermöglicht:

- ✅ **Einheitlichkeit**: Konsistenter Mechanismus für Assembly-Scanning
- ✅ **Erweiterbarkeit**: Decorator-Pattern für zusätzliche Scans
- ✅ **Modularität**: Konsumierende Libraries können eigene Wrapper erstellen
- ✅ **Klarheit**: Explizite Registrierungs-Pipeline

### Methoden

#### `void RegisterServices(IServiceCollection services, params Assembly[] assemblies)`

Registriert Services aus den angegebenen Assemblies in die `IServiceCollection`.

**Parameter:**
- `services` (`IServiceCollection`): Die Service-Collection, in die registriert werden soll.
- `assemblies` (`params Assembly[]`): Die zu scannenden Assemblies.

**Implementierungsrichtlinien:**

Implementierungen sollten typischerweise:
1. `IServiceModule`-Implementierungen scannen und registrieren
2. `IEqualityComparer<T>`-Implementierungen scannen und registrieren
3. Weitere projektspezifische Scans durchführen

### Verwendungsbeispiel

```csharp
using Common.Bootstrap;

var builder = Host.CreateApplicationBuilder(args);

var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();
```

### Decorator-Pattern

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

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper) – Standard-Implementierung
- [`IServiceModule`](#iservicemodule) – Modul-Interface
- [Modulare Registrierung](Modulare-Registrierung.md) – Vollständiger Leitfaden

---

## IServiceModule

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Schnittstelle für modulare DI-Registrierungen. Jede Bibliothek implementiert ein Modul, um ihre eigenen Services anzumelden.

### Design-Philosophie

Das Modul-Pattern ermöglicht:

- ✅ **Wiederverwendbarkeit**: Module können in verschiedenen Projekten genutzt werden
- ✅ **Trennung**: Jede Bibliothek ist für ihre eigene DI-Konfiguration verantwortlich
- ✅ **Testbarkeit**: Module können isoliert getestet werden
- ✅ **Übersichtlichkeit**: Klare Struktur statt einer riesigen `Program.cs`

### Methoden

#### `void Register(IServiceCollection services)`

Führt alle DI-Registrierungen dieses Moduls aus.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.

**Implementierungsrichtlinien:**

1. **Idempotenz**: Nutzen Sie `TryAdd*`-Methoden für sichere Mehrfach-Aufrufe
2. **Keine Seiteneffekte**: Keine I/O, keine globalen Zustandsänderungen
3. **Fokussiert**: Ein Modul pro Verantwortungsbereich
4. **Fehlerbehandlung**: Keine Exceptions bei fehlenden optionalen Dependencies

### Verwendungsbeispiel

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
        services.AddTransient<IMyFactory, MyFactory>();
    }
}
```

### Best Practices

```csharp
// Ein Modul pro Verantwortungsbereich
public class DatabaseModule : IServiceModule { }
public class MessagingModule : IServiceModule { }
public class ValidationModule : IServiceModule { }

// Idempotente Registrierungen
public void Register(IServiceCollection services)
{
    services.TryAddSingleton<IMyService, MyService>();
}
```

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper) – Registriert Module automatisch
- [`ServiceCollectionModuleExtensions`](#servicecollectionmoduleextensions) – Automatisches Modul-Scanning
- [Service-Module Dokumentation](ServiceModules.md) – Vollständiger Leitfaden

---

## ServiceCollectionEqualityComparerExtensions

**Namespace:** `Common.Extensions`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Extensions zur automatischen Registrierung von `IEqualityComparer<T>`-Implementierungen aus Assemblies.

### Methoden

#### `IServiceCollection AddEqualityComparersFromAssembly<TMarker>(this IServiceCollection services)`

Scannt die Assembly des angegebenen Marker-Typs nach allen konkreten Implementierungen von `IEqualityComparer<T>` und registriert sie als Singleton.

**Typ-Parameter:**
- `TMarker`: Ein beliebiger Typ aus der zu scannenden Assembly.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.

**Rückgabewert:**
- Die erweiterte `IServiceCollection` für Fluent-API.

**Exceptions:**
- `ArgumentNullException`: Wenn `services` null ist.

#### `IServiceCollection AddEqualityComparersFromAssembly(this IServiceCollection services, Assembly assembly)`

Scannt die angegebene Assembly nach allen konkreten Implementierungen von `IEqualityComparer<T>` und registriert sie als Singleton.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.
- `assembly` (`Assembly`): Die zu scannende Assembly.

**Rückgabewert:**
- Die erweiterte `IServiceCollection` für Fluent-API.

**Exceptions:**
- `ArgumentNullException`: Wenn `services` oder `assembly` null ist.

### Filter-Kriterien

Diese Methode findet nur Typen, die **alle** folgenden Bedingungen erfüllen:

✅ **Gefunden werden:**
- Konkrete Klassen (nicht abstract, nicht interface)
- Öffentlich (`IsPublic`) oder nested public (`IsNestedPublic`)
- Haben einen öffentlichen parameterlosen Konstruktor
- Keine offenen generischen Typen
- Implementieren `IEqualityComparer<T>`

❌ **NICHT gefunden werden:**
- Abstrakte Klassen und Interfaces
- Generische Klassen mit ungebundenen Typparametern
- Klassen ohne öffentlichen parameterlosen Konstruktor
- Private oder internal Klassen

### Registrierungs-Verhalten

- **Idempotent**: Nutzt `TryAddSingleton` – bestehende Registrierungen werden nicht überschrieben
- **Singleton**: Alle gefundenen Comparer werden als Singleton registriert
- **Fehlertoleranz**: `ReflectionTypeLoadException` wird automatisch behandelt

### Verwendungsbeispiel

```csharp
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Diese Comparer werden automatisch gefunden:
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y) => x?.Id == y?.Id;
    public int GetHashCode(Customer obj) => obj.Id.GetHashCode();
}

// In Program.cs (via DefaultBootstrapWrapper):
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
// CustomerComparer ist jetzt automatisch registriert

// Oder manuell:
builder.Services.AddEqualityComparersFromAssembly<Program>();
```

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper) – Nutzt diese Extension automatisch
- [`ServiceProviderEqualityComparerExtensions`](#serviceproviderequal­itycomparerextensions) – Comparer auflösen
- [EqualityComparer-Management](EqualityComparer.md) – Vollständiger Leitfaden

---

## ServiceCollectionModuleExtensions

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Erweiterungen zur automatischen Erkennung und Ausführung aller `IServiceModule`-Implementierungen aus Assemblies.

### Methoden

#### `IServiceCollection AddModulesFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)`

Sucht in den angegebenen (oder allen geladenen) Assemblies nach Klassen, die `IServiceModule` implementieren, erzeugt Instanzen und ruft `Register` auf.

**Parameter:**
- `services` (`IServiceCollection`): Die zu erweiternde Service-Collection.
- `assemblies` (`params Assembly[]`): Liste der zu scannenden Assemblies. Wenn leer oder null, werden alle aktuell geladenen Assemblies gescannt.

**Rückgabewert:**
- Die gleiche `IServiceCollection` für Fluent-API-Verkettung.

**Exceptions:**
- `MissingMethodException`: Wenn ein gefundenes `IServiceModule` keinen öffentlichen parameterlosen Konstruktor hat.
- `InvalidOperationException`: Wenn während der Registrierung in einem Modul ein Fehler auftritt.

### Filter-Kriterien

✅ **Gefunden werden:**
- Konkrete, nicht-abstrakte Klassen, die `IServiceModule` implementieren
- Öffentliche und interne Klassen
- Nested Classes (auch private, wenn zugänglich)

❌ **NICHT gefunden werden:**
- Abstrakte Klassen und Interfaces
- Generische Klassen mit ungebundenen Typparametern
- Klassen ohne öffentlichen parameterlosen Konstruktor

### Fehlerbehandlung

`ReflectionTypeLoadException` wird automatisch behandelt – nur erfolgreich geladene Typen werden verarbeitet.

### Verwendungsbeispiele

#### Einzelne Assembly scannen

```csharp
using Common.Bootstrap;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);

var app = builder.Build();
await app.RunAsync();
```

#### Mehrere Assemblies scannen

```csharp
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly,
    typeof(DomainModule).Assembly
);

var app = builder.Build();
```

### Best Practices

```csharp
// Explizite Assembly-Liste (empfohlen)
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,
    typeof(MyModule).Assembly
);

// Nur eigene Assemblies scannen
builder.Services.AddModulesFromAssemblies(
    AppDomain.CurrentDomain.GetAssemblies()
        .Where(a => a.FullName?.StartsWith("MyCompany") == true)
        .ToArray()
);
```

### Siehe auch

- [`IServiceModule`](#iservicemodule) – Basis-Interface
- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper) – Nutzt diese Extension
- [Modulare Registrierung](Modulare-Registrierung.md) – Vollständiger Leitfaden

---

## ServiceProviderEqualityComparerExtensions

**Namespace:** `Common.Extensions`  
**Assembly:** Common.Bootstrap.dll

### Beschreibung

Erweiterungen für `IServiceProvider` zur Auflösung von EqualityComparern mit automatischem Fallback.

### Methoden

#### `IEqualityComparer<T> GetEqualityComparer<T>(this IServiceProvider serviceProvider)`

Löst einen EqualityComparer für den angegebenen Typ auf. Versucht zunächst die Auflösung über DI, dann Fallback auf `EqualityComparer<T>.Default`.

**Typ-Parameter:**
- `T`: Der Typ, für den ein Comparer benötigt wird.

**Parameter:**
- `serviceProvider` (`IServiceProvider`): Der DI-Container.

**Rückgabewert:**
- Ein registrierter `IEqualityComparer<T>` aus dem DI-Container, oder `EqualityComparer<T>.Default` als Fallback.

**Exceptions:**
- `ArgumentNullException`: Wenn `serviceProvider` null ist.

### Verhalten

Diese Methode ermöglicht es, EqualityComparer optional zu registrieren. Wenn kein expliziter Comparer für `T` registriert wurde, wird automatisch `EqualityComparer<T>.Default` verwendet.

**Phase-Zuordnung:**

Diese Methode ist für Phase 3 (Startup-Initialisierung) vorgesehen und wird typischerweise in Service-Konstruktoren verwendet.

### Verwendungsbeispiel

```csharp
using Common.Extensions;
using System.Collections.Generic;

public class LiteDbRepository<T> : IRepository<T>
{
    private readonly IEqualityComparer<T> _comparer;
    
    public LiteDbRepository(IServiceProvider serviceProvider)
    {
        // Versucht DI-Auflösung, Fallback auf Default
        _comparer = serviceProvider.GetEqualityComparer<T>();
    }
    
    public bool Contains(T entity)
    {
        return _collection.Any(e => _comparer.Equals(e, entity));
    }
}
```

### Fallback-Verhalten

```csharp
// Szenario 1: Comparer ist registriert
services.AddSingleton<IEqualityComparer<Customer>>(new CustomerComparer());
var comparer = serviceProvider.GetEqualityComparer<Customer>();
// → Gibt CustomerComparer zurück

// Szenario 2: Comparer ist NICHT registriert
var comparer = serviceProvider.GetEqualityComparer<Product>();
// → Gibt EqualityComparer<Product>.Default zurück
```

### Siehe auch

- [`ServiceCollectionEqualityComparerExtensions`](#servicecollectionequalitycomparerextensions) – Comparer registrieren
- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper) – Automatische Comparer-Registrierung
- [EqualityComparer-Management](EqualityComparer.md) – Vollständiger Leitfaden

---

## Verwandte Dokumentation

- 📖 [Service-Module verstehen](ServiceModules.md)
- 📦 [Modulare Registrierung](Modulare-Registrierung.md)
- ⚖️ [EqualityComparer-Management](EqualityComparer.md)
- 🏠 [Zurück zur Projekt-README](../README.md)

---

## Lizenz & Repository

- **Repository**: [https://github.com/ReneRose1971/Common.Bootstrap](https://github.com/ReneRose1971/Common.Bootstrap)
- **Lizenz**: MIT – Siehe LICENSE-Datei im Repository
