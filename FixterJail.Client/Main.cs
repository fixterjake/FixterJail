using FixterJail.Client.Extensions;
using FixterJail.Shared.Models;
using FixterJail.Shared.Scripts;

namespace FixterJail.Client
{
    public class Main : BaseScript
    {
        public static Main Instance;
        private Config _config;
        private PlayerList _playerList;

        private int[] CHAT_COLOR_ERROR = new[] { 255, 0, 76 };
        private Blip _prisonBlip;

        private bool _playerJailed;
        private bool _nui;
        private int _jailDuration;

        private bool _jailTimeProcessorRunning;

        private int _scaleform;

        private readonly Vector3 _jailPos;
        private readonly Vector3 _releasePos;

        public Main()
        {
            Debug.WriteLine("LOADED CLIENT JAIL");

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };

            _config = Configuration.Get;
            _jailPos = _config.Locations.Jail.AsVector();
            _releasePos = _config.Locations.JailRelease.AsVector();

            Instance = this;
            _playerList = Players;

            EventHandlers["fixterjail:jail:imprison"] += new Action<int>(OnJailPlayer);
            EventHandlers["fixterjail:jail:release"] += new Action(OnReleasePlayer);
            EventHandlers["fixterjail:jail:useNui"] += new Action<bool>(OnPlayerCanUseNui);
            EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);

            RegisterNuiCallbackType("closeUI");
            EventHandlers["__cfx_nui:closeUI"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                NUIClose();
                cb("ok");
            });

            RegisterNuiCallbackType("jailNuiCallback");
            EventHandlers["__cfx_nui:jailNuiCallback"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                int playerId;
                int jailTime;
                string reason = data["reason"].ToString();

                if (!int.TryParse(data["id"].ToString(), out playerId))
                {
                    SendChatError("Invalid Player ID.");
                    return;
                }

                if (!int.TryParse(data["time"].ToString(), out jailTime))
                {
                    SendChatError("Invalid Jail Time, must be a number.");
                    return;
                }

                NUIClose();
                
                TriggerServerEvent("fixterjail:jail:incarcerate", playerId, jailTime, reason);

                cb("ok");
            });

            TriggerServerEvent("fixterjail:jail:connect");
        }

        private void OnReleasePlayer()
        {
            _jailDuration = 0;
            TriggerServerEvent("fixterjail:jail:playerReleased");
        }

        private void OnPlayerCanUseNui(bool canUseNui)
        {
            if (canUseNui)
                AttachTickHandler(AsyncCheckLocation);
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            API.SetNuiFocus(false, false);

            _prisonBlip = World.CreateBlip(new Vector3(1792.49f, 2593.75f, 45.8f));
            _prisonBlip.Sprite = BlipSprite.Key;
            _prisonBlip.Color = BlipColor.TrevorOrange;
            _prisonBlip.IsShortRange = true;
            _prisonBlip.Name = "Prison";
        }

        private void OnClientResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            if (_prisonBlip != null)
            {
                if (_prisonBlip.Exists())
                    _prisonBlip.Delete();
            }
        }

        private void SendChatError(string message)
        {
            TriggerEvent("chatMessage", "[Judge]", CHAT_COLOR_ERROR, message);
        }

        public void AddEventHandler(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }

        private void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        private void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }

        private async Task TeleportPlayerToPosition(Vector3 position, int delay)
        {
            Ped playerPed = LocalPlayer.Character;
            playerPed.IsInvincible = true;
            playerPed.IsCollisionEnabled = false;
            playerPed.HasGravity = false;
            API.SwitchOutPlayer(playerPed.Handle, 0, 1);
            await Delay(delay);
            playerPed.Position = position;
            await Delay(500);
            API.SwitchInPlayer(playerPed.Handle);
            await Delay(delay);
            playerPed.IsInvincible = false;
            playerPed.IsCollisionEnabled = true;
            playerPed.HasGravity = true;
            await Delay(delay);
        }

        private async Task SetupAndDisplayBigMessageScaleform(string mainText, string description)
        {
            _scaleform = API.RequestScaleformMovie("mp_big_message_freemode");
            while (!API.HasScaleformMovieLoaded(_scaleform))
            {
                await Delay(10);
            }
            API.PushScaleformMovieFunction(_scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
            API.PushScaleformMovieFunctionParameterString(mainText);
            API.PushScaleformMovieFunctionParameterString(description);
            API.PopScaleformMovieFunctionVoid();

            AttachTickHandler(AsyncDisplayBigMessageScaleform);
        }

        async Task AsyncDisplayBigMessageScaleform()
        {
            API.DrawScaleformMovieFullscreen(_scaleform, 255, 255, 255, 255, 0);
            Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to continue.");

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                API.PlaySoundFrontend(-1, "TextHit", "WastedSounds", true);
                API.SetScaleformMovieAsNoLongerNeeded(ref _scaleform);
                DetachTickHandler(AsyncDisplayBigMessageScaleform);
            }
        }

        private async void OnJailPlayer(int length)
        {
            _jailDuration = length;
            LocalPlayer.Character.Weapons.RemoveAll();
            await TeleportPlayerToPosition(_jailPos, 2500);
            _playerJailed = true;
            ProcessJailTime();
            await SetupAndDisplayBigMessageScaleform("~r~JAILED", "You've been jailed for " + _jailDuration + " seconds");
            AttachTickHandler(CheckIfPlayerIsAttemptingToEscape);
        }

        private void ToggleNui()
        {
            _nui = !_nui;
            API.SetNuiFocus(_nui, _nui);

            Dictionary<int, string> players = new();
            if (_nui)
            {
                foreach (Player player in _playerList)
                {
                    if (player != LocalPlayer)
                    {
                        players.Add(player.ServerId, player.Name);
                    }
                }
            }

            NuiMessage nuiMessage = new(_nui ? "DISPLAY_JAIL_UI" : "DISABLE_ALL_UI", players);
            API.SendNuiMessage(nuiMessage.ToJson());
        }

        private void NUIClose()
        {
            _nui = false;
            NuiMessage nuiMessage = new("DISABLE_ALL_UI");
            API.SendNuiMessage(nuiMessage.ToJson());
            API.SetNuiFocus(false, false);
        }

        private async void ProcessJailTime()
        {
            if (_jailTimeProcessorRunning) return;
            _jailTimeProcessorRunning = true;

            while (_jailDuration > 0 && _playerJailed)
            {
                await Delay(1000);
                _jailDuration -= 1;

                if (_jailDuration % 30 == 0 && _jailDuration != 0)
                {
                    TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 }, $"{_jailDuration} More seconds until release");
                }

                if (_jailDuration == 0)
                {
                    _playerJailed = false;
                    await TeleportPlayerToPosition(_releasePos, 1800);
                    await SetupAndDisplayBigMessageScaleform("~b~RELEASED", "You've been released from jail.");
                    DetachTickHandler(CheckIfPlayerIsAttemptingToEscape);
                    break;
                }
            }

            _jailTimeProcessorRunning = false;
        }

        public async Task CheckIfPlayerIsAttemptingToEscape()
        {
            if (!LocalPlayer.Character.IsInRangeOf(_jailPos, 140f) && _playerJailed)
            {
                await TeleportPlayerToPosition(_jailPos, 2000);
                _jailDuration += 60;
                await SetupAndDisplayBigMessageScaleform("~r~ESCAPE ATTEMPT FAILED", "Your sentence has been extended by 60 seconds.");
            }

            await Delay(20);
        }

        private async Task AsyncCheckLocation()
        {
            Vector3 closestPosition = LocalPlayer.Character.Position.FindClosestPoint(_config.Locations.PoliceDepartments);

            if (LocalPlayer.Character.IsInRangeOf(closestPosition, 1.5f) && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    ToggleNui();
                }
            }
            
            await Task.FromResult(0);
        }
    }
}