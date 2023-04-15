using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

using VTracker.ValApi;

namespace VTracker
{
    /// <summary>
    /// Interaktionslogik für LiveTracker.xaml
    /// </summary>
    public partial class LiveTracker : Window
    {
        private int TimerValue = 15;
        private string this_puuid;
        public LiveTracker(string puuid)
        {
            this_puuid = puuid;
            InitializeComponent();

            PlayerTeamList.Items.Clear();
            EnemyTeamList.Items.Clear();
            TryAndGetData(puuid);

            System.Timers.Timer aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += TimerTick;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
            aTimer.Start();
        }
        private void TimerTick(object sender, EventArgs e)
        {
            TimerValue--;
            if (TimerValue == 0)
            {
                TimerValue = 15;
                Action invokeAction = new Action(() => {
                    TryAndGetData(this_puuid);
                });
                Dispatcher.Invoke(invokeAction);
            }
            Action invokeAction1 = new Action(() => {
                TimerToNextRequest.Content = TimerValue + "s";
            });
            Dispatcher.Invoke(invokeAction1);
        }
        private async void TryAndGetData(string puuid)
        {
            LiveGameResponse match = await ValAPI.CheckMatchAsync(puuid, "eu", "eu");
            if (!match.isMatch)
            {
                PlayerTeamList.Items.Clear();
                EnemyTeamList.Items.Clear();
                return;
            }
            
            string Gamemode = "";
            List<LiveMatchPlayer> Players = new List<LiveMatchPlayer>();
            if (match.type == LiveGameResponseType.Game)
            {
                if (PlayerTeamList.Items.Count > 0)
                {
                    LiveMatchPlayer player = ((LiveMatchPlayer)PlayerTeamList.Items[0]);
                    if (player.hasStarted && player.MatchID == match.MatchID)
                    {
                        return;
                    }
                }
                #region inMatch
                Gamemode = match.response.ModeID;
                PlayerTeamList.Items.Clear();
                foreach (var item in match.response.Players)
                {
                    LiveMatchPlayer player = new LiveMatchPlayer();
                    player.puuid = item.PlayerIdentity.Subject;
                    player.AccountLevel = item.PlayerIdentity.AccountLevel;
                    player.MatchID = match.MatchID;
                    player.hasStarted = true;

                    Task<string> NameAndTagTask = GetNameServiceGetUsernameAsync(player.puuid, "eu");
                    NameAndTagTask.Wait();
                    player.NameAndTag = NameAndTagTask.Result;
                    dynamic agent = ValAPI.DataRequest($"https://valorant-api.com/v1/agents/{item.CharacterID}").data;
                    player.AgentImage = agent.displayIconSmall;

                    try { player.Team = item?.TeamID; }
                    catch (Exception) { }


                    //bool d = await ValAPI.GetPlayerHistoryAsync(player.puuid, "eu");

                    RestResponse mmrResponse = await ValAPI.DoCachedRequestAsync(Method.Get,
                $"https://pd.{"eu"}.a.pvp.net/mmr/v1/players/{player.puuid}/competitiveupdates?endIndex=10&queue=competitive",
                true);
                    Debug.WriteLine(mmrResponse.Content);
                    dynamic rawMMR = JsonConvert.DeserializeObject(mmrResponse.Content);
                    try
                    {
                        player.RankTier = rawMMR.Matches[0].TierAfterUpdate;
                        player.RR_InRank = rawMMR.Matches[0].RankedRatingAfterUpdate;
                    }
                    catch (Exception)
                    {
                        player.RankTier = "0";
                        player.RR_InRank = 0;
                    }
                    player.RankImage = $"https://media.valorant-api.com/competitivetiers/03621f52-342b-cf4e-4f86-9350a49c6d04/{player.RankTier}/smallicon.png";
                    player.MMRChanges = new List<MMRChange>();
                    foreach (var mmrChange in rawMMR.Matches)
                    {
                        player.MMRChanges.Add(new MMRChange((int)mmrChange.RankedRatingEarned));
                    }
                    player.MMRChanges.Reverse();
                    Players.Add(player);
                }
                PlayerTeamList.Items.Clear();
                EnemyTeamList.Items.Clear();
                #endregion
            }
            else
            {
                #region inPreMatch
                Gamemode = match.response.Mode;
                PlayerTeamList.Items.Clear();
                foreach (var Team in match.response.Teams)
                {
                    GameInfo.Team teamId = Team.TeamID;
                    foreach (var item in Team.Players)
                    {
                        LiveMatchPlayer player = new LiveMatchPlayer();
                        player.puuid = item.Subject;
                        player.AccountLevel = item.PlayerIdentity.AccountLevel;
                        player.MatchID = match.MatchID;
                        player.hasStarted = false;

                        Task<string> NameAndTagTask = GetNameServiceGetUsernameAsync(player.puuid, "eu");
                        NameAndTagTask.Wait();
                        player.NameAndTag = NameAndTagTask.Result;
                        if (item.CharacterID != "")
                        {
                            dynamic agent = ValAPI.DataRequest($"https://valorant-api.com/v1/agents/{item.CharacterID}").data;
                            player.AgentImage = agent.displayIconSmall;
                        }                       
                        player.Team = teamId;

                        RestResponse mmrResponse = await ValAPI.DoCachedRequestAsync(Method.Get,
                    $"https://pd.{"eu"}.a.pvp.net/mmr/v1/players/{player.puuid}/competitiveupdates?endIndex=10&queue=competitive",
                    true);
                        Debug.WriteLine(mmrResponse.Content);
                        dynamic rawMMR = JsonConvert.DeserializeObject(mmrResponse.Content);
                        try
                        {
                            player.RankTier = rawMMR.Matches[0].TierAfterUpdate;
                            player.RR_InRank = rawMMR.Matches[0].RankedRatingAfterUpdate;
                        }
                        catch (Exception)
                        {
                            player.RankTier = "0";
                            player.RR_InRank = 0;
                        }
                        player.RankImage = $"https://media.valorant-api.com/competitivetiers/03621f52-342b-cf4e-4f86-9350a49c6d04/{player.RankTier}/smallicon.png";
                        player.MMRChanges = new List<MMRChange>();
                        foreach (var mmrChange in rawMMR.Matches)
                        {
                            player.MMRChanges.Add(new MMRChange((int)mmrChange.RankedRatingEarned));
                        }
                        player.MMRChanges.Reverse();
                        Players.Add(player);
                    }
                }
               
                PlayerTeamList.Items.Clear();
                EnemyTeamList.Items.Clear();
                #endregion
            }

            if (Gamemode.Contains("/Game/GameModes/Deathmatch/"))
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (i <= 5)
                    {
                        PlayerTeamList.Items.Add(Players[i]);
                    }
                    else
                    {
                        EnemyTeamList.Items.Add(Players[i]);
                    }
                }
            }
            else
            {
                foreach (var item in Players)
                {
                    if (item.Team == GameInfo.Team.Blue)
                    {
                        PlayerTeamList.Items.Add(item);
                    }
                    else
                    {
                        EnemyTeamList.Items.Add(item);
                    }
                }
            }
        }
        public static async Task<string> GetNameServiceGetUsernameAsync(string puuid, string reg)
        {
            if (puuid == "") return null;
            var options = new RestClientOptions(new Uri($"https://pd.{reg}.a.pvp.net/name-service/v2/players"))
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            var client = new RestClient(options);
            RestRequest request = new RestRequest()
            {
                RequestFormat = RestSharp.DataFormat.Json
            };

            string[] body = { puuid.ToString() };
            request.AddJsonBody(body);
            var response = await client.ExecutePutAsync(request).ConfigureAwait(false);
            if (response.IsSuccessful)
                try
                {
                    var incorrectContent = response.Content.Replace("[", string.Empty).Replace("]", string.Empty).Replace("\n", string.Empty);
                    dynamic content = JsonConvert.DeserializeObject(incorrectContent);
                    return content.GameName + "#" + content.TagLine;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("GetNameServiceGetUsernameAsync Failed: {e}", e);
                    return "";
                }
            return "";
        }
        private void TopBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MainWindow.Instance != null)
            {
                MainWindow.Instance.liveTracker = null;
            }
            Close();
        }
    }      
}
