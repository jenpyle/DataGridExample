namespace DataGridAnimation
{
    public interface ISelectable<T> : IReadonlySelectable<T>
    {
        new T SelectedItem { get; set; }
    }
}
