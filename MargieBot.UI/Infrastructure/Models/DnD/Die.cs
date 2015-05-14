using System;

namespace MargieBot.UI.Infrastructure.Models.DnD
{
    public class Die
    {
        public int NumberOfSides { get; set; }
        private Random Random { get; set; }

        public Die()
        {
            this.Random = new Random();
        }

        public int Roll()
        {
            return (this.Random.Next(NumberOfSides)) + 1;
        }
    }
}