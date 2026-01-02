using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Common.Extensions;

namespace Common.BootStrap.Tests
{
    public class MyType { public int Id { get; set; } }

    public class MyComparer : IEqualityComparer<MyType>
    {
        public bool Equals(MyType? x, MyType? y) => x?.Id == y?.Id;
        public int GetHashCode(MyType obj) => obj.Id.GetHashCode();
    }

    public class EqualityComparerRegistrationTests
    {
        [Fact]
        public void AddEqualityComparersFromAssembly_RegistersComparer()
        {
            var services = new ServiceCollection();
            services.AddEqualityComparersFromAssembly<MyComparer>();

            var provider = services.BuildServiceProvider();
            var comparer = provider.GetService<IEqualityComparer<MyType>>();

            Assert.NotNull(comparer);
            Assert.IsType<MyComparer>(comparer);
        }

        [Fact]
        public void AddEqualityComparers_Works_With_Different_Markers()
        {
            var services = new ServiceCollection();
            
            // Verschiedene Marker aus derselben Assembly
            services.AddEqualityComparersFromAssembly<MyComparer>();
            services.AddEqualityComparersFromAssembly<MyType>();

            var provider = services.BuildServiceProvider();
            var comparer = provider.GetService<IEqualityComparer<MyType>>();

            Assert.NotNull(comparer);
        }

        [Fact]
        public void AddEqualityComparers_Is_Idempotent()
        {
            var services = new ServiceCollection();

            // Mehrfach aufrufen
            services.AddEqualityComparersFromAssembly<MyComparer>();
            services.AddEqualityComparersFromAssembly<MyComparer>();

            var provider = services.BuildServiceProvider();
            var comparer = provider.GetService<IEqualityComparer<MyType>>();

            Assert.NotNull(comparer);
        }
    }
}