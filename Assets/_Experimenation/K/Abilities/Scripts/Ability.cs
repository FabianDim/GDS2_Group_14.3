using System;
using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

namespace _Experimenation.K.Abilities.Scripts
{
    public enum AbilityType { Stats, Technique }
    public enum AbilityScope { General, Runner, Chaser }

    [CreateAssetMenu(fileName = "New Ability", menuName = "Abilities/New Ability")]
    public class Ability : ScriptableObject
    {
        public string abilityName;
        public Color abilityColor;
        public AbilityType abilityType;
        public AbilityScope abilityScope;
        public string abilityDescription;
        [SerializeReference, SR] public List<AbilityEffect> effects;

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
        public abstract void ApplyEffect(MonoBehaviour runner);

        public virtual void OnUpdate(MonoBehaviour runner) { }

        protected IEnumerator ExecuteAfterDelay(Action callback, float delay = 3f)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
    }

    #region Ability Effects
    [Serializable]
    public class Ability1 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability1 Activated");
        }
    }

    [Serializable]
    public class Ability2 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability2 Activated");
        }
    }

    [Serializable]
    public class Ability3 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability3 Activated");
        }
    }

    [Serializable]
    public class Ability4 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability4 Activated");
        }
    }

    [Serializable]
    public class Ability5 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability5 Activated");
        }
    }

    [Serializable]
    public class Ability6 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability6 Activated");
        }
    }

    [Serializable]
    public class Ability7 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability7 Activated");
        }
    }

    [Serializable]
    public class Ability8 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability8 Activated");
        }
    }

    [Serializable]
    public class Ability9 : AbilityEffect
    {
        public override void ApplyEffect(MonoBehaviour runner)
        {
            Debug.Log("Ability9 Activated");
        }
    }
    #endregion
}