using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Bootstrap;

/// <summary>
/// Erweiterungen zur automatischen Erkennung und Ausführung aller <see cref="IServiceModule"/>-Implementierungen.
/// </summary>
public static class ServiceCollectionModuleExtensions
{
    /// <summary>
    /// Sucht in den angegebenen (oder allen geladenen) Assemblies nach Klassen,
    /// die <see cref="IServiceModule"/> implementieren, erzeugt Instanzen
    /// und ruft <see cref="IServiceModule.Register"/> auf.
    /// </summary>
    /// <param name="services">Die <see cref="IServiceCollection"/>, die erweitert werden soll.</param>
    /// <param name="assemblies">
    /// Liste der zu scannenden Assemblies. Wenn leer oder null, werden alle
    /// aktuell geladenen Assemblies (<see cref="System.AppDomain.GetAssemblies"/> ) gescannt.
    /// </param>
    /// <returns>
    /// Die gleiche <see cref="IServiceCollection"/> für Fluent-API-Verkettung.
    /// </returns>
    /// <exception cref="MissingMethodException">
    /// Wenn ein gefundenes <see cref="IServiceModule"/> keinen öffentlichen parameterlosen Konstruktor hat.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Wenn während der Registrierung in einem Modul ein Fehler auftritt.
    /// </exception>
    /// <remarks>
    /// <para>
    /// <b>Gefunden werden:</b>
    /// - Konkrete, nicht-abstrakte Klassen, die <see cref="IServiceModule"/> implementieren
    /// - Öffentliche und interne Klassen
    /// - Nested Classes (auch private, wenn zugänglich)
    /// </para>
    /// <para>
    /// <b>NICHT gefunden werden:</b>
    /// - Abstrakte Klassen und Interfaces
    /// - Generische Klassen mit ungebundenen Typparametern
    /// - Klassen ohne öffentlichen parameterlosen Konstruktor
    /// </para>
    /// <para>
    /// <b>Fehlerbehandlung:</b>
    /// <see cref="ReflectionTypeLoadException"/> wird automatisch behandelt - nur erfolgreich
    /// geladene Typen werden verarbeitet, fehlende Abhängigkeiten führen nicht zum Abbruch.
    /// </para>
    /// </remarks>
    /// <example>
    /// Typische Verwendung in Program.cs:
    /// <code>
    /// var builder = Host.CreateApplicationBuilder(args);
    /// 
    /// // Scannt nur die App-Assembly
    /// builder.Services.AddModulesFromAssemblies(typeof(Program).Assembly);
    /// 
    /// // Oder mehrere Assemblies
    /// builder.Services.AddModulesFromAssemblies(
    ///     typeof(Program).Assembly,
    ///     typeof(InfrastructureModule).Assembly,
    ///     typeof(DomainModule).Assembly
    /// );
    /// 
    /// var app = builder.Build();
    /// await app.RunAsync();
    /// </code>
    /// </example>
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
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!; }
            })
            .Where(t => moduleType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .Select(t => (IServiceModule)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var module in modules)
            module.Register(services);

        return services;
    }
}
