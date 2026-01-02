using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Common.BootStrap.Tests;

/// <summary>
/// Tests für <see cref="ServiceCollectionEqualityComparerExtensions"/>.
/// </summary>
public sealed class EqualityComparerRegistrationExtensionsTests
{
    [Fact]
    public void AddEqualityComparersFromAssembly_Registers_Concrete_Comparers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEqualityComparersFromAssembly<TestEntity>();
        var provider = services.BuildServiceProvider();

        // Assert - TestEntityComparer sollte registriert sein
        var comparer = provider.GetService<IEqualityComparer<TestEntity>>();
        Assert.NotNull(comparer);
        Assert.IsType<TestEntityComparer>(comparer);
    }

    [Fact]
    public void AddEqualityComparersFromAssembly_Is_Idempotent()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - zweimal aufrufen
        services.AddEqualityComparersFromAssembly<TestEntity>();
        services.AddEqualityComparersFromAssembly<TestEntity>();
        var provider = services.BuildServiceProvider();

        // Assert - sollte nicht doppelt registriert sein
        var comparers = provider.GetServices<IEqualityComparer<TestEntity>>().ToList();
        Assert.Single(comparers);
    }

    [Fact]
    public void AddEqualityComparersFromAssembly_Respects_Existing_Registrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var customComparer = new TestEntityComparer2();
        services.AddSingleton<IEqualityComparer<TestEntity>>(customComparer);

        // Act
        services.AddEqualityComparersFromAssembly<TestEntity>();
        var provider = services.BuildServiceProvider();

        // Assert - die vorherige Registrierung bleibt bestehen (TryAdd)
        var comparer = provider.GetRequiredService<IEqualityComparer<TestEntity>>();
        Assert.Same(customComparer, comparer);
    }

    [Fact]
    public void AddEqualityComparersFromAssembly_Finds_Multiple_Comparers_From_Same_Type()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEqualityComparersFromAssembly<MultiComparer>();
        var provider = services.BuildServiceProvider();

        // Assert - MultiComparer implementiert IEqualityComparer<int> und IEqualityComparer<string>
        var intComparer = provider.GetService<IEqualityComparer<int>>();
        var stringComparer = provider.GetService<IEqualityComparer<string>>();

        Assert.NotNull(intComparer);
        Assert.NotNull(stringComparer);
        Assert.IsType<MultiComparer>(intComparer);
        Assert.IsType<MultiComparer>(stringComparer);
    }

    [Fact]
    public void AddEqualityComparersFromAssembly_Handles_Empty_Assembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - sollte nicht crashen, auch wenn keine Comparer gefunden werden
        services.AddEqualityComparersFromAssembly<object>(); // mscorlib/System.Private.CoreLib
        var provider = services.BuildServiceProvider();

        // Assert - kein Crash
        Assert.NotNull(provider);
    }

    // Test-Typen
    public sealed class TestEntity
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public sealed class TestEntityComparer : IEqualityComparer<TestEntity>
    {
        public bool Equals(TestEntity? x, TestEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id == y.Id;
        }

        public int GetHashCode(TestEntity obj) => obj?.Id.GetHashCode() ?? 0;
    }

    public sealed class TestEntityComparer2 : IEqualityComparer<TestEntity>
    {
        public bool Equals(TestEntity? x, TestEntity? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Id == y.Id && x.Name == y.Name;
        }

        public int GetHashCode(TestEntity obj) => HashCode.Combine(obj?.Id, obj?.Name);
    }

    // Comparer, der mehrere Typen implementiert
    public sealed class MultiComparer : IEqualityComparer<int>, IEqualityComparer<string>
    {
        public bool Equals(int x, int y) => x == y;
        public int GetHashCode(int obj) => obj.GetHashCode();

        public bool Equals(string? x, string? y) => string.Equals(x, y, StringComparison.Ordinal);
        public int GetHashCode(string obj) => obj?.GetHashCode() ?? 0;
    }
}
