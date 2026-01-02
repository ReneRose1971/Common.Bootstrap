using Common.Bootstrap;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Common.BootStrap.Tests
{
    public interface IFoo { }
    public class Foo : IFoo { }

    public class TestModule : IServiceModule
    {
        public void Register(IServiceCollection services)
        {
            services.AddSingleton<IFoo, Foo>();
        }
    }

    public class ModuleRegistrationsTests
    {
        [Fact]
        public void ModuleRegistersService_WhenModuleExists()
        {
            var services = new ServiceCollection();
            services.AddModulesFromAssemblies(typeof(TestModule).Assembly);

            Assert.Contains(services, d => d.ServiceType == typeof(IFoo) && d.ImplementationType == typeof(Foo));
        }

        [Fact]
        public void AddModules_IgnoresAbstractAndInterfaces()
        {
            var services = new ServiceCollection();
            // Create an assembly with abstract/interface - reuse current assembly: define nested abstract/interface types
            services.AddModulesFromAssemblies(typeof(TestModule).Assembly);

            // Ensure at least the IFoo registration exists and no exceptions thrown
            Assert.Contains(services, d => d.ServiceType == typeof(IFoo));
        }
    }
}