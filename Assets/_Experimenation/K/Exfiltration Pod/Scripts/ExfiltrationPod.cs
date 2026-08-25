using System.Collections;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;

namespace _Experimenation.K.Exfiltration_Pod.Scripts
{
    public class ExfiltrationPod : MonoBehaviour
    {
        [SerializeField] private float distanceFromRunner = 10f;
        [SerializeField] private int pointTarget = 100;
        private GameObject _runner;
        private bool _conditionCleared;
        private bool _runnerReachedPod;
        private GameObject _pod;

        [SerializeField] private float podAvailableTime = 30f;
        private WaitForSeconds _podAvailableTime;
        [SerializeField] private float podUnavailableTime = 15f;
        private WaitForSeconds _podUnavailableTime;
        
        private void Awake()
        {
            _runner = GameObject.FindGameObjectWithTag("Runner");
            _pod = transform.GetChild(0).gameObject;
            _pod.SetActive(false);
            _podAvailableTime = new WaitForSeconds(podAvailableTime);
            _podUnavailableTime = new WaitForSeconds(podUnavailableTime);
            
            EventBus.Subscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Runner")) return;
            EventBus.Raise(new RoundOverEvent(true));
        }
        
        #region Event Bus Handlers
        private void OnTimeRunsOut(TimeRunsOutEvent ev)
        {
            if(_conditionCleared) return;
            StartCoroutine(SpawnPod());
            _conditionCleared = true;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if(_conditionCleared) return;
            pointTarget -= ev.points;
            if (pointTarget > 0) return;
            StartCoroutine(SpawnPod());
            _conditionCleared = true;
        }

        private IEnumerator SpawnPod()
        {
            if(_conditionCleared) yield break;
            while (!_runnerReachedPod)
            {
                var spawnPos = _runner.transform.position + _runner.transform.forward * distanceFromRunner;
                transform.position = spawnPos;
                _pod.SetActive(true);
                yield return _podAvailableTime;
                _pod.SetActive(false);
                yield return _podUnavailableTime;
            }
        }
        #endregion
    }
}
