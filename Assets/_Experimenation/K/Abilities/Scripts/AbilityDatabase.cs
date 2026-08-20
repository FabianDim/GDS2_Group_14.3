using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Experimenation.K.Abilities.Scripts
{
    [CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Abilities/Ability Database")]
    public class AbilityDatabase : ScriptableObject
    {
        public List<Ability> allAbilities;

        #if UNITY_EDITOR
        // Editor-only: run manually whenever you add/remove an ability asset.
        // Nothing in this block ships in a build.
        [ContextMenu("Refresh From Folder")]
        private void RefreshFromFolder()
        {
            Debug.Log("Refreshing Ability Database");
            allAbilities.Clear();
            var guids = AssetDatabase.FindAssets(
                "t:Ability", new[] { "Assets/_Experimenation/K/Abilities" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ability = AssetDatabase.LoadAssetAtPath<Ability>(path);
                if (ability != null) allAbilities.Add(ability);
            }
            EditorUtility.SetDirty(this);
        }
        #endif
    }
}