
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;


namespace VTracker.ValApi
{
    public static class ValAPI
    {
        private static readonly string _gameVersion;
        public static string EntitlementToken { get; set; }
        public static string AccessToken { get; set; }
        public static string ClientVersion;

        public static ShortPlayerDATA MyData;

        public static string LockfilePassword;
        public static string LockfilePort;
        public static EntitlementTokens ET;

        static ValAPI()
        {
            _gameVersion = GetGameVersion();
        }
        private static string GetGameVersion()
        {
            string gameVersion = "";
            while (gameVersion == "")
            {
                if (!File.Exists(Constants.ShooterGameLogPath))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                using (var fileStream = new FileStream(Constants.ShooterGameLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fileStream))
                {
                    string line;
                    do
                    {
                        line = sr.ReadLine();
                        if (line != null && line.Contains(Constants.GameVersionLineInfo))
                        {
                            var data = line.Split(Constants.GameVersionLineInfo.ToCharArray());
                            gameVersion = data.Last();
                            break;
                        }
                    }
                    while (line != null);
                }

                if (gameVersion == null)
                {
                    Thread.Sleep(1000);
                }
            }

            return gameVersion;
        }
        public static void Login()
        {
            GetLockfile();

            dynamic dynamicET = LocalCall("/entitlements/v1/token", Method.Get);
            ET = new EntitlementTokens()
            {
                EntitlementToken = dynamicET.token,
                AccessToken = dynamicET.accessToken,
                UserPlayerId = dynamicET.subject
            };
            dynamic Client = LocalCall("/chat/v1/session", Method.Get);
            MyData = new ShortPlayerDATA()
            {
                Name = Client.game_name,
                Tag = Client.game_tag,
                puuid = Client.puuid,
                region = Client.region,
            };
        }
        public static void SendMessage(string message, messageType type)
        {
            var Body = new
            {
                cid = "",
                message = message,
                type = type.ToString(),
            };
            dynamic messageback = LocalCall("/chat/v6/messages", Method.Post, JsonConvert.SerializeObject(Body));
        }
        public static List<StoreOffer> GetStoreOffers(out bool isNightmarket, out List<NightStoreOffer> nightmarketOffers)
        {
            isNightmarket = false;
            List<StoreOffer> offers = new List<StoreOffer>();

            dynamic offersData = RemoteCall($"https://pd.eu.a.pvp.net/store/v2/storefront/{MyData.puuid}",Method.Get);

            foreach (var item in offersData.SkinsPanelLayout.SingleItemStoreOffers)
            {
                dynamic Skin = DataRequest($"https://valorant-api.com/v1/weapons/skinlevels/{item.Rewards[0].ItemID}").data;
                JObject jObject = JObject.FromObject(item.Cost);
                StoreOffer so = new StoreOffer()
                {
                    Name = Skin.displayName,
                    Image = Skin.displayIcon,
                    Cost = jObject.First.ToString().Split(':')[1],                
                };
                so.Video = new Uri("https://valorant.dyn.riotcdn.net/x/videos/release-06.06/197511ac-4d5a-97fc-78f8-feb5d090dd20_default_universal.mp4");

                var mediaPlayer = new MediaPlayer();
                mediaPlayer.Play();

                try
                {

                    if (Skin.streamedVideo != null)
                    {
                        so.Video = new Uri(Skin.streamedVideo);
                    }
                }
                catch (Exception){}             
                offers.Add(so);                
            }
            List<NightStoreOffer> nightOffers = new List<NightStoreOffer>();
            try
            {
                foreach (var item in offersData.BonusStore.BonusStoreOffers)
                {
                    dynamic Skin = DataRequest($"https://valorant-api.com/v1/weapons/skinlevels/{item.Offer.Rewards[0].ItemID}").data;
                    JObject jCost = JObject.FromObject(item.Offer.Cost);
                    JObject jDiscountCost = JObject.FromObject(item.DiscountCosts);
                    NightStoreOffer so = new NightStoreOffer()
                    {
                        Name = Skin.displayName,
                        Image = Skin.displayIcon,
                        Cost = jCost.First.ToString().Split(':')[1],
                        DiscountCosts = jDiscountCost.First.ToString().Split(':')[1],
                        DiscountPercent = "-" + item.DiscountPercent + "%",
                    };

                    var mediaPlayer = new MediaPlayer();
                    mediaPlayer.Play();

                    try
                    {

                        if (Skin.streamedVideo != null)
                        {
                            so.Video = new Uri(Skin.streamedVideo);
                        }
                    }
                    catch (Exception) { }
                    nightOffers.Add(so);
                }
                isNightmarket = true;
            }
            catch (Exception)
            {

                throw;
            }
            nightmarketOffers = nightOffers;
            return offers;
        }
        static void PrintJObjects(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                Debug.WriteLine($"{property.Name}: {property.Value}");

                if (property.Value is JObject)
                {
                    PrintJObjects((JObject)property.Value);
                }
            }
        }
        public static dynamic DataRequest(string url)
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(url);
            httpRequest.Accept = "application/json";
            var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
            var steamreader = new StreamReader(httpResponse.GetResponseStream());
            return JsonConvert.DeserializeObject(steamreader.ReadToEnd());
        }
        public static List<ShortPlayerDATA> GetFriends()
        {
            dynamic friends = LocalCall("/chat/v4/friends", Method.Get);

            List<ShortPlayerDATA> _friends = new List<ShortPlayerDATA>();
            foreach (var friend in friends.friends)
            {
                _friends.Add(new ShortPlayerDATA
                {
                    Name = friend.game_name,
                    Tag = friend.game_tag,
                    puuid = friend.puuid,

                    region = friend.region,
                    NameAndTagDisplay = $"{friend.game_name}#{friend.game_tag}"
                });
            }
            Debug.WriteLine("--Friends--");
            foreach (var item in _friends)
            {
                Debug.WriteLine($"{item.Name}#{item.Tag} --- {item.puuid}");
            }
            return _friends;
        }
        public static List<dynamic> GetLatestGames(string puuid, string region)
        {
            List<dynamic> matches = new List<dynamic>();
            try
            {
                string shard = region;

                if (region == "latam" || region == "br")
                {
                    shard = "na";
                }

                dynamic RecentMatches = RemoteCall($"https://pd.{shard}.a.pvp.net/match-history/v1/history/{puuid}?queue=competitive", Method.Get);
                foreach (var RecentMatch in RecentMatches.History)
                {
                    dynamic match = RemoteCall($"https://pd.{shard}.a.pvp.net/match-details/v1/matches/{RecentMatch.MatchID}", Method.Get);
                    string s = match.matchInfo.isRanked;
                    matches.Add(match);
                }
            }
            catch (Exception){}
            return matches;
        }
        public static async Task<bool> GetPlayerHistoryAsync(string puuid, string region)
        {
            string shard = region;

            if (region == "latam" || region == "br")
            {
                shard = "na";
            }
            Debug.WriteLine(puuid);
            var response = await DoCachedRequestAsync(Method.Get,
               $"https://pd.{shard}.a.pvp.net/mmr/v1/players/{puuid}",
               true).ConfigureAwait(false);
            Debug.WriteLine("::::::::::::::::: "+response.Content);

            return false;
        }
        public static void GetCurrentGame(string player_puuid, string reg, string shard)
        {
            dynamic currentPlayerGame = RemoteCall($"https://glz-{reg}-{shard}.a.pvp.net/core-game/v1/players/{player_puuid}", Method.Get, null, RemoteType.CurrentGame);
            string MatchID = currentPlayerGame.MatchID;

            dynamic current_game = RemoteCall($"https://glz-{reg}-{shard}.a.pvp.net/core-game/v1/matches/{MatchID}", Method.Get,null, RemoteType.CurrentGame);
            foreach (dynamic item in current_game.Players)
            {
                string puuid = item.Subject;
                Debug.WriteLine(puuid);
            }
        }
        private static dynamic LocalCall(string endpoint, Method method, object body = null)
        {
            var url = $"https://127.0.0.1:{LockfilePort}{endpoint}";
            var restClient = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            });

            var request = new RestRequest(new Uri(url), method);
            if (body != null)
            {
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }
            request.AddHeader("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"riot:{LockfilePassword}"))}");
            request.AddHeader("X-Riot-ClientPlatform", Constants.ClientPlatform);
            request.AddHeader("X-Riot-ClientVersion", _gameVersion);

            var response = restClient.Execute(request);
            return JsonConvert.DeserializeObject(response.Content);
        }
        public static dynamic RemoteCall(string url, Method method, object body = null, RemoteType type = RemoteType.None)
        {
            var restClient = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            });

            var request = new RestRequest(new Uri(url), method);
            if (body != null)
            {
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }
            if (type == RemoteType.None)
            {
                request.AddHeader("Authorization", $"Bearer {ET.AccessToken}");
                request.AddHeader("X-Riot-Entitlements-JWT", ET.EntitlementToken);
                request.AddHeader("X-Riot-ClientPlatform", Constants.ClientPlatform);
                request.AddHeader("X-Riot-ClientVersion", _gameVersion);
            }
            else if (type == RemoteType.CurrentGame)
            {
                request.AddHeader("Authorization", $"Bearer {ET.AccessToken}");
                request.AddHeader("X-Riot-Entitlements-JWT", ET.EntitlementToken);
            }
            var response = restClient.Execute(request);
            Debug.WriteLine(url);
            Debug.WriteLine(response.StatusCode);
            Debug.WriteLine(response.Content);
            return JsonConvert.DeserializeObject(response.Content);
        }
        public static async Task<RestResponse> DoCachedRequestAsync(Method method, string url, bool addRiotAuth)
        {
            var client = new RestClient(url);
            var request = new RestRequest();
            if (addRiotAuth)
            {
                request.AddHeader("X-Riot-Entitlements-JWT", ET.EntitlementToken);
                request.AddHeader("Authorization", $"Bearer {ET.AccessToken}");
                request.AddHeader("X-Riot-ClientPlatform",
                    "ew0KCSJwbGF0Zm9ybVR5cGUiOiAiUEMiLA0KCSJwbGF0Zm9ybU9TIjogIldpbmRvd3MiLA0KCSJwbGF0Zm9ybU9TVmVyc2lvbiI6ICIxMC4wLjE5MDQyLjEuMjU2LjY0Yml0IiwNCgkicGxhdGZvcm1DaGlwc2V0IjogIlVua25vd24iDQp9");
                request.AddHeader("X-Riot-ClientVersion", _gameVersion);
            }

            var response = await client.ExecuteAsync(request, method).ConfigureAwait(false);
            return response;
        }
        public static void GetLockfile()
        {
            string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            FileStream fileStream = new FileStream(localAppDataPath + "/Riot Games/Riot Client/Config/lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            StreamReader reader = new StreamReader(fileStream);

            string[] fileContents = reader.ReadToEnd().Split(':');

            reader.Close();
            fileStream.Close();

            Console.WriteLine(fileContents);

            LockfilePassword = fileContents[3];
            LockfilePort = fileContents[2];
        }
        public static async Task<string> GetPlayerByNameAndTag(string name,string tag)
        {
            try
            {
                var httpRequest = (HttpWebRequest)WebRequest.Create($"https://api.henrikdev.xyz/valorant/v1/account/{name}/{tag}");
                httpRequest.Accept = "application/json";


                var httpResponse = await httpRequest.GetResponseAsync();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result2 = streamReader.ReadToEnd();
                    dynamic player = JsonConvert.DeserializeObject(result2);
                    return player.data.puuid;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Source);
                return "";
            }
        }

        public static async Task<LiveGameResponse> CheckMatchAsync(string player_puuid, string reg, string shard)
        {
            var client = new RestClient($"https://glz-{reg}-1.{shard}.a.pvp.net/core-game/v1/players/{player_puuid}");
            var request = new RestRequest();
            request.AddHeader("X-Riot-Entitlements-JWT", ET.EntitlementToken);
            request.AddHeader("Authorization", $"Bearer {ET.AccessToken}");
            var response = await client.ExecuteGetAsync(request).ConfigureAwait(false);

            LiveGameResponse TaskResponse = new LiveGameResponse();

            if (response.IsSuccessful)
            {
                Debug.WriteLine(response.Content);
                string responsePlayerGame = JsonConvert.DeserializeObject<dynamic>(response.Content).MatchID;

                client = new RestClient($"https://glz-{reg}-1.{shard}.a.pvp.net/core-game/v1/matches/{responsePlayerGame}"); 
                response = await client.ExecuteGetAsync(request).ConfigureAwait(false);
                if (response.IsSuccessful)
                {
                    Debug.WriteLine(response.Content);

                    TaskResponse.isMatch = true;
                    TaskResponse.type = LiveGameResponseType.Game;
                    TaskResponse.response = JsonConvert.DeserializeObject(response.Content);
                    TaskResponse.MatchID = responsePlayerGame;
                    return TaskResponse;
                }            
            }
            else
            {
                client = new RestClient($"https://glz-{shard}-1.{reg}.a.pvp.net/pregame/v1/players/{player_puuid}");
                response = await client.ExecuteGetAsync(request).ConfigureAwait(false);
                if (response.IsSuccessful)
                {
                    string matchID_prematch = JsonConvert.DeserializeObject<dynamic>(response.Content).MatchID;
                    client = new RestClient($"https://glz-{reg}-1.{shard}.a.pvp.net/pregame/v1/matches/{matchID_prematch}");
                    response = await client.ExecuteGetAsync(request).ConfigureAwait(false);
                    if (response.IsSuccessful)
                    {
                        TaskResponse.isMatch = true;
                        TaskResponse.type = LiveGameResponseType.Pregame;
                        TaskResponse.MatchID = matchID_prematch;
                        TaskResponse.response = JsonConvert.DeserializeObject(response.Content);
                        Debug.WriteLine(response.Content);
                        return TaskResponse;
                    }
                }
            }

            TaskResponse.isMatch = false;
            return TaskResponse;
        }
    }
}
