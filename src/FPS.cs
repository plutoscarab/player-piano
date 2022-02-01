
using System;

namespace pi_ano
{
    public class FPS
    {
        DateTime t;

        public FPS()
        {
            t = DateTime.Now;
        }

        public void Update()
        {
            var u = DateTime.Now;
            var e = u - t;
            t = u;
            Console.Write($"\rFPS: {1.0 / e.TotalSeconds:F1}\t");
        }

        public void Stop()
        {
            Console.WriteLine();
        }
    }
}