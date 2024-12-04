#if UNITY_WEBGL
using System;
using UnityEngine;
#if !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace Playgama.Modules.Platform
{
    public class PlatformModule : MonoBehaviour
    {
#if !UNITY_EDITOR
        public string id { get; } = PlaygamaBridgeGetPlatformId();
        public string language { get; } = PlaygamaBridgeGetPlatformLanguage();
        public string payload { get; } = PlaygamaBridgeGetPlatformPayload();
        public string tld { get; } = PlaygamaBridgeGetPlatformTld();

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetPlatformId();

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetPlatformLanguage();

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetPlatformPayload();

        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetPlatformTld();
        
        [DllImport("__Internal")]
        private static extern void PlaygamaBridgeSendMessageToPlatform(string message);
        
        [DllImport("__Internal")]
        private static extern string PlaygamaBridgeGetServerTime();
#else
        public string id => "mock";
        public string language => "en";
        public string payload => null;
        public string tld => null;
#endif
        private Action<DateTime?> _getServerTimeCallback;

        
        public void SendMessage(PlatformMessage message)
        {
#if !UNITY_EDITOR
            var messageString = "";

            switch (message)
            {
                case PlatformMessage.GameReady:
                    messageString = "game_ready";
                    break;
                
                case PlatformMessage.InGameLoadingStarted:
                    messageString = "in_game_loading_started";
                    break;

                case PlatformMessage.InGameLoadingStopped:
                    messageString = "in_game_loading_stopped";
                    break;

                case PlatformMessage.GameplayStarted:
                    messageString = "gameplay_started";
                    break;

                case PlatformMessage.GameplayStopped:
                    messageString = "gameplay_stopped";
                    break;

                case PlatformMessage.PlayerGotAchievement:
                    messageString = "player_got_achievement";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(message), message, null);
            }

            PlaygamaBridgeSendMessageToPlatform(messageString);
#endif
        }

        public void GetServerTime(Action<DateTime?> callback)
        {
            _getServerTimeCallback = callback;
#if !UNITY_EDITOR
            PlaygamaBridgeGetServerTime();
#else
            OnGetServerTimeCompleted(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
#endif
        }


        // Called from JS
        private void OnGetServerTimeCompleted(string result)
        {
            DateTime? date = null;

            if (double.TryParse(result, out var ticks))
            {
                var time = TimeSpan.FromMilliseconds(ticks);
                date = new DateTime(1970, 1, 1) + time;
            }
            
            _getServerTimeCallback?.Invoke(date);
            _getServerTimeCallback = null;
        }
    }
}
#endif