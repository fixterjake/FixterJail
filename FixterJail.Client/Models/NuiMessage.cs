using System.Runtime.Serialization;

namespace FixterJail.Client.Models
{

    [DataContract]
    public class NuiMessage
    {
        [DataMember(Name = "type")]
        public string Type;

        [DataMember(Name = "players", EmitDefaultValue = false)]
        public Dictionary<int, string>? Players;

        public NuiMessage(string type)
        {
            Type = type;
        }

        public NuiMessage(string type, Dictionary<int, string> players)
        {
            Type = type;
            Players = players;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
