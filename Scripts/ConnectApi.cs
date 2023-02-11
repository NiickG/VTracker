using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ValorantNET.Models;
using static VTracker.ConnectApi;

namespace VTracker
{
    class ConnectApi
    {
        public enum ApiType
        {
            GetlastGamesMain,
            GetPlayer
        }

        public static List<GameInfo> Matches = new List<GameInfo>();
        public ConnectApi()
        {
        }
        public void newRequest(string url, ApiType apiType , string _name, string _tag)
        {
            var task = new Task(async () =>
            {
                try
                {
                    var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                    httpRequest.Accept = "application/json";


                    var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();
                        if (apiType == ApiType.GetlastGamesMain)
                        {
                            DeserializelastGames(result, _name, _tag);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return;
                }           
            });
            task.Start();
        }

        private void DeserializelastGames(string result, string _name, string _tag)
        {
            Matches.Clear();
            dynamic obj = JsonConvert.DeserializeObject(result);

            string str = ((object)obj.data[0].metadata.map).ToString();
            /*
            List<string> RR_Change = new List<string>();
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create($"https://api.henrikdev.xyz/valorant/v1/mmr-history/eu/{_name}/{_tag}");
                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result1 = streamReader.ReadToEnd();

                    dynamic rr_object = JsonConvert.DeserializeObject(result1);
                    for (int i = 0; i < 5; i++)
                    {
                        RR_Change.Add(rr_object.data[i].mmr_change_to_last_game);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            */
            int game = 0;
            foreach (var item in obj.data)
            {
                GameInfo m = new GameInfo();

                GameInfo.Team team;
                if (item.teams.red.has_won == "true")
                {
                    team = GameInfo.Team.Red;
                }
                else
                {
                    team = GameInfo.Team.Blue;
                }
                m.data = new GameInfo.Data()
                {
                    Map = item.metadata.map,
                    Red_RoundsWon = item.teams.red.rounds_won,
                    Blue_RoundsWon = item.teams.blue.rounds_won,
                    MatchId = item.metadata.matchid,
                    Cluster = item.metadata.cluster,
                    teamthatwon = team,
                    Rounds = item.metadata.rounds_played,
                    game_start = item.metadata.game_start_patched,
                    //RR_Change = RR_Change[game],
                };
                
                foreach (var player in item.players.all_players)
                {
                    GameInfo.GamePlayer p = new GameInfo.GamePlayer();

                    p.name = player.name;
                    p.tag= player.tag;
                    if (player.team == "Blue")
                    {
                        p.team= GameInfo.Team.Blue;
                    }
                    else
                    {
                        p.team = GameInfo.Team.Red;
                    }
                    
                    
                    
                    
                    p.Level = player.level;
                    p.character= player.character;
                    p.Rank = player.currenttier;
                    p.AgentImage = player.assets.agent.small;
                    p.RankImage = $"https://media.valorant-api.com/competitivetiers/03621f52-342b-cf4e-4f86-9350a49c6d04/{p.Rank}/smallicon.png";

                    dynamic agent = NewRequestTo($"https://valorant-api.com/v1/agents/{Get_AgentUUID_byName(p.character)}");

                    var abilities = agent.data.abilities;

                    foreach (var ab in abilities)
                    {
                        if (ab.slot == "Grenade")
                        {
                            p.c_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ability1")
                        {
                            p.q_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ability2")
                        {
                            p.e_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ultimate")
                        {
                            p.x_cast_Image = ab.displayIcon;
                        }
                    }

                    if (_name == p.name && _tag == p.tag)
                    {
                        p.IsMain= true;
                    }
                    else
                    {
                        p.IsMain= false;
                    }
                    p.Playerstats = new GameInfo.GamePlayer.Stats()
                    {
                        score = player.stats.score,

                        c_cast = player.ability_casts.c_cast,
                        q_cast = player.ability_casts.q_cast,
                        e_cast = player.ability_casts.e_cast,
                        x_cast = player.ability_casts.x_cast,

                        Kills = player.stats.kills,
                        deaths = player.stats.deaths,
                        assists = player.stats.assists,

                        bodyshots = player.stats.bodyshots,
                        headshots = player.stats.headshots,
                        legshots = player.stats.legshots,

                        damage_made= player.damage_made,
                        damage_received= player.damage_received,
                    };
                    p.Playerstats.KDAShort = (float)(int.Parse(p.Playerstats.Kills) + ((float)int.Parse(p.Playerstats.assists) / 2)) / (float)int.Parse(p.Playerstats.deaths);

                    m.players.Add(p);
                }
                Matches.Add(m);
                game++;
            }

            foreach (var item in Matches)
            {
                Debug.WriteLine("---------------------");
                Debug.WriteLine(item.data.teamthatwon + "/" + item.data.Map);
                Debug.WriteLine("{");
                foreach (var p in item.players)
                {
                    Debug.WriteLine($"  {p.name} --- {p.team}");
                }
                Debug.WriteLine("}");
            }

            Action invokeAction = new Action(() => {
                MainWindow.Instance.GameCollection.Items.Clear();
                for (int i = 0; i < Matches.Count; i++)
                {
                    MainWindow.Instance.GameCollection.Items.Add(new Game(Matches[i], _name, _tag));
                }
            });
            MainWindow.Instance.GameCollection.Dispatcher.Invoke(invokeAction);

            UpdateAverage();
        }
        private void UpdateAverage()
        {
            //Average KDA
            float AverageK;
            float AverageD;
            float AverageA;

            int CombinedK = 0;
            int CombinedD = 0;
            int CombinedA = 0;

            int Counted = 0;
            foreach (Game item in MainWindow.Instance.GameCollection.Items)
            {
                CombinedK += int.Parse(item.Player.Playerstats.Kills); 
                CombinedD += int.Parse(item.Player.Playerstats.deaths); 
                CombinedA += int.Parse(item.Player.Playerstats.assists);

                Counted++;
            }

            AverageK = CombinedK/Counted;
            AverageD = CombinedD/Counted;
            AverageA = CombinedA/Counted;

            Action invokeAction = new Action(() => {               
                MainWindow.Instance.AvergageKills.Content = AverageK;
                MainWindow.Instance.AvergageDeaths.Content = AverageD;
                MainWindow.Instance.AvergageAssists.Content = AverageA;
            });
            MainWindow.Instance.GameCollection.Dispatcher.Invoke(invokeAction);

            //Average HitPercentage
            float AverageHeadshotPercentage = 0;
            float AverageBodyshotPercentage = 0;
            float AverageLegshotPercentage = 0;

            int ShotsCombined = 0;

            float CombinedHeadshots = 0;
            float CombinedBodyshots = 0;
            float CombinedLegshots = 0;

            foreach (Game item in MainWindow.Instance.GameCollection.Items)
            {
                ShotsCombined += int.Parse(item.Player.Playerstats.headshots) + int.Parse(item.Player.Playerstats.bodyshots) + int.Parse(item.Player.Playerstats.legshots);

                CombinedHeadshots += int.Parse(item.Player.Playerstats.headshots);
                CombinedBodyshots += int.Parse(item.Player.Playerstats.bodyshots);
                CombinedLegshots += int.Parse(item.Player.Playerstats.legshots);
            }

            AverageHeadshotPercentage = (float)CombinedHeadshots / ((float)ShotsCombined / (float)100);
            AverageBodyshotPercentage = (float)CombinedBodyshots / ((float)ShotsCombined / (float)100);
            AverageLegshotPercentage = (float)CombinedLegshots / ((float)ShotsCombined / (float)100);

            float AllIncomingDamage = 0;
            float AllOutgoingDamage = 0;

            foreach (Game item in MainWindow.Instance.GameCollection.Items)
            {
                AllIncomingDamage += float.Parse(item.Player.Playerstats.damage_received);
                AllOutgoingDamage += float.Parse(item.Player.Playerstats.damage_made);
            }

            

            Action invokeAction2 = new Action(() => {
                MainWindow.Instance.AverageHeadshotPercentage.Content = AverageHeadshotPercentage.ToString("#.#") + "%";
                MainWindow.Instance.AverageBodyshotPercentage.Content = AverageBodyshotPercentage.ToString("#.#") + "%";
                MainWindow.Instance.AverageLegshotPercentage.Content = AverageLegshotPercentage.ToString("#.#") + "%";
                MainWindow.Instance.AverageIncoming.Content = ((float)AllIncomingDamage / (float)MainWindow.Instance.GameCollection.Items.Count).ToString("#.#");
                MainWindow.Instance.AverageOutgoing.Content = ((float)AllOutgoingDamage / (float)MainWindow.Instance.GameCollection.Items.Count).ToString("#.#");
            });
            MainWindow.Instance.GameCollection.Dispatcher.Invoke(invokeAction2);
        }
        public string Get_AgentUUID_byName(string AgentName)
        {
            switch (AgentName)
            {
                case "Fade" :    return "dade69b4-4f5a-8528-247b-219e5a1facd6";
                case "Breach":   return "5f8d3a7f-467b-97f3-062c-13acf203c006";
                case "Raze" :    return "f94c3b30-42be-e959-889c-5aa313dba261";
                case "Chamber":  return "22697a3d-45bf-8dd7-4fec-84a9e28c69d7";
                case "KAY/O":    return "601dbbe7-43ce-be57-2a40-4abd24953621";
                case "Skye":     return "6f2a04ca-43e0-be17-7f36-b3908627744d";
                case "Cypher":   return "117ed9e3-49f3-6512-3ccf-0cada7e3823b";
                case "Sova":     return "320b2a48-4d9b-a075-30f1-1f93a9b638fa";
                case "Killjoy":  return "1e58de9c-4950-5125-93e9-a0aee9f98746";
                case "Harbor":   return "95b78ed7-4637-86d9-7e41-71ba8c293152";
                case "Viper":    return "707eab51-4836-f488-046a-cda6bf494859";
                case "Phoenix":  return "eb93336a-449b-9c1b-0a54-a891f7921d69";
                case "Astra":    return "41fb69c1-4189-7b37-f117-bcaf1e96f1bf";
                case "Brimstone":return "9f0d8ba9-4140-b941-57d3-a7ad57c6b417";
                case "Neon":     return "bb2a4828-46eb-8cd1-e765-15848195d751";
                case "Yoru":     return "7f94d92c-4234-0a36-9646-3a87eb8b5c89";
                case "Sage":     return "569fdd95-4d10-43ab-ca70-79becc718b46";
                case "Reyna":    return "a3bfb853-43b2-7238-a4f1-ad90e9e46bcc";
                case "Omen":     return "8e253930-4c05-31dd-1b6c-968525494517";
                case "Jett":     return "add6443a-41bd-e414-f6ad-e58d267f4e95";
                    
            }
            return "WrongName";
        }
        public dynamic NewRequestTo(string url)
        {
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create(url);
                httpRequest.Accept = "application/json";


                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result2 = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject(result2);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Source);
                return null;
            }
        }
    }
}
