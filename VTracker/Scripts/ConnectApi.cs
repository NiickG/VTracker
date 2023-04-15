using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VTracker.ValApi;

namespace VTracker
{
    class ConnectApi
    {
        public Storyboard LoadingAnimation;
        public bool LoadingRequest;
        public static List<GameInfo> Matches = new List<GameInfo>();
        public void newRequest(string name,string tag, MainWindow window)
        {
            window.GameCollection.Items.Clear();
            window.LoadingSymbol.Visibility = Visibility.Visible;
            window.LoadingSymbol.RenderTransform = new RotateTransform();

            LoadingAnimation = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = 0;
            animation.To = -360;
            animation.Duration = new System.Windows.Duration(TimeSpan.FromSeconds(1));
            animation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            Storyboard.SetTarget(animation, window.LoadingSymbol);
            LoadingAnimation.Children.Add(animation);
            window.NameTagField.CaretIndex = window.NameTagField.Text.Length;
            Keyboard.ClearFocus();

            LoadingAnimation.Begin();

            var task = new Task(async () =>
            {               
                try
                {
                    Task<string> nameTask = ValAPI.GetPlayerByNameAndTag(name, tag);
                    nameTask.Wait();
                    string puuid = nameTask.Result;
                    Debug.WriteLine(puuid);
                    Matches = ConvertData.ConvertGames(ValAPI.GetLatestGames(puuid, "eu"), puuid);

                    Action invokeAction1 = new Action(() => {
                        window.GameCollection.Items.Clear();
                        for (int i = 0; i <Matches.Count; i++)
                        {
                            window.GameCollection.Items.Add(new Game(Matches[i], name, tag));
                            LoadingRequest = false;
                        }
                        window.GameCollection.SelectedItem = window.GameCollection.Items[0];
                        window.LoadingSymbol.Visibility = Visibility.Hidden;
                        LoadingAnimation.Stop();
                        if (window.Average_Scores.Visibility == Visibility.Hidden)
                        {
                            window.Average_Scores.Visibility = Visibility.Visible;
                            window.ScaleAnimation(window.Average_Scores);
                            window.NoPlayerSelectedLabel.Visibility = Visibility.Hidden;
                        }
                    });
                    window.GameCollection.Dispatcher.Invoke(invokeAction1);
                    UpdateAverage(window);
                }
                catch (Exception e)
                {
                    LoadingAnimation.Stop();
                    LoadingRequest = false;
                    Action invokeAction2 = new Action(() => {
                        window.LoadingSymbol.Visibility = Visibility.Hidden;
                    });
                    window.GameCollection.Dispatcher.Invoke(invokeAction2);
                    Debug.WriteLine(e);
                    return;
                }
            });
            LoadingRequest = true;
            task.Start();
        }
        private void UpdateAverage(MainWindow window)
        {
            //Average KDA
            float AverageK;
            float AverageD;
            float AverageA;

            int CombinedK = 0;
            int CombinedD = 0;
            int CombinedA = 0;

            int Counted = 0;
            foreach (Game item in window.GameCollection.Items)
            {
                CombinedK += int.Parse(item.Player.Playerstats.Kills); 
                CombinedD += int.Parse(item.Player.Playerstats.deaths); 
                CombinedA += int.Parse(item.Player.Playerstats.assists);

                Counted++;
            }

            AverageK = (float)CombinedK/Counted;
            AverageK = (float)Math.Round(AverageK);

            AverageD = (float)CombinedD /Counted;
            AverageD = (float)Math.Round(AverageD);

            AverageA = (float)CombinedA /Counted;
            AverageA = (float)Math.Round(AverageA);


            Action invokeAction = new Action(() => {               
                window.AvergageKDA.Content = $"{AverageK} / {AverageD} / {AverageA}";
            });
            window.GameCollection.Dispatcher.Invoke(invokeAction);

            //Average HitPercentage
            float AverageHeadshotPercentage = 0;
            float AverageBodyshotPercentage = 0;
            float AverageLegshotPercentage = 0;

            int ShotsCombined = 0;

            float CombinedHeadshots = 0;
            float CombinedBodyshots = 0;
            float CombinedLegshots = 0;

            foreach (Game item in window.GameCollection.Items)
            {
                ShotsCombined += item.Player.Playerstats.headshots + item.Player.Playerstats.bodyshots + item.Player.Playerstats.legshots;

                CombinedHeadshots += item.Player.Playerstats.headshots;
                CombinedBodyshots += item.Player.Playerstats.bodyshots;
                CombinedLegshots += item.Player.Playerstats.legshots;
            }

            AverageHeadshotPercentage = (float)CombinedHeadshots / ((float)ShotsCombined / (float)100);
            AverageBodyshotPercentage = (float)CombinedBodyshots / ((float)ShotsCombined / (float)100);
            AverageLegshotPercentage = (float)CombinedLegshots / ((float)ShotsCombined / (float)100);

            float AllIncomingDamage = 0;
            float AllOutgoingDamage = 0;

            foreach (Game item in window.GameCollection.Items)
            {
                AllIncomingDamage += (float)item.Player.Playerstats.damage_received;
                AllOutgoingDamage += (float)item.Player.Playerstats.damage_made;
            }

            

            Action invokeAction2 = new Action(() => {
                window.AverageHeadshotPercentage.Content = AverageHeadshotPercentage.ToString("#.#") + "%";
                window.AverageBodyshotPercentage.Content = AverageBodyshotPercentage.ToString("#.#") + "%";
                window.AverageLegshotPercentage.Content = AverageLegshotPercentage.ToString("#.#") + "%";
                window.AverageIncoming.Content = ((float)AllIncomingDamage / (float)window.GameCollection.Items.Count).ToString("#.#");
                window.AverageOutgoing.Content = ((float)AllOutgoingDamage / (float)window.GameCollection.Items.Count).ToString("#.#");
 
            });
            window.GameCollection.Dispatcher.Invoke(invokeAction2);
        }
    }
}
