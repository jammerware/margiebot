using System;

namespace MargieBot.SampleResponders.Models
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

            return exclamations[new Random().Next(exclamations.Length)];
        }

        public string GetQuery()
        {
            string[] queries = new string[] {
                "Hey, hun! What's up?",
                "Hey, sugar. Need help?",
                "Well, hi there, puddin'. What can I do for ya?",
                "*[yawns]*. Whew. 'Scuse me. Sorry 'bout that. You rang?"
            };

            return queries[new Random().Next(queries.Length)];
        }

        public string GetScoreboardHype()
        {
            string[] hypes = new string[] {
                "Alright, y'all. Here's how this here dogpile's shaping up so far.",
                "Hooboy! It's a _fierce_ day o' competition on the scoreboard. Here's where we're at.",
                "Howdy, friends! It's time for an update on this here rodeo. Here's how we're lookin."
            };

            return hypes[new Random().Next(hypes.Length)];
        }

        public string GetSlackbotSalutation()
        {
            string[] salutations = new string[] {
                "Hey, Slackbot! How you doin', cutie?",
                "Howdy, Slackbot! Aren't you lookin' fine?",
                "Mornin', Slackbot! Heard you were out with Rita Bot last night. How'd it go?",
                "Well, howdy, Slackbot. You're lookin' mighty handsome today."
            };

            return salutations[new Random().Next(salutations.Length)];
        }

        public string GetWeatherAnalysis(double temp)
        {
            if (temp < 40) { return "If it gets any colder, y'all are gonna have to rewrite me to implement IRunsOnHoth. Brrrrr!"; }
            else if (temp < 50) { return "Good thing I brought my puffy coat today."; }
            else if (temp < 60) { return "Not bad, I s'pose, but it ain't hardly hoedown weather, is it?"; }
            else if (temp < 70) { return "Mmm mmm. That's how I like 'em!"; }
            else if (temp < 80) { return "A li'l too warm for my tastes, but I wouldn't say no to a hayride and some cornhole I guess."; }
            else if (temp < 90) { return "It's just gettin' a little stupid out there. I'm not leavin' this hard drive without a longneck and a Super Soaker."; }
            else { return "_Really?_ I'd say you could fry an egg on the sidewalk, but dern it all if my goofy friend RoosterBot didn't already try, and he sunburned his head before he made it three feet out the door."; }
        }

        public string GetYoureWelcome()
        {
            string[] youreWelcomes = new string[] {
                "Happy to, darlin'!",
                "It's my pleasure, hun!",
                "No problem, sugarbean."
            };

            return youreWelcomes[new Random().Next(youreWelcomes.Length)];
        }
    }
}