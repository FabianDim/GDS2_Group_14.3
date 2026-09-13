using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;

namespace _Project.Audio.Scripts
{
    [RequireComponent(typeof(AudioSource))]
    public class BGM : MonoBehaviour
    {
        [SerializeField] private AudioClip runPhaseBgm;
        private AudioSource _as;
        
        private void Awake()
        {
            _as = GetComponent<AudioSource>();
            EventBus.Subscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);
        }
        
        private void OnDestroy() => 
            EventBus.Unsubscribe<RunPhaseStartsEvent>(OnRunPhaseStarts);

        private void OnRunPhaseStarts(RunPhaseStartsEvent ev)
        {
            _as.clip = runPhaseBgm;
            _as.Play();
        }
    }
}
