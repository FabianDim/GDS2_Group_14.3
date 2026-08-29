using UnityEngine;

namespace _Experimenation.K._Third_Parties.UI.SlimUI.Modern_Menu_1.Scripts.ThemeEditor{
	[ExecuteInEditMode()]
	[global::System.Serializable]
	public class ThemedUI : MonoBehaviour {

		public ThemedUIData themeController;

		protected virtual void OnSkinUI(){

		}

		public virtual void Awake(){
			OnSkinUI();
		}

		public virtual void Update(){
			OnSkinUI();
		}
	}
}
