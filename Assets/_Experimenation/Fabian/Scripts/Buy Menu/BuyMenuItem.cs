using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuItem : MonoBehaviour
    {
        [SerializeField] private Image abilityImage;
        [SerializeField] private TMP_Text[] abilityName;
        [SerializeField] private TMP_Text[] description;
        [SerializeField] private TMP_Text price;
        [SerializeField] private TMP_Text keyBind;
        private Ability _ability;
        private int _abilityIndex = -1;
        private bool _purchaseRequested;

        private void Awake()
        {
            if (abilityImage == null)
                abilityImage = GetComponentInChildren<Image>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<AbilityPurchasedEvent>(OnPurchaseAccepted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AbilityPurchasedEvent>(OnPurchaseAccepted);
        }

        public void Setup(Ability ability, int abilityIndex)
        {
            if (ability == null)
                return;

            _ability = ability;
            _abilityIndex = abilityIndex;
            if (abilityImage == null)
                abilityImage = GetComponentInChildren<Image>();

            if (abilityImage != null)
                abilityImage.sprite = ability.abilitySprite;
            else
            {
                Debug.LogWarning("abilityImage is null – no Image component found on this BuyMenuItem.");
            }

            if (abilityName != null)
            {
                foreach (var item in abilityName)
                {
                    if (item != null)
                        item.SetText(ability.abilityName);
                }
            }

            if (description != null)
            {
                foreach (var item in description)
                {
                    if (item != null)
                        item.SetText(ability.abilityDescription);
                }
            }

            if (price != null)
                price.SetText($"{ability.abilityPrice}");
        }

        public void BuyItem()
        {
            if (_purchaseRequested || _ability == null || _abilityIndex < 0)
                return;

            var gameData = GameData.Instance;
            if (gameData == null)
            {
                Debug.LogWarning("Cannot purchase ability because GameData is not ready.", this);
                return;
            }

            var runner = gameData.Runner;
            if (runner == null || !runner.IsRunning || runner.IsShutdown)
            {
                Debug.LogWarning("Cannot purchase ability because the network runner is not ready.", this);
                return;
            }

            _purchaseRequested = true;
            gameData.RequestPurchase(_abilityIndex);
        }

        private void OnPurchaseAccepted(AbilityPurchasedEvent ev)
        {
            var gameData = GameData.Instance;
            if (gameData == null || ev.Buyer != gameData.Runner.LocalPlayer ||
                ev.AbilityIndex != _abilityIndex)
                return;

            _purchaseRequested = false;
            if (!ev.Accepted)
                return;

            if (price != null)
                price.SetText("Sold");
        }
    }
}