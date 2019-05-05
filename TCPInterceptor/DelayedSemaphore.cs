using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPInterceptor
{
    class DelayedSemaphore
    {
        private readonly SemaphoreSlim _semaphoreSlim;
        private bool _stillHeld = false;
        private readonly Timer _timer;
        private readonly int _timerDelay;
        private readonly object _relaseSync = new object();

        public DelayedSemaphore(SemaphoreSlim semaphoreSlim, int timerDelay)
        {
            _semaphoreSlim = semaphoreSlim;
            _timer = new Timer(new TimerCallback(_timerCallback));
            _timerDelay = timerDelay;
        }

        private void _timerCallback(object state)
        {
            lock (_relaseSync)
            {
                if (_stillHeld)
                {
                    _semaphoreSlim.Release();
                    _stillHeld = false;
                }
            }
        }

        public Task Wait()
        {
            if (_stillHeld)
            {
                _stillHeld = false;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                return Task.CompletedTask;
            }
            else
            {
                return _semaphoreSlim.WaitAsync();
            }
        }

        public Task Wait(CancellationToken cancellationToken)
        {
            if (_stillHeld)
            {
                _stillHeld = false;
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
                return Task.CompletedTask;
            }
            else
            {
                return _semaphoreSlim.WaitAsync(cancellationToken);
            }
        }

        public void Release()
        {
            lock (_relaseSync)
            {
                if (!_stillHeld)
                {
                    _stillHeld = true;
                    _timer.Change(_timerDelay, Timeout.Infinite);
                }
                else
                {
                    _semaphoreSlim.Release();
                }
            }
        }
    }
}
