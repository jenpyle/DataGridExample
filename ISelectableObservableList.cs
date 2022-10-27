using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DataGridAnimation
{
    public interface ISelectableObservableList<T> : ISelectable<T>, IList<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
    }
}
