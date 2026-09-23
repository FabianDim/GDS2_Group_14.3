using _Experimenation.K.Game_Manager.Scripts;
using _Experimenation.K.Multiplayer.Scripts;
using UnityEngine;

namespace _Experimenation.Fabian.Scripts.Debug
{
    public class PlayerDebugOverlay : MonoBehaviour
    {
        [Header("Overlay")]
        [SerializeField] private KeyCode toggleKey = KeyCode.F3;
        [SerializeField] private Vector2 offset = new Vector2(20f, 20f);
        [SerializeField] private int fontSize = 20;
        [SerializeField] private bool startVisible = false;

        private bool _visible;
        private GUIStyle _panelStyle;
        private GUIStyle _textStyle;

        private void Awake()
        {
            _visible = startVisible;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible)
                return;

            // GUI.skin is only available during an OnGUI callback.
            if (_panelStyle == null || _textStyle == null)
                CreateStyles();

            var status = GetStatusText();
            var content = new GUIContent(status);
            var textSize = _textStyle.CalcSize(content);
            var rect = new Rect(offset.x, offset.y, textSize.x + 22f, textSize.y + 14f);

            GUI.Box(rect, GUIContent.none, _panelStyle);
            GUI.Label(new Rect(offset.x + 10f, offset.y + 6f, textSize.x, textSize.y), status, _textStyle);
        }

        private string GetStatusText()
        {
            if (GameData.Instance == null || GameData.Instance.Object == null ||
                !GameData.Instance.Object.IsValid)
                return "Host: Waiting\nRole: Waiting";

            var gameData = GameData.Instance;
            var isHost = gameData.HasStateAuthority;
            var localRole = isHost ? gameData.P1Data.Role : gameData.P2Data.Role;
            var roleText = localRole switch
            {
                PlayerRole.Chaser => "Chaser",
                PlayerRole.Runner => "Runner",
                _ => "Waiting"
            };

            return $"Host: {(isHost ? "Yes" : "No")}\nRole: {roleText}";
        }

        private void CreateStyles()
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 8, 4, 4)
            };
            _panelStyle.normal.textColor = Color.white;

            _textStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = fontSize,
                alignment = TextAnchor.MiddleLeft
            };
            _textStyle.normal.textColor = Color.white;
        }
    }
}
