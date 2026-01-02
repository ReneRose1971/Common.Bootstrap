using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.Bootstrap;

/// <summary>
/// Schnittstelle für Bootstrap-Wrapper, die Assembly-basierte Registrierungen kapseln.
/// Ermöglicht Decorator-Pattern für erweiterbare Registrierungs-Pipelines.
/// </summary>
/// <remarks>
/// Dieses Interface definiert einen einheitlichen Mechanismus für Assembly-Scanning
/// und Service-Registrierung. Implementierungen können:
/// <list type="bullet">
/// <item><description>Basisregistrierungen durchführen (z.B. <see cref="DefaultBootstrapWrapper"/>)</description></item>
/// <item><description>Andere Wrapper dekorieren, um zusätzliche Registrierungen hinzuzufügen</description></item>
/// <item><description>In konsumierenden Libraries erweitert werden</description></item>
/// </list>
/// <para>
/// <b>Decorator-Pattern:</b> Konsumierende Libraries können eigene Wrapper erstellen,
/// die den Standard-Wrapper dekorieren und weitere Assembly-Scans hinzufügen.
/// </para>
/// </remarks>
/// <example>
/// Verwendung in Program.cs:
/// <code>
/// var builder = Host.CreateApplicationBuilder(args);
/// 
/// // Standard-Bootstrap
/// var bootstrap = new DefaultBootstrapWrapper();
/// bootstrap.RegisterServices(builder.Services, typeof(Program).Assembly);
/// 
/// var app = builder.Build();
/// await app.RunAsync();
/// </code>
/// 
/// Decorator für erweiterte Registrierungen:
/// <code>
/// public class MyLibraryBootstrapWrapper : IBootstrapWrapper
/// {
///     private readonly IBootstrapWrapper _innerWrapper;
///     
///     public MyLibraryBootstrapWrapper(IBootstrapWrapper innerWrapper)
///     {
///         _innerWrapper = innerWrapper;
///     }
///     
///     public void RegisterServices(IServiceCollection services, params Assembly[] assemblies)
///     {
///         // Basis-Registrierungen
///         _innerWrapper.RegisterServices(services, assemblies);
///         
///         // Eigene erweiterte Scans
///         services.AddMyCustomScannersFromAssemblies(assemblies);
///     }
/// }
/// </code>
/// </example>
public interface IBootstrapWrapper
{
    /// <summary>
    /// Registriert Services aus den angegebenen Assemblies in die <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">Die Service-Collection, in die registriert werden soll.</param>
    /// <param name="assemblies">Die zu scannenden Assemblies.</param>
    /// <remarks>
    /// Implementierungen sollten typischerweise:
    /// <list type="number">
    /// <item><description><see cref="IServiceModule"/>-Implementierungen scannen und registrieren</description></item>
    /// <item><description><see cref="IEqualityComparer{T}"/>-Implementierungen scannen und registrieren</description></item>
    /// <item><description>Weitere projektspezifische Scans durchführen</description></item>
    /// </list>
    /// </remarks>
    void RegisterServices(IServiceCollection services, params Assembly[] assemblies);
}
