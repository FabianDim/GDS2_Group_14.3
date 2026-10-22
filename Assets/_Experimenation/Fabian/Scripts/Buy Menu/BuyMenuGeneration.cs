using System.Collections;
using System.Collections.Generic;
using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using _Project.Abilities.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.Fabian.Scripts.Buy_Menu
{
    public class BuyMenuGeneration : MonoBehaviour
    {
        [SerializeField] private AbilityDatabase database;
        [SerializeField] private Transform p1BuyMenuLocation;
        [SerializeField] private Transform p2BuyMenuLocation;
        private GameObject _menu;
        private RectTransform _cards;
        private TMP_Text _balance;
        private TMP_Text _timer;
        private TMP_Text _role;
        private PhaseManager _phase;
        private bool _visible = true;

        private void Awake()
        {
            // Keep the physical trigger and camera, replace the stretched world UI
            // with a screen-scaled menu that stays readable at any aspect ratio.
            var oldPanel = transform.Find("BuyMenuBlock");
            if (oldPanel != null) oldPanel.gameObject.SetActive(false);
            EventBus.Subscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
            BuildLayout();
        }

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => GameData.Instance != null && GameData.Instance.Object.IsValid &&
                GameData.Instance.LocalRole != 0);
            var data = GameData.Instance;
            var location = data.HasStateAuthority ? p1BuyMenuLocation : p2BuyMenuLocation;
            if (location != null) transform.SetPositionAndRotation(location.position, location.rotation);
            _phase = FindAnyObjectByType<PhaseManager>();
            // UI and server must use exactly the same catalogue indices.
            database = data.AbilityDatabase;
            GenerateCards(data.LocalRole);

            // The overlay can be ready before the local network player. Wait for
            // its Spawned callbacks before releasing the cursor and gameplay input.
            yield return new WaitUntil(() => FindLocalPlayer() != null);
            if (isActiveAndEnabled && _visible && _phase != null && !_phase.IsRunPhase)
                EventBus.Raise(new BuyZoneEnteredEvent(true));
        }

        private static Player FindLocalPlayer()
        {
            foreach (var player in FindObjectsByType<Player>(FindObjectsSortMode.None))
                if (player.Object != null && player.Object.IsValid && player.HasInputAuthority)
                    return player;
            return null;
        }

        private void Update()
        {
            var data = GameData.Instance;
            if (data == null || data.Object == null || !data.Object.IsValid || _menu == null)
                return;
            _balance.text = $"{data.LocalPoints} <size=16>POINTS</size>";
            if (_phase != null && _phase.Object != null && _phase.Object.IsValid)
                _timer.text = _phase.IsRunPhase ? "RUN IN PROGRESS" : $"RUN STARTS IN  {Mathf.CeilToInt(_phase.BuyTimeRemaining):00}s";
        }

        private void OnEnable() { if (_menu != null) _menu.SetActive(_visible); }
        private void OnDisable() { if (_menu != null) _menu.SetActive(false); }
        private void OnDestroy()
        {
            EventBus.Unsubscribe<BuyZoneEnteredEvent>(OnBuyZoneEntered);
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
            if (_menu != null) Destroy(_menu);
        }
        private void OnBuyZoneEntered(BuyZoneEnteredEvent ev) => SetVisible(ev.Entered);
        private void OnRunPhaseStarts(RunPhaseStartsEvent ev) => SetVisible(false);
        private void SetVisible(bool visible)
        {
            _visible = visible;
            if (_menu != null) _menu.SetActive(visible && isActiveAndEnabled);
        }
        private void ExitShop() => EventBus.Raise(new BuyZoneEnteredEvent(false));

        private void BuildLayout()
        {
            _menu = new GameObject("Buy menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = _menu.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = _menu.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var background = _menu.AddComponent<Image>();
            background.color = BuyMenuUI.Background;
            var panel = BuyMenuUI.Rect(_menu.transform, "Shop", Vector2.zero, new Vector2(1280, 720));
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
            BuyMenuUI.Text(panel, "Eyebrow", "PREPARE FOR THE CHASE", 13, BuyMenuUI.Accent, new Vector2(60, -26), new Vector2(600, 24));
            BuyMenuUI.Text(panel, "Title", "Build your loadout", 38, Color.white, new Vector2(60, -52), new Vector2(650, 56));
            _role = BuyMenuUI.Text(panel, "Role", "Choose upgrades and tools for this round.", 17, BuyMenuUI.Muted, new Vector2(60, -108), new Vector2(800, 30));
            _balance = BuyMenuUI.Text(panel, "Balance", "100 <size=16>POINTS</size>", 32, BuyMenuUI.Accent, new Vector2(930, -45), new Vector2(290, 42));
            _balance.alignment = TextAlignmentOptions.Right;
            _timer = BuyMenuUI.Text(panel, "Timer", "WAITING FOR PLAYERS", 16, BuyMenuUI.Muted, new Vector2(900, -100), new Vector2(320, 28));
            _timer.alignment = TextAlignmentOptions.Right;
            _cards = BuyMenuUI.Rect(panel, "Items", new Vector2(60, -156), new Vector2(1160, 476));
            var grid = _cards.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(374, 230);
            grid.spacing = new Vector2(19, 16);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            BuyMenuUI.Text(panel, "Help", "Upgrades apply immediately. Use numbered tools during the run.\nPowerup cubes award points for your next round.", 16, BuyMenuUI.Muted, new Vector2(60, -651), new Vector2(840, 52));
            BuyMenuUI.Button(panel, "LEAVE SHOP", ExitShop, new Vector2(1000, -653), new Vector2(220, 44), out _);
        }

        private void GenerateCards(PlayerRole role)
        {
            _role.text = $"{role.ToString().ToUpperInvariant()}  /  Purchases last for this round";
            if (database == null) return;
            var seen = new HashSet<Ability>();
            for (var index = 0; index < database.allAbilities.Count; index++)
            {
                var ability = database.allAbilities[index];
                if (ability == null || !seen.Add(ability) ||
                    (ability.abilityScope == AbilityScope.Runner && role != PlayerRole.Runner) ||
                    (ability.abilityScope == AbilityScope.Chaser && role != PlayerRole.Chaser)) continue;
                var card = BuyMenuUI.Rect(_cards, ability.abilityName, Vector2.zero, new Vector2(374, 230));
                card.gameObject.AddComponent<BuyMenuItem>().Setup(ability, index);
            }
        }
    }
}
