using System.Collections.Generic;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Abilities.Scripts
{
    public struct AbilityUI
    {
        public readonly Image Background;
        public readonly TextMeshProUGUI Name;
        public readonly TextMeshProUGUI Description;
            
        public AbilityUI(Image background, TextMeshProUGUI name, TextMeshProUGUI description)
        {
            Background = background;
            Name = name;
            Description = description;
        }
    }
    
    public class AbilityUIManager : MonoBehaviour
    {
        [SerializeField] private Transform ui;
        private readonly List<AbilityUI> _abilities = new();

        private bool _initialized;

        private void Awake()
        {
            Initialize();
            EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        private void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _abilities.Clear();

            if (ui == null)
            {
                Debug.LogError("AbilityUIManager: UI container is not assigned.", this);
                return;
            }

            // Setup Ability UIs.
            for (var i = 0; i < ui.childCount; ++i)
            {
                var child = ui.GetChild(i);
                if (child == null || child.childCount == 0 ||
                    child.GetComponent<Image>() == null ||
                    child.GetChild(0).GetComponent<TextMeshProUGUI>() == null ||
                    child.GetChild(0).childCount == 0 ||
                    child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>() == null)
                {
                    Debug.LogError($"AbilityUIManager: malformed ability card at index {i}.", this);
                    continue;
                }

                _abilities.Add(
                    new AbilityUI(
                        child.GetComponent<Image>(),
                        child.GetChild(0).GetComponent<TextMeshProUGUI>(),
                        child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()
                    )
                );
            }

            ShowUI(false);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);
        }

        private void ShowUI(bool show)
        {
            for (var i = 0; i < transform.childCount; ++i)
                transform.GetChild(i).gameObject.SetActive(show);
        }

        public void ShowAbilities(List<Ability> abilities)
        {
            Initialize();

            if (abilities == null || abilities.Count != _abilities.Count ||
                abilities.Exists(ability => ability == null))
            {
                Debug.LogWarning(
                    $"AbilityUIManager: received {abilities?.Count ?? 0} abilities " +
                    $"for {_abilities.Count} UI cards.", this);
                return;
            }

            for (var i = 0; i < _abilities.Count; ++i)
            {
                _abilities[i].Background.color = abilities[i].abilityColor;
                _abilities[i].Name.text = abilities[i].abilityName;
                _abilities[i].Description.text = abilities[i].abilityDescription;
            }

            ShowUI(true);
        }
        
        private void OnAbilitySelected(AbilitySelectedEvent ev)
        {
            ShowUI(false);
        }
    }
}
