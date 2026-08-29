using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Abilities.Scripts
{
    public struct AbilityUI
    {
        public Image Image;
        public TextMeshProUGUI AbilityName;
        public TextMeshProUGUI Description;
    }

    // Local presentation only. AbilityRoundState owns the networked ability set.
    public class AbilitySelection : MonoBehaviour
    {
        [SerializeField] private AbilityDatabase database;
        [SerializeField] private Transform abilitiesUI;

        private readonly List<AbilityUI> _abilitiesUI = new();
        private NetworkRunner _runner;
        private AbilityRoundState _roundState;
        private Player _localPlayer;
        private int _displayedSequence = -1;
        private bool _subscribed;

        private void Awake()
        {
            _runner = FindAnyObjectByType<NetworkRunner>();
            SetupUI();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = true;
        }

        private void OnDisable()
        {
            if (!_subscribed)
                return;

            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            _subscribed = false;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (_runner == null)
                _runner = FindAnyObjectByType<NetworkRunner>();

            if (_roundState == null)
                _roundState = FindAnyObjectByType<AbilityRoundState>();

            if (_localPlayer == null)
            {
                _localPlayer = FindObjectsByType<Player>()
                    .FirstOrDefault(player => player.HasInputAuthority);
            }

            if (_runner == null || _roundState == null || _localPlayer == null ||
                ev.CollectedBy != PlayerRole.Chaser ||
                ev.Collector != _runner.LocalPlayer ||
                !_roundState.HasSelectionFor(_runner.LocalPlayer, out var sequence))
            {
                return;
            }

            if (_displayedSequence != sequence)
            {
                DisplayCurrentChoices();
                _displayedSequence = sequence;
            }

            ShowUI(true);
        }

        private void SetupUI()
        {
            //Setup Ability UI
            ShowUI(false);
            
            //Setup Ability Selection UI
            for (var i = 0; i < abilitiesUI.childCount; i++)
            {
                var child = abilitiesUI.GetChild(i);
                _abilitiesUI.Add(new AbilityUI
                {
                    Image = child.GetComponent<Image>(),
                    AbilityName = child.GetChild(0).GetComponent<TextMeshProUGUI>(),
                    Description = child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()
                });
            }
        }

        private void DisplayCurrentChoices()
        {
            for (var i = 0; i < _abilitiesUI.Count; i++)
            {
                var abilityId = _roundState.GetAbilityId(_roundState.CurrentChoiceStart + i);
                if (database == null || database.allAbilities == null ||
                    abilityId < 0 || abilityId >= database.allAbilities.Count)
                    continue;

                var ability = database.allAbilities[abilityId];
                if (ability == null)
                    continue;
                _abilitiesUI[i].Image.color = ability.abilityColor;
                _abilitiesUI[i].AbilityName.SetText(ability.abilityName);
                _abilitiesUI[i].Description.SetText(ability.abilityDescription);
            }
        }

        private void ShowUI(bool show)
        {
            foreach (var child in GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.SetActive(show);
            }
        }
    }
}
