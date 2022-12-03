using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Server
{
    public class Server : BaseScript
    {
        int[] CHAT_COLOR_ERROR = new[] { 0, 100, 255 };

        PlayerList _playerList;
        int _maximumJailTime = 300;

        public Server()
        {
            _playerList = Players;

            if (int.TryParse(GetResourceMetadata(GetCurrentResourceName(), "max_jail_time", 0), out int maximumJailTime))
            {
                _maximumJailTime = maximumJailTime;
            }

            Debug.WriteLine("LOADED SERVER JAIL");
        }

        private void SendChatError(Player player, string message)
        {
            player.TriggerEvent("chatMessage", "[Judge]", CHAT_COLOR_ERROR, message);
        }

        private void SentChatMessageToAll(string message)
        {
            TriggerClientEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 }, message);
        }

        [Command("jail", Restricted = true)]
        private void JailCommand([FromSource] Player player, string[] args)
        {
            int playerId;
            int jailTime;
            string jailReason = "";

            if (args.Length < 3)
            {
                SendChatError(player, "Invalid Arguments. Usage: /jail [id] [time] [reason]");
                return;
            }

            if (!int.TryParse(args[0], out playerId))
            {
                SendChatError(player, "Invalid Player ID.");
                return;
            }

            Player playerToJail = _playerList[playerId];

            if (playerToJail == null)
            {
                SendChatError(player, "Invalid Player ID, Player not found.");
                return;
            }

            if (!int.TryParse(args[1], out jailTime))
            {
                SendChatError(player, "Invalid Jail Time, must be a number.");
                return;
            }

            if (jailTime == 0)
            {
                SendChatError(player, "Jail time must be greater than 0.");
                return;
            }

            jailTime = jailTime > _maximumJailTime ? _maximumJailTime : jailTime;

            for (int i = 2; i < args.Length; i++)
            {
                jailReason += args[i] + " ";
            }

            playerToJail.TriggerEvent("DOJ.Jail.PlayerJailed", jailTime);
            SentChatMessageToAll($"{playerToJail.Name} has been jailed for {jailTime} seconds for '{jailReason}'.");
        }
    }
}
