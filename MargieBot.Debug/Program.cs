using Microsoft.Extensions.Configuration;
using System;

namespace MargieBot.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RespondsTo("Hi").With("Hello, friend!");

            // if you want to use this as a crappy integration test like i've been doing, you'll need to add
            // a config.json to the project directory
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("Config/config.json");
            var botSecret = configBuilder.Build()["slackKey"];
            bot.Connect("xoxb-4599190677-OXvDreiWVO54yD3iim3LTwP5").Wait();

            Console.ReadKey();
        }
    }
}