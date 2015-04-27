using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MargieBot.Infrastructure.Models
{
    public class Phrasebook
    {
        public string GetAffirmation()
        {
            string[] affirmations = new string[] {
                "Git it!",
                "Give it up!",
                "Nice goin'!",
                "Yeah, buddy!",
                "You go!"
            };

            return affirmations[new Random().Next(affirmations.Length)];
        }

        public string GetExclamation()
        {
            string[] exclamations = new string[] {
                "Awwwright, y'all!",
                "Hoo boy!",
                "Whooo!",
                "Y'all!",
                "Yahoo!"
            };

            return exclamations[new Random().Next(exclamations.Length - 1)];
        }

        public string GetQuery()
        {
            string[] queries = new string[] {
                "Hey, hun! What's up?",
                "Hey, sugar. Need help?",
                "Well, hi there, puddin'. What can I do for ya?",
                "*[yawns]*. Whew. 'Scuse me. Sorry 'bout that. You rang?"
            };

            return queries[new Random().Next(queries.Length - 1)];
        }

        public string GetScoreboardHype()
        {
            string[] hypes = new string[] {
                "Alright, y'all. Here's how this here dogpile's shaping up so far.",
                "Hooboy! It's a _fierce_ day o' competition on the scoreboard. Here's where we're at.",
                "Howdy, friends! It's time for an update on this here rodeo. Here's how we're lookin."
            };

            return hypes[new Random().Next(hypes.Length - 1)];
        }

        public string GetSlackbotSalutation()
        {
            string[] salutations = new string[] {
                "Hey, Slackbot! How you doin', cutie?",
                "Howdy, Slackbot! Aren't you lookin' fine?",
                "Mornin', Slackbot! Heard you were out with Rita Bot last night. How'd it go?",
                "Well, howdy, Slackbot. You're lookin' mighty handsome today."
            };

            return salutations[new Random().Next(salutations.Length - 1)];
        }

        public string GetYoureWelcome()
        {
            string[] youreWelcomes = new string[] {
                "Happy to, darlin'!",
                "It's my pleasure, hun!",
                "No prob, sweetie."
            };

            return youreWelcomes[new Random().Next(youreWelcomes.Length - 1)];
        }
    }
}
