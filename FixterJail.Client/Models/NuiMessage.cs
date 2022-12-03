using System.Runtime.Serialization;

namespace FixterJail.Client.Models
{

    [DataContract]
    public class NuiMessage
    {
        [DataMember(Name = "type")]
        public string Type;

        public NuiMessage(string type)
        {
            Type = type;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
