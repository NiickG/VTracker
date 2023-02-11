using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ValorantNET.Models;
using static ValorantNET.Enums;
using static ValorantNET.Models.Content;
using static ValorantNET.Models.Match;

namespace VTracker
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        private ConnectApi Request;
        public MainWindow()
        {
            Instance = this;
            InitializeComponent();
            GameCollection.Items.Clear();

            NameTagField.Text = "UOL Chefstrobel#6077";
            /*
            var httpRequest = (HttpWebRequest)WebRequest.Create($"https://api.henrikdev.xyz/valorant/v1/mmr/eu/Knorke/2234");
            httpRequest.Accept = "application/json";
            using (var streamReader = new StreamReader(httpRequest.GetResponse().GetResponseStream()))
            {
                var Rankresult = streamReader.ReadToEnd();
                dynamic Rankobj = JsonConvert.DeserializeObject(Rankresult);

                Debug.WriteLine((string)Rankobj.data.images.small);
            }
            */
        }
        public void GetPlayer(string Name, string tag)
        {
            NameTagField.Text = Name + "#" + tag;

            Request = new ConnectApi();
            Request.newRequest($"https://api.henrikdev.xyz/valorant/v3/matches/eu/{Name}/{tag}?filter=competitive", ConnectApi.ApiType.GetlastGamesMain, Name, tag);
        }

        private void NameTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown)
            {
                if (e.Key == Key.Enter)
                {
                    string[] nametag = NameTagField.Text.Split('#');
                    Request = new ConnectApi();
                    Request.newRequest($"https://api.henrikdev.xyz/valorant/v3/matches/eu/{nametag[0]}/{nametag[1]}?filter=competitive", ConnectApi.ApiType.GetlastGamesMain, nametag[0], nametag[1]);
                }
            }          
        }

        private void TopBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void GameCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PlayerList.Items.Clear();

            Game current = (Game)GameCollection.SelectedItem;

            if (current.Player.team == GameInfo.Team.Blue)
            {
                TeamRoundsWon.Content = current.MatchInfo.data.Blue_RoundsWon;
                EnemyRoundsWon.Content = current.MatchInfo.data.Red_RoundsWon;
            }
            else
            {
                TeamRoundsWon.Content = current.MatchInfo.data.Red_RoundsWon;
                EnemyRoundsWon.Content = current.MatchInfo.data.Blue_RoundsWon;
            }

            List<GameInfo.GamePlayer> sortlist = new List<GameInfo.GamePlayer>();
            foreach (var item in current.MatchInfo.players)
            {
                sortlist.Add(item);
            }
            sortlist.Sort(new GamePlayerComparer());
            sortlist.Reverse();
            foreach (var player in sortlist)
            {
                if (player.team == current.Player.team)
                {
                    player.WithMain = true;
                }
                else
                {
                    player.WithMain = false;
                }

                string KDA = $"{player.Playerstats.Kills}/{player.Playerstats.deaths}/{player.Playerstats.assists}";

                PlayerList.Items.Add(new Playerview(player.name, KDA, player.RankImage, player.AgentImage, player.WithMain, player.IsMain, player));
            }

            int Totalshots = int.Parse(current.Player.Playerstats.headshots) + int.Parse(current.Player.Playerstats.bodyshots) + int.Parse(current.Player.Playerstats.legshots);

            HeadshotPercentageGame.Content = ((float)float.Parse(current.Player.Playerstats.headshots) / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            BodyshotPercentageGame.Content = ((float)float.Parse(current.Player.Playerstats.bodyshots) / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            LegshotPercentageGame.Content = ((float)float.Parse(current.Player.Playerstats.legshots) / ((float)Totalshots / (float)100)).ToString("#.#") +"%";

            

            c_cast_Image.Source = new BitmapImage(new Uri(current.Player.c_cast_Image, UriKind.Absolute));
            q_cast_Image.Source = new BitmapImage(new Uri(current.Player.q_cast_Image, UriKind.Absolute));
            e_cast_Image.Source = new BitmapImage(new Uri(current.Player.e_cast_Image, UriKind.Absolute));
            x_cast_Image.Source = new BitmapImage(new Uri(current.Player.x_cast_Image, UriKind.Absolute));

            c_castPerRound.Content = ((float)float.Parse(current.Player.Playerstats.c_cast) / ((float)float.Parse(current.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            c_casts.Content = current.Player.Playerstats.c_cast + " overall";

            q_castPerRound.Content = ((float)float.Parse(current.Player.Playerstats.q_cast) / ((float)float.Parse(current.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            q_casts.Content = current.Player.Playerstats.q_cast + " overall";

            e_castPerRound.Content = ((float)float.Parse(current.Player.Playerstats.e_cast) / ((float)float.Parse(current.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            e_casts.Content = current.Player.Playerstats.e_cast + " overall";

            x_castPerRound.Content = ((float)float.Parse(current.Player.Playerstats.x_cast) / ((float)float.Parse(current.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            x_casts.Content = current.Player.Playerstats.x_cast + " overall";



            GameStart.Content = current.MatchInfo.data.game_start;
        }
        public class GamePlayerComparer : IComparer<GameInfo.GamePlayer>
        {
            public int Compare(GameInfo.GamePlayer x, GameInfo.GamePlayer y)
            {
                return x.Playerstats.KDAShort.CompareTo(y.Playerstats.KDAShort);
            }
        }

        private void PlayerBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var baseobj = sender as FrameworkElement;
                var myObject = baseobj.DataContext as Playerview;

                MainWindow mw = new MainWindow(); mw.GetPlayer(myObject.Name, myObject.player.tag);
                mw.Show();
            }         
        }

        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void PlayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Playerview current = (Playerview)PlayerList.SelectedItem;
            if (current == null) { return; }
            Game game = (Game)GameCollection.SelectedItem;
            int Totalshots = int.Parse(current.player.Playerstats.headshots) + int.Parse(current.player.Playerstats.bodyshots) + int.Parse(current.player.Playerstats.legshots);
            HeadshotPercentageGame.Content = ((float)float.Parse(current.player.Playerstats.headshots) / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            BodyshotPercentageGame.Content = ((float)float.Parse(current.player.Playerstats.bodyshots) / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            LegshotPercentageGame.Content = ((float)float.Parse(current.player.Playerstats.legshots) / ((float)Totalshots / (float)100)).ToString("#.#") + "%";

            c_cast_Image.Source = new BitmapImage(new Uri(current.player.c_cast_Image, UriKind.Absolute)); 
            q_cast_Image.Source = new BitmapImage(new Uri(current.player.q_cast_Image, UriKind.Absolute)); 
            e_cast_Image.Source = new BitmapImage(new Uri(current.player.e_cast_Image, UriKind.Absolute)); 
            x_cast_Image.Source = new BitmapImage(new Uri(current.player.x_cast_Image, UriKind.Absolute));

            c_castPerRound.Content = ((float)float.Parse(current.player.Playerstats.c_cast) / ((float)float.Parse(game.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            c_casts.Content = current.player.Playerstats.c_cast + " overall";

            q_castPerRound.Content = ((float)float.Parse(current.player.Playerstats.q_cast) / ((float)float.Parse(game.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            q_casts.Content = current.player.Playerstats.q_cast + " overall";

            e_castPerRound.Content = ((float)float.Parse(current.player.Playerstats.e_cast) / ((float)float.Parse(game.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            e_casts.Content = current.player.Playerstats.e_cast + " overall";

            x_castPerRound.Content = ((float)float.Parse(current.player.Playerstats.x_cast) / ((float)float.Parse(game.MatchInfo.data.Rounds))).ToString("##.##") + "/Round";
            x_casts.Content = current.player.Playerstats.x_cast + " overall";
        }
    }
}
