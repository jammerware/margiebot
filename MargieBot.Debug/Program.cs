using Microsoft.Extensions.Configuration;
using System;
using MargieBot.Responders;
using System.Collections;
using MargieBot.Models;
using System.Collections.Generic;
using MargieBot.SampleResponders.Responders;
using System.Text.RegularExpressions;
using MargieBot.SampleResponders.Models;

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

            // set up responders
            foreach(var responder in GetResponders(bot))
            {
                bot.Responders.Add(responder);
            }

            // connect and wait
            bot.Connect(botSecret).Wait();

            while(Console.ReadLine() != "exit") { }
        }

        private static IEnumerable<IResponder> GetResponders(Bot bot)
        {
            // Some of these are more complicated than they need to be for the sake of example
            var responders = new List<IResponder>();

            // examples of semi-complex or "messier" responders (created in separate classes)
            responders.Add(new ScoreResponder());
            responders.Add(new ScoreboardRequestResponder());
            responders.Add(new WhatsNewResponder());
            responders.Add(new WikipediaResponder());

            // examples of simple-ish "inline" responders
            // this one hits on Slackbot when he talks 1/8 times or so
            responders.Add(bot.CreateResponder(
                (ResponseContext context) => { return (context.Message.User.IsSlackbot && new Random().Next(8) <= 1); },
                (ResponseContext context) => { return context.Get<Phrasebook>().GetSlackbotSalutation(); }
            ));

            // easiest one of all - this one responds if someone thanks Margie
            responders.Add(bot.CreateResponder(
                (ResponseContext context) => { return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase); },
                (ResponseContext context) => { return context.Get<Phrasebook>().GetYoureWelcome(); }
            ));

            // example of Supa Fly Mega EZ Syntactic Sugary Responder (not their actual name)
            bot
                .RespondsTo("get on that")
                .With("Sure, hun!")
                .With("I'll see what I can do, sugar.")
                .With("I'll try. No promises, though!")
                .IfBotIsMentioned();

            // you can do these with regexes too
            bot
                .RespondsTo("what (can|do) you do", true)
                .With(@"Lots o' things! I mean, potentially, anyway. Right now I'm real good at keepin' score (try plus-one-ing one of your buddies sometime). I'm learnin' about how to keep up with the weather from my friend DonnaBot. I also can't quite keep my eyes off a certain other bot around here :) If there's anythin' else you think I can help y'all with, just say so! The feller who made me tends to keep an eye on me and see how I'm doin'. So there ya have it.")
                .IfBotIsMentioned();

            // this last one just responds if someone says "hi" or whatever to Margie, but only if no other responder has responded
            responders.Add(bot.CreateResponder(
                (ResponseContext context) => {
                    return
                        context.Message.MentionsBot &&
                        !context.BotHasResponded &&
                        Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello|what's up|what's happening)\b", RegexOptions.IgnoreCase) &&
                        context.Message.User.ID != context.BotUserID &&
                        !context.Message.User.IsSlackbot;
                },
                (ResponseContext context) => {
                    return context.Get<Phrasebook>().GetQuery();
                }
            ));

            return responders;
        }
    }
}