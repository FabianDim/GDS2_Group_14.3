using System;
using System.Collections;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using TMPro;
using UnityEngine;

namespace _Experimenation.K.Game_Manager.Scripts
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private int roundDuration = 180;
        private TimeSpan _roundDuration;
        private readonly TimeSpan _1S = new(0, 0, 1);
        private readonly WaitForSeconds _wait1S = new(1);
        
        private TextMeshProUGUI _timeText;
        
        private void Awake()
        {
            _timeText = GetComponentInChildren<TextMeshProUGUI>();
            _roundDuration = TimeSpan.FromSeconds(roundDuration);
            _timeText.SetText(_roundDuration.ToString(@"mm\:ss"));
        }

        private IEnumerator Start()
        {
            while (roundDuration > 0)
            {
                yield return _wait1S;
                _roundDuration -= _1S;
                _timeText.SetText(_roundDuration.ToString(@"mm\:ss"));
            }
            EventBus.Raise(new TimeRunsOutEvent());
        }
    }
}