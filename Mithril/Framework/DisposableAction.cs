using System;

namespace Mithril
{
    public sealed class DisposableAction : IDisposable
    {
        private readonly Action _action;
        private Boolean _isCanceled;
        private Boolean _isSafe;

        public DisposableAction(Action action, Boolean isSafe = false)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _isSafe = isSafe;
        }

        public void Dispose()
        {
            try
            {
                if (!_isCanceled)
                    _action();
            }
            catch
            {
                if (!_isSafe)
                    throw;
            }
        }

        public void Cancel()
        {
            _isCanceled = true;
        }
    }
}