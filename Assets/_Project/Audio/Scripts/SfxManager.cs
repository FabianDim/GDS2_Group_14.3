using UnityEngine;
using UnityEngine.UI;
namespace _Project.Audio.Scripts
{
	[RequireComponent(typeof(AudioSource))]
	public class SfxManager : MonoBehaviour
	{
		private static SfxManager _instance;
		private AudioSource _source;
		[SerializeField] private AudioClip buttonSfx;
		[SerializeField] private float volume = 1f;

		private void Awake()
		{
			if(_instance && _instance != this) Destroy(gameObject);
			else _instance = this;
			
			_source = GetComponent<AudioSource>();

			foreach(Button button in FindObjectsByType<Button>())
				button.onClick.AddListener(ButtonSfx);
		}

		private void ButtonSfx() => _source.PlayOneShot(buttonSfx, volume);
	}
}
