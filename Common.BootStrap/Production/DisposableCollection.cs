using System;
using System.Collections.Generic;

namespace Common.Bootstrap;

/// <summary>
/// Container für mehrere <see cref="IDisposable"/>-Objekte.
/// Beim Dispose werden alle enthaltenen Disposables in umgekehrter Reihenfolge entsorgt.
/// </summary>
/// <remarks>
/// <para>
/// <b>Thread-Sicherheit:</b> Alle Operationen sind thread-sicher.
/// </para>
/// <para>
/// <b>Idempotenz:</b> Mehrfaches Aufrufen von <see cref="Dispose"/> ist sicher - 
/// jedes enthaltene Disposable wird nur einmal entsorgt.
/// </para>
/// <para>
/// <b>Auto-Dispose:</b> Wenn ein Disposable zu einem bereits disposed Container 
/// hinzugefügt wird, wird es sofort entsorgt.
/// </para>
/// </remarks>
/// <example>
/// Typische Verwendung für Ressourcen-Management:
/// <code>
/// public sealed class MyService : IDisposable
/// {
///     private readonly DisposableCollection _disposables = new();
///     
///     public MyService(IDataStore&lt;Entity&gt; dataStore)
///     {
///         // Event-Subscriptions sammeln
///         var subscription1 = dataStore.Items.SubscribeToChanges(OnChanged);
///         _disposables.Add(subscription1);
///         
///         var subscription2 = SomeObservable.Subscribe(OnNext);
///         _disposables.Add(subscription2);
///     }
///     
///     public void Dispose()
///     {
///         // Alle Subscriptions werden automatisch entsorgt
///         _disposables.Dispose();
///     }
/// }
/// </code>
/// </example>
public sealed class DisposableCollection : IDisposable
{
    private readonly object _lock = new();
    private List<IDisposable>? _disposables = new();
    private volatile bool _isDisposed;

    /// <summary>
    /// Gibt an, ob dieser Container bereits entsorgt wurde.
    /// </summary>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// Gibt die aktuelle Anzahl der verwalteten Disposables zurück.
    /// </summary>
    /// <remarks>
    /// Gibt 0 zurück, wenn der Container bereits disposed ist.
    /// </remarks>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _disposables?.Count ?? 0;
            }
        }
    }

    /// <summary>
    /// Erstellt eine leere DisposableCollection.
    /// </summary>
    public DisposableCollection()
    {
    }

    /// <summary>
    /// Erstellt eine DisposableCollection mit initialen Disposables.
    /// </summary>
    /// <param name="disposables">Die initialen Disposables (null-Werte werden ignoriert).</param>
    /// <exception cref="ArgumentNullException">
    /// Wenn <paramref name="disposables"/> null ist.
    /// </exception>
    public DisposableCollection(params IDisposable[] disposables)
    {
        if (disposables == null) throw new ArgumentNullException(nameof(disposables));

        foreach (var disposable in disposables)
        {
            if (disposable != null)
                _disposables!.Add(disposable);
        }
    }

    /// <summary>
    /// Fügt ein Disposable zum Container hinzu.
    /// </summary>
    /// <param name="disposable">Das hinzuzufügende Disposable.</param>
    /// <returns>
    /// Das übergebene Disposable (für Fluent-API).
    /// </returns>
    /// <remarks>
    /// <para>
    /// Wenn der Container bereits disposed ist, wird das Disposable sofort entsorgt.
    /// </para>
    /// <para>
    /// Null-Werte werden stillschweigend ignoriert.
    /// </para>
    /// </remarks>
    /// <example>
    /// Fluent-Style-Verwendung:
    /// <code>
    /// var subscription = _disposables.Add(
    ///     observable.Subscribe(OnNext));
    /// </code>
    /// </example>
    public IDisposable? Add(IDisposable? disposable)
    {
        if (disposable == null) return null;

        lock (_lock)
        {
            if (_isDisposed)
            {
                disposable.Dispose();
                return disposable;
            }

            _disposables!.Add(disposable);
            return disposable;
        }
    }

    /// <summary>
    /// Fügt mehrere Disposables auf einmal hinzu.
    /// </summary>
    /// <param name="disposables">Die hinzuzufügenden Disposables (null-Werte werden ignoriert).</param>
    /// <exception cref="ArgumentNullException">
    /// Wenn <paramref name="disposables"/> null ist.
    /// </exception>
    public void AddRange(IEnumerable<IDisposable> disposables)
    {
        if (disposables == null) throw new ArgumentNullException(nameof(disposables));

        foreach (var disposable in disposables)
        {
            Add(disposable);
        }
    }

    /// <summary>
    /// Entfernt ein Disposable aus dem Container, ohne es zu entsorgen.
    /// </summary>
    /// <param name="disposable">Das zu entfernende Disposable.</param>
    /// <returns>
    /// <c>true</c>, wenn das Disposable gefunden und entfernt wurde; 
    /// andernfalls <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Das entfernte Disposable wird NICHT automatisch entsorgt - 
    /// der Aufrufer ist dafür verantwortlich.
    /// </remarks>
    public bool Remove(IDisposable? disposable)
    {
        if (disposable == null) return false;

        lock (_lock)
        {
            if (_isDisposed) return false;
            return _disposables!.Remove(disposable);
        }
    }

    /// <summary>
    /// Entfernt alle Disposables aus dem Container, ohne sie zu entsorgen.
    /// </summary>
    /// <remarks>
    /// Die entfernten Disposables werden NICHT automatisch entsorgt.
    /// </remarks>
    public void Clear()
    {
        lock (_lock)
        {
            if (_isDisposed) return;
            _disposables!.Clear();
        }
    }

    /// <summary>
    /// Entsorgt alle enthaltenen Disposables in umgekehrter Reihenfolge (LIFO).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Reihenfolge:</b> Die Disposables werden in umgekehrter Reihenfolge entsorgt 
    /// (zuletzt hinzugefügt wird zuerst entsorgt). Dies entspricht dem typischen 
    /// Aufräum-Muster (Destruktor-Reihenfolge).
    /// </para>
    /// <para>
    /// <b>Fehlerbehandlung:</b> Wenn beim Entsorgen eines Disposables eine Exception 
    /// auftritt, werden die verbleibenden Disposables trotzdem entsorgt. Die erste 
    /// aufgetretene Exception wird nach dem Aufräumen geworfen.
    /// </para>
    /// <para>
    /// <b>Mehrfachaufruf:</b> Wiederholtes Aufrufen von Dispose ist sicher und hat 
    /// keine Auswirkungen (idempotent).
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        List<IDisposable>? toDispose = null;

        lock (_lock)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            toDispose = _disposables;
            _disposables = null;
        }

        if (toDispose == null || toDispose.Count == 0) return;

        List<Exception>? exceptions = null;

        for (int i = toDispose.Count - 1; i >= 0; i--)
        {
            try
            {
                toDispose[i]?.Dispose();
            }
            catch (Exception ex)
            {
                exceptions ??= new List<Exception>();
                exceptions.Add(ex);
            }
        }

        if (exceptions != null && exceptions.Count > 0)
        {
            throw new AggregateException(
                "Beim Entsorgen von einem oder mehreren Disposables sind Fehler aufgetreten.",
                exceptions);
        }
    }
}
