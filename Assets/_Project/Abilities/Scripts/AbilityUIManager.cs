using System.Text;
using _Experimenation.Fabian.Scripts.Buy_Menu;
using _Experimenation.Fabian.Scripts.PhaseController;
using _Experimenation.K.Game_Manager.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Abilities.Scripts
{
    /// <summary>Displays owned tool shortcuts, never random cube rewards.</summary>
    public class AbilityUIManager : MonoBehaviour
    {
        private GameObject _hud;
        private TMP_Text _tools;
        private PhaseManager _phase;
        private uint _lastMask = uint.MaxValue;

        private void Awake()
        {
            // Existing scene children are the retired random-choice cards.
            foreach (Transform child in transform) child.gameObject.SetActive(false);
            _phase = transform.root.GetComponentInChildren<PhaseManager>(true);
            _hud = new GameObject("Purchased tools", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            _hud.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = _hud.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var panel = BuyMenuUI.Rect(_hud.transform, "Tool shortcuts", Vector2.zero, new Vector2(700, 40));
            panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0);
            panel.anchoredPosition = new Vector2(0, 24);
            panel.gameObject.AddComponent<Image>().color = BuyMenuUI.Background;
            _tools = BuyMenuUI.Text(panel, "Tools", "", 18, BuyMenuUI.Accent, Vector2.zero, new Vector2(700, 40));
            _tools.alignment = TextAlignmentOptions.Center;
            _hud.SetActive(false);
        }

        private void Update()
        {
            var data = GameData.Instance;
            if (data == null || data.Object == null || !data.Object.IsValid || _phase == null ||
                _phase.Object == null || !_phase.Object.IsValid) return;
            data.PurchasedAbilities.TryGet(data.Runner.LocalPlayer, out var mask);
            if (mask != _lastMask)
            {
                _lastMask = mask;
                var text = new StringBuilder();
                for (var slot = 0; slot < 4; slot++)
                {
                    var index = data.GetToolAbilityIndex(data.Runner.LocalPlayer, slot);
                    if (index < 0) break;
                    if (text.Length > 0) text.Append("     ");
                    text.Append($"[{slot + 1}] {data.AbilityDatabase.allAbilities[index].abilityName}");
                }
                _tools.text = text.ToString();
            }
            _hud.SetActive(_phase.IsRunPhase && _tools.text.Length > 0);
        }

        private void OnDisable() { if (_hud != null) _hud.SetActive(false); }
        private void OnDestroy() { if (_hud != null) Destroy(_hud); }
    }
}
