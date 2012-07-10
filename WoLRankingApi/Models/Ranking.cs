using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WoLRankingApi.Models {

    public enum Difficulty {
        LFR,
        Normal,
        Heroic
    }

    public enum Class {
        DeathKnight,
        Druid,
        Hunter,
        Mage,
        Paladin,
        Priest,
        Rogue,
        Shaman,
        Warlock,
        Warrior
    }

    public static class Helpers {
        private static Dictionary<string, Difficulty> StringToDifficulty = new Dictionary<string, Difficulty>() {
            { "LFR", Difficulty.LFR },
            { "Normal", Difficulty.Normal },
            { "Heroic", Difficulty.Heroic }
        };
        private static Dictionary<string, Class> ClassIconToClass = new Dictionary<string, Class>() {
            { "death_knight", Class.DeathKnight },
            { "druid", Class.Druid },
            { "hunter", Class.Hunter },
            { "mage", Class.Mage },
            { "paladin", Class.Paladin },
            { "priest", Class.Priest },
            { "rogue", Class.Rogue },
            { "shaman", Class.Shaman },
            { "warlock", Class.Warlock },
            { "warrior", Class.Warrior }
        };
        public static Class ParseClass(string s) {
            return ClassIconToClass[s];
        }
        public static Difficulty ParseDifficulty(string s) {
            return StringToDifficulty[s];
        }
    }

    public class Ranking {
        public int Rank;
        public Uri RankUri;
        public string Player;
        public Uri ParseUri;
        public Class Class;
        public string Spec;
        public string Date;
        public string Encounter;
        public int Size;
        public Difficulty Difficulty;
        public long OutputRate;
        public long OutputTotal;
        public double Contribution;
        public string Duration;
    }
}