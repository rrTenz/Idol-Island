using System.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;
using ImmersiveVRTools.Runtime.Common.Utilities;

namespace ImmersiveVRTools.Editor.Common
{
    [InitializeOnLoad]
    public static class MissingAssetNotifierSceneOpenedEventWatcher
    {
        static MissingAssetNotifierSceneOpenedEventWatcher()
        {
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += SceneOpenedCallback;
        }

        static void SceneOpenedCallback(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            var allMissingAssetsNotifierComponents = scene.GetRootGameObjects()
                .Select(g => g.GetComponent<MissingAssetsNotifier>())
                .Where(c => c != null)
                .ToList();

            foreach (var missingAssetsNotifier in allMissingAssetsNotifierComponents)
            {
                missingAssetsNotifier.CheckFormMissingAssets();
            }
        }
    }
}
