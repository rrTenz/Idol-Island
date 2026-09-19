using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public static class CachedGUIContent
    {
        private static readonly Dictionary<string, GUIContent> KeyToGuiContentMap = new Dictionary<string, GUIContent>();

        public static GUIContent Text(string text) => GetOrCreate(text, () => new GUIContent(text));

        public static GUIContent GetOrCreate(string cacheKey, Func<GUIContent> createIfNotFound)
        {
            if (!KeyToGuiContentMap.ContainsKey(cacheKey))
            {
                KeyToGuiContentMap[cacheKey] = createIfNotFound();
            }

            return KeyToGuiContentMap[cacheKey];
        }
    }
}