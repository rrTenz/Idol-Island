using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public static class CachedGUIStyle
    {
        private static readonly Dictionary<string, GUIStyle> KeyToGuiStyleMap = new Dictionary<string, GUIStyle>();

        public static GUIStyle GetOrCreate(string cacheKey, Func<GUIStyle> createIfNotFound)
        {
            if (!KeyToGuiStyleMap.ContainsKey(cacheKey))
            {
                KeyToGuiStyleMap[cacheKey] = createIfNotFound();
            }

            return KeyToGuiStyleMap[cacheKey];
        }
    }
}