# Common.Bootstrap – API-Referenz

Contract-Dokumentation für alle öffentlichen Schnittstellen und Klassen.

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

Standard-Implementierung des Bootstrap-Wrappers. Registriert `IServiceModule` und `IEqualityComparer<T>` aus Assemblies.

### Contract

```csharp
public class DefaultBootstrapWrapper : IBootstrapWrapper
{
    public virtual void RegisterServices(IServiceCollection services, params Assembly[] assemblies);
}
```

**Parameter:**
- `services`: Die Service-Collection
- `assemblies`: Zu scannende Assemblies

**Exceptions:**
- `ArgumentNullException`: Wenn `services` oder `assemblies` null

**Verhalten:**
1. Scannt nach `IServiceModule`-Implementierungen und ruft `Register` auf
2. Scannt nach `IEqualityComparer<T>`-Implementierungen und registriert als Singleton

### Verwendung

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
```

### Siehe auch

- [`IBootstrapWrapper`](#ibootstrapwrapper)
- [`IServiceModule`](#iservicemodule)

---

## IBootstrapWrapper

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

Schnittstelle für Bootstrap-Wrapper, die Assembly-basierte Registrierungen kapseln.

### Contract

```csharp
public interface IBootstrapWrapper
{
    void RegisterServices(IServiceCollection services, params Assembly[] assemblies);
}
```

**Parameter:**
- `services`: Die Service-Collection
- `assemblies`: Zu scannende Assemblies

**Anforderungen:**
Implementierungen müssen `IServiceModule`- und `IEqualityComparer<T>`-Implementierungen scannen und registrieren.

### Verwendung

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
```

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper)
- [`IServiceModule`](#iservicemodule)

---

## IServiceModule

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

Schnittstelle für modulare DI-Registrierungen.

### Contract

```csharp
public interface IServiceModule
{
    void Register(IServiceCollection services);
}
```

**Parameter:**
- `services`: Die zu erweiternde Service-Collection

**Anforderungen:**
- Öffentlicher parameterloser Konstruktor
- Konkrete Klasse (nicht `abstract`)
- Idempotente Registrierungen (TryAdd empfohlen)

### Verwendung

```csharp
public sealed class InfrastructureModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.TryAddSingleton<IMyService, MyService>();
    }
}
```

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper)
- [`ServiceCollectionModuleExtensions`](#servicecollectionmoduleextensions)

---

## ServiceCollectionEqualityComparerExtensions

**Namespace:** `Common.Extensions`  
**Assembly:** Common.Bootstrap.dll

Extensions zur automatischen Registrierung von `IEqualityComparer<T>`-Implementierungen.

### Contract

```csharp
public static IServiceCollection AddEqualityComparersFromAssembly<TMarker>(
    this IServiceCollection services)

public static IServiceCollection AddEqualityComparersFromAssembly(
    this IServiceCollection services, 
    Assembly assembly)
```

**Parameter:**
- `services`: Service-Collection
- `TMarker` / `assembly`: Assembly-Quelle

**Rückgabe:** Service-Collection (Fluent-API)

**Exceptions:**
- `ArgumentNullException`: Wenn Parameter null

**Filter:** Konkrete Klassen mit parameterlosem Konstruktor, die `IEqualityComparer<T>` implementieren.

**Registrierung:** Als Singleton via `TryAddSingleton` (idempotent).

### Verwendung

```csharp
builder.Services.AddEqualityComparersFromAssembly<Program>();
```

### Siehe auch

- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper)
- [`ServiceProviderEqualityComparerExtensions`](#serviceproviderequal­itycomparerextensions)

---

## ServiceCollectionModuleExtensions

**Namespace:** `Common.Bootstrap`  
**Assembly:** Common.Bootstrap.dll

Erweiterungen zur automatischen Erkennung und Ausführung aller `IServiceModule`-Implementierungen.

### Contract

```csharp
public static IServiceCollection AddModulesFromAssemblies(
    this IServiceCollection services,
    params Assembly[] assemblies)
```

**Parameter:**
- `services`: Service-Collection
- `assemblies`: Zu scannende Assemblies (leer = alle geladenen)

**Rückgabe:** Service-Collection (Fluent-API)

**Exceptions:**
- `MissingMethodException`: Kein parameterloser Konstruktor
- `InvalidOperationException`: Fehler während Registrierung

**Filter:** Konkrete Klassen mit parameterlosem Konstruktor, die `IServiceModule` implementieren.

### Verwendung

```csharp
builder.Services.AddModulesFromAssemblies(
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly
);
```

### Siehe auch

- [`IServiceModule`](#iservicemodule)
- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper)

---

## ServiceProviderEqualityComparerExtensions

**Namespace:** `Common.Extensions`  
**Assembly:** Common.Bootstrap.dll

Erweiterungen für `IServiceProvider` zur Auflösung von EqualityComparern mit Fallback.

### Contract

```csharp
public static IEqualityComparer<T> GetEqualityComparer<T>(
    this IServiceProvider serviceProvider)
```

**Parameter:**
- `serviceProvider`: DI-Container

**Rückgabe:** Registrierter Comparer oder `EqualityComparer<T>.Default`

**Exceptions:**
- `ArgumentNullException`: Wenn `serviceProvider` null

**Verhalten:** Versucht DI-Auflösung, fällt bei Nicht-Registrierung auf `EqualityComparer<T>.Default` zurück.

### Verwendung

```csharp
public class LiteDbRepository<T>
{
    private readonly IEqualityComparer<T> _comparer;
    
    public LiteDbRepository(IServiceProvider serviceProvider)
    {
        _comparer = serviceProvider.GetEqualityComparer<T>();
    }
}
```

### Siehe auch

- [`ServiceCollectionEqualityComparerExtensions`](#servicecollectionequalitycomparerextensions)
- [`DefaultBootstrapWrapper`](#defaultbootstrapwrapper)

---

## Lizenz

MIT – Siehe LICENSE-Datei im Repository ([GitHub](https://github.com/ReneRose1971/Common.Bootstrap))
