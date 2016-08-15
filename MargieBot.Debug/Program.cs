using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MargieBot.Debug
{
    public class Program
    {
        /// <summary>
        /// This is just a poor man's integration test. Don't judge me.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var bot = new Bot();
            
            // if you want to use this as a crappy integration test like i've been doing, you'll need to add
            // a config.json to the project directory
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("config.json");
            var botSecret = configBuilder.Build()["slackKey"];
            
            // connect
            bot.Connect(botSecret).Wait();
            
            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) { break; }


                var sayTask = bot.Say(new BotMessage()
                {
                    ChatHub = bot.ConnectedDMs.Where(dm => dm.Name.Equals("@jammer", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault(),
                    Text = input
                });
            }
        }
    }
}