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
        public enum Team
        {
            Red,
            Blue
        }
        public Data data;
        public List<GamePlayer> players = new List<GamePlayer>();
        public class Data
        {
            public string Map;

            public string game_start;

            public string Red_RoundsWon;
            public string Blue_RoundsWon;

            public string MatchId;
            public string Cluster;
            public string Rounds;

            public string RR_Change;

            public Team teamthatwon;
        }
        public class Players
        {
            public GamePlayer Player1;
            public GamePlayer Player2;
            public GamePlayer Player3;
            public GamePlayer Player4;
            public GamePlayer Player5;
            public GamePlayer Player6;
            public GamePlayer Player7;
            public GamePlayer Player8;
            public GamePlayer Player9;
            public GamePlayer Player10;
        }
        public class GamePlayer
        {
            public string name;
            public string tag;
            public Team team;
            public int Level;
            public string character;
            public string Rank;
            public Stats Playerstats;

            public string c_cast_Image;
            public string q_cast_Image;
            public string e_cast_Image;
            public string x_cast_Image;

            public string AgentImage;
            public string RankImage;

            public bool WithMain;
            public bool IsMain;
            public class Stats
            {
                public string score;

                public string c_cast;
                public string q_cast;
                public string e_cast;
                public string x_cast;

                public string Kills;
                public string deaths;
                public string assists;

                public float KDAShort;

                public string bodyshots;
                public string headshots;
                public string legshots;

                public string damage_made;
                public string damage_received;
            }
        }
    }
    
}
