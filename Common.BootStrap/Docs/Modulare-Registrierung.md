# Modulare Registrierung

Vollständiger Leitfaden zum Assembly-Scanning und zur automatischen Service-Registrierung in Common.Bootstrap.

## 📑 Inhaltsverzeichnis

- [Überblick](#überblick)
- [Der Registrierungsprozess](#der-registrierungsprozess)
- [DefaultBootstrapWrapper im Detail](#defaultbootstrapwrapper-im-detail)
- [Assembly-Scanning](#assembly-scanning)
- [Decorator-Pattern](#decorator-pattern)
- [Performance-Überlegungen](#performance-überlegungen)

---

## Überblick

Modulare Registrierung in Common.Bootstrap basiert auf drei Kernkomponenten:

1. **`IServiceModule`** – Kapselt DI-Registrierungen
2. **`IBootstrapWrapper`** – Orchestriert Assembly-Scanning
3. **`ServiceCollectionModuleExtensions`** – Führt Module-Scanning durch

### Grundprinzip

```csharp
// Module definieren
public sealed class MyModule : IServiceModule
{
    public void Register(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}

// Bootstrap-Wrapper nutzen
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);

// Automatisch: MyModule.Register wird aufgerufen
```

---

## Der Registrierungsprozess

### Phase-Übersicht

Der Registrierungsprozess läuft in drei Phasen ab:

```
Phase 1: Assembly-Scanning
    ↓
Phase 2: Modul-Instanziierung
    ↓
Phase 3: Service-Registrierung
```

### Phase 1: Assembly-Scanning

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly
);
```

**Ablauf:**
1. `DefaultBootstrapWrapper.RegisterServices` wird aufgerufen
2. Die Methode delegiert an `ServiceCollectionModuleExtensions.AddModulesFromAssemblies`
3. Jede angegebene Assembly wird nach `IServiceModule`-Implementierungen durchsucht

### Phase 2: Modul-Instanziierung

**Anforderungen an Module:**
- Konkrete Klasse (nicht `abstract`, nicht `interface`)
- Implementiert `IServiceModule`
- Hat einen öffentlichen parameterlosen Konstruktor
- Keine offenen generischen Typparameter

**Modul-Instanziierung:**
```csharp
var modules = assemblies
    .SelectMany(a => a.GetTypes())
    .Where(t => typeof(IServiceModule).IsAssignableFrom(t) 
                && !t.IsAbstract 
                && !t.IsInterface)
    .Select(t => (IServiceModule)Activator.CreateInstance(t)!)
    .ToList();
```

### Phase 3: Service-Registrierung

Für jedes gefundene Modul wird `Register` aufgerufen:

```csharp
foreach (var module in modules)
{
    module.Register(services);
}
```
**Hinweis:** Die Reihenfolge ist nicht garantiert (Reflection-basiert). Module sollten unabhängig voneinander sein.

---

## DefaultBootstrapWrapper im Detail

### Implementierung

```csharp
public class DefaultBootstrapWrapper : IBootstrapWrapper
{
    public virtual void RegisterServices(
        IServiceCollection services, 
        params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        // Phase 1: IServiceModule-Implementierungen scannen
        services.AddModulesFromAssemblies(assemblies);

        // Phase 2: IEqualityComparer-Implementierungen scannen
        foreach (var assembly in assemblies)
        {
            services.AddEqualityComparersFromAssembly(assembly);
        }
    }
}
```

### Zwei-Phasen-Registrierung

Der `DefaultBootstrapWrapper` führt zwei separate Scans durch:

**1. Modul-Scan:**
```csharp
services.AddModulesFromAssemblies(assemblies);
```
Findet alle `IServiceModule`-Implementierungen und ruft `Register` auf.

**2. EqualityComparer-Scan:**
```csharp
foreach (var assembly in assemblies)
{
    services.AddEqualityComparersFromAssembly(assembly);
}
```
Findet alle `IEqualityComparer<T>`-Implementierungen und registriert sie als Singleton.

### Erweiterbarkeit

Die Methode ist `virtual` und kann überschrieben werden:

```csharp
public class CustomBootstrapWrapper : DefaultBootstrapWrapper
{
    public override void RegisterServices(
        IServiceCollection services, 
        params Assembly[] assemblies)
    {
        // Basis-Registrierungen
        base.RegisterServices(services, assemblies);
        
        // Zusätzliche Scans
        services.AddValidatorsFromAssemblies(assemblies);
        services.AddMappersFromAssemblies(assemblies);
    }
}
```

---

## Assembly-Scanning

### AddModulesFromAssemblies

Die Kern-Extension für Modul-Scanning:

```csharp
public static IServiceCollection AddModulesFromAssemblies(
    this IServiceCollection services,
    params Assembly[] assemblies)
{
    if (assemblies == null || assemblies.Length == 0)
        assemblies = AppDomain.CurrentDomain.GetAssemblies();

    var moduleType = typeof(IServiceModule);

    var modules = assemblies
        .SelectMany(a =>
        {
            try { return a.GetTypes(); }
            catch (ReflectionTypeLoadException ex) 
            { 
                return ex.Types.Where(t => t != null)!; 
            }
        })
        .Where(t => moduleType.IsAssignableFrom(t) 
                    && !t.IsAbstract 
                    && !t.IsInterface)
        .Select(t => (IServiceModule)Activator.CreateInstance(t)!)        
        .ToList();

    foreach (var module in modules)
        module.Register(services);

    return services;
}
```

### Filter-Kriterien

**Gefunden werden:**
```csharp
// Öffentliche Klasse
public sealed class MyModule : IServiceModule { }

// Interne Klasse
internal sealed class InternalModule : IServiceModule { }

// Nested Public Class
public class OuterClass
{
    public sealed class NestedModule : IServiceModule { }
}
```

**NICHT gefunden werden:**
- Abstrakte Klassen und Interfaces
- Offene generische Typen
- Klassen ohne öffentlichen parameterlosen Konstruktor

### Fehlerbehandlung: ReflectionTypeLoadException

```csharp
try 
{ 
    return a.GetTypes(); 
}
catch (ReflectionTypeLoadException ex) 
{ 
    return ex.Types.Where(t => t != null)!; 
}
```

**Begründung:** Assemblies können Typen enthalten, die nicht geladen werden können (fehlende Dependencies). Diese Fehlerbehandlung stellt sicher, dass nur die ladbaren Typen verarbeitet werden.

### Assembly-Auswahl-Strategien

#### Explizite Assembly-Liste (empfohlen)

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(InfrastructureModule).Assembly,
    typeof(DomainModule).Assembly
);
```

**Vorteile:** Volle Kontrolle, keine unerwarteten Module, bessere Performance.

#### Convention-basiert (gefiltert)

```csharp
var assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => a.FullName?.StartsWith("MyCompany") == true)
    .ToArray();

bootstrap.RegisterServices(builder.Services, assemblies);
```

**Vorteile:** Automatisch neue Assemblies, keine manuelle Pflege.

---

## Decorator-Pattern

Das Bootstrap-Wrapper-Pattern ermöglicht Decorator-basierte Erweiterungen.

### Grundkonzept

```csharp
public interface IBootstrapWrapper
{
    void RegisterServices(IServiceCollection services, params Assembly[] assemblies);
}
```

Ein Decorator kann einen anderen Wrapper einwickeln:

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
        // Basis-Registrierungen durchführen
        _innerWrapper.RegisterServices(services, assemblies);
        
        // Eigene Registrierungen hinzufügen
        services.AddMyValidatorsFromAssemblies(assemblies);
        services.AddMyMappersFromAssemblies(assemblies);
    }
}
```

### Verwendung

```csharp
var bootstrap = new MyLibraryBootstrapWrapper(
    new DefaultBootstrapWrapper()
);

bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
```

### Mehrfach-Decorator

```csharp
var bootstrap = new LoggingBootstrapWrapper(
    new ValidationBootstrapWrapper(
        new MyLibraryBootstrapWrapper(
            new DefaultBootstrapWrapper()
        )
    )
);
```

**Ausführungsreihenfolge:**
1. `LoggingBootstrapWrapper` startet
2. Delegiert an `ValidationBootstrapWrapper`
3. Delegiert an `MyLibraryBootstrapWrapper`
4. Delegiert an `DefaultBootstrapWrapper`
5. `DefaultBootstrapWrapper` führt Basis-Scans durch
6. Zurück zu `MyLibraryBootstrapWrapper` → eigene Scans
7. Zurück zu `ValidationBootstrapWrapper` → eigene Scans
8. Zurück zu `LoggingBootstrapWrapper` → eigene Scans

### Beispiel: Logging-Decorator

```csharp
public class LoggingBootstrapWrapper : IBootstrapWrapper
{
    private readonly IBootstrapWrapper _innerWrapper;
    private readonly ILogger<LoggingBootstrapWrapper> _logger;
    
    public LoggingBootstrapWrapper(
        IBootstrapWrapper innerWrapper,
        ILogger<LoggingBootstrapWrapper> logger)
    {
        _innerWrapper = innerWrapper;
        _logger = logger;
    }
    
    public void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
    {
        _logger.LogInformation(
            "Bootstrap started for assemblies: {Assemblies}", 
            string.Join(", ", assemblies.Select(a => a.GetName().Name)));
        
        var stopwatch = Stopwatch.StartNew();
        
        _innerWrapper.RegisterServices(services, assemblies);
        
        stopwatch.Stop();
        
        _logger.LogInformation(
            "Bootstrap completed in {ElapsedMs}ms. Registered {ServiceCount} services.",
            stopwatch.ElapsedMilliseconds,
            services.Count);
    }
}
```

---

## Performance-Überlegungen

### Assembly-Scanning-Performance

**Faktoren:**
- Anzahl der Assemblies
- Anzahl der Typen pro Assembly
- Reflection-Overhead

**Benchmark (typische ASP.NET-App):**
```
1 Assembly (100 Typen):      ~5-10ms
5 Assemblies (500 Typen):    ~20-30ms
10 Assemblies (1000 Typen):  ~40-60ms
```

### Optimierungen

#### Explizite Assembly-Liste

```csharp
// Schnell: Nur relevante Assemblies
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(MyModule).Assembly
);
```

#### Vermeiden von System-Assemblies

```csharp
// Nur eigene Assemblies
var assemblies = AppDomain.CurrentDomain.GetAssemblies()
    .Where(a => !a.FullName.StartsWith("System.") 
                && !a.FullName.StartsWith("Microsoft."))
    .ToArray();
```

### Startup-Zeit-Analyse

```csharp
var stopwatch = Stopwatch.StartNew();

var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);

stopwatch.Stop();
Console.WriteLine($"Bootstrap took {stopwatch.ElapsedMilliseconds}ms");
```

---

## Zusammenfassung

### Kernkonzepte

1. **Assembly-Scanning** findet automatisch alle `IServiceModule`-Implementierungen
2. **DefaultBootstrapWrapper** orchestriert Modul- und EqualityComparer-Scans
3. **Decorator-Pattern** ermöglicht Erweiterungen für konsumierende Libraries
4. **Fehlertoleranz** durch ReflectionTypeLoadException-Handling

### Best Practices

- Explizite Assembly-Liste verwenden
- Module idempotent gestalten (TryAdd)
- Keine Reihenfolge-Abhängigkeiten zwischen Modulen
- System-Assemblies vom Scanning ausschließen

### Verwandte Dokumentation

- [IServiceModule verstehen](ServiceModules.md) – Modul-Konzept im Detail
- [EqualityComparer-Management](EqualityComparer.md) – Comparer-Scanning
- [API-Referenz](API-Referenz.md) – Vollständige API-Dokumentation

---

**[← Zurück zur Übersicht](../README.md)**
