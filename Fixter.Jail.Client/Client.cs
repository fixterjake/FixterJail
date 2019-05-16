using System;
using System.Threading.Tasks;
using chat_client;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using DOJ.Core.Models;

namespace Client
{
    public class Client : BaseScript {
        private bool _firstTick = true;
        private bool _playerJailed;
        private bool _nui;
        private int _jailLength;
        private readonly Vector3 _jailPos = new Vector3(1662.6f, 2615.29f, 45.50f);
        private readonly Vector3 _releasePos = new Vector3(1848.62f, 2585.95f, 45.67f);
        private readonly Vector3 _missionRow = new Vector3(459.79f, -989.13f, 24.91f);
        private readonly Vector3 _sandyPd = new Vector3(1853.11f, 3690.12f, 34.27f);
        private readonly Vector3 _paletoPd = new Vector3(-449.48f, 6012.42f, 31.72f);
        private readonly Vector3 _bolingbrokePrison = new Vector3(1792.49f, 2593.75f, 45.8f);

        public Client()
        {
            EventHandlers["DOJ.Jail.PlayerJailed"] += new Action<int>(PlayerJailed);
            EventHandlers["DOJ.Jail.NUI"] += new Action(ToggleNui);
            EventHandlers["DOJ.Jail.Submitted"] += new Action<int, int, string>(JailSubmitted);
            EventHandlers["DOJ.Jail.ProcessJailTime"] += new Action(ProcessJailTime);
            EventHandlers["DOJ.Jail.Command"] += new Action<int>(PlayerJailed);
            Tick += CheckEscape;
            Tick += CheckLocation;
        }

        private static async Task SwitchOut(Vector3 position, int delay)
        {
            int player = API.PlayerPedId();
            API.SetEntityInvincible(player, true);
            API.SetEntityCollision(player, false, false);
            API.SetEntityHasGravity(player, false);
            API.SwitchOutPlayer(player, 0, 1);
            await Delay(delay);
            API.SetEntityCoords(player, position.X, position.Y, position.Z, false, false, false, false);
            Function.Call((Hash)0xD8295AF639FD9CB8, player);
            await Delay(delay);
            API.SetEntityInvincible(player, false);
            API.SetEntityCollision(player, true, true);
            API.SetEntityHasGravity(player, true);
            await Delay(delay);
        }

        private async Task Scaleform(string mainText, string description, bool addTime)
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
            if (addTime)
            {
                _jailLength += 60;
            }
            if (_playerJailed)
            {
                TriggerEvent("DOJ.Jail.ProcessJailTime");
            }
            while (true)
            {
                API.DrawScaleformMovieFullscreen(scaleform, 255, 255, 255, 255, 0);
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to continue.");
                await Delay(10);
                if (Game.IsControlPressed(0, Control.Context))
                {
                    API.PlaySoundFrontend(-1, "TextHit", "WastedSounds", true);
                    break;
                }
            }
            API.SetScaleformMovieAsNoLongerNeeded(ref scaleform);
        }

        private async void PlayerJailed(int length)
        {
            int player = API.PlayerPedId();
            if (length > 600)
            {
                _jailLength = 600;
            }
            else
            {
                _jailLength = length;
            }
            API.RemoveAllPedWeapons(player, true);
            await SwitchOut(_jailPos, 2500);
            _playerJailed = true;
            await Scaleform("~r~JAILED", "You've been jailed for " + _jailLength + " seconds", false);
        }

        private void ToggleNui()
        {
            if (!_nui)
            {
                API.SetNuiFocus(true, true);
                API.SendNuiMessage(Json.Stringify(new
                {
                    type = "DISPLAY_JAIL_UI"
                }));
                _nui = true;
            }
            else
            {
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(Json.Stringify(new
                {
                    type = "DISABLE_ALL_UI"
                }));
                _nui = false;
            }
        }

        private void JailSubmitted(int id, int length, string reason)
        {

            API.SendNuiMessage(Json.Stringify(new
            {
                type = "DISABLE_ALL_UI"
            }));
            API.SetNuiFocus(false, false);
            Screen.ShowNotification("Subject has been Jailed for " + length + " seconds.");
            _nui = false;

            TriggerServerEvent("DOJ.Jail.SendToServer", id, length, reason);
        }

        private async void ProcessJailTime()
        {
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
                    await SwitchOut(_releasePos, 1800);
                    await Scaleform("~b~RELEASED", "You've been released from jail.", false);
                    break;
                }
            }
        }

        private async void DiableControls()
        {
            if (_nui)
            {
                
            }
        }

        public async Task CheckEscape()
        {
            if (_firstTick)
            {
                API.SetNuiFocus(false, false);
                BlipColor blipColor = BlipColor.TrevorOrange;
                string blipName = "Prison";
                Blip blip = new Blip(CitizenFX.Core.World.CreateBlip(_bolingbrokePrison).Handle)
                {
                    Sprite = BlipSprite.Key,
                    Color = blipColor,
                    IsShortRange = true,
                    Name = blipName
                };
                _firstTick = false;
            }

            if (_playerJailed)
            {
                if (LocalPlayer.Character.Position.DistanceTo(_jailPos) > 140f)
                {
                    await SwitchOut(_jailPos, 2000);
                    await Scaleform("~r~ESCAPE ATTEMPT FAILED", "Your sentence has been extended by 60 seconds.", true);
                }
            }

            await Delay(20);
        }

        private async Task CheckLocation()
        {
            if (LocalPlayer.Character.Position.DistanceTo(_missionRow) <= 1f && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    TriggerEvent("DOJ.Jail.NUI");
                }
            }

            if (LocalPlayer.Character.Position.DistanceTo(_sandyPd) <= 1f && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    TriggerEvent("DOJ.Jail.NUI");
                }
            }

            if (LocalPlayer.Character.Position.DistanceTo(_paletoPd) <= 1f && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    TriggerEvent("DOJ.Jail.NUI");
                }
            }

            if (LocalPlayer.Character.Position.DistanceTo(_bolingbrokePrison) <= 1f && !_nui)
            {
                Screen.DisplayHelpTextThisFrame("Press ~y~E~w~ to open jail interface.");
                if (Game.IsControlPressed(0, Control.Context))
                {
                    TriggerEvent("DOJ.Jail.NUI");
                }
            }
            await Task.FromResult(0);
        }
    }
}
