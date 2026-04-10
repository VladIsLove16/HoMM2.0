using Adventure.Integration.Battle;
using UnityEngine;

namespace Adventure.Multiplayer
{
    /// <summary>
    /// Resolves the local owned network player and delegates online battle requests to it.
    /// </summary>
    public sealed class OnlineBattleLaunchService
    {
        public bool TryLaunch(BattleLaunchContext context)
        {
            if (context == null)
            {
                Debug.LogError("[OnlineBattleLaunchService] Battle context is null.");
                return false;
            }

            var localPlayer = FindLocalOwnedPlayer();
            if (localPlayer == null)
            {
                Debug.LogError("[OnlineBattleLaunchService] Local owned NetworkAdventurePlayer was not found.");
                return false;
            }

            return localPlayer.RequestStartOnlineBattle(context);
        }

        private static NetworkAdventurePlayer FindLocalOwnedPlayer()
        {
#if UNITY_2023_1_OR_NEWER
            var players = Object.FindObjectsByType<NetworkAdventurePlayer>(FindObjectsSortMode.None);
#else
            var players = Object.FindObjectsOfType<NetworkAdventurePlayer>();
#endif

            foreach (var player in players)
            {
                if (player != null && player.IsSpawned && player.IsOwner)
                    return player;
            }

            return null;
        }
    }
}
