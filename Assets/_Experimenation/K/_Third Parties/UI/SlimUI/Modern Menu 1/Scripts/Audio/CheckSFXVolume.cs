using UnityEngine;

namespace _Experimenation.K._Third_Parties.UI.SlimUI.Modern_Menu_1.Scripts.Audio{
	public class CheckSFXVolume : MonoBehaviour {
		public void  Start (){
			// remember volume level from last time
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFXVolume");
			Debug.Log(PlayerPrefs.GetFloat("SFXVolume"));
		}

		public void UpdateVolume (){
			GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("SFXVolume");
		}
	}
}