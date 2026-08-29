using UnityEngine;

namespace _Experimenation.K._Third_Parties.UI.SlimUI.Modern_Menu_1.Scripts.ThemeEditor{
	[CreateAssetMenu(menuName = "ThemeSettings")]
	[global::System.Serializable]
	public class ThemedUIData : ScriptableObject {
		[global::System.Serializable]
		public class Custom1{
			[Header("Text")]	
			public Color graphic1;
			public Color32 text1;
		}

		[global::System.Serializable]
		public class Custom2{
			[Header("Text")]	
			public Color graphic2;
			public Color32 text2;
		}

		[global::System.Serializable]
		public class Custom3{
			[Header("Text")]	
			public Color graphic3;
			public Color32 text3;
		}

		[Header("PRESETS")]
		public Custom1 custom1;
		public Custom2 custom2;
		public Custom3 custom3;

		[HideInInspector]
		public Color currentColor;
		[HideInInspector]
		public Color32 textColor;
	}
}