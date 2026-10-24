using UnityEngine;
namespace _Project.Audio.Scripts
{
	public class AnimStateAudio : StateMachineBehaviour
	{
		[SerializeField] private GameObject sourceObject;
		[SerializeField] private AudioSource source;
		[SerializeField] private AudioClip clip;
		
		private void Awake() => source = sourceObject.GetComponent<AudioSource>();
		
		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) => 
			source.PlayOneShot(clip);
	}
}
