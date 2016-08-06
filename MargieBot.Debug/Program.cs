using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;
using MargieBot.SampleResponders.Models;
using MargieBot.SampleResponders.Responders;
using Microsoft.Extensions.Configuration;

namespace MargieBot.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // simple configuration
            var bot = new Bot();
            
            // if you want to use this as a crappy integration test like i've been doing, you'll need to add
            // a config.json to the project directory
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("config.json");
            var botSecret = configBuilder.Build()["slackKey"];

            // connect
            bot.Connect(botSecret).Wait();

            // loop and listen. entering "exit" will end the program. entering the name of a chat hub (like "@jammer" or "#news") that the bot's connected to, followed by a message,
            // will cause the bot to speak your message in the appropriate hub. useful for weirding out your coworkers :)
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