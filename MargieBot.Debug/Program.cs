using System;

namespace MargieBot.Debug
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var bot = new Bot();
            bot.RespondsTo("Hi").With("Hello, friend!");
            bot.Connect("xoxb-4599190677-6e2xHAZaP3CP6RLFPxJ72Bfu").Wait();

            Console.ReadKey();
        }
    }
}