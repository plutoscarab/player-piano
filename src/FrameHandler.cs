
using System;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace pi_ano
{
    public class FrameHandler
    {
        int width;
        int height;
        int yBufferSize;
        int cBufferSize;
        byte[] lastFrame;
        int frameCount;
        FPS fps;
        DateTime nextMotion;
        SKSurface composite;
        int incidentCount;

        public FrameHandler(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.yBufferSize = width * height;
            this.cBufferSize = (width / 2) * (height / 2);
            this.fps = new FPS();
            this.lastFrame = new byte[yBufferSize + 2 * cBufferSize];
            this.composite = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888));
            this.nextMotion = DateTime.Now;
        }

        public void Update(ref byte[] frame)
        {
            frameCount++;
            fps.Update();

            if (DateTime.Now > nextMotion)
            {
                var count = 0.0;

                for (var i = 0; i < yBufferSize; i++)
                {
                    count += Math.Abs(frame[i] - lastFrame[i]);
                }

                count /= yBufferSize;
                
                if (count > 2.6)
                {
                    Console.WriteLine($"Motion at {DateTime.Now}");
                    nextMotion = DateTime.Now.AddSeconds(5);
                    var x = (incidentCount % 4) * (width / 4);
                    var y = (incidentCount / 4) * (height / 4);
                    incidentCount = ++incidentCount % 16;
                    var canvas = composite.Canvas;
                    
                    using (var bitmap = YuvToBitmap(frame))
                    using (var image = SKImage.FromBitmap(bitmap))
                    {
                        canvas.DrawImage(image, new SKRect(0, 0, width, height), new SKRect(x, y, x + width / 4, y + height / 4), null);
                    }

                    SaveSurfaceToJpeg(composite, "motion.jpg");
                }
            }           

            var temp = frame;
            frame = lastFrame;
            lastFrame = temp;
        }

        public bool IsCompleted => 
            frameCount < 0;

        public Task Stop()
        {
            fps.Stop();
            
            if (lastFrame != null)
            {
                SaveYuvToJpeg(lastFrame, "frame.jpg");
            }

            return Task.CompletedTask;
        }

        private unsafe void YuvToBgra(byte[] frame, byte* ptr)
        {
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var Y = frame[y * width + x];
                    var U = frame[yBufferSize + (y / 2) * (width / 2) + (x / 2)];
                    var V = frame[yBufferSize + cBufferSize + (y / 2) * (width / 2) + (x / 2)];
                    var Y_ = Y * 1.164;
                    var R = Math.Max(0, Math.Min(255, Y_             + V * 1.793 - 248.101));
                    var G = Math.Max(0, Math.Min(255, Y_ - U * 0.213 - V * 0.533 +  76.878));
                    var B = Math.Max(0, Math.Min(255, Y_ + U * 2.112             - 289.018));
                    *ptr++ = (byte)B;
                    *ptr++ = (byte)G;
                    *ptr++ = (byte)R;
                    *ptr++ = 255; // alpha
                }
            }
        }

        private SKBitmap BgraToBitmap(byte[] frame)
        {
            var bmp = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));
            var ptr = bmp.GetPixels();
            Marshal.Copy(frame, 0, ptr, frame.Length);
            return bmp;
        }

        private unsafe SKBitmap YuvToBitmap(byte[] frame)
        {
            var bmp = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888));
            var ptr = bmp.GetPixels();
            YuvToBgra(frame, (byte*)ptr);
            return bmp;
        }

        private void SaveSurfaceToJpeg(SKSurface surface, Stream stream)
        {
            using (var image = surface.Snapshot())
            using (var bmp = SKBitmap.FromImage(image))
            {
                bmp.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
            }
        }

        private void SaveBgraToJpeg(byte[] frame, Stream stream)
        {
            using (var bmp = BgraToBitmap(frame))
            {
                bmp.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
            }
        }

        private void SaveYuvToJpeg(byte[] frame, Stream stream)
        {
            using (var bmp = YuvToBitmap(frame))
            {
                bmp.Encode(stream, SKEncodedImageFormat.Jpeg, 90);
            }
        }

        private void SaveSurfaceToJpeg(SKSurface surface, string filename)
        {
            using (var file = File.Create(filename))
            {
                SaveSurfaceToJpeg(surface, file);
            }
        }

        private void SaveBgraToJpeg(byte[] frame, string filename)
        {
            using (var file = File.Create(filename))
            {
                SaveBgraToJpeg(frame, file);
            }
        }

        private void SaveYuvToJpeg(byte[] frame, string filename)
        {
            using (var file = File.Create(filename))
            {
                SaveYuvToJpeg(frame, file);
            }
        }
    }
}