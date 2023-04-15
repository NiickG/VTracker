using System;
using System.Collections.Generic;
using VTracker.ValApi;
using static VTracker.GameInfo;

namespace VTracker
{
    public static class ConvertData
    {
        public static List<GameInfo> ConvertGames(List<dynamic> RawGames, string puuid)
        {
            List<GameInfo> Games = new List<GameInfo>();
            foreach (dynamic rawgame in RawGames)
            {
                string rawMapID = rawgame.matchInfo.mapId;
                string MapName = "";
                string MapImage = "";
                GameInfo.Team teamtThatWon;
                switch (rawMapID)
                {
                    case "/Game/Maps/Triad/Triad": 
                        MapName = "Haven";
                        MapImage = "https://media.valorant-api.com/maps/" + "2bee0dc9-4ffe-519b-1cbd-7fbe763a6047/listviewicon.png";
                        break;
                    case "/Game/Maps/Pitt/Pitt": 
                        MapName = "Pearl";
                        MapImage = "https://media.valorant-api.com/maps/" + "fd267378-4d1d-484f-ff52-77821ed10dc2/listviewicon.png";
                        break;
                    case "/Game/Maps/Port/Port": 
                        MapName = "Icebox";
                        MapImage = "https://media.valorant-api.com/maps/" + "e2ad5c54-4114-a870-9641-8ea21279579a/listviewicon.png";
                        break;
                    case "/Game/Maps/Jam/Jam": 
                        MapName = "Lotus";
                        MapImage = "https://media.valorant-api.com/maps/" + "2fe4ed3a-450a-948b-6d6b-e89a78e680a9/listviewicon.png";
                        break;
                    case "/Game/Maps/Foxtrot/Foxtrot": 
                        MapName = "Breeze";
                        MapImage = "https://media.valorant-api.com/maps/" + "2fb9a4fd-47b8-4e7d-a969-74b4046ebd53/listviewicon.png"; 
                        break;
                    case "/Game/Maps/Duality/Duality": 
                        MapName = "Bind";
                        MapImage = "https://media.valorant-api.com/maps/" + "2c9d57ec-4431-9c5e-2939-8f9ef6dd5cba/listviewicon.png"; 
                        break;
                    case "/Game/Maps/Canyon/Canyon": 
                        MapName = "Fracture";
                        MapImage = "https://media.valorant-api.com/maps/" + "b529448b-4d60-346e-e89e-00a4c527a405/listviewicon.png";
                        break;
                    case "/Game/Maps/Bonsai/Bonsai": 
                        MapName = "Split";
                        MapImage = "https://media.valorant-api.com/maps/" + "d960549e-485c-e861-8d71-aa9d1aed12a2/listviewicon.png"; 
                        break;
                    case "/Game/Maps/Ascent/Ascent": 
                        MapName = "Ascent";
                        MapImage = "https://media.valorant-api.com/maps/" + "7eaecc1b-4337-bbf6-6ab9-04b8f06b3319/listviewicon.png";
                        break;
                    default:
                    case "/Game/Maps/Poveglia/Range": 
                        MapName = "The Range";
                        MapImage = "https://media.valorant-api.com/maps/" + "ee613ee9-28b7-4beb-9666-08db13bb2244/listviewicon.png";
                        break;
                }
                GameInfo gameInfo = new GameInfo()
                {
                    Map = MapName,
                    MapImageURL = MapImage,
                    MatchId = rawgame.matchInfo.matchId,
                };
                foreach (dynamic rawTeam in rawgame.teams)
                {
                    string teamName = rawTeam.teamId;
                    if (teamName == "Red")
                    {
                        if (rawTeam.won == true)
                        {
                            gameInfo.teamthatwon = GameInfo.Team.Red;
                        }
                        gameInfo.Red_RoundsWon = rawTeam.roundsWon;
                    }
                    else if (teamName == "Blue")
                    {
                        if (rawTeam.won == true)
                        {
                            gameInfo.teamthatwon = GameInfo.Team.Blue;                            
                        }
                        gameInfo.Blue_RoundsWon = rawTeam.roundsWon;
                    }
                }
                gameInfo.Rounds = gameInfo.Blue_RoundsWon + gameInfo.Red_RoundsWon;
                GameInfo.Team MainPlayerTeam = GameInfo.Team.Blue;
                foreach (var player in rawgame.players)
                {
                    dynamic agent = ValAPI.DataRequest($"https://valorant-api.com/v1/agents/{player.characterId}").data;
                    dynamic abilities = agent.abilities;
                    GamePlayer gamePlayer = new GamePlayer()
                    {
                        name = player.gameName,
                        tag = player.tagLine,
                        puuid = player.subject,
                        Level = player.accountLevel,
                        character = agent.displayName,
                        AgentImage = agent.displayIconSmall,
                        Rank = player.competitiveTier,
                        RankImage = $"https://media.valorant-api.com/competitivetiers/03621f52-342b-cf4e-4f86-9350a49c6d04/{player.competitiveTier}/smallicon.png",
                        team = player.teamId,
                    };
                    foreach (var ab in abilities)
                    {
                        if (ab.slot == "Grenade")
                        {
                            gamePlayer.c_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ability1")
                        {
                            gamePlayer.q_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ability2")
                        {
                            gamePlayer.e_cast_Image = ab.displayIcon;
                        }
                        else if (ab.slot == "Ultimate")
                        {
                            gamePlayer.x_cast_Image = ab.displayIcon;
                        }
                    }
                    if (puuid == gamePlayer.puuid)
                    {
                        gamePlayer.IsMain = true;
                        MainPlayerTeam = gamePlayer.team;
                    }
                    else
                    {
                        gamePlayer.IsMain = false;
                    }
                    gamePlayer.Playerstats = new GamePlayer.Stats()
                    {
                        score = player.stats.score,

                        Kills = player.stats.kills,
                        deaths = player.stats.deaths,
                        assists = player.stats.assists,
                    };
                    gamePlayer.Playerstats.KDAShort = (float)(int.Parse(gamePlayer.Playerstats.Kills) + ((float)int.Parse(gamePlayer.Playerstats.assists) / 2)) / (float)int.Parse(gamePlayer.Playerstats.deaths);
                    try
                    {
                        gamePlayer.Playerstats.c_cast = player.stats.abilityCasts.grenadeCasts;         
                    }
                    catch (Exception){ gamePlayer.Playerstats.c_cast = 0; }
                    try
                    {
                        gamePlayer.Playerstats.q_cast = player.stats.abilityCasts.ability1Casts;
                    }
                    catch (Exception) { gamePlayer.Playerstats.q_cast = 0; }
                    try
                    {
                        gamePlayer.Playerstats.e_cast = player.stats.abilityCasts.ability2Casts;
                    }
                    catch (Exception) { gamePlayer.Playerstats.e_cast = 0; }
                    try
                    {
                        gamePlayer.Playerstats.x_cast = player.stats.abilityCasts.ultimateCasts;
                    }
                    catch (Exception) { gamePlayer.Playerstats.x_cast = 0; }

                    int damageMade = 0;
                    int MaxDamage = 0;
                    int MaxKills = 0;
                    int MaxMoneySpent = 0;
                    foreach (var round in rawgame.roundResults)
                    {
                        foreach (var playerstat in round.playerStats)
                        {
                            if (playerstat.subject == gamePlayer.puuid)
                            {
                                int spent = playerstat.economy.spent;
                                if (MaxMoneySpent > spent)
                                {
                                    MaxMoneySpent = spent;
                                }
                                gamePlayer.MoneySpentPerRound.Add(spent);
                                if (playerstat.kills != null)
                                {
                                    int kills = 0;
                                    foreach (var item in playerstat.kills)
                                    {
                                        if (item.victim != gamePlayer.puuid)
                                        {
                                            kills += 1;
                                        }
                                    }
                                    if (kills > MaxKills)
                                    {
                                        MaxKills = kills;
                                    }
                                    gamePlayer.KillsPerRound.Add(kills);
                                }
                                else
                                {
                                    gamePlayer.KillsPerRound.Add(0);
                                }
                                int RoundDamage = 0;
                                foreach (var item in playerstat.damage)
                                {
                                    int dmg = item.damage;

                                    int legshots = item.legshots;
                                    int bodyshots = item.bodyshots;
                                    int headshots = item.headshots;

                                    damageMade += dmg;
                                    RoundDamage += dmg;

                                    gamePlayer.Playerstats.legshots += legshots;
                                    gamePlayer.Playerstats.bodyshots += bodyshots;
                                    gamePlayer.Playerstats.headshots += headshots;
                                }
                                gamePlayer.DamagePerRound.Add(RoundDamage);
                                if (RoundDamage > MaxDamage)
                                {
                                    MaxDamage = RoundDamage;
                                }
                            }
                            foreach (var item in playerstat.damage)
                            {
                                if (item.receiver == gamePlayer.puuid)
                                {
                                    gamePlayer.Playerstats.damage_received += (int)item.damage;
                                }
                            }
                        }
                    }
                    gamePlayer.Playerstats.damage_made = damageMade;
                    gamePlayer.MaxDamage = MaxDamage;
                    gamePlayer.MaxKills = MaxKills;
                    gameInfo.players.Add(gamePlayer);
                }
                //WithMain
                foreach (var player in gameInfo.players)
                {
                    if (player.team == MainPlayerTeam)
                    {
                        player.WithMain = true;
                    }
                }

                Games.Add(gameInfo);
            }
            return Games;
        }       
    }
}
