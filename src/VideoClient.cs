
using System;
using System.Net;
using System.Net.Sockets;

namespace pi_ano
{
    public delegate void FrameUpdate(ref byte[] frame);

    public class VideoClient
    {
        int port;
        int bufferSize;
        CancellationTokenSource tokenSource;
        Task task;

        public VideoClient(int width, int height, int port, FrameUpdate frameHandler)
        {
            bufferSize = (width * height * 3) / 2;
            this.port = port;
            tokenSource = new CancellationTokenSource();
            task = StartWatcher(frameHandler, tokenSource.Token);
        }

        public async Task Stop()
        {
            tokenSource.Cancel();
            await task;
        }

        private async Task<TcpClient> GetConnection()
        {
            var tcp = new TcpClient();
            var endpoint = new IPEndPoint(IPAddress.Loopback, port);

            while (true)
            {
                try
                {
                    tcp.Connect(endpoint);
                    return tcp;
                }
                catch (Exception ex) when (ex.Message.Contains("Connection refused"))
                {
                    // libcamera wasn't ready yet
                    await Task.Delay(137);
                }
            }
        }

        private static async Task ReadFrame(Stream stream, byte[] frame, CancellationToken token)
        {
            // Read from TCP stream until buffer is filled
            var offset = 0;

            while (offset < frame.Length)
            {
                try
                {
                    var read = await stream.ReadAsync(frame, offset, frame.Length - offset, token); 
                    offset += read;

                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Exit early if we are terminating
                    return;
                }
            }
        }

        private async Task StartWatcher(FrameUpdate frameHandler, CancellationToken token)
        {
            // Allocate frame buffer
            var frame = new byte[bufferSize];

            using (var tcp = await GetConnection())
            using (var stream = tcp.GetStream())
            {
                while (true)
                {
                    await ReadFrame(stream, frame, token);

                    if (token.IsCancellationRequested)
                    {
                        break;
                    }

                    frameHandler(ref frame);
                }
            }
        }
    }
}