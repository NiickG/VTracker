using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VTracker
{
    public class Game
    {
        public string MyAgentImage { get; set; }
        public string Map { get; set; }
        public string Time { get; set; }
        public string KDA { get; set; }
        public string MapImageURL { get; set; }

        public string RR_Change { get; set; }
        public bool Focus { get; set; }


        public GameInfo.GamePlayer Player;
        public GameInfo MatchInfo;

        public System.Windows.Media.Brush Color { get; set; }

        public Game(GameInfo _gameInfo, string _name, string _tag)
        {
            Player = GetPlayer(_gameInfo, _name, _tag, out bool haswon);
            MatchInfo = _gameInfo;

            MyAgentImage = Player.AgentImage;
            Map = _gameInfo.Map;
            KDA = $"{Player.Playerstats.Kills}/{Player.Playerstats.deaths}/{Player.Playerstats.assists}";

            var converter = new System.Windows.Media.BrushConverter();
            if (!haswon)
            {
                Color = (Brush)converter.ConvertFromString("#FFFC4754");
            }
            else
            {
                Color = (Brush)converter.ConvertFromString("#FF78FF74");
            }
        }
        private GameInfo.GamePlayer GetPlayer(GameInfo _gameInfo, string Name, string Tag , out bool haswon)
        {
            MapImageURL = _gameInfo.MapImageURL;
            foreach (var item in _gameInfo.players)
            {
                if (item.name == Name && item.tag == Tag)
                {
                    if (_gameInfo.teamthatwon == item.team)
                    {
                        haswon = true;
                    }
                    else
                    {
                        haswon = false;
                    }
                    return item;
                }
            }
            haswon = false;
            return null;
        }

    }
}
