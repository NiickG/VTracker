using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.TextFormatting;

namespace VTracker
{
    public class GameInfo
    {
        public string Map;//
        public string MapImageURL;//

        public int Red_RoundsWon;//
        public int Blue_RoundsWon;//

        public string MatchId;//
        public int Rounds;//

        public Team teamthatwon;//
        public List<GamePlayer> players = new List<GamePlayer>();//
        public class GamePlayer
        {
            public string name;//
            public string tag;//
            public string puuid;//

            public Team team;
            public int Level;//
            public string character;//
            public string Rank;//
            public Stats Playerstats;//

            public List<int> KillsPerRound  = new List<int>();//
            public List<int> DamagePerRound = new List<int>();//
            public List<int> MoneySpentPerRound = new List<int>();
            public int MaxKills;//
            public int MaxDamage;//
            public int MaxMoneySpent;

            public string c_cast_Image;//
            public string q_cast_Image;//
            public string e_cast_Image;//
            public string x_cast_Image;//

            public string AgentImage;//
            public string RankImage;//

            public bool WithMain;//
            public bool IsMain;//
            public class Stats
            {
                public string score;//

                public int c_cast;//
                public int q_cast;//
                public int e_cast;//
                public int x_cast;//

                public string Kills;//
                public string deaths;//
                public string assists;//

                public float KDAShort;//

                public int bodyshots;//
                public int headshots;//
                public int legshots;//

                public int damage_made;//
                public int damage_received;//
            }
        }
        public enum Team
        {
            Red,
            Blue
        }
    }    
}
