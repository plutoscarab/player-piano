﻿
using System;
using System.Text;
using System.Threading.Tasks;

namespace pi_ano
{
    public static class Program
    {
        // Image dimensions
        const int Width = 1024;
        const int Height = 768;

        // libcamera parameters
        const int Port = 8888;

        public async static Task Main()
        {
            // Show the opening scroll
            await Intro();

            // Start the camera
            var server = new VideoServer(Width, Height, Port);

            // Start the frame processing
            var frameHandler = new FrameHandler(Width, Height);
            var client = new VideoClient(Width, Height, Port, frameHandler.Update);

            // Wait until frame processing stops or camera stops
            while (!frameHandler.IsCompleted && !server.IsCompleted)
            {
                await Task.Delay(0);
            }

            // Stop the camera first so that it doesn't complain about TCP client
            await server.Stop();

            // Stop the TCP client to stop reading frames from camera
            await client.Stop();

            // Save the last frame
            await frameHandler.Stop();
        }

        private static async Task Intro()
        {
            var logo = new[] {
                @"\e[48;2;0;0;0m \e[38;2;48;69;16m▄\e[48;2;7;10;2m\e[38;2;113;163;39m▄\e[48;2;28;41;9m\e[38;2;116;167;40m▄\e[48;2;30;44;10m\e[38;2;117;169;40m▄\e[48;2;14;21;4m\e[38;2;115;166;39m▄\e[48;2;0;1;0m\e[38;2;89;128;30m▄\e[48;2;0;0;0m\e[38;2;8;11;2m▄\e[48;2;0;0;0m\e[38;2;5;8;2m▄\e[48;2;0;0;0m\e[38;2;86;124;29m▄\e[48;2;14;21;4m\e[38;2;115;166;39m▄\e[48;2;29;42;10m\e[38;2;117;169;40m▄\e[48;2;29;43;9m\e[38;2;116;168;40m▄\e[48;2;8;11;2m\e[38;2;114;165;39m▄\e[48;2;0;0;0m\e[38;2;52;75;18m▄\e[48;2;0;0;0m \e[0m",
                @"\e[48;2;0;0;0m \e[48;2;51;74;18m\e[38;2;10;15;3m▄\e[48;2;117;169;40m\e[38;2;109;158;37m▄\e[48;2;106;153;36m\e[38;2;117;169;40m▄\e[48;2;88;127;30m\e[38;2;117;169;40m▄\e[48;2;109;158;37m\e[38;2;80;115;27m▄\e[48;2;117;169;40m\e[38;2;79;115;27m▄\e[48;2;55;81;19m\e[38;2;42;60;14m▄\e[48;2;50;73;17m\e[38;2;37;54;13m▄\e[48;2;117;169;40m\e[38;2;81;117;28m▄\e[48;2;110;160;38m\e[38;2;77;112;26m▄\e[48;2;88;127;30m\e[38;2;116;168;40m▄\e[48;2;105;152;36m\e[38;2;117;169;40m▄\e[48;2;117;169;40m\e[38;2;111;160;38m▄\e[48;2;56;82;19m\e[38;2;13;19;4m▄\e[48;2;0;0;0m \e[0m",
                @"\e[48;2;0;0;0m  \e[48;2;40;59;13m\e[38;2;0;0;0m▄\e[48;2;111;160;38m\e[38;2;18;27;6m▄\e[48;2;117;169;40m\e[38;2;59;86;20m▄\e[48;2;117;169;40m\e[38;2;41;59;14m▄\e[48;2;47;68;16m\e[38;2;8;2;3m▄\e[48;2;0;0;0m\e[38;2;60;5;21m▄\e[48;2;0;0;0m\e[38;2;58;5;20m▄\e[48;2;42;61;14m\e[38;2;6;1;2m▄\e[48;2;117;169;40m\e[38;2;39;56;13m▄\e[48;2;117;169;40m\e[38;2;60;87;21m▄\e[48;2;112;161;38m\e[38;2;20;30;7m▄\e[48;2;45;65;15m\e[38;2;0;0;0m▄\e[48;2;0;0;0m  \e[0m",
                @"\e[48;2;0;0;0m  \e[48;2;11;1;3m\e[38;2;110;10;38m▄\e[48;2;125;11;44m\e[38;2;181;16;64m▄\e[48;2;178;16;62m\e[38;2;62;6;22m▄\e[48;2;54;5;19m\e[38;2;2;0;1m▄\e[48;2;159;14;56m\e[38;2;119;11;42m▄\e[48;2;188;17;66m\e[38;2;182;16;64m▄\e[48;2;188;17;66m\e[38;2;185;17;65m▄\e[48;2;157;14;55m\e[38;2;133;12;47m▄\e[48;2;89;8;31m\e[38;2;6;1;2m▄\e[48;2;164;15;58m\e[38;2;106;10;37m▄\e[48;2;90;8;32m\e[38;2;188;17;66m▄\e[48;2;0;0;0m\e[38;2;65;6;23m▄\e[48;2;0;0;0m  \e[0m",
                @"\e[48;2;0;0;0m \e[38;2;42;3;14m▄\e[48;2;106;10;37m\e[38;2;25;2;9m▄\e[48;2;35;3;12m\e[38;2;83;8;29m▄\e[48;2;81;7;28m\e[38;2;188;17;66m▄\e[48;2;143;13;50m\e[38;2;188;17;66m▄\e[48;2;109;10;38m\e[38;2;188;17;66m▄\e[48;2;7;1;2m\e[38;2;81;7;29m▄\e[48;2;12;1;4m\e[38;2;87;8;30m▄\e[48;2;129;12;45m\e[38;2;188;17;66m▄\e[48;2;164;15;58m\e[38;2;188;17;66m▄\e[48;2;102;9;36m\e[38;2;188;17;66m▄\e[48;2;71;6;25m\e[38;2;96;9;34m▄\e[48;2;89;8;31m\e[38;2;49;4;17m▄\e[48;2;0;0;0m\e[38;2;29;2;10m▄\e[48;2;0;0;0m \e[0m",
                @"\e[48;2;19;1;6m\e[38;2;62;5;21m▄\e[48;2;180;16;63m\e[38;2;188;17;66m▄\e[48;2;76;7;27m\e[38;2;64;6;22m▄\e[48;2;154;14;54m\e[38;2;140;13;49m▄\e[48;2;188;17;66m  \e[38;2;183;17;64m▄\e[48;2;97;9;34m\e[38;2;32;3;11m▄\e[48;2;92;8;32m\e[38;2;24;2;8m▄\e[48;2;188;17;66m\e[38;2;177;16;62m▄\e[48;2;188;17;66m  \e[48;2;157;14;55m\e[38;2;133;12;47m▄\e[48;2;115;10;40m\e[38;2;101;9;35m▄\e[48;2;164;15;58m\e[38;2;188;17;66m▄\e[48;2;3;0;0m\e[38;2;24;1;8m▄\e[0m",
                @"\e[48;2;32;2;11m\e[38;2;0;0;0m▄\e[48;2;186;17;65m\e[38;2;38;3;13m▄\e[48;2;21;2;8m\e[38;2;81;7;28m▄\e[48;2;30;3;10m\e[38;2;49;4;17m▄\e[48;2;134;12;47m\e[38;2;0;0;0m▄\e[48;2;137;12;48m\e[38;2;12;1;4m▄\e[48;2;73;7;26m\e[38;2;172;16;60m▄\e[48;2;102;9;36m\e[38;2;188;17;66m▄\e[48;2;105;10;37m\e[38;2;188;17;66m▄\e[48;2;64;6;23m\e[38;2;179;16;63m▄\e[48;2;116;10;41m\e[38;2;22;2;8m▄\e[48;2;114;10;40m\e[38;2;6;1;2m▄\e[48;2;20;2;7m\e[38;2;108;10;38m▄\e[48;2;55;5;19m\e[38;2;127;11;45m▄\e[48;2;173;16;61m\e[38;2;29;3;10m▄\e[48;2;4;0;1m\e[38;2;0;0;0m▄\e[0m",
                @"\e[48;2;0;0;0m \e[48;2;19;1;6m\e[38;2;3;0;1m▄\e[48;2;188;17;66m\e[38;2;174;16;61m▄\e[48;2;188;17;66m \e[48;2;89;8;31m\e[38;2;185;17;65m▄\e[48;2;48;4;17m\e[38;2;33;3;12m▄\e[48;2;188;17;66m\e[38;2;170;15;60m▄\e[48;2;188;17;66m  \e[38;2;178;16;62m▄\e[48;2;66;6;23m\e[38;2;69;6;24m▄\e[48;2;132;12;46m\e[38;2;188;17;66m▄\e[48;2;188;17;66m \e[38;2;174;16;61m▄\e[48;2;12;0;3m\e[38;2;2;0;0m▄\e[48;2;0;0;0m \e[0m",
                @"\e[48;2;0;0;0m  \e[48;2;62;5;22m\e[38;2;0;0;0m▄\e[48;2;183;17;64m\e[38;2;22;1;8m▄\e[48;2;188;17;66m\e[38;2;56;5;20m▄\e[48;2;53;5;19m\e[38;2;12;1;4m▄\e[48;2;22;2;8m\e[38;2;92;8;32m▄\e[48;2;98;9;34m\e[38;2;134;12;47m▄\e[48;2;102;9;36m\e[38;2;138;12;49m▄\e[48;2;29;3;10m\e[38;2;103;9;36m▄\e[48;2;82;7;29m\e[38;2;24;2;8m▄\e[48;2;188;17;66m\e[38;2;68;6;24m▄\e[48;2;184;17;64m\e[38;2;25;2;9m▄\e[48;2;61;5;21m\e[38;2;0;0;0m▄\e[48;2;0;0;0m  \e[0m",
                @"\e[48;2;0;0;0m     \e[48;2;19;1;7m\e[38;2;0;0;0m▄\e[48;2;164;15;58m\e[38;2;6;0;2m▄\e[48;2;188;17;66m\e[38;2;58;5;20m▄\e[48;2;188;17;66m\e[38;2;55;5;19m▄\e[48;2;156;14;55m\e[38;2;4;0;1m▄\e[48;2;13;0;4m\e[38;2;0;0;0m▄\e[48;2;0;0;0m     \e[0m",
            };

            var dots = new[] {
                @"    /  ∎∎∎∎∎∎  ∎∎∎∎        ∎∎∎∎∎  ∎∎∎    ∎∎  ∎∎∎∎∎∎   \    ",
                @"   /   ∎∎   ∎∎  ∎∎        ∎∎   ∎∎ ∎∎∎∎   ∎∎ ∎∎    ∎∎   \   ",
                @"  /    ∎∎∎∎∎∎   ∎∎  ∎∎∎∎∎ ∎∎∎∎∎∎∎ ∎∎ ∎∎  ∎∎ ∎∎    ∎∎    \  ",
                @" /     ∎∎       ∎∎        ∎∎   ∎∎ ∎∎  ∎∎ ∎∎ ∎∎    ∎∎     \ ",
                @"/      ∎∎      ∎∎∎∎       ∎∎   ∎∎ ∎∎   ∎∎∎∎  ∎∎∎∎∎∎       \",
                @"▏                                                         ▕",
                @"▏                                                         ▕",
                @"▏  ∎∎∎∎∎∎  ∎∎    ∎∎     ∎∎∎∎∎∎  ∎∎∎∎∎∎  ∎∎∎∎∎∎∎ ∎∎∎∎∎∎∎∎  ▕",
                @"▏  ∎∎   ∎∎  ∎∎  ∎∎      ∎∎   ∎∎ ∎∎   ∎∎ ∎∎         ∎∎     ▕",
                @"▏  ∎∎∎∎∎∎    ∎∎∎∎       ∎∎∎∎∎∎  ∎∎∎∎∎∎  ∎∎∎∎∎      ∎∎     ▕",
                @"▏  ∎∎   ∎∎    ∎∎        ∎∎   ∎∎ ∎∎   ∎∎ ∎∎         ∎∎     ▕",
                @"▏  ∎∎∎∎∎∎     ∎∎        ∎∎∎∎∎∎  ∎∎   ∎∎ ∎∎∎∎∎∎∎    ∎∎     ▕",
                @"▏                                                         ▕",
                @"▏                                                         ▕",
                @"▏  ∎∎∎    ∎∎∎ ∎∎    ∎∎ ∎∎     ∎∎    ∎∎ ∎∎∎∎∎∎∎ ∎∎    ∎∎   ▕",
                @"▏  ∎∎∎∎  ∎∎∎∎ ∎∎    ∎∎ ∎∎     ∎∎    ∎∎ ∎∎       ∎∎  ∎∎    ▕",
                @"▏  ∎∎ ∎∎∎∎ ∎∎ ∎∎    ∎∎ ∎∎     ∎∎    ∎∎ ∎∎∎∎∎     ∎∎∎∎     ▕",
                @"▏  ∎∎  ∎∎  ∎∎ ∎∎    ∎∎ ∎∎      ∎∎  ∎∎  ∎∎         ∎∎      ▕",
                @"▏  ∎∎      ∎∎  ∎∎∎∎∎∎  ∎∎∎∎∎∎∎  ∎∎∎∎   ∎∎∎∎∎∎∎    ∎∎      ▕",
                @"▏                                                         ▕",
                @" ▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔▔",
            };

            var sequence = new List<string>();

            sequence.AddRange(new[] {
                @" /▔\",
                @"/ ○ \",
                @"▏   ▕",
            });

            sequence.AddRange(Enumerable.Range(2, 22).Select(i => "/" + new string(' ', 1 + 2 * i) + @"\"));
            sequence.AddRange(dots);
            var index = -12;

            foreach (var line in sequence)
            {
                var s = new string(' ', 30 - line.Length / 2) + line.Replace("/", "╱").Replace(@"\", "╲");

                if (index >= 0 && index < logo.Length)
                {
                    s = s.Substring(0, 22) + logo[index].Replace(@"\e", "") + s.Substring(38);
                }

                Console.WriteLine(s);
                index++;
                await Task.Delay(50);
            }
        }
    }
}