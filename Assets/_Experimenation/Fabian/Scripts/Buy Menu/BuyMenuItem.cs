using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuItem : MonoBehaviour
    {
        private Ability _ability;
        private int _abilityIndex;
        private bool _purchaseRequested;
        private bool _accepted;
        private float _requestTime;
        private Button _button;
        private TMP_Text _status;
        private TMP_Text _feedback;

        private void OnEnable() => EventBus.Subscribe<AbilityPurchasedEvent>(OnPurchaseResult);
        private void OnDisable() => EventBus.Unsubscribe<AbilityPurchasedEvent>(OnPurchaseResult);

        public void Setup(Ability ability, int abilityIndex)
        {
            _ability = ability;
            _abilityIndex = abilityIndex;
            var panel = gameObject.AddComponent<Image>();
            panel.color = BuyMenuUI.Card;
            BuyMenuUI.Text(transform, "Type", ability.abilityType == AbilityType.Tool ? "TOOL / ACTIVATE IN RUN" : "UPGRADE / ACTIVE THIS ROUND",
                13, BuyMenuUI.Accent, new Vector2(18, -14), new Vector2(330, 22));
            BuyMenuUI.Text(transform, "Name", ability.abilityName, 25, Color.white,
                new Vector2(18, -42), new Vector2(330, 36));
            BuyMenuUI.Text(transform, "Description", ability.abilityDescription, 17, BuyMenuUI.Muted,
                new Vector2(18, -84), new Vector2(330, 62));
            _feedback = BuyMenuUI.Text(transform, "Feedback", "", 13, BuyMenuUI.Muted,
                new Vector2(18, -145), new Vector2(330, 22));
            _button = BuyMenuUI.Button(transform, "Purchase", BuyItem,
                new Vector2(18, -174), new Vector2(330, 40), out _status);
            Refresh();
        }

        private void Update()
        {
            if (_purchaseRequested && Time.unscaledTime - _requestTime > 8f)
            {
                _purchaseRequested = false;
                _feedback.text = "No response. Try again.";
            }
            Refresh();
        }

        private void Refresh()
        {
            if (_button == null || _ability == null)
                return;
            var data = GameData.Instance;
            var ready = data != null && data.Object != null && data.Object.IsValid &&
                data.Runner != null && data.Runner.IsRunning;
            var owned = _accepted || (ready && data.HasPurchased(data.Runner.LocalPlayer, _abilityIndex));
            var affordable = ready && data.LocalPoints >= _ability.abilityPrice;
            _button.interactable = ready && !owned && !_purchaseRequested && affordable;
            _status.text = owned ? "OWNED" : _purchaseRequested ? "PURCHASING..." :
                !ready ? $"{_ability.abilityPrice} POINTS" : !affordable ? $"NEED {_ability.abilityPrice - data.LocalPoints} MORE POINTS" :
                $"BUY / {_ability.abilityPrice} POINTS";
            if (!owned)
                return;
            _feedback.text = "Applied to your player for this round";
            if (_ability.abilityType == AbilityType.Tool)
            {
                _feedback.text = "Assigned to your loadout";
                for (var slot = 0; ready && slot < 4; slot++)
                    if (data.GetToolAbilityIndex(data.Runner.LocalPlayer, slot) == _abilityIndex)
                        _feedback.text = $"Assigned / press {slot + 1} during the run";
            }
        }

        public void BuyItem()
        {
            var data = GameData.Instance;
            if (_purchaseRequested || _accepted || _ability == null || data == null ||
                data.Runner == null || !data.Runner.IsRunning || data.LocalPoints < _ability.abilityPrice ||
                data.HasPurchased(data.Runner.LocalPlayer, _abilityIndex))
                return;
            _purchaseRequested = true;
            _requestTime = Time.unscaledTime;
            _feedback.text = "";
            Refresh();
            data.RequestPurchase(_abilityIndex);
        }

        private void OnPurchaseResult(AbilityPurchasedEvent ev)
        {
            var data = GameData.Instance;
            if (data == null || data.Runner == null || ev == null ||
                ev.Buyer != data.Runner.LocalPlayer || ev.AbilityIndex != _abilityIndex)
                return;
            _purchaseRequested = false;
            _accepted = ev.Accepted;
            if (!ev.Accepted)
                _feedback.text = "Purchase unavailable. Try again during buy phase.";
            Refresh();
        }
    }

    // Shared layout helpers keep the shop and its cards on the same spacing scale.
    internal static class BuyMenuUI
    {
        internal static readonly Color Background = new(0.035f, 0.055f, 0.075f, 0.98f);
        internal static readonly Color Card = new(0.075f, 0.105f, 0.135f);
        internal static readonly Color Accent = new(0.35f, 0.92f, 0.73f);
        internal static readonly Color Muted = new(0.65f, 0.73f, 0.79f);

        internal static RectTransform Rect(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        internal static TMP_Text Text(Transform parent, string name, string text, float size, Color color, Vector2 position, Vector2 dimensions)
        {
            var label = Rect(parent, name, position, dimensions).gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            return label;
        }

        internal static Button Button(Transform parent, string name, UnityEngine.Events.UnityAction onClick,
            Vector2 position, Vector2 size, out TMP_Text label)
        {
            var rect = Rect(parent, name, position, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = Accent;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.8f, 1f, 0.9f);
            colors.pressedColor = new Color(0.55f, 0.8f, 0.7f);
            colors.disabledColor = new Color(0.3f, 0.45f, 0.45f);
            button.colors = colors;
            button.onClick.AddListener(onClick);
            label = Text(rect, "Label", name, 15, Background, Vector2.zero, size);
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            return button;
        }
    }
}
