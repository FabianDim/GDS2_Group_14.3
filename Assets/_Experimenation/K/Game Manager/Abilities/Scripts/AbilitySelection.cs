using System.Collections.Generic;
using System.Linq;
using _Experimenation.K.Abilities.Scripts;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Experimenation.K.Game_Manager.Abilities.Scripts
{
    public struct AbilityUI
    {
        public Image Image;
        public TextMeshProUGUI AbilityName;
        public TextMeshProUGUI Description;
    }

    public class AbilitySelection : MonoBehaviour
    {
        private bool _newAbilitySet = true;
        [SerializeField] private AbilityDatabase database;
        private List<Ability> _allAbilities = new();
        private readonly List<Ability> _abilitySet = new();
        private int _abilitySetIndex;

        [SerializeField] private Transform abilitiesUI;
        private readonly List<AbilityUI> _abilitiesUI = new();

        #region Setup Methods
        private void Awake()
        {
            SetupUI();
            SetupAbilities();
            
            var localPlayer = FindObjectsByType<Player>()
                .FirstOrDefault(player => player.HasInputAuthority);
            if (localPlayer != null && localPlayer.Role == PlayerRole.Runner)
            {
                Destroy(gameObject);
                return;
            }
            
            gameObject.SetActive(false);

            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Subscribe<RoundOverEvent>(OnLegEnds);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<RoundOverEvent>(OnLegEnds);
        }

        private void SetupUI()
        {
            for (var i = 0; i < abilitiesUI.childCount; i++)
            {
                var child = abilitiesUI.GetChild(i);
                _abilitiesUI.Add(
                    new AbilityUI
                    {
                        Image = child.GetComponent<Image>(),
                        AbilityName = child.GetChild(0).GetComponent<TextMeshProUGUI>(),
                        Description = child.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>()
                    });
            }
        }

        private void SetupAbilities() =>
            _allAbilities = database.allAbilities.Where(a => a.abilityScope != AbilityScope.Runner).ToList();
        #endregion

        #region Event Handlers
        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (!ev.CollectedBy.Equals("Chaser") || gameObject.activeSelf) return;

            foreach (var abilityUI in _abilitiesUI)
            {
                Ability ability;
                if (_newAbilitySet)
                {
                    ability = _allAbilities[Random.Range(0, _allAbilities.Count)];
                    _abilitySet.Add(ability);
                    _allAbilities.Remove(ability);
                }
                else
                {
                    ability = _abilitySet[_abilitySetIndex];
                }

                _abilitySetIndex++;
                abilityUI.Image.color = ability.abilityColor;
                abilityUI.AbilityName.SetText(ability.abilityName);
                abilityUI.Description.SetText(ability.abilityDescription);
            }

            gameObject.SetActive(true);
        }

        private void OnLegEnds(RoundOverEvent ev)
        {
            _newAbilitySet = !_newAbilitySet;
            if (_newAbilitySet)
                _abilitySet.Clear();
            else
                SetupAbilities();
            _abilitySetIndex = 0;
        }
        #endregion

        #region Input Handlings
        private void SelectAbility(InputAction.CallbackContext ctx, int index)
        {
            if (!ctx.performed || !gameObject.activeSelf) return;
            Debug.Log($"Ability {_abilitySet[_abilitySetIndex - (3 - index) - 1].abilityName} selected");
            foreach (var ability in _abilitySet[_abilitySetIndex - (3 - index) - 1].effects)
            {
                ability.ApplyEffect(this);
            }
            gameObject.SetActive(false);
        }

        public void SelectAbility1(InputAction.CallbackContext ctx) =>
            SelectAbility(ctx, 1);
        public void SelectAbility2(InputAction.CallbackContext ctx) =>
            SelectAbility(ctx, 2);
        public void SelectAbility3(InputAction.CallbackContext ctx) =>
            SelectAbility(ctx, 3);
        #endregion
    }
}