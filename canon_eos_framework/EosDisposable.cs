using System;
using Canon.Eos.Framework.Interfaces;

namespace Canon.Eos.Framework
{
    public abstract class EosDisposable : IDisposable, IEosAssertable
    {
        ~EosDisposable()
        {
            Dispose(false);
        }

        internal protected void CheckDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        internal protected bool IsDisposed { get; private set; }

        internal protected virtual void DisposeManaged() { }

        internal protected virtual void DisposeUnmanaged() { }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                    DisposeManaged();
                DisposeUnmanaged();
                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
