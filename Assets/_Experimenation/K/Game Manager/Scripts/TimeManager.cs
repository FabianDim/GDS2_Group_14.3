using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class TimeManager : NetworkBehaviour
    {
        private readonly float _roundDuration = GameData.Instance.roundDuration * 0.75f;
        [Networked] private TickTimer Timer { get; set; }
        
        private TextMeshProUGUI _timeText;
        private int _lastDisplayedSecond = -1;
        
        public override void Spawned()
        {
            _timeText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable() => Timer = TickTimer.CreateFromSeconds(Runner, _roundDuration);
        
        private void Update()
        {
            if (_timeText == null)
                return;

            // Repaint only when the displayed second changes - avoids per-frame
            // string interpolation and TMP re-layout.
            var remaining = Timer.IsRunning
                ? Mathf.CeilToInt((float)(Timer.RemainingTime(Runner) ?? 0d))
                : 0;
            remaining = Mathf.Max(0, remaining);

            if (remaining == _lastDisplayedSecond)
                return;

            _lastDisplayedSecond = remaining;
            _timeText.SetText($"{remaining / 60}:{remaining % 60:00}");
        }
    }
}