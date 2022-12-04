using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FixterJail.Server
{
    public class Main : BaseScript
    {
        int[] CHAT_COLOR_ERROR = new[] { 255, 0, 76 };
        int[] CHAT_COLOR_SUCCESS = new[] { 0, 100, 255 };

        PlayerList _playerList;
        int _maximumJailTime = 300;

        public Main()
        {
            _playerList = Players;

            if (int.TryParse(GetResourceMetadata(GetCurrentResourceName(), "max_jail_time", 0), out int maximumJailTime))
            {
                _maximumJailTime = maximumJailTime;
            }

            EventHandlers["fixterjail:jail:incarcerate"] += new Action<Player, int, int, string>(OnIncarcerateEvent);
            EventHandlers["fixterjail:jail:connect"] += new Action<Player>(OnConnect);
            EventHandlers["fixterjail:jail:playerReleased"] += new Action<Player>(OnPlayerReleased);

            Debug.WriteLine("LOADED SERVER JAIL");
        }

        private void OnConnect([FromSource] Player player)
        {
            bool isCommandAllowed = IsPlayerAceAllowed(player.Handle, "command.jail");
            player.TriggerEvent("fixterjail:jail:useNui", isCommandAllowed);
        }

        private void OnIncarcerateEvent([FromSource] Player player, int playerId, int jailTime, string reason)
        {
            if (!IsPlayerAceAllowed(player.Handle, "command.jail"))
            {
                SendChatError(player, "Player does not have permissions to jail.");
                return;
            }

            IncarceratePlayer(player, playerId, jailTime, reason);
        }

        private void OnPlayerReleased([FromSource] Player playerBeingReleased)
        {
            SentChatMessageToAll($"{playerBeingReleased.Name} has been released from jail.");
        }

        private void SendChatError(Player player, string message)
        {
            player.TriggerEvent("chatMessage", "[Judge]", CHAT_COLOR_ERROR, message);
        }

        private void SentChatMessageToAll(string message)
        {
            TriggerClientEvent("chatMessage", "[Judge]", CHAT_COLOR_SUCCESS, message);
        }

        [Command("unjail", Restricted = true)]
        private void UnjailCommand([FromSource] Player player, string[] args)
        {
            int playerId;
            
            if (args.Length == 0)
            {
                SendChatError(player, "Invalid Arguments. Usage: /unjail [id]");
                return;
            }

            if (!int.TryParse(args[0], out playerId))
            {
                SendChatError(player, "Invalid Player ID.");
                return;
            }

            ReleasePlayer(player, playerId);
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

            if (!int.TryParse(args[1], out jailTime))
            {
                SendChatError(player, "Invalid Jail Time, must be a number.");
                return;
            }

            for (int i = 2; i < args.Length; i++)
            {
                jailReason += args[i] + " ";
            }

            IncarceratePlayer(player, playerId, jailTime, jailReason);
        }

        private void IncarceratePlayer(Player playerWhoSentCommand, int playerId, int jailTime, string jailReason)
        {

            Player playerToJail = _playerList[playerId];

            if (playerToJail == null)
            {
                SendChatError(playerWhoSentCommand, "Invalid Player ID, Player not found.");
                return;
            }

            if (jailTime == 0)
            {
                SendChatError(playerWhoSentCommand, "Jail time must be greater than 0.");
                return;
            }

            jailTime = jailTime > _maximumJailTime ? _maximumJailTime : jailTime;

            playerToJail.TriggerEvent("fixterjail:jail:imprison", jailTime);
            SentChatMessageToAll($"{playerToJail.Name} has been jailed for {jailTime} seconds for '{jailReason}'.");
        }

        private void ReleasePlayer(Player playerWhoSentCommand, int playerId)
        {
            Player playerToRelease = _playerList[playerId];

            if (playerToRelease == null)
            {
                SendChatError(playerWhoSentCommand, "Invalid Player ID, Player not found.");
                return;
            }

            playerToRelease.TriggerEvent("fixterjail:jail:release");
        }
    }
}