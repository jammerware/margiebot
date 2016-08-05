using Microsoft.Extensions.Configuration;
using System;

namespace MargieBot.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // simple configuration
            var bot = new Bot();
            bot.RespondsTo("Hi").With("Hello, friend!");

            // if you want to use this as a crappy integration test like i've been doing, you'll need to add
            // a config.json to the project directory
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("config.json");
            var botSecret = configBuilder.Build()["slackKey"];

            // connect and wait
            bot.Connect(botSecret).Wait();
            Console.ReadKey();
        }
    }
}