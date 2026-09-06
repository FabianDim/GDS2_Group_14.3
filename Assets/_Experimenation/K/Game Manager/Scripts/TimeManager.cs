using Fusion;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class TimeManager : NetworkBehaviour
    {
        [SerializeField] private int roundDuration = 180;
        [Networked] private TickTimer Timer { get; set; }
        
        private TextMeshProUGUI _timeText;
        
        public override void Spawned()
        {
            _timeText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void OnEnable() => Timer = TickTimer.CreateFromSeconds(Runner, roundDuration);
        
        private void Update()
        {
            if (!Timer.IsRunning)
            {
                _timeText.SetText("0:00");
                return;
            }

            var remaining = Mathf.CeilToInt((float)Timer.RemainingTime(Runner));

            _timeText.SetText(
                $"{remaining / 60}:{remaining % 60:00}"
            );
        }
    }
}