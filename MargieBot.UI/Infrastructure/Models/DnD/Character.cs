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

        // base attributes
        public int Charisma { get; set; }
        public int Constitution { get; set; }
        public int Dexterity { get; set; }
        public int Intelligence { get; set; }
        public int Strength { get; set; }
        public int Wisdom { get; set; }
        
        // other attributes
        public int ProficiencyBonus { get; set; }

        public IReadOnlyList<CharacterAttribute> ProficiencyAttributes { get; set; }
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

            Charisma = 17;
            Constitution = 15;
            Dexterity = 12;
            Intelligence = 15;
            Strength = 12;
            Wisdom = 13;

            ProficiencyBonus = 2;

            ProficiencyAttributes = new List<CharacterAttribute>() { CharacterAttribute.CHA, CharacterAttribute.WIS };
            SpellAttackBonus = 5;
            SpellAttribute = "Charisma";
            SpellSaveDC = 13;
        }

        public int GetAttributeValue(CharacterAttribute attr)
        {
            switch (attr) {
                case CharacterAttribute.CHA:
                    return Charisma;
                case CharacterAttribute.CON:
                    return Constitution;
                case CharacterAttribute.DEX:
                    return Dexterity;
                case CharacterAttribute.INT:
                    return Intelligence;
                case CharacterAttribute.STR:
                    return Strength;
                case CharacterAttribute.WIS:
                    return Wisdom;
            }

            return 0;
        }

        public int GetAttributeBonus(CharacterAttribute attr)
        {
            int attrValue = GetAttributeValue(attr);
            return (int)Math.Round((double)(attrValue - 10) / 2);
        }

        public int GetAttrProficiency(CharacterAttribute attr)
        {
            if (ProficiencyAttributes.Contains(attr)) return ProficiencyBonus;
            return 0;
        }
    }
}