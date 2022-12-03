using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Client
{
    /*
     * Changes to make
     * - Positions to config or meta data
     * - tick controller, we don't need to check 24/7
     * - use API methods
     * - use Newtonsoft.Json correctly, as they are already using it
     * */


    public class Client : BaseScript
    {
        public static Client Instance;

        private int[] CHAT_COLOR_ERROR = new[] { 255, 0, 76 };
        private Blip _prisonBlip;

        private bool _playerJailed;
        private bool _nui;
        private int _jailLength;

        private bool _jailTimeProcessorRunning;
        private bool _drawScaleform;
        
        private readonly Vector3 _jailPos = new Vector3(1662.6f, 2615.29f, 45.50f);
        private readonly Vector3 _releasePos = new Vector3(1848.62f, 2585.95f, 45.67f);
        private readonly Vector3 _missionRow = new Vector3(459.79f, -989.13f, 24.91f);
        private readonly Vector3 _sandyPd = new Vector3(1853.11f, 3690.12f, 34.27f);
        private readonly Vector3 _paletoPd = new Vector3(-449.48f, 6012.42f, 31.72f);
        private readonly Vector3 _bolingbrokePrison = new Vector3(1792.49f, 2593.75f, 45.8f);

        public Client()
        {
            Instance = this;
            
            EventHandlers["fixterjail:jail:imprison"] += new Action<int>(JailPlayer);
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

                if (!int.TryParse(data["jailTime"].ToString(), out jailTime))
                {
                    SendChatError("Invalid Jail Time, must be a number.");
                    return;
                }

                NUIClose();
                Screen.ShowNotification("Subject has been Jailed for " + jailTime + " seconds.");
                TriggerServerEvent("fixterjail:jail:incarcerate", playerId, jailTime, reason);
            });

            InternalGameEvents.PlayerJoined += InternalGameEvents_PlayerJoined;
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            API.SetNuiFocus(false, false);

            _prisonBlip = World.CreateBlip(_bolingbrokePrison);
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

        private void InternalGameEvents_PlayerJoined()
        {
            if (IsAceAllowed("fixterjail.jail"))
            {
                AttachTickHandler(AsyncCheckLocation);
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

        private async Task ShowBigMessageScaleform(string mainText, string description)
        {
            int scaleform = API.RequestScaleformMovie("mp_big_message_freemode");
            while (!API.HasScaleformMovieLoaded(scaleform))
            {
                await Delay(10);
            }
            API.PushScaleformMovieFunction(scaleform, "SHOW_SHARD_WASTED_MP_MESSAGE");
            API.PushScaleformMovieFunctionParameterString(mainText);
            API.PushScaleformMovieFunctionParameterString(description);
            API.PopScaleformMovieFunctionVoid();

            _drawScaleform = true;

            while (_drawScaleform)
            {
                API.DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
                Screen.DisplayHelpTextThisFrame("Press ~y~INPUT_CONTEXT~w~ to continue.");
                await Delay(10);
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    _drawScaleform = false;
                    API.PlaySoundFrontend(-1, "TextHit", "WastedSounds", true);
                    break;
                }
            }
            API.SetScaleformMovieAsNoLongerNeeded(ref scaleform);
        }

        private async void JailPlayer(int length)
        {
            _jailLength = length;
            LocalPlayer.Character.Weapons.RemoveAll();
            await TeleportPlayerToPosition(_jailPos, 2500);
            _playerJailed = true;
            ProcessJailTime();
            await ShowBigMessageScaleform("~r~JAILED", "You've been jailed for " + _jailLength + " seconds");
            AttachTickHandler(CheckIfPlayerIsAttemptingToEscape);
        }

        private void ToggleNui()
        {
            _nui = !_nui;
            API.SetNuiFocus(_nui, _nui);
            API.SendNuiMessage(JsonConvert.SerializeObject(new { type = _nui ? "DISPLAY_JAIL_UI" : "DISABLE_ALL_UI" }));
        }

        private void NUIClose()
        {
            _nui = false;
            API.SendNuiMessage(JsonConvert.SerializeObject(new { type = "DISABLE_ALL_UI" }));
            API.SetNuiFocus(false, false);
        }

        private async void ProcessJailTime()
        {
            if (_jailTimeProcessorRunning) return;
            _jailTimeProcessorRunning = true;

            while (_jailLength > 0 && _playerJailed)
            {
                await Delay(1000);
                _jailLength -= 1;

                if (_jailLength % 30 == 0 && _jailLength != 0)
                {
                    TriggerEvent("chatMessage", "[Judge]", new[] { 0, 100, 255 }, $"{_jailLength} More seconds until release");
                }

                if (_jailLength == 0)
                {
                    _playerJailed = false;
                    await TeleportPlayerToPosition(_releasePos, 1800);
                    await ShowBigMessageScaleform("~b~RELEASED", "You've been released from jail.");
                    DetachTickHandler(CheckIfPlayerIsAttemptingToEscape);
                    break;
                }
            }

            _jailTimeProcessorRunning = false;
        }

        public async Task CheckIfPlayerIsAttemptingToEscape()
        {
            if (!LocalPlayer.Character.IsInRangeOf(_jailPos, 140f))
            {
                await TeleportPlayerToPosition(_jailPos, 2000);
                _jailLength += 60;
                await ShowBigMessageScaleform("~r~ESCAPE ATTEMPT FAILED", "Your sentence has been extended by 60 seconds.");
            }

            await Delay(20);
        }

        private async Task AsyncCheckLocation()
        {
            if (!IsAceAllowed("fixterjail.jail"))
            {
                DetachTickHandler(AsyncCheckLocation);
            }
            
            if (LocalPlayer.Character.IsInRangeOf(_missionRow, 1f) && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    ToggleNui();
                }
            }

            if (LocalPlayer.Character.IsInRangeOf(_sandyPd, 1f) && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    ToggleNui();
                }
            }

            if (LocalPlayer.Character.IsInRangeOf(_paletoPd, 1f) && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    ToggleNui();
                }
            }

            if (LocalPlayer.Character.IsInRangeOf(_bolingbrokePrison, 1f) && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    ToggleNui();
                }
            }
            await Task.FromResult(0);
        }
    }
}
