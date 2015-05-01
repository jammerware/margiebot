using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class WhatDoYouDoResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, "(.+)?what (can|do) you do(.+)?", RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            return @"Lots o' things! I mean, potentially, anyway. Right now I'm real good at keepin' score (try plus-one-ing one of your buddies sometime). I'm learnin' about how to keep up with the weather from my friend DonnaBot. I also can't quite keep my eyes off a certain other bot around here :) If there's anythin' else you think I can help y'all with, just say so! The feller who made me tends to keep an eye on me and see how I'm doin'. So there ya have it.";
        }

        public bool ResponseRequiresBotMention(MargieContext context)
        {
            return true;
        }
    }
}