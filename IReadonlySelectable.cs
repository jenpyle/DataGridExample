namespace DataGridAnimation
{
    public interface IReadonlySelectable<T>
    {
        event ValueChangedEventHandler<T> SelectedItemChangedEvent;

        T SelectedItem { get; }
    }
}
