using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Abilities.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.K.Game_Manager.Abilities.Scripts
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

        private void Awake()
        {
            _runner = FindAnyObjectByType<NetworkRunner>();
            SetupUI();
            abilitiesUI.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_runner == null)
                return;

            _roundState ??= FindAnyObjectByType<AbilityRoundState>();
            _localPlayer ??= FindObjectsByType<Player>()
                .FirstOrDefault(player => player.HasInputAuthority);

            if (_roundState == null || _localPlayer == null ||
                _localPlayer.Role != PlayerRole.Chaser ||
                !_roundState.HasNewSelectionFor(_runner.LocalPlayer, out var sequence))
            {
                abilitiesUI.gameObject.SetActive(false);
                return;
            }

            if (_displayedSequence != sequence)
            {
                DisplayCurrentChoices();
                _displayedSequence = sequence;
            }

            abilitiesUI.gameObject.SetActive(true);
        }

        private void SetupUI()
        {
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
                if (abilityId < 0 || abilityId >= database.allAbilities.Count)
                    continue;

                var ability = database.allAbilities[abilityId];
                _abilitiesUI[i].Image.color = ability.abilityColor;
                _abilitiesUI[i].AbilityName.SetText(ability.abilityName);
                _abilitiesUI[i].Description.SetText(ability.abilityDescription);
            }
        }
    }
}
