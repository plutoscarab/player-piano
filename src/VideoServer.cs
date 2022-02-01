
using System;
using System.Diagnostics;

namespace pi_ano
{
    public class VideoServer
    {
        int width;
        int height;
        int port;
        CancellationTokenSource tokenSource;
        Task task;

        public VideoServer(int width, int height, int port)
        {
            this.width = width;
            this.height = height;
            this.port = port;
            tokenSource = new CancellationTokenSource();
            task = StartCamera(tokenSource.Token);
        }

        public bool IsCompleted =>
            task == null ? false : task.IsCompleted;

        public async Task Stop()
        {
            tokenSource.Cancel();
            await task;
        }

        private async Task RunCamera(CancellationToken token)
        {
            // Use libcamera to send camera frames over TCP
            var command = $"libcamera-vid --codec yuv420 --width {width} --height {height} --nopreview -t 0 --listen -o tcp://0.0.0.0:{port} >libcamera-vid.txt 2>&1";

            var camera = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    UseShellExecute = false,
                },
            };

            camera.Start();
            
            // Wait until cancelled
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
            
            // Terminate libcamera
            camera.Kill();
        }

        private async Task StartCamera(CancellationToken token)
        {
            while (true)
            {
                // Start libcamera
                var camera = RunCamera(token);
                await Task.Delay(2000);

                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Return if it started and stayed running
                if (!camera.IsCompleted)
                {
                    await camera;
                    return;
                }

                // Loop if it started and then stopped quickly (failed to bind to TCP port)
            }
        }
    }
}