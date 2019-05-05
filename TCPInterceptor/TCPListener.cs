using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TCPInterceptor
{
    class TCPListener
    {
        private readonly int _port;
        private readonly IPAddress _target;
        private readonly Logger _logger;
        private readonly int _id;
        private static int _staticId = 0;

        public TCPListener(int port, IPAddress target, Logger logger)
        {
            _port = port == 0 ? throw new ArgumentOutOfRangeException(nameof(port)) : port;
            _target = target ?? throw new ArgumentOutOfRangeException(nameof(target));
            _logger = logger;
            _id = Interlocked.Increment(ref _staticId);
        }

        public async void Start(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return;
            System.Net.Sockets.TcpListener tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Any, _port);
            tcpListener.Start();
            cancellationToken.Register(() =>
            {
                tcpListener.Stop();
            });
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient = null;
                try
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                StartClient(cancellationToken, tcpClient);
            }

        }

        private async void StartClient(CancellationToken cancellationToken, TcpClient tcpClient)
        {
            cancellationToken.Register(() => tcpClient.Dispose());
            using (tcpClient)
            {
                try
                {
                    tcpClient.LingerState = new LingerOption(true, 5);
                    using (var outClient = new TcpClient())
                    {
                        cancellationToken.Register(() => outClient.Dispose());
                        outClient.LingerState = new LingerOption(true, 5);
                        await outClient.ConnectAsync(_target, _port);

                        var stream1 = tcpClient.GetStream();
                        var stream2 = tcpClient.GetStream();
                        var dir1 = CopyStream(true, stream1, stream2, cancellationToken);
                        var dir2 = CopyStream(false, stream2, stream1, cancellationToken);
                        await Task.WhenAny(dir1, dir2);

                        outClient.Close();
                    }
                    tcpClient.Close();
                }
                catch
                {

                }
            }
        }

        private async Task CopyStream(bool outbound, NetworkStream inStream, NetworkStream outStream, CancellationToken cancellationToken)
        {
            DelayedSemaphore sync = _logger?.GetDelayedSemaphore();
            byte[] buffer = new byte[65536];
            while (true)
            {
                int bytesRead = await inStream.ReadAsync(buffer, 0, 65536, cancellationToken);
                if (bytesRead == 0) return;
                if (sync != null)
                {
                    await (sync.Wait(cancellationToken));
                    try
                    {
                        await _logger.Log(_id, _port, outbound, buffer, 0, bytesRead);
                    }
                    finally
                    {
                        sync.Release();
                    }
                }
                await outStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            }
        }
    }
}
