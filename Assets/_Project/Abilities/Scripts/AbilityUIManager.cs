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

        private void Awake()
        {
            //Setup Ability UIs
            for (var i = 0; i < ui.childCount; ++i)
            {
                var child = ui.GetChild(i);
                _abilities.Add(
                    new AbilityUI(
                        child.GetComponent<Image>(),
                        child.GetChild(0).GetComponent<TextMeshProUGUI>(),
                        child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()
                    )
                );
            }
            
            //Turn off all UIs
            ShowUI(false);
            
            //Register Event Bus Callbacks
            EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
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
