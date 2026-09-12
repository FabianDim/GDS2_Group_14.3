using System;
using System.Collections;
using System.Collections.Generic;
using _Experimenation.K.Multiplayer.Scripts;
using SerializeReferenceEditor;
using UnityEngine;

namespace _Project.Abilities.Scripts
{
    public enum AbilityType { Stats, Technique }
    public enum AbilityScope { General, Runner, Chaser }

    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/New Ability")]
    public class Ability : ScriptableObject
    {
        public string abilityName;
        [SerializeField] public Sprite abilitySprite;
        public Color abilityColor;
        public AbilityType abilityType;
        public AbilityScope abilityScope;
        public string abilityDescription;
        [SerializeReference, SR] public List<AbilityEffect> effects;
        public int abilityPrice;


        public void OnEnable()
        {
            abilityColor = abilityType switch
            {
                AbilityType.Stats => Color.orange,
                AbilityType.Technique => Color.blue,
                _ => Color.white
            };
        }
    }

    [Serializable]
    public abstract class AbilityEffect
    {
        public abstract void ApplyEffect(Player target);

        protected IEnumerator EndEffect(Action callback, float delay = 3f)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
    }
}