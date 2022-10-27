using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridAnimation
{
    /// <summary>
    /// Extension methods for type <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Sorts the contents of a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Type of item in the collection.
        /// </typeparam>
        /// <param name="collection">
        /// A collection.
        /// </param>
        /// <param name="direction">
        /// Sort direction.  The default value is <see cref="ListSortDirection.Ascending"/>.
        /// </param>
        /// <returns>
        /// Returns the original collection, with its contents sorted.
        /// </returns>
        public static ObservableCollection<T> Sort<T>(
            this ObservableCollection<T> collection,
            ListSortDirection direction = ListSortDirection.Ascending)
            where T : IComparable
        {
            return collection.Sort(null, direction);
        }

        /// <summary>
        /// Sorts the contents of a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Type of item in the collection.
        /// </typeparam>
        /// <param name="collection">
        /// A collection.
        /// </param>
        /// <param name="comparer">
        /// A comparer for determining the relative order of items in the collection.  If <c>null</c>
        /// <see cref="Comparer{T}.Default"/> is used.
        /// </param>
        /// <param name="direction">
        /// Sort direction.  The default value is <see cref="ListSortDirection.Ascending"/>.
        /// </param>
        /// <returns>
        /// Returns the original collection, with its contents sorted.
        /// </returns>
        public static ObservableCollection<T> Sort<T>(
            this ObservableCollection<T> collection,
            IComparer<T> comparer,
            ListSortDirection direction = ListSortDirection.Ascending)
        {
            return collection.Sort(
                item => item,
                comparer,
                direction);
        }

        /// <summary>
        /// Sorts the contents of a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Type of item in the collection.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// Type of object used for comparing items in the collection.
        /// </typeparam>
        /// <param name="collection">
        /// A collection.
        /// </param>
        /// <param name="keySelector">
        /// For each item in the collection, returns an object to be used for comparing the items for
        /// relative order.  For example, to sort the items based on the value of a property named "Name",
        /// return the value of the "Name" property.
        /// </param>
        /// <param name="direction">
        /// Sort direction.  The default value is <see cref="ListSortDirection.Ascending"/>.
        /// </param>
        /// <returns>
        /// Returns the original collection, with its contents sorted.
        /// </returns>
        public static ObservableCollection<T> Sort<T, TKey>(
            this ObservableCollection<T> collection,
            Func<T, TKey> keySelector,
            ListSortDirection direction = ListSortDirection.Ascending)
            where TKey : IComparable
        {
            return collection.Sort(
                keySelector,
                null,
                direction);
        }

        /// <summary>
        /// Sorts the contents of a collection.
        /// </summary>
        /// <typeparam name="T">
        /// Type of item in the collection.
        /// </typeparam>
        /// <typeparam name="TKey">
        /// Type of object used for comparing items in the collection.
        /// </typeparam>
        /// <param name="collection">
        /// A collection.
        /// </param>
        /// <param name="keySelector">
        /// For each item in the collection, returns an object to be used for comparing the items for
        /// relative order.  For example, to sort the items based on the value of a property named "Name",
        /// return the value of the "Name" property.
        /// </param>
        /// <param name="comparer">
        /// A comparer for determining the relative order of items in the collection.  If <c>null</c>
        /// <see cref="Comparer{TKey}.Default"/> is used.
        /// </param>
        /// <param name="direction">
        /// Sort direction.  The default value is <see cref="ListSortDirection.Ascending"/>.
        /// </param>
        /// <returns>
        /// Returns the original collection, with its contents sorted.
        /// </returns>
        public static ObservableCollection<T> Sort<T, TKey>(
            this ObservableCollection<T> collection,
            Func<T, TKey> keySelector,
            IComparer<TKey> comparer,
            ListSortDirection direction = ListSortDirection.Ascending)
        {
            comparer = comparer ?? Comparer<TKey>.Default;

            IEnumerable<T> sortedCollection = direction == ListSortDirection.Ascending
                ? collection.OrderBy(keySelector, comparer)
                : collection.OrderByDescending(keySelector, comparer);

            collection.UpdateContents(sortedCollection.ToArray());

            return collection;
        }

        /// <summary>
        /// Updates the contents of a collection to be the same as another list of elements.
        /// </summary>
        /// <remarks>
        /// The collection is updated to have the same contents as <see cref="elements"/>, in the same
        /// order.  If the contents are already the same, no changes are made.
        /// </remarks>
        /// <typeparam name="T">
        /// Type of element in the collection.
        /// </typeparam>
        /// <param name="collection">
        /// The collection to update.
        /// </param>
        /// <param name="elements">
        /// The list of elements that the collection should be updated to match.
        /// </param>
        /// <returns>
        /// Returns the original collection, with its contents updated to be the same as
        /// <paramref name="elements"/>.
        /// </returns>
        public static ObservableCollection<T> UpdateContents<T>(
            this ObservableCollection<T> collection,
            IEnumerable<T> elements)
        {
            // insert or move items, matching order of 'elements'
            int index = 0;

            foreach (T element in elements)
            {
                int foundIndex = collection.IndexOf(element, index);

                if (foundIndex == -1)
                    collection.Insert(index, element);
                else if (foundIndex != index)
                    collection.Move(foundIndex, index);

                index++;
            }

            int elementCount = index;

            // remove extra items (those not in 'elements')
            for (index = collection.Count - 1; index > elementCount - 1; index--)
                collection.RemoveAt(index);

            return collection;
        }

        public static ObservableCollection<T> AddRange<T>(
            this ObservableCollection<T> @this,
            IEnumerable<T> elements)
        {
            foreach (T element in elements)
            {
                @this.Add(element);
            }

            return @this;
        }
    }
}
