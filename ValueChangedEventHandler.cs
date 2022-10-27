using System.Diagnostics;
using System;

namespace DataGridAnimation
{
    [DebuggerDisplay("{NewValue}")]
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }

        public T NewValue { get; }
    }

    public delegate void ValueChangedEventHandler<T>(object sender, ValueChangedEventArgs<T> args);
}
