using System;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Server
{
    public class Server : BaseScript
    {

        private string _jailReason = "";

        public Server()
        {
            Debug.Write("LOADED SERVER JAIL");
            EventHandlers["chatMessage"] += new Action<int, string, string>(HandleChatMessage);
            EventHandlers["DOJ.Jail.SendToServer"] += new Action<int, int, string>(SendToServer);
        }

        private async void HandleChatMessage(int source, string color, string message)
        {
            int _id;
            int _jailTime;
            string[] args = message.Split(' ');
            Player pSrc = new PlayerList()[source];

            if (args[0].ToLower() == "/jail")
            {
                API.CancelEvent();
                int.TryParse(args[1], out _id);
                Player pl = new PlayerList()[_id];
                if (args.Length > 1 && pl.Name != null)
                {
                    if (int.TryParse(args[2], out _jailTime))
                    {
                        if (_jailTime > 600)
                        {
                            _jailTime = 600;
                        }
                    }

                    if (_jailTime == 0)
                    {
                        pl.TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 },
                            "Jail time must be larger then zero!");
                    }
                    pl.TriggerEvent("DOJ.Jail.Command", _jailTime);
                    await ProcessReason(args);
                    foreach (Player plr in Players)
                    {
                        plr.TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 },
                            $"{pl.Name} has been jailed for {_jailTime} seconds for {_jailReason}.");
                    }

                    _jailReason = "";
                }
                else
                {
                    pSrc.TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 }, "Correct usage: /jail [id] [time] [reason]");
                }
            }
            _jailReason = "";
        }

        private async Task ProcessReason(string[] args)
        {
            for (int i = 3; i < args.Length; i++)
            {
                if (i == args.Length)
                {
                    _jailReason += args[i];
                }
                else
                {
                    _jailReason += args[i] + " ";
                }
                await Delay(10);
            }
        }

        public void SendToServer(int id, int length, string reason)
        {
            PlayerList playerlist = new PlayerList();
            Player player = playerlist[id];

            if (player.Name != null)
            {
                player.TriggerEvent("DOJ.Jail.PlayerJailed", length);

                foreach (Player pl in Players)
                {
                    pl.TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 }, $"{player.Name} has been jailed for {length} seconds for {reason}.");
                }
            }
        }
    }
}
