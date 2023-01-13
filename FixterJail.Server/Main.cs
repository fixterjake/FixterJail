using FixterJail.Shared.Models;
using FixterJail.Shared.Scripts;

namespace FixterJail.Server
{
    public class Main : BaseScript
    {
        int[] CHAT_COLOR_ERROR = new[] { 255, 0, 76 };
        int[] CHAT_COLOR_SUCCESS = new[] { 0, 100, 255 };

        PlayerList _playerList;
        int _maximumJailTime = 600;
        Config _config;

        public Main()
        {
            _playerList = Players;

            _config = Configuration.Get;
            _maximumJailTime = _config.JailTimeMaximum;

            EventHandlers["fixterjail:jail:incarcerate"] += new Action<Player, int, int, string>(OnIncarcerateEvent);
            EventHandlers["fixterjail:jail:connect"] += new Action<Player>(OnConnect);
            EventHandlers["fixterjail:jail:playerReleased"] += new Action<Player>(OnPlayerReleased);

            RegisterCommand("jail", new Action<int, List<object>, string>(OnJailCommand), true);
            RegisterCommand("unjail", new Action<int, List<object>, string>(OnUnjailCommand), true);

            Debug.WriteLine("LOADED SERVER JAIL");
        }

        private void OnConnect([FromSource] Player player)
        {
            bool isCommandAllowed = IsPlayerAceAllowed(player.Handle, "command.jail");
            Debug.WriteLine($"^5Player {player.Name} connected. Is command.jail allowed: {isCommandAllowed}^7");
            player.TriggerEvent("fixterjail:jail:useNui", isCommandAllowed);
        }

        private void OnIncarcerateEvent([FromSource] Player player, int playerId, int jailTime, string reason)
        {
            bool isCommandAllowed = IsPlayerAceAllowed(player.Handle, "command.jail");
            if (!isCommandAllowed)
            {
                Debug.WriteLine($"^1Player {player.Name} tried to incarcerate. Is command.jail allowed: {isCommandAllowed}^7");
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

        private void OnUnjailCommand(int source, List<object> args, string raw)
        {
            int playerId;
            Player player = _playerList[source];

            if (args.Count == 0)
            {
                SendChatError(player, "Invalid Arguments. Usage: /unjail [id]");
                return;
            }

            if (!int.TryParse($"{args.ElementAt(0)}", out playerId))
            {
                SendChatError(player, "Invalid Player ID.");
                return;
            }

            ReleasePlayer(player, playerId);
        }

        private void OnJailCommand(int source, List<object> args, string raw)
        {
            int playerId;
            int jailTime;
            string jailReason = "";
            Player player = _playerList[source];

            if (args.Count < 3)
            {
                SendChatError(player, "Invalid Arguments. Usage: /jail [id] [time] [reason]");
                return;
            }

            if (!int.TryParse($"{args.ElementAt(0)}", out playerId))
            {
                SendChatError(player, "Invalid Player ID.");
                return;
            }

            if (!int.TryParse($"{args.ElementAt(1)}", out jailTime))
            {
                SendChatError(player, "Invalid Jail Time, must be a number.");
                return;
            }

            for (int i = 2; i < args.Count; i++)
            {
                jailReason += $"{args.ElementAt(i)} ";
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