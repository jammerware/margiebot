using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace MargieBot.ConsoleHost
{
    public class Program
    {
        private static IConfigurationRoot _appConfig;

        public static void Main(string[] args)
        {
            // let's make a bot!
            var bot = new Bot();

            // load the slack key out of the config.json file - remember to copy the "config.sample" file from the repo to "config.json" and replace the default value with your bot's slack key.
            if(File.Exists("config.json"))
            {
                _appConfig = new ConfigurationBuilder()
                    .AddJsonFile("config.json")
                    .Build();
            }

            // this part just tells the end developer to add his/her slack key if he/she hasn't already
            if (_appConfig?["slackApiKey"] == null)
            {
                Console.WriteLine("To run your bot, you first need to create a key for it at https://<yourteam>.slack.com/apps/manage. Then copy the file called \"config.sample\" in the root of this project to \"config.json\" and replace the value of the key \"slackApiKey\" with your bot's key.\n\n Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // I threw a bunch of common configuration things in a simple wrapper class so it's easier to see what each one does. look at the implementation of
            // the class BotConfig in this project if you want to customize your bot using this project as a base.
            var botConfig = new SampleBotConfig();

            bot.Aliases = botConfig.GetAliases();
            bot.Responders.AddRange(botConfig.GetResponders(bot, _appConfig));
            foreach(var pair in botConfig.GetStaticResponseContextData())
            {
                bot.ResponseContext.Add(pair.Key, pair.Value);
            }

            // connect
            var slackApiKey = _appConfig["slackApiKey"];
            Console.WriteLine($@"Connecting with Slack key ""{slackApiKey}""...");
            bot.Connect(slackApiKey).Wait();
            Console.WriteLine("Connected!");

            // loop and listen. entering "exit" will end the program. entering the name of a chat hub (like "@jammer" or "#news") that the bot's connected to, followed by a message,
            // will cause the bot to speak your message in the appropriate hub. useful for weirding out your coworkers :)
            while (true)
            {
                var input = Console.ReadLine();
                if (input.Equals("exit", StringComparison.CurrentCultureIgnoreCase)) { break; }
                
                // TODO: parse out the chat hub

                var sayTask = bot.Say(new BotMessage()
                {
                    ChatHub = bot.ConnectedDMs.Where(dm => dm.Name.Equals("@jammer", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault(),
                    Text = input
                });
            }

            // clean up when we're done
            bot.Disconnect();
        }
    }
}