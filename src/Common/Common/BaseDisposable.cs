/* 
 * BaseDisposable
 * Author: Weifeng Xie
 * 
 */

namespace OrderProcessing.Domain
{
    using System;
    /// <summary>
    /// This class is used for safely releasing managed and unmanaged resource.
    /// </summary>
    public abstract class BaseDisposable : IDisposable
    {
        protected bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        ~BaseDisposable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    Disposing();
                }
            }
            isDisposed = true;
        }

        protected abstract void Disposing();
    }
}