using System.Collections;
using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using _Experimenation.K.Multiplayer.Scripts;
using Fusion;
using UnityEngine;

namespace _Experimenation.K.Exfiltration_Pod.Scripts
{
    public class ExfiltrationPod : NetworkBehaviour
    {
        [SerializeField] private float distanceFromRunner = 10f;
        [SerializeField] private int pointTarget = 100;
        private NetworkObject _runner;  // swapped to network obj
        private bool _conditionCleared;
        private bool _runnerReachedPod;
        private GameObject _pod;

        [SerializeField] private float podAvailableTime = 30f;
        private WaitForSeconds _podAvailableTime;
        [SerializeField] private float podUnavailableTime = 15f;
        private WaitForSeconds _podUnavailableTime;
        
        [Networked] private TickTimer PodTimer { get; set; }
        [Networked] private bool PodActive { get; set; }
        
        private void Awake()
        {
            //_runner = GameObject.FindGameObjectWithTag("Runner");
            _pod = transform.GetChild(0).gameObject;
            /*_podAvailableTime = new WaitForSeconds(podAvailableTime);
            _podUnavailableTime = new WaitForSeconds(podUnavailableTime);*/
            
            EventBus.Subscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Subscribe<PlayersSpawnedEvent>(OnPlayersSpawned);
        }

        public override void Spawned()
        {
            _pod.SetActive(false);
            
            if (!Object.HasStateAuthority) return;

            /*// find the runner network object 
            foreach (var player in Runner.ActivePlayers)
            {
                var obj = Runner.GetPlayerObject(player);
                if (obj != null && obj.GetComponent<Player>().Role == PlayerRole.Runner)
                {
                    Debug.Log($"Runner found.");
                    _runner = obj;
                    break;
                }
            }*/
        }

        private void OnDestroy()
        {
            EventBus.Unsubscribe<TimeRunsOutEvent>(OnTimeRunsOut);
            EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
            EventBus.Unsubscribe<PlayersSpawnedEvent>(OnPlayersSpawned);
        }
        
        public override void FixedUpdateNetwork()
        {
            if (Object.HasStateAuthority)
            {
                // converted pod coroutine behaviour 
                if (_runner == null) return;
                if (!_conditionCleared) return;

                if (PodActive)
                {
                    if (PodTimer.Expired(Runner))
                    {
                        PodActive = false;
                        PodTimer = TickTimer.CreateFromSeconds(Runner, podUnavailableTime);
                        Debug.Log($"Pod deactivated for {podUnavailableTime}s.");
                    }
                    
                    if (RunnerReachedPod())
                    {
                        EventBus.Raise(new RoundOverEvent(true));
                        _runnerReachedPod = true;
                        Debug.Log($"Runner reached pod.");
                    }
                }
                else
                {
                    if (PodTimer.Expired(Runner))
                    {
                        SpawnPod();
                        PodActive = true;
                        PodTimer = TickTimer.CreateFromSeconds(Runner, podAvailableTime);
                    }
                }
            }
            _pod.SetActive(PodActive);
        }

        /*private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Runner")) return;
            EventBus.Raise(new RoundOverEvent(true));
        }*/

        // using a distance check instead of OnTriggerEnter to avoid using fusion physics
        bool RunnerReachedPod()
        {
            if (_runner == null) return false;
            return Vector3.Distance(_runner.transform.position, transform.position) < 2f;
        }
        
        private void SpawnPod()
        {
            if (_runner == null) return;

            var spawnPos = _runner.transform.position + _runner.transform.forward * distanceFromRunner;
            transform.position = spawnPos;
            Debug.Log($"Pod activated at {spawnPos} for {podAvailableTime}s.");
        }
        
        #region Event Bus Handlers
        private void OnTimeRunsOut(TimeRunsOutEvent ev)
        {
            if (!Object.HasStateAuthority) return;
            if(_conditionCleared) return;
            //StartCoroutine(SpawnPod());
            _conditionCleared = true;
            PodTimer = TickTimer.CreateFromSeconds(Runner, podAvailableTime);
            PodActive = true;
        }

        private void OnTokenCollected(TokenCollectedEvent ev)
        {
            if (!Object.HasStateAuthority) return;
            if(_conditionCleared) return;
            pointTarget -= ev.Points;
            if (pointTarget > 0) return;
            //StartCoroutine(SpawnPod());
            SpawnPod();
            _conditionCleared = true;
            PodTimer = TickTimer.CreateFromSeconds(Runner, podAvailableTime);
            PodActive = true;
        }
        
        private void OnPlayersSpawned(PlayersSpawnedEvent ev)
        {
            _runner = ev.Runner;
        }

        /*private IEnumerator SpawnPod()
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
        }*/
        #endregion
    }
}
