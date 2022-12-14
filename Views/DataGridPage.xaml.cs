using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using DataGridAnimation.Core.Models;
using DataGridAnimation.Core.Services;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.Primitives;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace DataGridAnimation.Views
{
    public sealed partial class DataGridPage : Page, INotifyPropertyChanged
    {
        public SelectableObservableCollection<SampleOrder> Source { get; set; } = new SelectableObservableCollection<SampleOrder>();
        public ObservableCollection<SampleOrder> Backup { get; set; } = new ObservableCollection<SampleOrder>();

        public ObservableCollection<SampleOrder> SourceDP
        {
            get { return (ObservableCollection<SampleOrder>)GetValue(SourceDPProperty); }
            set { this.SetValue(SourceDPProperty, value); }
        }

        public static readonly DependencyProperty SourceDPProperty = DependencyProperty.Register(
            nameof(SourceDP), typeof(ObservableCollection<SampleOrder>), typeof(DataGridPage), new PropertyMetadata(default(ObservableCollection<SampleOrder>)));

        // TODO: Change the grid as appropriate to your app, adjust the column definitions on DataGridPage.xaml.
        // For more details see the documentation at https://docs.microsoft.com/windows/communitytoolkit/controls/datagrid
        public DataGridPage()
        {
            InitializeComponent();
            this.DataContext = this;
            SourceDP = new ObservableCollection<SampleOrder>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Source.Clear();
            Backup = new ObservableCollection<SampleOrder>(Source);
            SourceDP.Clear();

            // Replace this with your actual data
            var data = await SampleDataService.GetGridDataAsync();

            foreach (var item in data)
            {
                SourceDP.Add(item);
                Source.Add(item);
                Backup = new ObservableCollection<SampleOrder>(Source);
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
            Backup = new ObservableCollection<SampleOrder>(Source);
            //Backup.Sort(keySelector, listSortDirection);
            SourceDP.Sort(keySelector, listSortDirection);

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

        public bool ToggleValue { get; set; }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SampleOrder newOrder = new SampleOrder();
            newOrder.OrderDate = DateTime.Now;
            newOrder.OrderTotal = 0;
            newOrder.SymbolCode = 57619;
            newOrder.Status = "Pending";
            newOrder.ShipTo = "Beverly Hills, CA";
            newOrder.Company = "Thermo Fisher Scientific";
            Source.Add(newOrder);
            Backup = new ObservableCollection<SampleOrder>(Source);
            SourceDP.Add(newOrder);
        }

        public int PresenterHeight
        {
            get { return (int)GetValue(PresenterHeightProperty); }
            set { SetValue(PresenterHeightProperty, value); }
        }

        public static readonly DependencyProperty PresenterHeightProperty = DependencyProperty.Register(
            nameof(PresenterHeight), typeof(int), typeof(DataGridPage), new PropertyMetadata(50));

        private void CellsPresenter_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var x = sender as DataGridCellsPresenter;
            if (ToggleValue)
            {
                x.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
                PresenterHeight = 10;
            }
            else
            {
                x.Background = new SolidColorBrush(Windows.UI.Colors.MediumVioletRed);
                PresenterHeight = 50;
            }
            ToggleValue = !ToggleValue;
        }

        public static T FindControl<T>(UIElement parent, Type targetType, string ControlName) where T : FrameworkElement
        {
            if (parent == null) return null;

            if (parent.GetType() == targetType && ((T)parent).Name == ControlName)
            {
                return (T)parent;
            }
            T result = null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                UIElement child = (UIElement)VisualTreeHelper.GetChild(parent, i);

                if (FindControl<T>(child, targetType, ControlName) != null)
                {
                    result = FindControl<T>(child, targetType, ControlName);
                    break;
                }
            }
            return result;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Source.RemoveAt(0);

            SourceDP.RemoveAt(0);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            DataGridCellsPresenter dataGridCellsPresenter = FindControl<DataGridCellsPresenter>(this, typeof(DataGridCellsPresenter), "CellsPresenter");

            Source.Clear();
            SourceDP.Clear();

            Backup.Clear();
            //Source.UpdateContents(Source);
            //Backup.UpdateContents(Source);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Source.UpdateContents(Source);
            Backup = new ObservableCollection<SampleOrder>();
            Backup = new ObservableCollection<SampleOrder>(Source);
            SourceDP.UpdateContents(SourceDP);
        }

        public async void RefillList()
        {
            var data = await SampleDataService.GetGridDataAsync();

            foreach (var item in data)
            {
                SourceDP.Add(item);
                Source.Add(item);
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            RefillList();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Source.UpdateContents(Source);
            SourceDP = new ObservableCollection<SampleOrder>();
            SourceDP.UpdateContents(Source);
        }
    }
}
