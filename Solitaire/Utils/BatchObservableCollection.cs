using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace Solitaire.Utils;

public sealed class BatchObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged,
    IDisposable
{
    private const string CountString = "Count";

    private const string IndexerName = "Item[]";

    // ReSharper disable once StaticMemberInGenericType
    private static readonly NotifyCollectionChangedEventHandler EmptyDelegate = delegate { };

    private readonly ReentryMonitor _monitor = new();

    private readonly NotificationInfo? _notifyInfo;

    private bool _disableReentry;


    private Action _fireCountAndIndexerChanged = delegate { };


    private Action _fireIndexerChanged = delegate { };

    private event PropertyChangedEventHandler? PropertyChanged;

    private event NotifyCollectionChangedEventHandler CollectionChanged = EmptyDelegate;

    public BatchObservableCollection()
    {
    }


    private BatchObservableCollection(BatchObservableCollection<T>? parent, bool notify)
        : base(parent?.Items!)
    {
        _notifyInfo = new NotificationInfo
        {
            RootCollection = parent
        };

        if (notify)
        {
            CollectionChanged = _notifyInfo.Initialize();
        }
    }

    ~BatchObservableCollection()
    {
        DisposeInternal();
    }

    event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            if (null == _notifyInfo)
            {
                if (null == PropertyChanged)
                {
                    _fireCountAndIndexerChanged = delegate
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(CountString));
                        OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
                    };
                    _fireIndexerChanged = delegate { OnPropertyChanged(new PropertyChangedEventArgs(IndexerName)); };
                }

                PropertyChanged += value;
            }
            else if (_notifyInfo.RootCollection != null) _notifyInfo.RootCollection.PropertyChanged += value;
        }

        remove
        {
            if (null == _notifyInfo)
            {
                PropertyChanged -= value;

                if (null == PropertyChanged)
                {
                    _fireCountAndIndexerChanged = delegate { };
                    _fireIndexerChanged = delegate { };
                }
            }
            else if (_notifyInfo.RootCollection != null) _notifyInfo.RootCollection.PropertyChanged -= value;
        }
    }

    event NotifyCollectionChangedEventHandler? INotifyCollectionChanged.CollectionChanged
    {
        add
        {
            if (null == _notifyInfo)
            {
                if (1 == CollectionChanged.GetInvocationList().Length)
                    CollectionChanged -= EmptyDelegate;

                CollectionChanged += value;
                if (CollectionChanged != null) _disableReentry = CollectionChanged.GetInvocationList().Length > 1;
            }
            else if (_notifyInfo.RootCollection != null) _notifyInfo.RootCollection.CollectionChanged += value;
        }

        remove
        {
            if (null == _notifyInfo)
            {
                CollectionChanged -= value;

                if ((null == CollectionChanged) || (0 == CollectionChanged.GetInvocationList().Length))
                    CollectionChanged += EmptyDelegate;

                _disableReentry = CollectionChanged.GetInvocationList().Length > 1;
            }
            else if (_notifyInfo.RootCollection != null) _notifyInfo.RootCollection.CollectionChanged -= value;
        }
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Move(int oldIndex, int newIndex)
    {
        MoveItem(oldIndex, newIndex);
    }

    public BatchObservableCollection<T> DelayNotifications()
    {
        return new BatchObservableCollection<T>((null == _notifyInfo) ? this : _notifyInfo.RootCollection, true);
    }

    public BatchObservableCollection<T> DisableNotifications()
    {
        return new BatchObservableCollection<T>((null == _notifyInfo) ? this : _notifyInfo.RootCollection, false);
    }

    protected override void ClearItems()
    {
        CheckReentrancy();

        base.ClearItems();

        _fireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        var removedItem = this[index];

        base.RemoveItem(index);

        _fireCountAndIndexerChanged();
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
    }

    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();

        base.InsertItem(index, item);

        _fireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();

        var originalItem = this[index];
        base.SetItem(index, item);

        _fireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem,
            item, index));
    }

    private void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();

        var removedItem = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, removedItem);

        _fireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem,
            newIndex, oldIndex));
    }


    private void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        using (BlockReentrancy())
        {
            CollectionChanged(this, e);
        }
    }

    private IDisposable BlockReentrancy()
    {
        return _monitor.Enter();
    }

    private void CheckReentrancy()
    {
        if ((_disableReentry) && (_monitor.IsNotifying))
        {
            throw new InvalidOperationException("BatchObservableCollection Reentrancy Not Allowed");
        }
    }

    public void Dispose()
    {
        DisposeInternal();
        GC.SuppressFinalize(this);
    }

    private void DisposeInternal()
    {
        if (_notifyInfo is not { HasEventArgs: true }) return;
        if (null != _notifyInfo.RootCollection?.PropertyChanged)
        {
            if (_notifyInfo.IsCountChanged)
                _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(CountString));

            _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
        }

        using (_notifyInfo.RootCollection?.BlockReentrancy())
        {
            var args = _notifyInfo.EventArgs;

            foreach (var delegateItem in _notifyInfo.RootCollection!.CollectionChanged.GetInvocationList())
            {
                delegateItem.DynamicInvoke(_notifyInfo.RootCollection, args);
            }
        }

        CollectionChanged = _notifyInfo.Initialize();
    }

    private class ReentryMonitor : IDisposable
    {
        private int _referenceCount;

        public IDisposable Enter()
        {
            ++_referenceCount;

            return this;
        }

        public void Dispose()
        {
            --_referenceCount;
        }

        public bool IsNotifying
        {
            get { return _referenceCount != 0; }
        }
    }

    private class NotificationInfo
    {
        private NotifyCollectionChangedAction? _action;

        private IList? _newItems;

        private IList? _oldItems;

        private int _newIndex;

        private int _oldIndex;

        public NotifyCollectionChangedEventHandler Initialize()
        {
            _action = null;
            _newItems = null;
            _oldItems = null;

            return (sender, args) =>
            {
                var wrapper = sender as BatchObservableCollection<T>;
                Debug.Assert(null != wrapper, "Calling object must be BatchObservableCollection<T>");
                Debug.Assert(null != wrapper._notifyInfo, "Calling object must be Delayed wrapper.");

                _action = args.Action;

                switch (_action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _newItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (_, e) =>
                        {
                            AssertActionType(e);
                            if (e.NewItems == null) return;
                            foreach (T item in e.NewItems)
                                _newItems.Add(item);
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        _oldItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (_, e) =>
                        {
                            AssertActionType(e);
                            if (e.OldItems == null) return;
                            foreach (T item in e.OldItems)
                                _oldItems.Add(item);
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        _newItems = new List<T>();
                        _oldItems = new List<T>();
                        wrapper.CollectionChanged = (_, e) =>
                        {
                            AssertActionType(e);
                            if (e.NewItems != null)
                                foreach (T item in e.NewItems)
                                    _newItems.Add(item);

                            if (e.OldItems != null)
                                foreach (T item in e.OldItems)
                                    _oldItems.Add(item);
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Move:
                        _newIndex = args.NewStartingIndex;
                        _newItems = args.NewItems;
                        _oldIndex = args.OldStartingIndex;
                        _oldItems = args.OldItems;
                        wrapper.CollectionChanged = (_, _) =>
                        {
                            throw new InvalidOperationException(
                                "Due to design of NotifyCollectionChangedEventArgs combination of multiple Move operations is not possible");
                        };
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (_, e) => { AssertActionType(e); };
                        break;
                }
            };
        }

        public BatchObservableCollection<T>? RootCollection { get; set; }

        public bool IsCountChanged { get; private set; }

        public NotifyCollectionChangedEventArgs? EventArgs
        {
            get
            {
                switch (_action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        return new NotifyCollectionChangedEventArgs(_action.Value);

                    case NotifyCollectionChangedAction.Add:
                        return new NotifyCollectionChangedEventArgs(_action.Value, _newItems);

                    case NotifyCollectionChangedAction.Remove:
                        return new NotifyCollectionChangedEventArgs(_action.Value, _oldItems);

                    case NotifyCollectionChangedAction.Move:
                        return new NotifyCollectionChangedEventArgs(_action.Value, _oldItems?[0], _newIndex, _oldIndex);

                    case NotifyCollectionChangedAction.Replace:
                        if (_newItems != null)
                            if (_oldItems != null)
                                return new NotifyCollectionChangedEventArgs(_action.Value, _newItems, _oldItems);
                        break;
                }

                return null;
            }
        }

        public bool HasEventArgs
        {
            get { return _action.HasValue; }
        }

        private void AssertActionType(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != _action)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Attempting to perform {0} during {1}. Mixed actions on the same delayed interface are not allowed.",
                        e.Action, _action));
            }
        }
    }
}