using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using VTracker.ValApi;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace VTracker
{
    public class LiveMatchPlayer:INotifyPropertyChanged
    {
        public string NameAndTag { get; set; }

        public string RankImage { get; set; }
        public int AccountLevel { get; set; }
        public string AccountLevelString
        {
            get
            {
                return "lvl"+AccountLevel;
            }
            set
            {
                AccountLevelString = value;
            }
        }
        public string RankTier { get; set; }
        public double RR_InRank { get; set; }
        private string _SmurfScore;
        public string SmurfScore {
            get
            {
                return _SmurfScore;
            }
            set
            {
                _SmurfScore = value;
                OnPropertyChanged();
            }
        }

        private string _AverageKDA;
        public string AverageKDA {
            get
            {
                return _AverageKDA;
            }
            set
            {
                _AverageKDA = value;
                OnPropertyChanged();
            }
        }
        public List<MMRChange> MMRChanges { get; set; }
        public string AgentImage { get; set; }
        public string puuid { get; set; }
        public GameInfo.Team Team { get; set; }
        public string MatchID { get; set; }
        public bool hasStarted = false;

        private Visibility _GetHistoryInfoButtonVisibility;
        public Visibility GetHistoryInfoButtonVisibility 
        {
            get
            {
                return _GetHistoryInfoButtonVisibility;
            }
            set
            {
                _GetHistoryInfoButtonVisibility = value;
                OnPropertyChanged();
            }
        }

        private ICommand _GetShortHistoryInfo;
        public ICommand GetShortHistoryInfo
        {
            get
            {
                return _GetShortHistoryInfo ?? (_GetShortHistoryInfo = new CommandHandler(() => UpdateHistoryStats(), () => true));
            }
        }
        private ICommand _OpenInNew;
        public ICommand OpenInNew
        {
            get
            {
                return _OpenInNew ?? (_OpenInNew = new CommandHandler(() => OpenInNewWindow(), () => true));
            }
        }
        private async void UpdateHistoryStats()
        {
            GetHistoryInfoButtonVisibility = Visibility.Hidden;
            ShortMatchHistoryInfo history = await GetLastMatchesData(puuid);

            AverageKDA = history.AverageKDA;

            SmurfScore = CalculateSmurfScore();
        }
        private string CalculateSmurfScore()
        {
            double _SmurfScore = 0;
            int wins = 0;
            int loses = 0;
            int draws = 0;
            foreach (var item in MMRChanges)
            {
                //RRamount
                if (item.Value > 24 || (item.Value > -16 && item.Value <0))
                {
                    _SmurfScore += (double)30/MMRChanges.Count;
                }
                //WinRate
                if (item.Value > 10) 
                {
                    wins++;
                }
                else if (item.Value <0)
                {
                    loses++;
                }
                else { draws++; }
            }
            float winRate = (float)wins / ((float)wins + loses + draws / 100) * 100;
            if (winRate > 50)
            {
                _SmurfScore += 10;
            }
            else if (winRate > 60)
            {
                _SmurfScore += 20;
            }
            else if(winRate > 75)
            {
                _SmurfScore += 30;
            }

            _SmurfScore = Math.Round(_SmurfScore,2);

            if (_SmurfScore == 0)
            {
                return "";
            }
            else
            {
                return _SmurfScore.ToString();
            }
        }
        private void OpenInNewWindow()
        {
            try
            {
                string[] nameTag = NameAndTag.Split('#');
                if (MainWindow.Instance != null)
                {
                    MainWindow.Instance.GetPlayer(nameTag[0], nameTag[1]);
                }
                else
                {
                    new MainWindow().Show();
                    MainWindow.Instance.GetPlayer(nameTag[0], nameTag[1]);
                }
            }
            catch (Exception){}      
        }
        public static async Task<ShortMatchHistoryInfo> GetLastMatchesData(string puuid)
        {
            ShortMatchHistoryInfo sMatchHistoryInfo = new ShortMatchHistoryInfo();

            dynamic raw = ValAPI.GetLatestGames(puuid, "eu");

            int Kills = 0;
            int Deaths = 0;
            int Assists = 0;
            foreach (var game in raw)
            {
                foreach (var player in game.players)
                {
                    if (player.subject == puuid)
                    {
                        try
                        {
                            Kills += (int)player.stats.kills;
                            Deaths += (int)player.stats.deaths;
                            Assists += (int)player.stats.assists;
                        }
                        catch (Exception) { }
                    }
                }
            }

            sMatchHistoryInfo.AverageKDA = $"{(float)Kills / raw.Count}/{(float)Deaths / raw.Count}/{(float)Assists / raw.Count}";
            Debug.WriteLine(sMatchHistoryInfo.AverageKDA);
            return sMatchHistoryInfo;
        }

        //PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
