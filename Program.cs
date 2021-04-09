using System.IO;
using System;
using System.Threading.Tasks;
using DiscordRPC;
using DiscordRPC.Logging;
using Newtonsoft.Json;

namespace DiscordDaysSincePresence
{
    class Program
    {
        public static DiscordRpcClient client;
        public static Configuration config;
        static void Main()
        {
            if (File.Exists("./config.json")) {
                config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));
            } else {
                Console.WriteLine("Creating a new configuration file");
                File.WriteAllText("./config.json", JsonConvert.SerializeObject(new Configuration(), Formatting.Indented));
                Console.WriteLine("New configuration file created, press any key to close this window");
                Console.ReadKey();
                Environment.Exit(0);
            }
            
            Start().GetAwaiter().GetResult();
        }

        public static async Task Start() {
            client = new DiscordRpcClient(config.ApplicationId);
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Ready!");
            };
            client.Initialize();

            var timer = new System.Timers.Timer(1000);
            timer.Elapsed += (sender, args) => {
                config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("config.json"));

                DateTime target = new DateTime(config.Year, config.Month, config.Day);
                Double daysPassed = Math.Round((DateTime.Now - target).TotalDays);

                // Sets the new presence
                client.SetPresence(new RichPresence() {
                    Details = $"{config.Message}",
                    State = $"{daysPassed} days ago!"
                });
            };
            timer.Start();

            // Makes sure the application doesn't quit
            await Task.Delay(-1);
        }
    }
}
