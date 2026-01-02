using Common.Bootstrap;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.BootStrap.Tests.Tests;

/// <summary>
/// Integrationstests für den vollständigen Bootstrap-Prozess mit IBootstrapWrapper.
/// Testet die Kette: IBootstrapWrapper → RegisterServices → AddModulesFromAssemblies + AddEqualityComparersFromAssembly → 
/// BuildServiceProvider → GetEqualityComparer.
/// </summary>
public class BootstrapIntegrationTests
{
    #region Test Data Classes

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public override bool Equals(object? obj)
            => obj is Customer other && Id == other.Id;

        public override int GetHashCode() => Id.GetHashCode();
    }

    public class Order
    {
        public int OrderNumber { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
    }

    #endregion

    #region Custom Comparers

    public class CustomerComparer : IEqualityComparer<Customer>
    {
        public bool Equals(Customer? x, Customer? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(Customer obj)
        {
            if (obj is null) throw new System.ArgumentNullException(nameof(obj));
            return obj.Id.GetHashCode();
        }
    }

    public class OrderComparer : IEqualityComparer<Order>
    {
        public bool Equals(Order? x, Order? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.OrderNumber == y.OrderNumber;
        }

        public int GetHashCode(Order obj)
        {
            if (obj is null) throw new System.ArgumentNullException(nameof(obj));
            return obj.OrderNumber.GetHashCode();
        }
    }

    #endregion

    #region Service Module

    public class TestDataModule : IServiceModule
    {
        public void Register(IServiceCollection services)
        {
            // Module registrieren nur ihre Services, keine Comparer-Scans mehr
            // (wird jetzt von DefaultBootstrapWrapper übernommen)
        }
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullBootstrapProcess_WithBootstrapWrapper_RegistersAndResolvesComparer()
    {
        // Arrange - Phase 1: Service-Registrierung via Bootstrap-Wrapper
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();
        
        // Simuliert: bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly)
        bootstrap.RegisterServices(services, typeof(BootstrapIntegrationTests).Assembly);

        // Phase 2: Container Build
        var serviceProvider = services.BuildServiceProvider();

        // Act - Phase 3: Service-Auflösung via GetEqualityComparer
        var customerComparer = serviceProvider.GetEqualityComparer<Customer>();
        var orderComparer = serviceProvider.GetEqualityComparer<Order>();
        var productComparer = serviceProvider.GetEqualityComparer<Product>(); // Kein Comparer registriert

        // Assert
        Assert.NotNull(customerComparer);
        Assert.IsType<CustomerComparer>(customerComparer);
        
        Assert.NotNull(orderComparer);
        Assert.IsType<OrderComparer>(orderComparer);
        
        // Product: Fallback auf EqualityComparer<T>.Default
        Assert.NotNull(productComparer);
        Assert.Same(EqualityComparer<Product>.Default, productComparer);
    }

    [Fact]
    public void FullBootstrapProcess_ComparerWorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();
        bootstrap.RegisterServices(services, typeof(BootstrapIntegrationTests).Assembly);
        var serviceProvider = services.BuildServiceProvider();

        var customer1 = new Customer { Id = 1, Name = "Alice" };
        var customer2 = new Customer { Id = 1, Name = "Bob" }; // Gleiche Id
        var customer3 = new Customer { Id = 2, Name = "Alice" };

        // Act
        var comparer = serviceProvider.GetEqualityComparer<Customer>();

        // Assert - Funktionsprüfung
        Assert.True(comparer.Equals(customer1, customer2)); // Gleiche Id
        Assert.False(comparer.Equals(customer1, customer3)); // Verschiedene Id
        Assert.Equal(customer1.GetHashCode(), comparer.GetHashCode(customer1));
    }

    [Fact]
    public void FullBootstrapProcess_WithoutRegisteredComparer_UsesFallback()
    {
        // Arrange
        var services = new ServiceCollection();
        // KEINE Bootstrap-Registrierung
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var stringComparer = serviceProvider.GetEqualityComparer<string>();
        var intComparer = serviceProvider.GetEqualityComparer<int>();
        var customerComparer = serviceProvider.GetEqualityComparer<Customer>();

        // Assert - Alle fallen auf EqualityComparer<T>.Default zurück
        Assert.Same(EqualityComparer<string>.Default, stringComparer);
        Assert.Same(EqualityComparer<int>.Default, intComparer);
        Assert.Same(EqualityComparer<Customer>.Default, customerComparer);
    }

    [Fact]
    public void FullBootstrapProcess_ManualRegistrationOverridesScanning()
    {
        // Arrange - Phase 1: Manuelle Registrierung VOR Bootstrap
        var services = new ServiceCollection();
        
        var customComparer = new CustomerComparer();
        services.AddSingleton<IEqualityComparer<Customer>>(customComparer);
        
        // Jetzt Bootstrap (würde auch CustomerComparer scannen)
        var bootstrap = new DefaultBootstrapWrapper();
        bootstrap.RegisterServices(services, typeof(BootstrapIntegrationTests).Assembly);

        // Phase 2: Build
        var serviceProvider = services.BuildServiceProvider();

        // Act - Phase 3: Auflösung
        var resolvedComparer = serviceProvider.GetEqualityComparer<Customer>();

        // Assert - Manuelle Registrierung hat Vorrang (idempotent via TryAdd)
        Assert.Same(customComparer, resolvedComparer);
    }

    [Fact]
    public void FullBootstrapProcess_SimulatesRepositoryUsage()
    {
        // Arrange - Simuliert reale Anwendung
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();
        bootstrap.RegisterServices(services, typeof(BootstrapIntegrationTests).Assembly);
        
        // Simuliert Repository-Registrierung
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var repository = serviceProvider.GetRequiredService<ICustomerRepository>();
        
        var customer1 = new Customer { Id = 1, Name = "Alice" };
        var customer2 = new Customer { Id = 1, Name = "Bob" };

        // Assert - Repository nutzt den via GetEqualityComparer aufgelösten Comparer
        Assert.True(repository.AreEqual(customer1, customer2));
    }

    [Fact]
    public void BootstrapWrapper_MultipleAssemblies_RegistersDifferentComparers()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();
        
        // Mehrere Assemblies scannen
        bootstrap.RegisterServices(
            services,
            typeof(BootstrapIntegrationTests).Assembly,
            typeof(DefaultBootstrapWrapper).Assembly
        );
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var customerComparer = serviceProvider.GetEqualityComparer<Customer>();
        var orderComparer = serviceProvider.GetEqualityComparer<Order>();

        // Assert
        Assert.IsType<CustomerComparer>(customerComparer);
        Assert.IsType<OrderComparer>(orderComparer);
    }

    [Fact]
    public void GetEqualityComparer_ThrowsOnNullServiceProvider()
    {
        // Arrange
        IServiceProvider? serviceProvider = null;

        // Act & Assert
        Assert.Throws<System.ArgumentNullException>(() => 
            serviceProvider!.GetEqualityComparer<Customer>());
    }

    [Fact]
    public void BootstrapWrapper_CanBeDecorated()
    {
        // Arrange - Decorator-Pattern
        var services = new ServiceCollection();
        
        var baseBootstrap = new DefaultBootstrapWrapper();
        var decoratedBootstrap = new TestDecoratorBootstrapWrapper(baseBootstrap);
        
        decoratedBootstrap.RegisterServices(services, typeof(BootstrapIntegrationTests).Assembly);
        
        var serviceProvider = services.BuildServiceProvider();

        // Act
        var testService = serviceProvider.GetService<ITestService>();
        var customerComparer = serviceProvider.GetEqualityComparer<Customer>();

        // Assert - Beide Registrierungen funktionieren
        Assert.NotNull(testService);
        Assert.IsType<TestService>(testService);
        Assert.IsType<CustomerComparer>(customerComparer);
    }

    #endregion

    #region Test Helper - Simulated Repository

    public interface ICustomerRepository
    {
        bool AreEqual(Customer x, Customer y);
    }

    public class CustomerRepository : ICustomerRepository
    {
        private readonly IEqualityComparer<Customer> _comparer;

        public CustomerRepository(IServiceProvider serviceProvider)
        {
            _comparer = serviceProvider.GetEqualityComparer<Customer>();
        }

        public bool AreEqual(Customer x, Customer y)
        {
            return _comparer.Equals(x, y);
        }
    }

    #endregion

    #region Test Helper - Decorator

    public interface ITestService { }
    public class TestService : ITestService { }

    public class TestDecoratorBootstrapWrapper : IBootstrapWrapper
    {
        private readonly IBootstrapWrapper _innerWrapper;

        public TestDecoratorBootstrapWrapper(IBootstrapWrapper innerWrapper)
        {
            _innerWrapper = innerWrapper;
        }

        public void RegisterServices(IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        {
            // Basis-Registrierungen
            _innerWrapper.RegisterServices(services, assemblies);
            
            // Eigene erweiterte Registrierungen
            services.AddSingleton<ITestService, TestService>();
        }
    }

    #endregion
}
