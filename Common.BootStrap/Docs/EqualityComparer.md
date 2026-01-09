# EqualityComparer-Management

Vollständiger Leitfaden zur automatischen Registrierung und Verwendung von `IEqualityComparer<T>` in Common.Bootstrap.

## 📑 Inhaltsverzeichnis

- [Automatische Registrierung](#automatische-registrierung)
- [Auflösung mit Fallback](#auflösung-mit-fallback)
- [Verwendung in Repositories](#verwendung-in-repositories)
- [Custom Comparer implementieren](#custom-comparer-implementieren)
- [Best Practices](#best-practices)

---

## Automatische Registrierung

### Durch DefaultBootstrapWrapper

Der `DefaultBootstrapWrapper` scannt automatisch nach EqualityComparern:

```csharp
var bootstrap = new DefaultBootstrapWrapper();
bootstrap.RegisterServices(
    builder.Services,
    typeof(Program).Assembly,
    typeof(MyModule).Assembly
);
```

**Ablauf:**

```csharp
public virtual void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
{
    // Phase 1: IServiceModule-Scan
    services.AddModulesFromAssemblies(assemblies);

    // Phase 2: IEqualityComparer-Scan (automatisch!)
    foreach (var assembly in assemblies)
    {
        services.AddEqualityComparersFromAssembly(assembly);
    }
}
```

### Manuelle Registrierung

Falls kein Bootstrap-Wrapper verwendet wird:

```csharp
// Via Marker-Typ
builder.Services.AddEqualityComparersFromAssembly<Program>();

// Via Assembly
builder.Services.AddEqualityComparersFromAssembly(typeof(Program).Assembly);
```

### Filter-Kriterien

**Gefunden werden nur Typen, die ALLE folgenden Bedingungen erfüllen:**

```csharp
// Konkrete Klasse
public class CustomerComparer : IEqualityComparer<Customer> { }

// Öffentliche Klasse
public class CustomerComparer : IEqualityComparer<Customer> { }

// Nested Public Class
public class Comparers
{
    public class CustomerComparer : IEqualityComparer<Customer> { }
}

// Mit parameterlosem Konstruktor
public class CustomerComparer : IEqualityComparer<Customer>
{
    public CustomerComparer() { }
}
```

**NICHT gefunden werden:**
- Abstrakte Klassen
- Interfaces
- Offene generische Typen
- Klassen ohne öffentlichen parameterlosen Konstruktor
- Private/Internal Klassen (je nach Konfiguration)

### Registrierungs-Details

**Lifecycle:** Alle EqualityComparer werden als **Singleton** registriert.

```csharp
services.TryAddSingleton<IEqualityComparer<Customer>, CustomerComparer>();
```

**Idempotenz:** Verwendet `TryAddSingleton` → bestehende Registrierungen werden nicht überschrieben.

### Implementierung

```csharp
public static IServiceCollection AddEqualityComparersFromAssembly(
    this IServiceCollection services,
    Assembly assembly)
{
    foreach (var type in SafeGetTypes(assembly))
    {
        // Nur konkrete Klassen
        if (type.IsAbstract || type.IsInterface)
            continue;

        // Nur öffentliche Typen
        if (!type.IsPublic && !type.IsNestedPublic)
            continue;

        // Offene generische Typen überspringen
        if (type.ContainsGenericParameters)
            continue;

        // Nur Typen mit parameterlosem Konstruktor
        if (!HasPublicParameterlessConstructor(type))
            continue;

        // Alle IEqualityComparer<T>-Interfaces finden
        var comparerInterfaces = type
            .GetInterfaces()
            .Where(i => i.IsGenericType 
                        && i.GetGenericTypeDefinition() == typeof(IEqualityComparer<>))
            .Distinct();

        foreach (var serviceType in comparerInterfaces)
        {
            // Idempotent registrieren
            services.TryAddSingleton(serviceType, type);
        }
    }

    return services;
}
```

---

## Auflösung mit Fallback

### GetEqualityComparer<T>

Die Extension `GetEqualityComparer<T>` löst Comparer mit automatischem Fallback auf:

```csharp
public static IEqualityComparer<T> GetEqualityComparer<T>(
    this IServiceProvider serviceProvider)
{
    if (serviceProvider == null)
        throw new ArgumentNullException(nameof(serviceProvider));

    var comparer = serviceProvider.GetService<IEqualityComparer<T>>();
    
    return comparer ?? EqualityComparer<T>.Default;
}
```

### Verhalten

**Custom Comparer registriert:**
```csharp
// Registrierung
services.AddSingleton<IEqualityComparer<Customer>>(new CustomerComparer());

// Auflösung
var comparer = serviceProvider.GetEqualityComparer<Customer>();
// → Gibt CustomerComparer zurück
```

**Kein Custom Comparer registriert:**
```csharp
// Keine explizite Registrierung

// Auflösung
var comparer = serviceProvider.GetEqualityComparer<Product>();
// → Gibt EqualityComparer<Product>.Default zurück
```

### EqualityComparer<T>.Default-Verhalten

**Für Value Types:**
```csharp
var comparer = EqualityComparer<int>.Default;
// Verwendet Equals/GetHashCode von int

comparer.Equals(42, 42);  // true
comparer.GetHashCode(42);  // hashCode von 42
```

**Für Reference Types:**
```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var comparer = EqualityComparer<Product>.Default;
// Verwendet Referenz-Gleichheit (ReferenceEquals)

var p1 = new Product { Id = 1, Name = "A" };
var p2 = new Product { Id = 1, Name = "A" };

comparer.Equals(p1, p2);  // false (verschiedene Referenzen!)
```

**Für Typen mit überschriebenem Equals:**
```csharp
public class Product : IEquatable<Product>
{
    public int Id { get; set; }
    
    public bool Equals(Product? other) => other?.Id == Id;
    public override bool Equals(object? obj) => Equals(obj as Product);
    public override int GetHashCode() => Id.GetHashCode();
}

var comparer = EqualityComparer<Product>.Default;
// Verwendet IEquatable<Product>.Equals

var p1 = new Product { Id = 1 };
var p2 = new Product { Id = 1 };

comparer.Equals(p1, p2);  // true (verwendet Equals-Override)
```

### Begründung für Fallback

Der Fallback ermöglicht optionale Comparer-Registrierung. Repositories und Services können `GetEqualityComparer<T>` verwenden, ohne dass für jeden Typ explizit ein Comparer registriert sein muss.

```csharp
public class LiteDbRepository<T> : IRepository<T>
{
    private readonly IEqualityComparer<T> _comparer;
    
    public LiteDbRepository(IServiceProvider serviceProvider)
    {
        // Funktioniert IMMER, auch ohne Custom Comparer
        _comparer = serviceProvider.GetEqualityComparer<T>();
    }
    
    public bool Contains(T entity)
    {
        return _collection.Any(e => _comparer.Equals(e, entity));
    }
}
```

---

## Verwendung in Repositories

### Generic Repository

```csharp
public class LiteDbRepository<T> : IRepository<T>
{
    private readonly ILiteCollection<T> _collection;
    private readonly IEqualityComparer<T> _comparer;
    
    public LiteDbRepository(
        ILiteDatabase database,
        IServiceProvider serviceProvider)
    {
        _collection = database.GetCollection<T>();
        _comparer = serviceProvider.GetEqualityComparer<T>();
    }
    
    public bool Contains(T entity)
    {
        return _collection.FindAll().Any(e => _comparer.Equals(e, entity));
    }
    
    public IEnumerable<T> GetDistinct()
    {
        return _collection.FindAll().Distinct(_comparer);
    }
    
    public void AddRange(IEnumerable<T> entities)
    {
        // Duplikate entfernen vor dem Einfügen
        var unique = entities.Distinct(_comparer);
        _collection.InsertBulk(unique);
    }
}
```

### Specific Repository

```csharp
public class CustomerRepository : ICustomerRepository
{
    private readonly ILiteCollection<Customer> _collection;
    private readonly IEqualityComparer<Customer> _comparer;
    
    public CustomerRepository(
        ILiteDatabase database,
        IServiceProvider serviceProvider)
    {
        _collection = database.GetCollection<Customer>();
        _comparer = serviceProvider.GetEqualityComparer<Customer>();
    }
    
    public Customer? FindByEmail(string email)
    {
        var customers = _collection.Find(c => c.Email == email);
        return customers.FirstOrDefault();
    }
    
    public bool HasDuplicate(Customer customer)
    {
        return _collection.FindAll()
            .Any(c => _comparer.Equals(c, customer));
    }
    
    public IEnumerable<Customer> MergeWithoutDuplicates(
        IEnumerable<Customer> existing,
        IEnumerable<Customer> newCustomers)
    {
        return existing.Union(newCustomers, _comparer);
    }
}
```

### Service mit mehreren Comparern

```csharp
public class OrderService
{
    private readonly IEqualityComparer<Order> _orderComparer;
    private readonly IEqualityComparer<OrderItem> _itemComparer;
    
    public OrderService(IServiceProvider serviceProvider)
    {
        _orderComparer = serviceProvider.GetEqualityComparer<Order>();
        _itemComparer = serviceProvider.GetEqualityComparer<OrderItem>();
    }
    
    public bool AreDuplicateOrders(Order o1, Order o2)
    {
        return _orderComparer.Equals(o1, o2);
    }
    
    public IEnumerable<OrderItem> GetUniqueItems(IEnumerable<OrderItem> items)
    {
        return items.Distinct(_itemComparer);
    }
}
```

---

## Custom Comparer implementieren

### Basis-Implementierung

```csharp
public class CustomerComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
    {
        // Null-Checks
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        // Geschäftslogik: Vergleiche nach ID
        return x.Id == y.Id;
    }
    
    public int GetHashCode(Customer obj)
    {
        if (obj is null) return 0;
        return obj.Id.GetHashCode();
    }
}
```

**Anforderungen:**
- Null-Checks implementieren
- `GetHashCode` konsistent zu `Equals`
- Reflexiv: `Equals(x, x)` ist immer `true`
- Symmetrisch: `Equals(x, y)` == `Equals(y, x)`
- Transitiv: Wenn `Equals(x, y)` und `Equals(y, z)`, dann `Equals(x, z)`

### Multi-Property-Vergleich

```csharp
public class ProductComparer : IEqualityComparer<Product>
{
    public bool Equals(Product? x, Product? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        // Vergleiche nach mehreren Properties
        return x.Id == y.Id
            && x.Name == y.Name
            && x.Category == y.Category;
    }
    
    public int GetHashCode(Product obj)
    {
        if (obj is null) return 0;
        
        // Kombiniere HashCodes
        return HashCode.Combine(obj.Id, obj.Name, obj.Category);
    }
}
```

### Case-Insensitive String-Vergleich

```csharp
public class CustomerEmailComparer : IEqualityComparer<Customer>
{
    public bool Equals(Customer? x, Customer? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        // Case-insensitive Email-Vergleich
        return string.Equals(x.Email, y.Email, StringComparison.OrdinalIgnoreCase);
    }
    
    public int GetHashCode(Customer obj)
    {
        if (obj is null) return 0;
        
        // HashCode muss case-insensitive sein
        return obj.Email?.ToUpperInvariant().GetHashCode() ?? 0;
    }
}
```

### Nested Object-Vergleich

Bei verschachtelten Objekten kann direkte Property-Vergleichung verwendet werden:

```csharp
public class OrderComparer : IEqualityComparer<Order>
{
    public bool Equals(Order? x, Order? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        
        return x.Id == y.Id
            && x.Customer?.Id == y.Customer?.Id;
    }
    
    public int GetHashCode(Order obj)
    {
        if (obj is null) return 0;
        
        return HashCode.Combine(obj.Id, obj.Customer?.Id);
    }
}
```

---

## Best Practices

### 1. Immer Null-Checks

```csharp
public bool Equals(Customer? x, Customer? y)
{
    // Referenz-Gleichheit prüfen
    if (ReferenceEquals(x, y)) return true;
    
    // Null-Checks
    if (x is null || y is null) return false;
    
    return x.Id == y.Id;
}
```

### 2. GetHashCode konsistent zu Equals

```csharp
// Korrekt: Gleiche Properties in Equals und GetHashCode
public bool Equals(Product? x, Product? y)
{
    return x?.Id == y?.Id && x?.Name == y?.Name;
}

public int GetHashCode(Product obj)
{
    return HashCode.Combine(obj.Id, obj.Name);  // Gleiche Properties
}
```

### 3. Parameterlose Konstruktoren

```csharp
// Automatisch gefunden
public class CustomerComparer : IEqualityComparer<Customer>
{
    public CustomerComparer() { }
}

// ❌ Wird NICHT automatisch gefunden
public class CustomerComparer : IEqualityComparer<Customer>
{
    public CustomerComparer(string config) { }
}
```

Falls Konstruktor-Parameter notwendig: Manuelle Registrierung erforderlich.

### 4. Singleton-Lifecycle

EqualityComparer sind zustandslos und werden immer als Singleton registriert:

```csharp
services.TryAddSingleton<IEqualityComparer<Customer>, CustomerComparer>();
```

### 5. TryAdd für Idempotenz

```csharp
// Automatisch verwendet
services.TryAddSingleton<IEqualityComparer<Customer>, CustomerComparer>();

// Zweiter Aufruf: Ignoriert (keine Überschreibung)
services.TryAddSingleton<IEqualityComparer<Customer>, OtherComparer>();
```

### 6. GetEqualityComparer in Service-Konstruktoren

```csharp
public class MyRepository
{
    private readonly IEqualityComparer<Customer> _comparer;
    
    // In Konstruktor auflösen
    public MyRepository(IServiceProvider serviceProvider)
    {
        _comparer = serviceProvider.GetEqualityComparer<Customer>();
    }
}
```

---

## Zusammenfassung

### Kernkonzepte

1. **Automatisches Scanning** findet alle `IEqualityComparer<T>`-Implementierungen
2. **Singleton-Registrierung** für alle gefundenen Comparer
3. **Fallback auf Default** wenn kein Custom Comparer registriert
4. **DI-basiert** → testbar und austauschbar

### Workflow

1. Custom Comparer implementieren (mit parameterlosem Konstruktor)
2. Assembly im Bootstrap-Wrapper scannen
3. In Services via `GetEqualityComparer<T>()` auflösen
4. Automatischer Fallback auf `EqualityComparer<T>.Default`

### Best Practices

- Null-Checks in Equals und GetHashCode
- GetHashCode konsistent zu Equals
- Parameterlose Konstruktoren
- GetEqualityComparer in Konstruktoren auflösen
- TryAdd für Idempotenz

### Verwandte Dokumentation

- [IServiceModule verstehen](ServiceModules.md) – Modul-Konzept
- [Modulare Registrierung](Modulare-Registrierung.md) – Assembly-Scanning
- [API-Referenz](API-Referenz.md) – Vollständige API-Dokumentation

---

**[← Zurück zur Übersicht](../README.md)**
