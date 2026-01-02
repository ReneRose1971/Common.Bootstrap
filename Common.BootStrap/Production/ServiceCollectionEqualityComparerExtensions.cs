using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Common.Extensions
{
    /// <summary>
    /// Extensions zur automatischen Registrierung von <see cref="IEqualityComparer{T}"/>-Implementierungen
    /// aus Assemblies in <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionEqualityComparerExtensions
    {
        /// <summary>
        /// Scannt die Assembly des angegebenen Marker-Typs nach allen konkreten Implementierungen
        /// von <see cref="IEqualityComparer{T}"/> und registriert sie als Singleton.
        /// </summary>
        /// <typeparam name="TMarker">
        /// Ein beliebiger Typ aus der zu scannenden Assembly (z.B. das ServiceModule selbst).
        /// </typeparam>
        /// <param name="services">Die zu erweiternde <see cref="IServiceCollection"/>.</param>
        /// <returns>Die erweiterte <see cref="IServiceCollection"/> für Fluent-API.</returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="services"/> <c>null</c> ist.
        /// </exception>
        public static IServiceCollection AddEqualityComparersFromAssembly<TMarker>(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            var assembly = typeof(TMarker).Assembly;
            return AddEqualityComparersFromAssembly(services, assembly);
        }

        /// <summary>
        /// Scannt die angegebene Assembly nach allen konkreten Implementierungen
        /// von <see cref="IEqualityComparer{T}"/> und registriert sie als Singleton.
        /// </summary>
        /// <param name="services">Die zu erweiternde <see cref="IServiceCollection"/>.</param>
        /// <param name="assembly">Die zu scannende Assembly.</param>
        /// <returns>Die erweiterte <see cref="IServiceCollection"/> für Fluent-API.</returns>
        /// <exception cref="ArgumentNullException">
        /// Wenn <paramref name="services"/> oder <paramref name="assembly"/> <c>null</c> ist.
        /// </exception>
        /// <remarks>
        /// <para>
        /// <b>Filter-Kriterien:</b> Diese Methode findet nur Typen, die <b>alle</b> folgenden Bedingungen erfüllen:
        /// </para>
        /// <list type="bullet">
        /// <item>Konkrete Klassen (nicht abstract, nicht interface)</item>
        /// <item>Öffentlich (<c>IsPublic</c>) oder nested public (<c>IsNestedPublic</c>)</item>
        /// <item>Haben einen öffentlichen parameterlosen Konstruktor</item>
        /// <item>Keine offenen generischen Typen (kein <c>ContainsGenericParameters</c>)</item>
        /// <item>Implementieren <see cref="IEqualityComparer{T}"/></item>
        /// </list>
        /// <para>
        /// <b>Registrierung:</b> Gefundene Comparer werden als Singleton mittels 
        /// <see cref="ServiceCollectionDescriptorExtensions.TryAddSingleton(IServiceCollection, Type, Type)"/> registriert,
        /// d.h. bestehende Registrierungen werden nicht überschrieben (idempotent).
        /// </para>
        /// <para>
        /// <b>Fehlerbehandlung:</b> <see cref="ReflectionTypeLoadException"/> wird automatisch behandelt -
        /// nur erfolgreich geladene Typen werden verarbeitet.
        /// </para>
        /// <para>
        /// <b>Hinweis:</b> Offene generische Typen (z.B. mit ungebundenen Typ-Parametern)
        /// werden automatisch übersprungen, da sie nicht direkt instanziiert werden können. 
        /// Nur geschlossene generische Typen (z.B. <c>CustomerComparer : IEqualityComparer&lt;Customer&gt;</c>) werden gefunden.
        /// </para>
        /// </remarks>
        public static IServiceCollection AddEqualityComparersFromAssembly(
            this IServiceCollection services,
            Assembly assembly)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            foreach (var type in SafeGetTypes(assembly))
            {
                // Nur konkrete Klassen berücksichtigen
                if (type.IsAbstract || type.IsInterface)
                    continue;

                // Nur öffentliche Typen (auch nested public)
                if (!type.IsPublic && !type.IsNestedPublic)
                    continue;

                // Offene generische Typen überspringen
                if (type.ContainsGenericParameters)
                    continue;

                // Nur Typen mit öffentlichem parameterlosen Konstruktor
                if (!HasPublicParameterlessConstructor(type))
                    continue;

                // Alle IEqualityComparer<T>-Interfaces finden, die dieser Typ implementiert
                var comparerInterfaces = type
                    .GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEqualityComparer<>))
                    .Distinct();

                foreach (var serviceType in comparerInterfaces)
                {
                    // Idempotent registrieren: TryAdd überschreibt keine bestehenden Registrierungen
                    services.TryAddSingleton(serviceType, type);
                }
            }

            return services;
        }

        /// <summary>
        /// Prüft, ob ein Typ einen öffentlichen parameterlosen Konstruktor hat.
        /// </summary>
        private static bool HasPublicParameterlessConstructor(Type type)
        {
            return type.GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null) != null;
        }

        /// <summary>
        /// Liefert bei TypeLoad-Problemen nur die erfolgreich ladbaren Typen zurück.
        /// </summary>
        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null)!;
            }
        }
    }
}
