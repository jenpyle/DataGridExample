using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using DataGridAnimation.Core.Models;
using DataGridAnimation.Core.Services;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace DataGridAnimation.Views
{
    public sealed partial class DataGridPage : Page, INotifyPropertyChanged
    {
        public SelectableObservableCollection<SampleOrder> Source { get; } = new SelectableObservableCollection<SampleOrder>();

        // TODO: Change the grid as appropriate to your app, adjust the column definitions on DataGridPage.xaml.
        // For more details see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid
        public DataGridPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Source.Clear();

            // Replace this with your actual data
            var data = await SampleDataService.GetGridDataAsync();

            foreach (var item in data)
            {
                Source.Add(item);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public void DataGridSorting(object sender, DataGridColumnEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            ListSortDirection listSortDirection = ListSortDirection.Ascending;

            if (e.Column.SortDirection == null || e.Column.SortDirection == DataGridSortDirection.Descending)
            {
                e.Column.SortDirection = DataGridSortDirection.Ascending;
            }
            else
            {
                //DataGridSortDirection is only for displaying the arrow icon
                e.Column.SortDirection = DataGridSortDirection.Descending;
                //ListSortDirection is for the actual sorting direction
                listSortDirection = ListSortDirection.Descending;
            }
            string columnTag = e.Column.Tag.ToString();

            Func<SampleOrder, IComparable> keySelector = GetKeySelector(columnTag);

            Source.Sort(keySelector, listSortDirection);

            ResetColumns(dataGrid, columnTag, e.Column.SortDirection);
        }

        private Func<SampleOrder, IComparable> GetKeySelector(string columnTag)
        {
            switch (columnTag)
            {
                case "OrderDate":
                    return experiment => experiment.OrderDate;

                case "OrderID":
                    return experiment => experiment.OrderID;

                case "OrderTotal":
                    return experiment => experiment.OrderTotal;

                case "Company":
                    return experiment => experiment.Company;

                case "ShipTo":
                    return experiment => experiment.ShipTo;

                case "Status":
                    return experiment => experiment.Status;

                case "Symbol":
                    return experiment => experiment.Symbol;

                default:
                    return null;
            }
        }

        private void ResetColumns(DataGrid dataGrid, string tag, DataGridSortDirection? sortDirection)
        {
            // Remove sorting arrow icons from other columns
            foreach (var column in dataGrid.Columns)
            {
                if (column.Tag.ToString() != tag)
                {
                    column.SortDirection = null;
                }
                else
                {
                    column.SortDirection = sortDirection;
                }
            }
        }
    }
}
