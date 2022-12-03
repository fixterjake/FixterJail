using System;
using System.Collections.Generic;

namespace Client
{
    public delegate void PlayerJoined();

    public class InternalGameEvents
    {
        public const string damageEventName = "DamageEvents";

        public static void Init()
        {
            Client.Instance.AddEventHandler("gameEventTriggered", new Action<string, List<object>>(GameEventTriggered));
        }
        
        public static event PlayerJoined PlayerJoined;

        /// <summary>
        /// Used internally to trigger the other events.
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="data"></param>
        private static void GameEventTriggered(string eventName, List<object> data)
        {
            switch (eventName)
            {
                case "CEventNetworkStartSession":
                    {
                        PlayerJoined?.Invoke();
                    }
                    break;
            }
        }
    }
}
