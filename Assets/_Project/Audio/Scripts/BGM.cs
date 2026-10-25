using System.Collections;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;

namespace _Project.Audio.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class BGM : MonoBehaviour
    {
        [SerializeField] private AudioClip runPhaseBgm;
        [SerializeField] private AudioSource as1;
        [SerializeField] private AudioSource as2;

        [SerializeField] private float loopPoint;
        private WaitForSeconds _loopPoint;
        private AudioSource _currentAs;
        
        private void Awake()
        {
            _loopPoint = new WaitForSeconds(loopPoint);
            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }
        
        private void OnDestroy() => 
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev)
        {
            StartCoroutine(RunPhaseBgm());
        }

        private IEnumerator RunPhaseBgm()
        {
            _currentAs = as1;
            _currentAs.clip = runPhaseBgm;
            _currentAs.Play();

            while (true)
            {
                yield return _loopPoint;
                _currentAs = _currentAs == as1 ? as2 : as1;
                _currentAs.Play();
            }
        }
    }
}
