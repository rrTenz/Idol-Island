using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ImmersiveVRTools.Editor.Common.Utilities
{
    public class AssetFinder
    {
        public static List<T> GetAllInstances<T>() where T : ScriptableObject
        {
            var guids = AssetDatabase.FindAssets("t:"+ typeof(T).Name);  //FindAssets uses tags check documentation for more info
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>).ToList();
        }
    }
}