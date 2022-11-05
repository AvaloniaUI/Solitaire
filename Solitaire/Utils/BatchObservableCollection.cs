using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace Solitaire.Utils;

/// <summary>
/// Observable collection with ability to delay or suspend CollectionChanged notifications
/// </summary>
/// <typeparam name="T"></typeparam>
public class BatchObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged,
    IDisposable
{
    //-----------------------------------------------------
    //  Constants
    //-----------------------------------------------------

    #region Constants

    private const string _countString = "Count";

    // This must agree with Binding.IndexerName.  It is declared separately 
    // here so as to avoid a dependency on PresentationFramework.dll. 
    private const string _indexerName = "Item[]";

    /// <summary>
    /// Empty delegate used to initialize <see cref="CollectionChanged"/> event if it is empty
    /// </summary>
    private static readonly NotifyCollectionChangedEventHandler _emptyDelegate = delegate { };

    #endregion

    //-----------------------------------------------------
    //  Private Fields 
    //----------------------------------------------------- 

    #region Private Fields

    /// <summary>
    /// 
    /// </summary>
    private ReentryMonitor _monitor = new ReentryMonitor();

    /// <summary>
    /// Placeholder for all data related to delayed 
    /// notifications.
    /// </summary>
    private NotificationInfo _notifyInfo;

    /// <summary>
    /// Indicates if modification of container allowed during change notification.
    /// </summary>
    private bool _disableReentry;


    Action FireCountAndIndexerChanged = delegate { };


    Action FireIndexerChanged = delegate { };

    #endregion Private Fields

    //-----------------------------------------------------
    //  Protected Fields 
    //----------------------------------------------------- 

    #region Protected Fields

    /// <summary> 
    /// PropertyChanged event <see cref="INotifyPropertyChanged" />.
    /// </summary> 
    protected virtual event PropertyChangedEventHandler PropertyChanged;

    /// <summary> 
    /// Occurs when the collection changes, either by adding or removing an item.
    /// </summary>
    /// <remarks>See <seealso cref="INotifyCollectionChanged"/></remarks>
    protected virtual event NotifyCollectionChangedEventHandler CollectionChanged = _emptyDelegate;

    #endregion Protected Fields

    //-----------------------------------------------------
    //  Constructors
    //-----------------------------------------------------

    #region Constructors

    /// <summary> 
    /// Initializes a new instance of BatchObservableCollection that is empty and has default initial capacity. 
    /// </summary>
    public BatchObservableCollection()
        : base()
    {
    }

    /// <summary>
    /// Initializes a new instance of the BatchObservableCollection class
    /// that contains elements copied from the specified list 
    /// </summary>
    /// <param name="list">The list whose elements are copied to the new list.</param> 
    /// <remarks> 
    /// The elements are copied onto the BatchObservableCollection in the
    /// same order they are read by the enumerator of the list. 
    /// </remarks>
    /// <exception cref="ArgumentNullException"> list is a null reference </exception>
    public BatchObservableCollection(List<T> list)
        : base((list != null) ? new List<T>(list.Count) : list)
    {
        foreach (T item in list)
        {
            Items.Add(item);
        }
    }

    /// <summary>
    /// Initializes a new instance of the ObservableCollection class that contains 
    /// elements copied from the specified collection and has sufficient capacity 
    /// to accommodate the number of elements copied.
    /// </summary> 
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    /// <remarks>
    /// The elements are copied onto the ObservableCollection in the
    /// same order they are read by the enumerator of the collection. 
    /// </remarks>
    /// <exception cref="ArgumentNullException"> collection is a null reference </exception> 
    public BatchObservableCollection(IEnumerable<T> collection)
    {
        if (collection == null)
            throw new ArgumentNullException("collection");

        using (IEnumerator<T> enumerator = collection.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                Items.Add(enumerator.Current);
            }
        }
    }

    /// <summary>
    /// Constructor that configures the container to delay or disable notifications.
    /// </summary>
    /// <param name="parent">Reference to an original collection whos events are being postponed</param>
    /// <param name="notify">Specifies if notifications needs to be delayed or disabled</param>
    public BatchObservableCollection(BatchObservableCollection<T> parent, bool notify)
        : base(parent.Items)
    {
        _notifyInfo = new NotificationInfo();
        _notifyInfo.RootCollection = parent;

        if (notify)
        {
            CollectionChanged = _notifyInfo.Initialize();
        }
    }

    /// <summary>
    /// Distructor
    /// </summary>
    ~BatchObservableCollection()
    {
        Dispose(false);
    }

    #endregion Constructors

    //------------------------------------------------------ 
    //  Public Events 
    //------------------------------------------------------

    #region Public Events

    #region INotifyPropertyChanged implementation

    /// <summary> 
    /// PropertyChanged event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add
        {
            if (null == _notifyInfo)
            {
                if (null == PropertyChanged)
                {
                    FireCountAndIndexerChanged = delegate
                    {
                        OnPropertyChanged(new PropertyChangedEventArgs(_countString));
                        OnPropertyChanged(new PropertyChangedEventArgs(_indexerName));
                    };
                    FireIndexerChanged = delegate { OnPropertyChanged(new PropertyChangedEventArgs(_indexerName)); };
                }

                PropertyChanged += value;
            }
            else
                _notifyInfo.RootCollection.PropertyChanged += value;
        }

        remove
        {
            if (null == _notifyInfo)
            {
                PropertyChanged -= value;

                if (null == PropertyChanged)
                {
                    FireCountAndIndexerChanged = delegate { };
                    FireIndexerChanged = delegate { };
                }
            }
            else
                _notifyInfo.RootCollection.PropertyChanged -= value;
        }
    }

    #endregion INotifyPropertyChanged implementation

    #region INotifyCollectionChanged implementation

    /// <summary> 
    /// Occurs when the collection changes, either by adding or removing an item.
    /// </summary>
    /// <remarks>
    /// see <seealso cref="INotifyCollectionChanged"/> 
    /// </remarks>
    event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
    {
        add
        {
            if (null == _notifyInfo)
            {
                // Remove ballast delegate if necessary
                if (1 == CollectionChanged.GetInvocationList().Length)
                    CollectionChanged -= _emptyDelegate;

                CollectionChanged += value;
                _disableReentry = CollectionChanged.GetInvocationList().Length > 1;
            }
            else
                _notifyInfo.RootCollection.CollectionChanged += value;
        }

        remove
        {
            if (null == _notifyInfo)
            {
                CollectionChanged -= value;

                if ((null == CollectionChanged) || (0 == CollectionChanged.GetInvocationList().Length))
                    CollectionChanged += _emptyDelegate;

                _disableReentry = CollectionChanged.GetInvocationList().Length > 1;
            }
            else
                _notifyInfo.RootCollection.CollectionChanged -= value;
        }
    }

    #endregion INotifyCollectionChanged implementation

    #endregion Public Events

    //------------------------------------------------------ 
    //  Public Methods
    //-----------------------------------------------------

    #region Public Methods

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Move item at oldIndex to newIndex. 
    /// </summary> 
    public void Move(int oldIndex, int newIndex)
    {
        MoveItem(oldIndex, newIndex);
    }

    /// <summary>
    /// Returns an instance of <see cref="BatchObservableCollection<T>"/>
    /// class which manipulates original collection but suppresses notifications
    /// untill this instance has been released and Dispose() method has been called.
    /// To supress notifications it is recommended to use this instance inside 
    /// using() statement:
    /// <code>
    ///         using (var iSuppressed = collection.DelayNotifications()) 
    ///         {
    ///             iSuppressed.Add(x); 
    ///             iSuppressed.Add(y); 
    ///             iSuppressed.Add(z); 
    ///         } 
    /// </code>
    /// Each delayed interface is bound to only one type of operation such as Add, Remove, etc.
    /// Different types of operation on the same delayed interface are not allowed. In order to
    /// do other type of opertaion you can allocate another wrapper by calling .DelayNotifications() on
    /// either original object or any delayed instances.
    /// </summary>
    /// <returns><see cref="BatchObservableCollection<T>"/></returns>
    public BatchObservableCollection<T> DelayNotifications()
    {
        return new BatchObservableCollection<T>((null == _notifyInfo) ? this : _notifyInfo.RootCollection, true);
    }

    /// <summary>
    /// Returns a wrapper instance of an BatchObservableCollection class.
    /// Calling methods of this instance will modify original collection
    /// but will not generate any notifications.
    /// </summary>
    /// <returns><see cref="BatchObservableCollection<T>"/></returns>
    public BatchObservableCollection<T> DisableNotifications()
    {
        return new BatchObservableCollection<T>((null == _notifyInfo) ? this : _notifyInfo.RootCollection, false);
    }

    #endregion Public Methods

    //-----------------------------------------------------
    //  Protected Methods
    //----------------------------------------------------- 

    #region Protected Methods

    /// <summary>
    /// Called by base class Collection&lt;T&gt; when the list is being cleared;
    /// raises a CollectionChanged event to any listeners. 
    /// </summary>
    protected override void ClearItems()
    {
        CheckReentrancy();

        base.ClearItems();

        FireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <summary> 
    /// Called by base class Collection&lt;T&gt; when an item is removed from list; 
    /// raises a CollectionChanged event to any listeners.
    /// </summary> 
    protected override void RemoveItem(int index)
    {
        CheckReentrancy();
        T removedItem = this[index];

        base.RemoveItem(index);

        FireCountAndIndexerChanged();
        OnCollectionChanged(
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItem, index));
    }

    /// <summary> 
    /// Called by base class Collection&lt;T&gt; when an item is added to list;
    /// raises a CollectionChanged event to any listeners. 
    /// </summary> 
    protected override void InsertItem(int index, T item)
    {
        CheckReentrancy();

        base.InsertItem(index, item);

        FireCountAndIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
    }

    /// <summary> 
    /// Called by base class Collection&lt;T&gt; when an item is set in list;
    /// raises a CollectionChanged event to any listeners.
    /// </summary>
    protected override void SetItem(int index, T item)
    {
        CheckReentrancy();

        T originalItem = this[index];
        base.SetItem(index, item);

        FireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, originalItem,
            item, index));
    }

    /// <summary>
    /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list; 
    /// raises a CollectionChanged event to any listeners. 
    /// </summary>
    protected virtual void MoveItem(int oldIndex, int newIndex)
    {
        CheckReentrancy();

        T removedItem = this[oldIndex];
        base.RemoveItem(oldIndex);
        base.InsertItem(newIndex, removedItem);

        FireIndexerChanged();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, removedItem,
            newIndex, oldIndex));
    }


    /// <summary>
    /// Raises a PropertyChanged event (per <see cref="INotifyPropertyChanged" />). 
    /// </summary> 
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged(this, e);
    }

    /// <summary> 
    /// Raise CollectionChanged event to any listeners.
    /// Properties/methods modifying this ObservableCollection will raise 
    /// a collection changed event through this virtual method. 
    /// </summary>
    /// <remarks> 
    /// When overriding this method, either call its base implementation
    /// or call <see cref="BlockReentrancy"/> to guard against reentrant collection changes.
    /// </remarks>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        using (BlockReentrancy())
        {
            CollectionChanged(this, e);
        }
    }

    /// <summary> 
    /// Disallow reentrant attempts to change this collection. E.g. a event handler 
    /// of the CollectionChanged event is not allowed to make changes to this collection.
    /// </summary> 
    /// <remarks>
    /// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:
    /// <code>
    ///         using (BlockReentrancy()) 
    ///         {
    ///             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index)); 
    ///         } 
    /// </code>
    /// </remarks> 
    protected IDisposable BlockReentrancy()
    {
        return _monitor.Enter();
    }

    /// <summary> Check and assert for reentrant attempts to change this collection. </summary> 
    /// <exception cref="InvalidOperationException"> raised when changing the collection
    /// while another collection change is still being notified to other listeners </exception> 
    protected void CheckReentrancy()
    {
        // we can allow changes if there's only one listener - the problem
        // only arises if reentrant changes make the original event args 
        // invalid for later listeners.  This keeps existing code working 
        // (e.g. Selector.SelectedItems).
        if ((_disableReentry) && (_monitor.IsNotifying))
        {
            throw new InvalidOperationException("BatchObservableCollection Reentrancy Not Allowed");
        }
    }

    #endregion Protected Methods

    //-----------------------------------------------------
    //  IDisposable 
    //------------------------------------------------------ 

    #region IDisposable

    /// <summary>
    /// Called by the application code to fire all delayed notifications.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Fires notification with all accumulated events
    /// </summary>
    /// <param name="reason">True is called by App code. False if called from GC.</param>
    protected virtual void Dispose(bool reason)
    {
        // Fire delayed notifications
        if (null != _notifyInfo)
        {
            if (_notifyInfo.HasEventArgs)
            {
                if (null != _notifyInfo.RootCollection.PropertyChanged)
                {
                    if (_notifyInfo.IsCountChanged)
                        _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(_countString));

                    _notifyInfo.RootCollection.OnPropertyChanged(new PropertyChangedEventArgs(_indexerName));
                }

                using (_notifyInfo.RootCollection.BlockReentrancy())
                {
                    NotifyCollectionChangedEventArgs args = _notifyInfo.EventArgs;

                    foreach (Delegate delegateItem in _notifyInfo.RootCollection.CollectionChanged.GetInvocationList())
                    {
                        try
                        {
                            delegateItem.DynamicInvoke(new object[] { _notifyInfo.RootCollection, args });
                        }
                        catch (TargetInvocationException e)
                        {
                            // if ((e.InnerException is NotSupportedException) && (delegateItem.Target is ICollectionView))
                            // {
                            //     (delegateItem.Target as ICollectionView).Refresh();
                            // }
                            // else
                            throw;
                        }
                    }
                }

                // Reset and reuse if necessary
                CollectionChanged = _notifyInfo.Initialize();
            }
        }
    }

    #endregion

    //-----------------------------------------------------
    //  Private Types 
    //------------------------------------------------------ 

    #region Private Types

    [Serializable()]
    private class ReentryMonitor : IDisposable
    {
        #region Fields

        int _referenceCount;

        #endregion

        #region Methods

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

        #endregion
    }

    private class NotificationInfo
    {
        #region Fields

        private Nullable<NotifyCollectionChangedAction> _action;

        private IList _newItems;

        private IList _oldItems;

        private int _newIndex;

        private int _oldIndex;

        #endregion

        #region Methods

        public NotifyCollectionChangedEventHandler Initialize()
        {
            _action = null;
            _newItems = null;
            _oldItems = null;

            return (sender, args) =>
            {
                BatchObservableCollection<T> wrapper = sender as BatchObservableCollection<T>;
                Debug.Assert(null != wrapper, "Calling object must be BatchObservableCollection<T>");
                Debug.Assert(null != wrapper._notifyInfo, "Calling object must be Delayed wrapper.");

                // Setup 
                _action = args.Action;

                switch (_action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _newItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            foreach (T item in e.NewItems)
                                _newItems.Add(item);
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        _oldItems = new List<T>();
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            foreach (T item in e.OldItems)
                                _oldItems.Add(item);
                        };
                        wrapper.CollectionChanged(sender, args);
                        break;

                    case NotifyCollectionChangedAction.Replace:
                        _newItems = new List<T>();
                        _oldItems = new List<T>();
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            AssertActionType(e);
                            foreach (T item in e.NewItems)
                                _newItems.Add(item);

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
                        wrapper.CollectionChanged = (s, e) =>
                        {
                            throw new InvalidOperationException(
                                "Due to design of NotifyCollectionChangedEventArgs combination of multiple Move operations is not possible");
                        };
                        break;

                    case NotifyCollectionChangedAction.Reset:
                        IsCountChanged = true;
                        wrapper.CollectionChanged = (s, e) => { AssertActionType(e); };
                        break;
                }
            };
        }

        #endregion

        #region Properties

        public BatchObservableCollection<T> RootCollection { get; set; }

        public bool IsCountChanged { get; private set; }

        public NotifyCollectionChangedEventArgs EventArgs
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
                        return new NotifyCollectionChangedEventArgs(_action.Value, _oldItems[0], _newIndex, _oldIndex);

                    case NotifyCollectionChangedAction.Replace:
                        return new NotifyCollectionChangedEventArgs(_action.Value, _newItems, _oldItems);
                }

                return null;
            }
        }

        public bool HasEventArgs
        {
            get { return _action.HasValue; }
        }

        #endregion

        #region Private Helper Methods

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

        #endregion
    }

    #endregion Private Types
}