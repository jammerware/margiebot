using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MargieBot.UI.Infrastructure.Models.DnD
{
    public class Character
    {
        public int Age { get; set; }
        public string EyeColor { get; set; }
        public int Height { get; set; }
        public string SkinColor { get; set; }
        public int Weight { get; set; }

        public string Alignment { get; set; }
        public string Background { get; set; }
        public string Class { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Race { get; set; }

        public int SpellAttackBonus { get; set; }
        public string SpellAttribute { get; set; }
        public int SpellSaveDC { get; set; }

        public Character()
        {
            Age = 320;
            EyeColor = "Yellow";
            Height = 85;
            SkinColor = "Purple ceramic";
            Weight = 410;

            Alignment = "Chaotic Good";
            Background = "Charlatan";
            Class = "Warlock";
            Level = 1;
            Name = "Margieworth von Willowbot";
            Race = "Clockwork Gal";

            SpellAttackBonus = 5;
            SpellAttribute = "Charisma";
            SpellSaveDC = 13;
        }
    }
}