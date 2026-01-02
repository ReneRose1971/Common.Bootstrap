using Common.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Common.BootStrap.Tests;

public sealed class DisposableCollectionTests
{
    [Fact]
    public void Constructor_Creates_Empty_Collection()
    {
        var collection = new DisposableCollection();

        Assert.Equal(0, collection.Count);
        Assert.False(collection.IsDisposed);
    }

    [Fact]
    public void Constructor_With_Params_Adds_Disposables()
    {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();

        var collection = new DisposableCollection(d1, d2, d3);

        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void Constructor_With_Params_Ignores_Null_Values()
    {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();

        var collection = new DisposableCollection(d1, null!, d2);

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void Constructor_With_Null_Array_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DisposableCollection(null!));
    }

    [Fact]
    public void Add_Increases_Count()
    {
        var collection = new DisposableCollection();
        var disposable = new TestDisposable();

        collection.Add(disposable);

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Add_Returns_Added_Disposable()
    {
        var collection = new DisposableCollection();
        var disposable = new TestDisposable();

        var result = collection.Add(disposable);

        Assert.Same(disposable, result);
    }

    [Fact]
    public void Add_Null_Returns_Null_And_Does_Not_Increase_Count()
    {
        var collection = new DisposableCollection();

        var result = collection.Add(null);

        Assert.Null(result);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Add_Multiple_Disposables_Increases_Count()
    {
        var collection = new DisposableCollection();

        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());

        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void Add_To_Disposed_Collection_Disposes_Immediately()
    {
        var collection = new DisposableCollection();
        collection.Dispose();

        var disposable = new TestDisposable();
        collection.Add(disposable);

        Assert.True(disposable.IsDisposed);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void AddRange_Adds_All_Disposables()
    {
        var collection = new DisposableCollection();
        var disposables = new[]
        {
            new TestDisposable(),
            new TestDisposable(),
            new TestDisposable()
        };

        collection.AddRange(disposables);

        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void AddRange_Ignores_Null_Values()
    {
        var collection = new DisposableCollection();
        var disposables = new IDisposable?[]
        {
            new TestDisposable(),
            null,
            new TestDisposable()
        };

        collection.AddRange(disposables!);

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void AddRange_With_Null_Collection_Throws()
    {
        var collection = new DisposableCollection();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange(null!));
    }

    [Fact]
    public void Remove_Existing_Disposable_Returns_True()
    {
        var collection = new DisposableCollection();
        var disposable = new TestDisposable();
        collection.Add(disposable);

        var result = collection.Remove(disposable);

        Assert.True(result);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Remove_Non_Existing_Disposable_Returns_False()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());

        var otherDisposable = new TestDisposable();
        var result = collection.Remove(otherDisposable);

        Assert.False(result);
        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Remove_Does_Not_Dispose_Removed_Item()
    {
        var collection = new DisposableCollection();
        var disposable = new TestDisposable();
        collection.Add(disposable);

        collection.Remove(disposable);

        Assert.False(disposable.IsDisposed);
    }

    [Fact]
    public void Remove_Null_Returns_False()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());

        var result = collection.Remove(null);

        Assert.False(result);
        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Remove_From_Disposed_Collection_Returns_False()
    {
        var collection = new DisposableCollection();
        var disposable = new TestDisposable();
        collection.Add(disposable);
        collection.Dispose();

        var result = collection.Remove(disposable);

        Assert.False(result);
    }

    [Fact]
    public void Clear_Removes_All_Disposables()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());

        collection.Clear();

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Clear_Does_Not_Dispose_Items()
    {
        var collection = new DisposableCollection();
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        collection.Add(d1);
        collection.Add(d2);

        collection.Clear();

        Assert.False(d1.IsDisposed);
        Assert.False(d2.IsDisposed);
    }

    [Fact]
    public void Clear_On_Disposed_Collection_Does_Nothing()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());
        collection.Dispose();

        var exception = Record.Exception(() => collection.Clear());

        Assert.Null(exception);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Dispose_Disposes_All_Disposables()
    {
        var d1 = new TestDisposable();
        var d2 = new TestDisposable();
        var d3 = new TestDisposable();
        var collection = new DisposableCollection(d1, d2, d3);

        collection.Dispose();

        Assert.True(d1.IsDisposed);
        Assert.True(d2.IsDisposed);
        Assert.True(d3.IsDisposed);
    }

    [Fact]
    public void Dispose_Sets_IsDisposed_To_True()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());

        collection.Dispose();

        Assert.True(collection.IsDisposed);
    }

    [Fact]
    public void Dispose_Sets_Count_To_Zero()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());

        collection.Dispose();

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Dispose_In_LIFO_Order()
    {
        var disposeOrder = new List<int>();
        var d1 = new OrderTrackingDisposable(1, disposeOrder);
        var d2 = new OrderTrackingDisposable(2, disposeOrder);
        var d3 = new OrderTrackingDisposable(3, disposeOrder);

        var collection = new DisposableCollection();
        collection.Add(d1);
        collection.Add(d2);
        collection.Add(d3);

        collection.Dispose();

        Assert.Equal(new[] { 3, 2, 1 }, disposeOrder);
    }

    [Fact]
    public void Dispose_Multiple_Times_Is_Idempotent()
    {
        var disposable = new TestDisposable();
        var collection = new DisposableCollection(disposable);

        collection.Dispose();
        collection.Dispose();
        collection.Dispose();

        Assert.Equal(1, disposable.DisposeCount);
        Assert.True(collection.IsDisposed);
    }

    [Fact]
    public void Dispose_Empty_Collection_Does_Not_Throw()
    {
        var collection = new DisposableCollection();

        var exception = Record.Exception(() => collection.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_With_Null_Disposable_Does_Not_Throw()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());

        var exception = Record.Exception(() => collection.Dispose());

        Assert.Null(exception);
    }

    [Fact]
    public void Dispose_Continues_On_Exception_And_Throws_AggregateException()
    {
        var d1 = new TestDisposable();
        var d2 = new ThrowingDisposable();
        var d3 = new TestDisposable();

        var collection = new DisposableCollection(d1, d2, d3);

        var exception = Assert.Throws<AggregateException>(() => collection.Dispose());

        Assert.Single(exception.InnerExceptions);
        Assert.True(d1.IsDisposed);
        Assert.True(d3.IsDisposed);
    }

    [Fact]
    public void Dispose_Collects_Multiple_Exceptions()
    {
        var d1 = new ThrowingDisposable("Error 1");
        var d2 = new TestDisposable();
        var d3 = new ThrowingDisposable("Error 2");

        var collection = new DisposableCollection(d1, d2, d3);

        var exception = Assert.Throws<AggregateException>(() => collection.Dispose());

        Assert.Equal(2, exception.InnerExceptions.Count);
        Assert.Contains(exception.InnerExceptions, e => e.Message == "Error 1");
        Assert.Contains(exception.InnerExceptions, e => e.Message == "Error 2");
        Assert.True(d2.IsDisposed);
    }

    [Fact]
    public void Thread_Safety_Add_From_Multiple_Threads()
    {
        var collection = new DisposableCollection();
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    collection.Add(new TestDisposable());
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Assert.Equal(1000, collection.Count);
    }

    [Fact]
    public void Thread_Safety_Add_And_Dispose_Concurrent()
    {
        var collection = new DisposableCollection();
        var addTask = Task.Run(() =>
        {
            for (int i = 0; i < 100; i++)
            {
                collection.Add(new TestDisposable());
                Thread.Sleep(1);
            }
        });

        Thread.Sleep(50);
        var disposeTask = Task.Run(() => collection.Dispose());

        Task.WaitAll(addTask, disposeTask);

        Assert.True(collection.IsDisposed);
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Thread_Safety_Multiple_Dispose_Calls()
    {
        var disposable = new TestDisposable();
        var collection = new DisposableCollection(disposable);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => collection.Dispose()))
            .ToArray();

        Task.WaitAll(tasks);

        Assert.Equal(1, disposable.DisposeCount);
        Assert.True(collection.IsDisposed);
    }

    [Fact]
    public void Count_Returns_Zero_After_Dispose()
    {
        var collection = new DisposableCollection();
        collection.Add(new TestDisposable());
        collection.Add(new TestDisposable());

        collection.Dispose();

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Fluent_API_Works()
    {
        var collection = new DisposableCollection();

        var d1 = collection.Add(new TestDisposable());
        var d2 = collection.Add(new TestDisposable());

        Assert.NotNull(d1);
        Assert.NotNull(d2);
        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void Real_World_Scenario_Event_Subscriptions()
    {
        var eventSource = new EventSource();
        var collection = new DisposableCollection();

        var subscription1 = eventSource.Subscribe(() => { });
        collection.Add(subscription1);

        var subscription2 = eventSource.Subscribe(() => { });
        collection.Add(subscription2);

        Assert.Equal(2, eventSource.SubscriberCount);

        collection.Dispose();

        Assert.Equal(0, eventSource.SubscriberCount);
    }

    private sealed class TestDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            DisposeCount++;
        }
    }

    private sealed class OrderTrackingDisposable : IDisposable
    {
        private readonly int _id;
        private readonly List<int> _disposeOrder;

        public OrderTrackingDisposable(int id, List<int> disposeOrder)
        {
            _id = id;
            _disposeOrder = disposeOrder;
        }

        public void Dispose()
        {
            _disposeOrder.Add(_id);
        }
    }

    private sealed class ThrowingDisposable : IDisposable
    {
        private readonly string _message;

        public ThrowingDisposable(string message = "Dispose failed")
        {
            _message = message;
        }

        public void Dispose()
        {
            throw new InvalidOperationException(_message);
        }
    }

    private sealed class EventSource
    {
        private readonly List<Action> _subscribers = new();

        public int SubscriberCount => _subscribers.Count;

        public IDisposable Subscribe(Action handler)
        {
            _subscribers.Add(handler);
            return new Subscription(this, handler);
        }

        private void Unsubscribe(Action handler)
        {
            _subscribers.Remove(handler);
        }

        private sealed class Subscription : IDisposable
        {
            private EventSource? _source;
            private readonly Action _handler;

            public Subscription(EventSource source, Action handler)
            {
                _source = source;
                _handler = handler;
            }

            public void Dispose()
            {
                _source?.Unsubscribe(_handler);
                _source = null;
            }
        }
    }
}
