using _Experimenation.K.Event_Bus;
using _Experimenation.K.Event_Bus.Events;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _Project.Post_Processing.Scripts
{
	[RequireComponent(typeof(Animator))]
	public class PostProcessingManager : MonoBehaviour
	{
		#region Animation Parameters
		private static readonly int CollectToken = Animator.StringToHash("Collect Token");
		private static readonly int SelectAbility = Animator.StringToHash("Select Ability");
		#endregion
		
		#region Effect Values
		[SerializeField] private float lensFlareIntensity;
		[SerializeField] private float chromaIntensity;
		
		private float _defaultLensFlareIntensity;
		private float _defaultChromaIntensity;
		#endregion
		
		#region Effects
		private ScreenSpaceLensFlare _lensFlare;
		private ChromaticAberration _chroma;
		#endregion
		
		private Animator _animator;
		
		private void Awake()
		{
			_animator = GetComponent<Animator>();
			InitProperties();
			InitEventBus();
		}

		private void OnDestroy()
		{
			EventBus.Unsubscribe<TokenCollectedEvent>(OnTokenCollected);
			EventBus.Unsubscribe<AbilitySelectedEvent>(OnAbilitySelected);
		}

		private void InitProperties()
		{
			var profile = GetComponentInChildren<Volume>().profile;
			
			profile.TryGet(out _lensFlare);
			profile.TryGet(out _chroma);
			
			_defaultLensFlareIntensity = (float)_lensFlare.intensity;
			_defaultChromaIntensity = (float)_chroma.intensity;
			
			lensFlareIntensity = _defaultLensFlareIntensity;
			chromaIntensity = _defaultChromaIntensity;
		}

		private void InitEventBus()
		{
			EventBus.Subscribe<TokenCollectedEvent>(OnTokenCollected);
			EventBus.Subscribe<AbilitySelectedEvent>(OnAbilitySelected);
		}

		private void Update()
		{
			_lensFlare.intensity = 
				new MinFloatParameter(lensFlareIntensity, _defaultLensFlareIntensity);
			_chroma.intensity = 
				new ClampedFloatParameter(chromaIntensity, _defaultChromaIntensity, _chroma.intensity.max);
		}

		private void Default()
		{
			lensFlareIntensity = _defaultLensFlareIntensity;
			chromaIntensity = _defaultChromaIntensity;
		}
		
		private void OnTokenCollected(TokenCollectedEvent ev) => _animator.SetTrigger(CollectToken);
		private void OnAbilitySelected(AbilitySelectedEvent ev) => _animator.SetTrigger(SelectAbility);
	}
}
