using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Experimenation.K._Third_Parties.UI.SlimUI.Modern_Menu_1.Scripts.Misc{
	public class ResetDemo : MonoBehaviour {

		void Update () {
			if(Input.GetKeyDown("r")){
				SceneManager.LoadScene(0);
			}
		}
	}
}