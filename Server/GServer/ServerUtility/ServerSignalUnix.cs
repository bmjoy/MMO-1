
  using System;
  using System.Threading.Tasks;
#if MONO
  using Mono.Unix;
#endif

namespace ServerUtility
{
    public class UnixExitSignal
    {
        public event EventHandler Exit;
#if MONO
        readonly UnixSignal[] signals = new UnixSignal[]{
        new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
        new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
        new UnixSignal(Mono.Unix.Native.Signum.SIGUSR1)
        };
#endif

        public Task CurrentWait { private set; get; }

        public UnixExitSignal()
        {
            CurrentWait= Task.Factory.StartNew(() =>
            {
#if MONO
                // blocking call to wait for any kill signal
                UnixSignal.WaitAny(signals, -1);
                Exit?.Invoke(null, EventArgs.Empty);
#endif
            });
        }
    }
}

