using System.Windows.Media;


namespace VTracker
{
    public class Playerview
    {
        public string Name { get; set; }
        public string KDA { get; set; }

        public string AgentImage { get; set; }
        public string RankImage { get; set; }

        public System.Windows.Media.Brush BackroundColor { get; set; }
        public System.Windows.Media.Brush BorderColor { get; set; }

        public GameInfo.GamePlayer player;

        public Playerview(string _Name, string _KDA, string _RankImage, string _AgentImage, bool WithMain, bool isMain, GameInfo.GamePlayer _player) 
        {
            Name= _Name;
            KDA= _KDA;
            RankImage= _RankImage;
            player = _player;
            AgentImage= _AgentImage;


            var converter = new System.Windows.Media.BrushConverter();

            if (WithMain)
            {
                BackroundColor = (Brush)converter.ConvertFromString("#FF93FFA4");
            }
            else
            {
                BackroundColor = (Brush)converter.ConvertFromString("#FFFF6C6C");
            }

            BorderColor = (Brush)converter.ConvertFromString("#FF2D2D2D");
            if (isMain)
            {
                BorderColor = (Brush)converter.ConvertFromString("#FF556FFF");
            }
        }
    }
}
