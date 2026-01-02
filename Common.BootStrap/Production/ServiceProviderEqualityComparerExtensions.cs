using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Common.Extensions;

/// <summary>
/// Erweiterungen für <see cref="IServiceProvider"/> zur Auflösung von EqualityComparern mit automatischem Fallback.
/// </summary>
public static class ServiceProviderEqualityComparerExtensions
{
    /// <summary>
    /// Löst einen EqualityComparer für den angegebenen Typ auf.
    /// Versucht zunächst die Auflösung über DI, dann Fallback auf <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <typeparam name="T">Der Typ, für den ein Comparer benötigt wird.</typeparam>
    /// <param name="serviceProvider">Der DI-Container.</param>
    /// <returns>
    /// Ein registrierter <see cref="IEqualityComparer{T}"/> aus dem DI-Container, 
    /// oder <see cref="EqualityComparer{T}.Default"/> als Fallback.
    /// </returns>
    /// <exception cref="ArgumentNullException">Wenn <paramref name="serviceProvider"/> null ist.</exception>
    /// <remarks>
    /// Diese Methode ermöglicht es, EqualityComparer optional zu registrieren. 
    /// Wenn kein expliziter Comparer für <typeparamref name="T"/> registriert wurde, 
    /// wird automatisch <see cref="EqualityComparer{T}.Default"/> verwendet.
    /// <para>
    /// Dies ist besonders nützlich in Repositories oder Services, die EqualityComparer benötigen,
    /// aber nicht für jeden Typ explizite Registrierungen erzwingen möchten.
    /// </para>
    /// <para>
    /// <b>Phase-Zuordnung:</b> Diese Methode ist für Phase 3 (Startup-Initialisierung) vorgesehen
    /// und wird typischerweise in Service-Konstruktoren verwendet.
    /// </para>
    /// </remarks>
    /// <example>
    /// Verwendung in einem Repository:
    /// <code>
    /// public class LiteDbRepository&lt;T&gt; : IRepository&lt;T&gt;
    /// {
    ///     private readonly IEqualityComparer&lt;T&gt; _comparer;
    ///     
    ///     public LiteDbRepository(IServiceProvider serviceProvider)
    ///     {
    ///         _comparer = serviceProvider.GetEqualityComparer&lt;T&gt;();
    ///     }
    ///     
    ///     public bool Contains(T entity)
    ///     {
    ///         return _collection.Any(e =&gt; _comparer.Equals(e, entity));
    ///     }
    /// }
    /// </code>
    /// </example>
    public static IEqualityComparer<T> GetEqualityComparer<T>(this IServiceProvider serviceProvider)
    {
        if (serviceProvider == null)
        {
            throw new ArgumentNullException(nameof(serviceProvider));
        }

        var comparer = serviceProvider.GetService<IEqualityComparer<T>>();
        
        return comparer ?? EqualityComparer<T>.Default;
    }
}
