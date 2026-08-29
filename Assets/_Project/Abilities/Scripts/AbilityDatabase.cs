using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _Project.Abilities.Scripts
{
    [CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Abilities/Ability Database")]
    public class AbilityDatabase : ScriptableObject
    {
        [SerializeField] private string abilityFolderPath = "Assets/_Project/Abilities/Ability SO";
        public List<Ability> allAbilities = new();

        #if UNITY_EDITOR
        // Editor-only: run manually whenever you add/remove an ability asset.
        // Nothing in this block ships in a build.
        [ContextMenu("Refresh From Folder")]
        private void RefreshFromFolder()
        {
            Debug.Log("Refreshing Ability Database");
            allAbilities ??= new List<Ability>();
            allAbilities.Clear();
            var guids = AssetDatabase.FindAssets(
                "t:Ability", new[] { abilityFolderPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ability = AssetDatabase.LoadAssetAtPath<Ability>(path);
                if (ability != null && !allAbilities.Contains(ability))
                    allAbilities.Add(ability);
            }
            EditorUtility.SetDirty(this);
        }
        #endif
    }
}