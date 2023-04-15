using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

using VTracker.ValApi;

namespace VTracker
{

    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        private List<Label> lastGraphLabels = new List<Label>();
        private List<ShortPlayerDATA> Friends = new List<ShortPlayerDATA>();
        public LiveTracker liveTracker;
        public bool isNightmarket;

        private ConnectApi Request;
        public MainWindow()
        {
            try
            {
                string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                FileStream fileStream = new FileStream(localAppDataPath + "/Riot Games/Riot Client/Config/lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                StreamReader reader = new StreamReader(fileStream);
            }
            catch (Exception)
            {
                Application.Current.Shutdown();
                MessageBox.Show("You need to start the Riot Client or VALORANT", "The Information File doesnt exist!!!", MessageBoxButton.OK);
                return;           
            }
            if (Instance == null)
            {
                Instance = this;
            }
            DataContext = this;
            InitializeComponent();
            GameCollection.Items.Clear();

            NameTagField.Text = "UOL Chefstrobel#6077";
            NameTagField.Text = "Immo uwu#UwU";

            PlayerStatsBox.Visibility = Visibility.Hidden;
            GameStandings.Visibility = Visibility.Hidden;
            Average_Scores.Visibility = Visibility.Hidden;
            GraphTypeBox.SelectionChanged += GraphTypeBox_SelectionChanged;

            ValAPI.Login();
            RefreshShop();
            NameTagField.Text = ValAPI.MyData.Name + "#" + ValAPI.MyData.Tag;

            TextBoxSuggestions.Items.Clear();
            foreach (var item in ValAPI.GetFriends())
            {
                //TextBoxSuggestions.Items.Add(item);
                Friends.Add(item);
            }

            TopLabel.Content = $"ValorantTracker --- {ValAPI.MyData.Name}#{ValAPI.MyData.Tag}";
        }
        public void GetPlayer(string Name, string tag)
        {
            NameTagField.Text = Name + "#" + tag;

            Request = new ConnectApi();
            Request.newRequest(Name, tag,this);
            this.BringIntoView();
            this.Activate();
        }
        public static void RefreshShop()
        {
            MainWindow.Instance.StoreOffers.Items.Clear();
            List<NightStoreOffer> Nightmarket;
            List<StoreOffer> offers = ValAPI.GetStoreOffers(out MainWindow.Instance.isNightmarket,out Nightmarket);
            foreach (var item in offers)
            {
                MainWindow.Instance.StoreOffers.Items.Add(item);
            }

            MainWindow.Instance.NightStoreOffers.Items.Clear();
            if (MainWindow.Instance.isNightmarket)
            {
                foreach (var item in Nightmarket)
                {
                    MainWindow.Instance.NightStoreOffers.Items.Add(item);
                }
            }

        }
        private void NameTag_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsDown)
            {
                if (e.Key == Key.Enter)
                {
                    if (Request == null || !Request.LoadingRequest)
                    {
                        try
                        {
                            string[] nametag = NameTagField.Text.Split('#');

                            Request = new ConnectApi();
                            Request.newRequest(nametag[0], nametag[1], this);
                        }
                        catch (Exception){}
                    }
                }
            }          
        }
        public void ScaleAnimation(UIElement border, bool reversed = false)
        {
            double min = 0.1;
            double max = 1;

            if (reversed)
            {
                double m = min;
                min = max;
                max = m;
            }

            ScaleTransform scaly = new ScaleTransform(min, min);
            border.RenderTransform = scaly;

            Duration mytime = new Duration(TimeSpan.FromSeconds(0.1f));
            Storyboard sb = new Storyboard();

            DoubleAnimation animationX = new DoubleAnimation(min, max, mytime);
            DoubleAnimation animationY = new DoubleAnimation(min, max, mytime);
            sb.Children.Add(animationX);
            sb.Children.Add(animationY);

            Storyboard.SetTarget(animationX, border);
            Storyboard.SetTargetProperty(animationX,
                new PropertyPath("RenderTransform.(ScaleTransform.ScaleX)"));
            Storyboard.SetTarget(animationY, border);
            Storyboard.SetTargetProperty(animationY,
                new PropertyPath("RenderTransform.(ScaleTransform.ScaleY)"));

            sb.Begin();
        }
        private void TopBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        public void GameCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameCollection.Items.Count == 0)
            {
                return;
            }
            bool wasHidden = false;
            if (PlayerStatsBox.Visibility == Visibility.Hidden)
            {
                wasHidden = true;
            }

            GameStandings.Visibility = Visibility.Visible;

            if (wasHidden)
            {
                PlayerStatsBox.Visibility = Visibility.Visible;
                ScaleAnimation(PlayerStatsBox);
            }
           
            PlayerList.Items.Clear();
            Game current = (Game)GameCollection.SelectedItem;
            /*
            if (current == null)
            {
                if (GameCollection.Items.Count == -1)
                {
                    current = (Game)GameCollection.Items[0];
                }
            }
            */
            if (current.Player.team == GameInfo.Team.Blue)
            {
                TeamRoundsWon.Content = current.MatchInfo.Blue_RoundsWon;
                EnemyRoundsWon.Content = current.MatchInfo.Red_RoundsWon;
            }
            else
            {
                TeamRoundsWon.Content = current.MatchInfo.Red_RoundsWon;
                EnemyRoundsWon.Content = current.MatchInfo.Blue_RoundsWon;
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

                Playerview playerview = new Playerview(player.name, KDA, player.RankImage, player.AgentImage, player.WithMain, player.IsMain, player);
                PlayerList.Items.Add(playerview);
                if (player.IsMain)
                {
                    PlayerList.SelectedItem = playerview;
                }
            }
            int Totalshots = current.Player.Playerstats.headshots + current.Player.Playerstats.bodyshots + current.Player.Playerstats.legshots;
            /*
            HeadshotPercentageGame.Content = ((float)current.Player.Playerstats.headshots / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            BodyshotPercentageGame.Content = ((float)current.Player.Playerstats.bodyshots / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            LegshotPercentageGame.Content = ((float)current.Player.Playerstats.legshots / ((float)Totalshots / (float)100)).ToString("#.#") +"%";

            

            c_cast_Image.Source = new BitmapImage(new Uri(current.Player.c_cast_Image, UriKind.Absolute));
            q_cast_Image.Source = new BitmapImage(new Uri(current.Player.q_cast_Image, UriKind.Absolute));
            e_cast_Image.Source = new BitmapImage(new Uri(current.Player.e_cast_Image, UriKind.Absolute));
            x_cast_Image.Source = new BitmapImage(new Uri(current.Player.x_cast_Image, UriKind.Absolute));

            c_castPerRound.Content = ((float)current.Player.Playerstats.c_cast / ((float)current.MatchInfo.Rounds)).ToString("##.##") + "/round";
            c_casts.Content = current.Player.Playerstats.c_cast + " overall";

            q_castPerRound.Content = ((float)current.Player.Playerstats.q_cast / ((float)current.MatchInfo.Rounds)).ToString("##.##") + "/round";
            q_casts.Content = current.Player.Playerstats.q_cast + " overall";

            e_castPerRound.Content = ((float)current.Player.Playerstats.e_cast / ((float)current.MatchInfo.Rounds)).ToString("##.##") + "/round";
            e_casts.Content = current.Player.Playerstats.e_cast + " overall";

            x_castPerRound.Content = ((float)current.Player.Playerstats.x_cast / ((float)current.MatchInfo.Rounds)).ToString("##.##") + "/round";
            x_casts.Content = current.Player.Playerstats.x_cast + " overall";

            
            
            
            
            
            ngDamagePlayer.Content = current.Player.Playerstats.damage_received;
            OutgoingDamagePlayer.Content = current.Player.Playerstats.damage_made;

            //UsernameBox.Content = $"{current.Player.name} #{current.Player.tag}";
            UsernameBox.Document = generateFlowDocument(current.Player.name, " #" + current.Player.tag);

            GenerateGraph(GraphTypeBox.SelectedIndex);
            */
        }
        public class GamePlayerComparer : IComparer<GameInfo.GamePlayer>
        {
            public int Compare(GameInfo.GamePlayer x, GameInfo.GamePlayer y)
            {
                return x.Playerstats.KDAShort.CompareTo(y.Playerstats.KDAShort);
            }
        }
        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Instance = null;
            Close();
        }
        private void PlayerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {           
            Playerview current = (Playerview)PlayerList.SelectedItem;
            if (current == null) { return; }
            Game game = (Game)GameCollection.SelectedItem;

            int Totalshots = current.player.Playerstats.headshots + current.player.Playerstats.bodyshots + current.player.Playerstats.legshots;
            HeadshotPercentageGame.Content = ((float)current.player.Playerstats.headshots / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            BodyshotPercentageGame.Content = ((float)current.player.Playerstats.bodyshots / ((float)Totalshots / (float)100)).ToString("#.#") + "%";
            LegshotPercentageGame.Content = ((float)current.player.Playerstats.legshots / ((float)Totalshots / (float)100)).ToString("#.#") + "%";

            c_cast_Image.Source = new BitmapImage(new Uri(current.player.c_cast_Image, UriKind.Absolute)); 
            q_cast_Image.Source = new BitmapImage(new Uri(current.player.q_cast_Image, UriKind.Absolute)); 
            e_cast_Image.Source = new BitmapImage(new Uri(current.player.e_cast_Image, UriKind.Absolute)); 
            x_cast_Image.Source = new BitmapImage(new Uri(current.player.x_cast_Image, UriKind.Absolute));

            c_castPerRound.Content = ((float)current.player.Playerstats.c_cast / ((float)game.MatchInfo.Rounds)).ToString("##.##") + "/round";
            c_casts.Content = current.player.Playerstats.c_cast + " overall";

            q_castPerRound.Content = ((float)current.player.Playerstats.q_cast / ((float)game.MatchInfo.Rounds)).ToString("##.##") + "/round";
            q_casts.Content = current.player.Playerstats.q_cast + " overall";

            e_castPerRound.Content = ((float)current.player.Playerstats.e_cast / ((float)game.MatchInfo.Rounds)).ToString("##.##") + "/round";
            e_casts.Content = current.player.Playerstats.e_cast + " overall";

            x_castPerRound.Content = ((float)current.player.Playerstats.x_cast / ((float)game.MatchInfo.Rounds)).ToString("##.##") + "/round";
            x_casts.Content = current.player.Playerstats.x_cast + " overall";

            
            
            IncomingDamagePlayer.Content = current.player.Playerstats.damage_received;
            OutgoingDamagePlayer.Content = current.player.Playerstats.damage_made;

            UsernameBox.Document = generateFlowDocument(current.player.name, " #" + current.player.tag);

            GenerateGraph(GraphTypeBox.SelectedIndex);
        }
        private void DrawContentGraph(List<int> data, int MaxValue, string type)
        {
            if (lastGraphLabels.Count > 0)
            {
                foreach (var item in lastGraphLabels)
                {
                    ((Grid)GraphObject.Child).Children.Remove(item);
                }
            }
            PathGeometry ContentPathGeometry = new PathGeometry();

            PathFigure ContentPathFigure = new PathFigure();
            ContentPathFigure.StartPoint = new Point(0, 0);

            LineSegment Zero_ls = new LineSegment(new Point(0, MaxValue - data[0]), false);
            ContentPathFigure.Segments.Add(Zero_ls);
            for (int i = 0; i <data.Count; i++)
            {
                LineSegment lineSegment = new LineSegment(new Point(i, MaxValue - data[i]), true);

                ContentPathFigure.Segments.Add(lineSegment);
            }

            ContentPathGeometry.Figures.Add(ContentPathFigure);

            KillPath.Data = ContentPathGeometry;

            //vertical Grid
            PathGeometry VerticalGridPathGeometry = new PathGeometry();

            PathFigure VerticalPathFigure = new PathFigure();
            VerticalPathFigure.StartPoint = new Point(0, 0);

            for (int i = 0; i <data.Count; i++)
            {
                LineSegment lineSegment = new LineSegment(new Point(i, 1), true);
                VerticalPathFigure.Segments.Add(lineSegment);
                if (i != data.Count - 1)
                {
                    LineSegment emptySegment = new LineSegment(new Point(i + 1, 0), false);
                    VerticalPathFigure.Segments.Add(emptySegment);
                }
            }

            //Vertical numbers
            VerticalGridPathGeometry.Figures.Add(VerticalPathFigure);

            VerticalGrid.Data = VerticalGridPathGeometry;            

            Vector HorizontalStartpoint = new Vector(12, 348);
            Vector HorizontalEndpoint = new Vector(383,-23);

            double HorizontalmultiplierX = (HorizontalEndpoint.X - HorizontalStartpoint.X)/(double)data.Count;
            double HorizontalmultiplierY = (HorizontalEndpoint.Y - HorizontalStartpoint.Y)/(double)data.Count;
            for (double i = 0; i <data.Count; i++)
            {
                Label label = new Label();
                label.Width = 20;
                label.Height = 20;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.Foreground = Brushes.Gray;
                label.FontFamily = new FontFamily("Bauhaus 93");
                int negativedataCount = -data.Count;
                label.FontSize = negativedataCount + 35;
                label.Content = i + 1;
                label.Padding = new Thickness(0,0,0,0);
                label.ToolTip = data[(int)i] + " " + type;
                ((Grid)GraphObject.Child).Children.Add(label);

                label.Margin = new Thickness(HorizontalStartpoint.X + HorizontalmultiplierX * i, 105, HorizontalStartpoint.Y + HorizontalmultiplierY * i, 3);

                lastGraphLabels.Add(label);
            }

            Vector VerticalStartpoint = new Vector(93, 14);
            Vector VerticalEndpoint = new Vector(0, 109);

            double VerticalmultiplierX = (VerticalEndpoint.X - VerticalStartpoint.X) / (double)MaxValue;
            double VerticalmultiplierY = (VerticalEndpoint.Y - VerticalStartpoint.Y) / (double)MaxValue;

            for (double i = 0; i <MaxValue + 1; i++)
            {
                Label label = new Label();
                label.Width = 20;
                label.Height = 20;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.VerticalContentAlignment = VerticalAlignment.Bottom;
                label.Foreground = Brushes.Gray;
                label.FontFamily = new FontFamily("Bauhaus 93");
                int negativedataCount = -MaxValue;
                label.FontSize = 7;
                label.Content = i;
                label.Padding = new Thickness(0, 0, 0, 0);
                ((Grid)GraphObject.Child).Children.Add(label);

                label.Margin = new Thickness(0, VerticalStartpoint.X + VerticalmultiplierX * i, 360, VerticalStartpoint.Y + VerticalmultiplierY * i);

                lastGraphLabels.Add(label);
                if (MaxValue > 5)
                {
                    i += MaxValue / (double)5-1;
                }
            }
            PathGeometry HorizontalGridPathGeometry = new PathGeometry();

            PathFigure HorizontalPathFigure = new PathFigure();
            HorizontalPathFigure.StartPoint = new Point(0, 0);

            for (double i = 0; i <MaxValue + 1; i += MaxValue/5)
            {               
                LineSegment lineSegment = new LineSegment(new Point(1, i), true);
                HorizontalPathFigure.Segments.Add(lineSegment);               
                if ((i >= MaxValue) == false)
                {
                    LineSegment emptySegment = new LineSegment(new Point(0, i + MaxValue / 5), false);
                    HorizontalPathFigure.Segments.Add(emptySegment);
                }               

            }

            HorizontalGridPathGeometry.Figures.Add(HorizontalPathFigure);
            HorizontalGrid.Data = HorizontalGridPathGeometry;
        }
        private void OpenPlayerButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Playerview current = (Playerview)PlayerList.SelectedItem;
            if (current == null)
            {
                current = (Playerview)PlayerList.Items[0];
            }

            MainWindow mw = new MainWindow(); mw.GetPlayer(current.Name, current.player.tag);
            mw.Show();
        }
        private FlowDocument generateFlowDocument(string first, string second)
        {
            var converter = new System.Windows.Media.BrushConverter();
            FlowDocument fd = new FlowDocument();
            System.Windows.Documents.Paragraph para = new System.Windows.Documents.Paragraph();

            Run word1 = new Run(first);
            word1.Foreground = (Brush)converter.ConvertFromString("#FFFC4754");
            para.Inlines.Add(word1);

            Run word2 = new Run(second);
            word2.Foreground = (Brush)converter.ConvertFromString("#FF808080");
            para.Inlines.Add(word2);

            fd.Blocks.Add(para);
            return fd;
        }
        private void GenerateGraph(int value)
        {
            Game game = (Game)GameCollection.SelectedItem;
            if (PlayerList.SelectedItem != null)
            {
                game.Player = ((Playerview)PlayerList.SelectedItem).player;
            }
            switch (value)
            {
                case 0: DrawContentGraph(game.Player.KillsPerRound, 5, "Kills"); break;
                case 1: DrawContentGraph(game.Player.MoneySpentPerRound, 6000, "money spent"); break;
                case 2: DrawContentGraph(game.Player.DamagePerRound, 800, "damage made"); break;
                default:
                    break;
            }
        }
        private void GraphTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GenerateGraph(GraphTypeBox.SelectedIndex);
        }
        private void ToggleShop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Visibility next;
            switch (Overlay.Visibility)
            {
                case Visibility.Visible:
                    next = Visibility.Hidden;
                    break;
                case Visibility.Hidden:
                    next = Visibility.Visible;
                    break;
                default:
                    next = Visibility.Hidden;
                    break;
            }
            StoreOffers.Visibility = next;
            Overlay.Visibility = next;
            if (isNightmarket)
            {
                NightStoreOffers.Visibility = next;
                Nightmarket_Label.Visibility = next;
            }
            if (next == Visibility.Visible)
            {
                ScaleAnimation(StoreOffers);
                if (isNightmarket)
                {
                    ScaleAnimation(NightStoreOffers);
                    ScaleAnimation(Nightmarket_Label);
                }
            }
        }
        private void NameTagField_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBoxSuggestions.Visibility = Visibility.Hidden;
        }
        private void NameTagField_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateNameTagFieldSuggestions();
        }
        private void NameTagField_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateNameTagFieldSuggestions();
        }
        private void UpdateNameTagFieldSuggestions()
        {
            TextBoxSuggestions.Visibility = Visibility.Visible;
            TextBoxSuggestions.Items.Clear();
            if (NameTagField.Text.Count() > 0)
            {
                foreach (var item in Friends)
                {
                    if (item.NameAndTagDisplay.ToLower().Contains(NameTagField.Text.ToLower()))
                    {
                        TextBoxSuggestions.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (var item in Friends)
                {
                    TextBoxSuggestions.Items.Add(item);
                }
            }
            if (TextBoxSuggestions.Items.Count == 0)
            {
                TextBoxSuggestions.Visibility = Visibility.Hidden;
            }
        }
        private void Suggestion_Selected(object sender, MouseButtonEventArgs e)
        {
            NameTagField.Text = ((Label)((Grid)sender).Children[1]).Content as string;
            if (Request == null || !Request.LoadingRequest)
            {
                string[] nametag = NameTagField.Text.Split('#');

                Request = new ConnectApi();
                Request.newRequest(nametag[0], nametag[1],this);
            }
            TextBoxSuggestions.Visibility = Visibility.Hidden;
        }

        private async void Open_LiveTracker(object sender, MouseButtonEventArgs e)
        {
            if (liveTracker == null)
            {
                liveTracker = new LiveTracker(ValAPI.MyData.puuid);
                liveTracker.Show();
            }
            else
            {
                liveTracker.BringIntoView();
                liveTracker.Activate();
            }
        }
    }
}
