using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media;

namespace VTracker
{
    public class MMRChange
    {
        public int Value { get; set; }
        public SolidColorBrush BackroundColor { get; set; }  
        public MMRChange(int value)
        {
            BrushConverter convert = new BrushConverter();
            Value = value;
            if (value <0)
            {
                BackroundColor = (SolidColorBrush)convert.ConvertFrom("#ff4654");
            }
            else if (value > 0)
            {
                BackroundColor = (SolidColorBrush)convert.ConvertFrom("#32e2b2");
            }
            else if (value == 0)
            {
                BackroundColor = (SolidColorBrush)convert.ConvertFrom("#7f7f7f");
            }
        }
    }
    public class Party
    {
        public string PartyID;
        public List<string> PartyPUUIDs = new List<string>();
        public bool IsParty = false;
    }
    public class ShortMatchHistoryInfo
    {
        public string AverageKDA { get; set; }
    }
    public enum region
    {
        na,
        latam,
        br,
        eu,
        ap,
        kr,
        pbe
    }
    public enum messageType
    {
        groupchat,
        chat,
        system,
    }
    public class ShortPlayerDATA
    {
        public string Name;
        public string Tag;

        public string puuid;

        public string region;

        public string shard;

        public string NameAndTagDisplay { get; set; }
    }
    public class NameServiceResponse
    {
        [JsonPropertyName("DisplayName")] public string DisplayName { get; set; }

        [JsonPropertyName("Subject")] public string Subject { get; set; }

        [JsonPropertyName("GameName")] public string GameName { get; set; }

        [JsonPropertyName("TagLine")] public string TagLine { get; set; }
    }
    public class StoreOffer
    {
        public string Name { get; set; }
        public string Cost { get; set; }

        public string Image { get; set; }
        public Uri Video { get; set; }
    }
    public class NightStoreOffer
    {
        public string Name { get; set; }
        public string Cost { get; set; }
        public string DiscountCosts { get; set; }
        public string DiscountPercent { get; set; }

        public string Image { get; set; }
        public Uri Video { get; set; }
    }
    public enum RemoteType
    {
        None,
        CurrentGame,
    }
    public class LiveGameResponse
    {
        public bool isMatch;
        public dynamic response;

        public string MatchID;

        public LiveGameResponseType type;
    }
    public class SkinData
    {
        public string VandalImage = "";
        public string PhantomImage = "";

        public string CharacterID = "";
    }
    public enum LiveGameResponseType
    {
        Game,
        Pregame
    }
}
