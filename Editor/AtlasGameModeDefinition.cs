using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MeatKit
{
    [CreateAssetMenu(menuName="MeatKit/Atlas/Game Mode Definition", fileName = "New game mode")]
    public class AtlasGameModeDefinition : ScriptableObject
    {
        public string GameModeName;
        public string[] ExtraDependencies;
        public bool AlsoADisplayMode;

        // If a new one is made, set the cache to dirty so it is refreshed the next time it's asked for
        private void OnEnable() { _dirty = true; }

        private static bool _dirty;
        private static Dictionary<string, AtlasGameModeDefinition> _cached;
        public static Dictionary<string, AtlasGameModeDefinition> AllGameModes
        {
            get
            {
                // If the cache is dirty, non-existent, or not up to date, refresh
                if (_dirty || _cached == null || _cached.Values.Any(x => !x))
                {
                    _cached = new Dictionary<string, AtlasGameModeDefinition>();
                    foreach (var gameMode in Extensions.GetAllInstances<AtlasGameModeDefinition>())
                    {
                        _cached.Add(gameMode.GameModeName, gameMode);
                    }

                    _dirty = false;
                }

                return _cached;
            }
        }
    }
}