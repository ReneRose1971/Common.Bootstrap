using Common.Bootstrap;
using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Xunit;

namespace Common.BootStrap.Tests;

/// <summary>
/// Tests für <see cref="DefaultBootstrapWrapper"/>.
/// </summary>
public sealed class DefaultBootstrapWrapperTests
{
    [Fact]
    public void RegisterServices_ScansAssemblyForModulesAndComparers()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();

        // Act
        bootstrap.RegisterServices(services, typeof(DefaultBootstrapWrapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert - Comparer aus Assembly registriert
        var comparer = provider.GetService<IEqualityComparer<TestObject>>();
        Assert.NotNull(comparer);
        Assert.IsType<TestObjectComparer>(comparer);
    }

    [Fact]
    public void RegisterServices_DoesNotOverrideExistingRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEqualityComparer<string>>(StringComparer.OrdinalIgnoreCase);
        
        var bootstrap = new DefaultBootstrapWrapper();

        // Act
        bootstrap.RegisterServices(services, typeof(DefaultBootstrapWrapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert - Manuelle Registrierung bleibt erhalten (idempotent via TryAdd)
        var comparer = provider.GetService<IEqualityComparer<string>>();
        Assert.Same(StringComparer.OrdinalIgnoreCase, comparer);
    }

    [Fact]
    public void RegisterServices_IsIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();

        // Act - Mehrfach aufrufen
        bootstrap.RegisterServices(services, typeof(DefaultBootstrapWrapperTests).Assembly);
        bootstrap.RegisterServices(services, typeof(DefaultBootstrapWrapperTests).Assembly);
        
        var provider = services.BuildServiceProvider();

        // Assert - Sollte nicht crashen
        var comparer = provider.GetService<IEqualityComparer<TestObject>>();
        Assert.NotNull(comparer);
    }

    [Fact]
    public void RegisterServices_ThrowsOnNullServices()
    {
        // Arrange
        var bootstrap = new DefaultBootstrapWrapper();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            bootstrap.RegisterServices(null!, typeof(DefaultBootstrapWrapperTests).Assembly));
    }

    [Fact]
    public void RegisterServices_ThrowsOnNullAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            bootstrap.RegisterServices(services, null!));
    }

    [Fact]
    public void RegisterServices_WorksWithMultipleAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var bootstrap = new DefaultBootstrapWrapper();

        // Act
        bootstrap.RegisterServices(
            services,
            typeof(DefaultBootstrapWrapperTests).Assembly,
            typeof(DefaultBootstrapWrapper).Assembly
        );
        
        var provider = services.BuildServiceProvider();

        // Assert
        var comparer = provider.GetService<IEqualityComparer<TestObject>>();
        Assert.NotNull(comparer);
    }

    [Fact]
    public void RegisterServices_CanBeOverriddenInDerivedClass()
    {
        // Arrange
        var services = new ServiceCollection();
        var customBootstrap = new CustomBootstrapWrapper();

        // Act
        customBootstrap.RegisterServices(services, typeof(DefaultBootstrapWrapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Assert - Custom-Service wurde registriert
        var customService = provider.GetService<ICustomService>();
        Assert.NotNull(customService);
        Assert.IsType<CustomService>(customService);
    }

    #region Test Helper Classes

    public sealed class TestObject
    {
        public int Value { get; set; }

        public override bool Equals(object? obj)
            => obj is TestObject other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();
    }

    public sealed class TestObjectComparer : IEqualityComparer<TestObject>
    {
        public bool Equals(TestObject? x, TestObject? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Value == y.Value;
        }

        public int GetHashCode(TestObject obj)
        {
            if (obj is null) throw new ArgumentNullException(nameof(obj));
            return obj.Value.GetHashCode();
        }
    }

    public interface ICustomService { }
    public class CustomService : ICustomService { }

    public class CustomBootstrapWrapper : DefaultBootstrapWrapper
    {
        public override void RegisterServices(IServiceCollection services, params System.Reflection.Assembly[] assemblies)
        {
            base.RegisterServices(services, assemblies);
            services.AddSingleton<ICustomService, CustomService>();
        }
    }

    #endregion
}
