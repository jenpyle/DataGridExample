using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.UI.Xaml.Controls.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataGridAnimation
{
    /// <summary>
    /// A specialized <see cref="ObservableCollection{T}"/> that provides support for tracking a 'selected' item
    /// in the list.
    /// </summary>
    /// <remarks>
    /// This class does not initially support multiple selection; but it might be enhanced or extended to do so
    /// in the future.
    /// </remarks>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    public class SelectableObservableCollection<T> : ObservableCollection<T>, ISelectableObservableList<T>
        where T : class
    {
        #region Construction

        public SelectableObservableCollection() : base()
        {
        }

        public SelectableObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SelectableObservableCollection(List<T> list) : base(list)
        {
        }

        #endregion

        #region Public

        private T selectedItem;

        /// <summary>
        /// The currently selected item in the collection, or <c>null</c> if no item is selected.
        /// </summary>
        /// <remarks>
        /// It is not strictly required that the selected item is a member of the collection.  However, some types
        /// of view respond to setting <see cref="SelectedItem"/> to an item that is not currently in the collection
        /// by re-setting the value to <c>null</c>.
        /// </remarks>
        public T SelectedItem
        {
            get => selectedItem;

            set
            {
                if (SelectionLockCount > 0)
                {
                    return;
                }

                if (Equals(selectedItem, value))
                {
                    return;
                }

                T oldValue = selectedItem;

                selectedItem = value;

                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItem)));
                SelectedItemChangedEvent?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, value));
            }
        }

        /// <summary>
        /// Raised when the value of <see cref="SelectedItem"/> has changed.
        /// </summary>
        public event ValueChangedEventHandler<T> SelectedItemChangedEvent;

        /// <summary>
        /// Prevents <see cref="SelectedItem"/> from changing value until the returned object is disposed.
        /// </summary>
        /// <remarks>
        /// The primary intended purpose for this method is to allow for the contents of the list to be updated
        /// without losing the current selection.  Until all objects returned by this method have been disposed,
        /// attempts to set <see cref="SelectedItem"/> will have no effect.
        /// <para>
        /// When the final remaining object returned by this method is disposed, the
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised for the <see cref="SelectedItem"/>
        /// property.  This notifies any subscriber that might have attempted to modify <see cref="SelectedItem"/>
        /// that the property should be restored to its former value.
        /// </para>
        /// </remarks>
        /// <returns>
        /// Returns an <see cref="IDisposable"/> object.
        /// </returns>
        public IDisposable DeferSelectedItemChange()
        {
            return new SelectionLock(this);
        }

        public IDisposable DeferSelectedItemChange(T itemToSelect)
        {
            return new SelectionLock(this, itemToSelect);
        }

        public IDisposable DeferChangeNotifications()
        {
            return new NotificationLock(this);
        }

        public virtual void Sort<TKey>(Func<T, TKey> keySelector,
            ListSortDirection direction = ListSortDirection.Ascending, bool deferNotifications = false) where TKey : IComparable
        {
            using (DeferSelectedItemChange())
            {
                using (deferNotifications ? DeferChangeNotifications() : null)
                {
                    ObservableCollectionExtensions.Sort(this, keySelector, direction);
                }
            }
        }

        public virtual void AddRange(IEnumerable<T> items, bool deferNotifications = false)
        {
            using (DeferSelectedItemChange())
            {
                using (deferNotifications ? DeferChangeNotifications() : null)
                {
                    foreach (T item in items)
                    {
                        Add(item);
                    }
                }
            }
        }

        public void UpdateContents(IEnumerable<T> items, bool deferNotifications = false)
        {
            UpdateContents(items, SelectedItem, deferNotifications);
        }

        public virtual void UpdateContents(IEnumerable<T> items, T itemToSelect, bool deferNotifications = false)
        {
            using (DeferSelectedItemChange(itemToSelect))
            {
                using (deferNotifications ? DeferChangeNotifications() : null)
                {
                    ObservableCollectionExtensions.UpdateContents(this, items);
                }
            }
        }

        public void ReplaceContents(IEnumerable<T> items, bool deferNotifications = false)
        {
            ReplaceContents(items, SelectedItem, deferNotifications);
        }

        public virtual void ReplaceContents(IEnumerable<T> items, T itemToSelect, bool deferNotifications = false)
        {
            if (ReferenceEquals(this, items))
            {
                return;
            }

            using (DeferSelectedItemChange(itemToSelect))
            {
                using (deferNotifications ? DeferChangeNotifications() : null)
                {
                    Clear();
                    AddRange(items);
                }
            }
        }

        #endregion

        #region Protected

        protected int SelectionLockCount { get; private set; }

        protected int NotificationLockCount { get; private set; }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (NotificationLockCount > 0)
            {
                return;
            }

            base.OnCollectionChanged(args);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (NotificationLockCount > 0)
            {
                return;
            }

            base.OnPropertyChanged(args);
        }

        protected override sealed void InsertItem(int index, T item)
        {
            using (DeferSelectedItemChange(SelectedItem))
            {
                base.InsertItem(index, item);
            }
        }

        protected override sealed void RemoveItem(int index)
        {
            T itemToSelect = IndexOf(SelectedItem) == index ? null : SelectedItem;

            using (DeferSelectedItemChange(itemToSelect))
            {
                base.RemoveItem(index);
            }
        }

        protected override sealed void MoveItem(int oldIndex, int newIndex)
        {
            using (DeferSelectedItemChange(SelectedItem))
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        #endregion

        #region Nested Classes

        protected class SelectionLock : Disposable
        {
            private readonly SelectableObservableCollection<T> list;
            private readonly T itemToSelect;

            public SelectionLock(SelectableObservableCollection<T> list)
                : this(list, list.SelectedItem)
            {
            }

            public SelectionLock(SelectableObservableCollection<T> list, T itemToSelect)
            {
                list.SelectionLockCount++;
                this.list = list;
                this.itemToSelect = itemToSelect;
            }

            protected override void DisposeResources()
            {
                try
                {
                    list.SelectionLockCount--;

                    if (list.SelectionLockCount == 0)
                    {
                        if (Equals(list.SelectedItem, itemToSelect))
                        {
                            // notify views to re-select the previously selected item
                            list.OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItem)));
                        }
                        else
                        {
                            // change the selected item
                            list.SelectedItem = itemToSelect;
                        }
                    }
                }
                finally
                {
                    base.DisposeResources();
                }
            }
        }

        protected class NotificationLock : Disposable
        {
            private readonly SelectableObservableCollection<T> list;

            public NotificationLock(SelectableObservableCollection<T> list)
            {
                list.NotificationLockCount++;
                this.list = list;
            }

            protected override void DisposeResources()
            {
                try
                {
                    list.NotificationLockCount--;
                    list.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
                finally
                {
                    base.DisposeResources();
                }
            }
        }

        #endregion
    }
}
