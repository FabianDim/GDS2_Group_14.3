using System.Collections.Generic;
using UnityEngine;

namespace TargetIndicators.Samples
{
    public class CompassTapeTargetIndicatorsSetupExample : MonoBehaviour
    {
        [SerializeField, Tooltip("The visual indicator manager used for adding compass tape visual indicators to.")]
        CompassTapeVisualIndicatorManager _visualIndicatorManager;

        [Header("Player Targets")]
        [SerializeField]
        Transform _player1Target;

        [SerializeField]
        CompassTapeVisualIndicator _player1UIPrefab;

        readonly Dictionary<Transform, TargetIndicatorId> _targetsToIndicatorIds = new();
        bool _started;

        void Start()
        {
            // Wait until the visual manager has initialized in Awake.
            _started = true;
            AddTarget();
        }

        void OnEnable()
        {
            if (_started)
                AddTarget();
        }

        public void SetTarget(Transform target)
        {
            RemoveTargets();
            _player1Target = target;

            if (_started && isActiveAndEnabled)
                AddTarget();
        }

        void AddTarget()
        {
            // Network players may not exist when the scene UI first enables.
            if (_visualIndicatorManager == null || _player1Target == null || _player1UIPrefab == null)
                return;

            // First set the `AddIndicatorMode` to manual so we can use custom prefabs for each target's visual indicator.
            _visualIndicatorManager.AddIndicatorMode = AddIndicatorMode.Manual;

            var wasAdded = _visualIndicatorManager.TryAddVisualIndicator(_player1Target, _player1UIPrefab, out var id);
            if (wasAdded)
                _targetsToIndicatorIds.Add(_player1Target, id);
        }

        void OnDisable()
        {
            RemoveTargets();
        }

        void RemoveTargets()
        {
            foreach (var (target, id) in _targetsToIndicatorIds)
            {
                if (_visualIndicatorManager != null)
                    _visualIndicatorManager.RemoveTargetIndicator(id);
            }

            _targetsToIndicatorIds.Clear();
        }
    }
}
