using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.SampleResponders;
using MargieBot.SampleResponders.Models;
using Microsoft.Extensions.Configuration;

namespace MargieBot.ConsoleHost
{
    public class SampleBotConfig
    {
        /// <summary>
        /// Replace the contents of the list returned from this method with any aliases you might want your bot to respond to. If you
        /// don't want your bot to respond to anything other than its actual name, just return an empty list here.
        /// </summary>
        /// <returns>A list of aliases that will cause the BotWasMentioned property of the ResponseContext to be true</returns>
        public IEnumerable<string> GetAliases()
        {
            return new List<string>() { "Margie" };
        }

        /// <summary>
        /// If you want to use this application to run your bot, here's where you start. Just scrap as many of the responders
        /// described in this method as you want and start fresh. Define your own responders using the methods describe
        /// at https://github.com/jammerware/margiebot/wiki/Configuring-responses and return them in an IEnumerable<IResponder>. 
        /// You can create them in this project, in a separate one, or even in the SampleResponders project if you want.
        /// 
        /// Boom! You have your own bot.
        /// </summary>
        /// <returns>A list of the responders this bot should respond with.</returns>
        public IEnumerable<IResponder> GetResponders(Bot bot, IConfigurationRoot appConfig)
        {
            // Some of these are more complicated than they need to be for the sake of example
            var responders = new List<IResponder>();
            
            // examples of semi-complex or "messier" responders (created in separate classes)
            responders.Add(new ScoreResponder());
            responders.Add(new ScoreboardRequestResponder());
            responders.Add(new WhatsNewResponder());
            responders.Add(new WikipediaResponder());

            // if you want to use these, you'll need to sign up for api keys from http://wunderground.com and http://www.dictionaryapi.com/ - they're free! Put them in your 
            // config.json, uncomment the lines below, and you're good to go.
            //responders.Add(new WeatherRequestResponder(appConfig["wundergroundApiKey"]));
            //responders.Add(new DefineResponder(appConfig["dictionaryApiKey"]));

            // examples of simple-ish "inline" responders
            // this one hits on Slackbot when he talks 1/8 times or so
            responders.Add(SimpleResponder.Create
            (
                (responseCtx) => responseCtx.Message.User.IsSlackbot && new Random().Next(8) <= 1,
                (responseCtx) => responseCtx.Get<Phrasebook>().GetSlackbotSalutation()
            ));

            // this one responds if someone thanks Margie
            responders.Add(SimpleResponder.Create
            (
                (responseCtx) => responseCtx.Message.MentionsBot && Regex.IsMatch(responseCtx.Message.Text, @"\b(thx|thanks|thank you)\b", RegexOptions.IgnoreCase),
                (responseCtx) => responseCtx.Get<Phrasebook>().GetYoureWelcome()
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
            responders.Add(SimpleResponder.Create
            (
                (responseCtx) =>
                {
                    return
                        responseCtx.Message.MentionsBot &&
                        !responseCtx.BotHasResponded &&
                        Regex.IsMatch(responseCtx.Message.Text, @"\b(hi|hey|hello|what's up|what's happening)\b", RegexOptions.IgnoreCase) &&
                        responseCtx.Message.User.ID != responseCtx.BotUserID &&
                        !responseCtx.Message.User.IsSlackbot;
                },
                (responseCtx) => responseCtx.Get<Phrasebook>().GetQuery()
            ));

            return responders;
        }

        /// <summary>
        /// If you want to share any data across all your responders, you can use the StaticResponseContextData property of the bot to do it. I elected
        /// to have most of my responders use a "Phrasebook" object to ensure a consistent tone across the bot's responses, so I stuff the Phrasebook
        /// into the context for use.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetStaticResponseContextData()
        {
            return new Dictionary<string, object>()
            {
                { "Phrasebook", new Phrasebook() }
            };
        }
    }
}