using UnityEngine;
namespace _Project.Audio.Scripts
{
	public class PlayerAudio : MonoBehaviour
	{
		[SerializeField] private AudioSource source;
		[SerializeField] private AudioClip[] walkClips;
		[SerializeField] private AudioClip[] crouchClips;
		[SerializeField] private AudioClip[] runClips;
		[SerializeField] private AudioClip[] jumpClips;
		[SerializeField] private AudioClip[] landClips;
		[SerializeField] private float sfxVolume = 2f;
		
		private void Walk() => PlaySound(walkClips[Random.Range(0, walkClips.Length)]);
		private void Crouch() => PlaySound(crouchClips[Random.Range(0, crouchClips.Length)]);
		private void Run() => PlaySound(runClips[Random.Range(0, runClips.Length)]);
		private void Jump() => PlaySound(jumpClips[Random.Range(0, jumpClips.Length)]);
		private void Land() => PlaySound(landClips[Random.Range(0, landClips.Length)]);
		
		private void PlaySound(AudioClip clip) => source.PlayOneShot(clip, sfxVolume);
	}
}
