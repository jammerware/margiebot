using MargieBot.Infrastructure.Models;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class WhatDoYouDoResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, "(.+)?what (can|do) you do(.+)?", RegexOptions.IgnoreCase) && Regex.IsMatch(context.Message.Text, context.MargieNameRegex, RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            return @"Lots o' things! I mean, potentially, anyway. Right now I'm real good at keepin' score (try plus-one-ing one of your buddies sometime). I also can't quite keep my eyes off a certain other bot around here :) If there's anythin' else you think I can help y'all with, just say so! The feller who made me tends to keep an eye on me and see how I'm doin'. So there ya have it.";
        }
    }
}