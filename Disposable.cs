using System.Runtime.CompilerServices;
using System.Threading;

namespace DataGridAnimation
{
    public sealed class AtomicBoolean
    {
        private static int FalseAsInt => 0;

        private static int TrueAsInt => 1;

        public AtomicBoolean()
            : this(false)
        {
        }

        public AtomicBoolean(bool value)
        {
            valueAsInt = ToInt(value);
        }

        /// <summary>
        /// Reads and sets the value as an atomic operation.
        /// </summary>
        /// <param name="value">
        /// The new value.
        /// </param>
        /// <returns>
        /// Returns <c>true</c> if and only if the value was modified.  Returns <c>false</c> if no change
        /// was made because the value was already equal to <paramref name="value"/>.
        /// </returns>
        public bool SetValue(bool value)
        {
            return value != ToBool(Interlocked.Exchange(ref valueAsInt, ToInt(value)));
        }

        /// <summary>
        /// The current value.
        /// </summary>
        public bool Value => ToBool(valueAsInt);

        private volatile int valueAsInt;

        private int ToInt(bool value) => value ? TrueAsInt : FalseAsInt;

        private bool ToBool(int value) => value == TrueAsInt;
    }

    /// <summary>
    /// Abstract base class providing an implementation of the 'Dispose' pattern.
    /// </summary>
    /// <seealso cref="DisposablePropertyChangedNotifier"/>
    /// <seealso cref="AsyncDisposable"/>
    public abstract class Disposable : System.IDisposable
    {
        protected Disposable()
        {
        }

        /// <summary>
        /// Determines whether <see cref="Dispose()"/> has been invoked on this object.
        /// </summary>
        public bool IsDisposed => isDisposed.Value;

        private readonly AtomicBoolean isDisposed = new AtomicBoolean();

        public void Dispose()
        {
            if (!isDisposed.SetValue(true))
            {
                return;
            }

            DisposeResources();
        }

        /// <summary>
        /// Derived classes should override this method to release managed resources.
        /// </summary>
        /// <remarks>
        /// <see cref="IsDisposed"/> will be set to <c>true</c> immediately before this method is invoked.
        /// <para>
        /// The recommended pattern for overriding this method is to use a <c>try/finally</c> statement.  Release
        /// managed resources within the <c>try</c> block, and then call <c>base.DisposeResources</c> within
        /// the <c>finally</c> block.
        /// </para>
        /// </remarks>
        protected virtual void DisposeResources()
        {
            // null implementation
        }

        /// <summary>
        /// Throws <see cref="ObjectDisposedException"/> if this object has been disposed.
        /// </summary>
        /// <remarks>
        /// Derived classes may optionally call this method at the beginning of non-private methods in order
        /// to disallow using an instance after it has been disposed.
        /// </remarks>
        protected virtual void ThrowExceptionIfDisposed(
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            if (IsDisposed)
            {
                throw new System.ObjectDisposedException(GetType().FullName, "lalala");
            }
        }
    }
}
