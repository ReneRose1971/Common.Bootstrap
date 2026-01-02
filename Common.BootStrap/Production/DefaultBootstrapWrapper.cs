using Common.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Common.Bootstrap;

/// <summary>
/// Standard-Implementierung des Bootstrap-Wrappers für Common.Bootstrap.
/// Registriert IServiceModule und IEqualityComparer aus Assemblies.
/// </summary>
/// <remarks>
/// Diese Implementierung führt folgende Registrierungen durch:
/// <list type="number">
/// <item><description>Scannt nach <see cref="IServiceModule"/>-Implementierungen und ruft deren <see cref="IServiceModule.Register"/> auf</description></item>
/// <item><description>Scannt nach <see cref="IEqualityComparer{T}"/>-Implementierungen und registriert sie als Singleton</description></item>
/// </list>
/// <para>
/// <b>Erweiterbarkeit:</b> Diese Klasse kann als Basis für Decorator-Implementierungen
/// dienen, die zusätzliche Assembly-Scans durchführen.
/// </para>
/// </remarks>
/// <example>
/// Standard-Verwendung:
/// <code>
/// var builder = Host.CreateApplicationBuilder(args);
/// 
/// var bootstrap = new DefaultBootstrapWrapper();
/// bootstrap.RegisterServices(
///     builder.Services,
///     typeof(Program).Assembly,
///     typeof(InfrastructureModule).Assembly
/// );
/// 
/// var app = builder.Build();
/// await app.RunAsync();
/// </code>
/// </example>
public class DefaultBootstrapWrapper : IBootstrapWrapper
{
    /// <summary>
    /// Registriert Services aus den angegebenen Assemblies.
    /// Führt Module- und EqualityComparer-Scanning durch.
    /// </summary>
    /// <param name="services">Die Service-Collection.</param>
    /// <param name="assemblies">Die zu scannenden Assemblies.</param>
    /// <exception cref="ArgumentNullException">
    /// Wenn <paramref name="services"/> oder <paramref name="assemblies"/> null ist.
    /// </exception>
    public virtual void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (assemblies == null)
            throw new ArgumentNullException(nameof(assemblies));

        // Phase 1: IServiceModule-Implementierungen scannen und registrieren
        services.AddModulesFromAssemblies(assemblies);

        // Phase 2: IEqualityComparer-Implementierungen scannen und registrieren
        foreach (var assembly in assemblies)
        {
            services.AddEqualityComparersFromAssembly(assembly);
        }
    }
}
