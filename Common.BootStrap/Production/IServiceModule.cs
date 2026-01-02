using Microsoft.Extensions.DependencyInjection;

namespace Common.Bootstrap;

/// <summary>
/// Schnittstelle für modulare DI-Registrierungen.
/// Jede Bibliothek implementiert ein Modul, um ihre eigenen Services anzumelden.
/// </summary>
public interface IServiceModule
{
    /// <summary>
    /// Führt alle DI-Registrierungen dieses Moduls aus.
    /// </summary>
    void Register(IServiceCollection services);
}
