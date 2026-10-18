using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Experimenation.K._Third_Parties.UI.SlimUI.Modern_Menu_1.Scripts.Managers{
	public class UISettingsManager : MonoBehaviour {

		public enum Platform {Desktop, Mobile};
		public Platform platform;
		// toggle buttons
		[Header("MOBILE SETTINGS")]
		public GameObject mobileSFXtext;
		public GameObject mobileMusictext;
		public GameObject mobileShadowofftextLINE;
		public GameObject mobileShadowlowtextLINE;
		public GameObject mobileShadowhightextLINE;

		[Header("VIDEO SETTINGS")]
		public GameObject fullscreentext;
		public GameObject ambientocclusiontext;
		public GameObject shadowofftextLINE;
		public GameObject shadowlowtextLINE;
		public GameObject shadowhightextLINE;
		public GameObject aaofftextLINE;
		public GameObject aa2xtextLINE;
		public GameObject aa4xtextLINE;
		public GameObject aa8xtextLINE;
		public GameObject vsynctext;
		public GameObject motionblurtext;
		public GameObject texturelowtextLINE;
		public GameObject texturemedtextLINE;
		public GameObject texturehightextLINE;
		public GameObject cameraeffectstext; 

		[Header("GAME SETTINGS")]
		public GameObject showhudtext;
		public GameObject tooltipstext;
		public GameObject difficultynormaltext;
		public GameObject difficultynormaltextLINE;
		public GameObject difficultyhardcoretext;
		public GameObject difficultyhardcoretextLINE;

		[Header("CONTROLS SETTINGS")]
		public GameObject invertmousetext;

		// sliders
		public GameObject musicSlider;
		public GameObject sensitivityXSlider;
		public GameObject sensitivityYSlider;
		public GameObject mouseSmoothSlider;

		

		public void  Start (){
			// check difficulty
			if(PlayerPrefs.GetInt("NormalDifficulty") == 1){
				SetIndicatorActive(difficultynormaltextLINE, true);
				SetIndicatorActive(difficultyhardcoretextLINE, false);
			}
			else
			{
				SetIndicatorActive(difficultyhardcoretextLINE, true);
				SetIndicatorActive(difficultynormaltextLINE, false);
			}

			// check slider values
			LoadSliderValue(musicSlider, "MusicVolume");
			LoadSliderValue(sensitivityXSlider, "XSensitivity");
			LoadSliderValue(sensitivityYSlider, "YSensitivity");
			LoadSliderValue(mouseSmoothSlider, "MouseSmoothing");

			// check full screen
			if(Screen.fullScreen == true){
				SetLabelText(fullscreentext, "on");
			}
			else if(Screen.fullScreen == false){
				SetLabelText(fullscreentext, "off");
			}

			// check hud value
			if(PlayerPrefs.GetInt("ShowHUD")==0){
				SetLabelText(showhudtext, "off");
			}
			else{
				SetLabelText(showhudtext, "on");
			}

			// check tool tip value
			if(PlayerPrefs.GetInt("ToolTips")==0){
				SetLabelText(tooltipstext, "off");
			}
			else{
				SetLabelText(tooltipstext, "on");
			}

			// check shadow distance/enabled
			if(platform == Platform.Desktop && (shadowofftextLINE != null || shadowlowtextLINE != null || shadowhightextLINE != null)){
				if(PlayerPrefs.GetInt("Shadows") == 0){
					QualitySettings.shadowCascades = 0;
					QualitySettings.shadowDistance = 0;
					SetIndicatorActive(shadowofftextLINE, true);
					SetIndicatorActive(shadowlowtextLINE, false);
					SetIndicatorActive(shadowhightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("Shadows") == 1){
					QualitySettings.shadowCascades = 2;
					QualitySettings.shadowDistance = 75;
					SetIndicatorActive(shadowofftextLINE, false);
					SetIndicatorActive(shadowlowtextLINE, true);
					SetIndicatorActive(shadowhightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("Shadows") == 2){
					QualitySettings.shadowCascades = 4;
					QualitySettings.shadowDistance = 500;
					SetIndicatorActive(shadowofftextLINE, false);
					SetIndicatorActive(shadowlowtextLINE, false);
					SetIndicatorActive(shadowhightextLINE, true);
				}
			}else if(platform == Platform.Mobile && (mobileShadowofftextLINE != null || mobileShadowlowtextLINE != null || mobileShadowhightextLINE != null)){
				if(PlayerPrefs.GetInt("MobileShadows") == 0){
					QualitySettings.shadowCascades = 0;
					QualitySettings.shadowDistance = 0;
					SetIndicatorActive(mobileShadowofftextLINE, true);
					SetIndicatorActive(mobileShadowlowtextLINE, false);
					SetIndicatorActive(mobileShadowhightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("MobileShadows") == 1){
					QualitySettings.shadowCascades = 2;
					QualitySettings.shadowDistance = 75;
					SetIndicatorActive(mobileShadowofftextLINE, false);
					SetIndicatorActive(mobileShadowlowtextLINE, true);
					SetIndicatorActive(mobileShadowhightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("MobileShadows") == 2){
					QualitySettings.shadowCascades = 4;
					QualitySettings.shadowDistance = 100;
					SetIndicatorActive(mobileShadowofftextLINE, false);
					SetIndicatorActive(mobileShadowlowtextLINE, false);
					SetIndicatorActive(mobileShadowhightextLINE, true);
				}
			}


			// check vsync
			if(QualitySettings.vSyncCount == 0){
				SetLabelText(vsynctext, "off");
			}
			else if(QualitySettings.vSyncCount == 1){
				SetLabelText(vsynctext, "on");
			}

			// check mouse inverse
			if(PlayerPrefs.GetInt("Inverted")==0){
				SetLabelText(invertmousetext, "off");
			}
			else if(PlayerPrefs.GetInt("Inverted")==1){
				SetLabelText(invertmousetext, "on");
			}

			// check motion blur
			if(PlayerPrefs.GetInt("MotionBlur")==0){
				SetLabelText(motionblurtext, "off");
			}
			else if(PlayerPrefs.GetInt("MotionBlur")==1){
				SetLabelText(motionblurtext, "on");
			}

			// check ambient occlusion
			if(PlayerPrefs.GetInt("AmbientOcclusion")==0){
				SetLabelText(ambientocclusiontext, "off");
			}
			else if(PlayerPrefs.GetInt("AmbientOcclusion")==1){
				SetLabelText(ambientocclusiontext, "on");
			}

			// check texture quality
			if(texturelowtextLINE != null || texturemedtextLINE != null || texturehightextLINE != null){
				if(PlayerPrefs.GetInt("Textures") == 0){
					QualitySettings.globalTextureMipmapLimit = 2;
					SetIndicatorActive(texturelowtextLINE, true);
					SetIndicatorActive(texturemedtextLINE, false);
					SetIndicatorActive(texturehightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("Textures") == 1){
					QualitySettings.globalTextureMipmapLimit = 1;
					SetIndicatorActive(texturelowtextLINE, false);
					SetIndicatorActive(texturemedtextLINE, true);
					SetIndicatorActive(texturehightextLINE, false);
				}
				else if(PlayerPrefs.GetInt("Textures") == 2){
					QualitySettings.globalTextureMipmapLimit = 0;
					SetIndicatorActive(texturelowtextLINE, false);
					SetIndicatorActive(texturemedtextLINE, false);
					SetIndicatorActive(texturehightextLINE, true);
				}
			}
		}

		// Customized menus can omit controls from the original template.
		private static void SetIndicatorActive(GameObject indicator, bool active){
			if(indicator != null){
				indicator.SetActive(active);
			}
		}

		private static void SetLabelText(GameObject label, string text){
			if(label != null && label.TryGetComponent<TMP_Text>(out var textComponent)){
				textComponent.text = text;
			}
		}

		private static void LoadSliderValue(GameObject sliderObject, string preferenceKey){
			if(sliderObject != null && sliderObject.TryGetComponent<Slider>(out var slider)){
				slider.value = PlayerPrefs.GetFloat(preferenceKey);
			}
		}

		private static void SaveSliderValue(GameObject sliderObject, string preferenceKey){
			if(sliderObject != null && sliderObject.TryGetComponent<Slider>(out var slider)){
				PlayerPrefs.SetFloat(preferenceKey, slider.value);
			}
		}

		public void FullScreen (){
			Screen.fullScreen = !Screen.fullScreen;

			if(Screen.fullScreen == true){
				SetLabelText(fullscreentext, "on");
			}
			else if(Screen.fullScreen == false){
				SetLabelText(fullscreentext, "off");
			}
		}

		public void MusicSlider (){
			SaveSliderValue(musicSlider, "MusicVolume");
		}

		public void SensitivityXSlider (){
			SaveSliderValue(sensitivityXSlider, "XSensitivity");
		}

		public void SensitivityYSlider (){
			SaveSliderValue(sensitivityYSlider, "YSensitivity");
		}

		public void SensitivitySmoothing (){
			SaveSliderValue(mouseSmoothSlider, "MouseSmoothing");
		}

		// the playerprefs variable that is checked to enable hud while in game
		public void ShowHUD (){
			if(PlayerPrefs.GetInt("ShowHUD")==0){
				PlayerPrefs.SetInt("ShowHUD",1);
				SetLabelText(showhudtext, "on");
			}
			else if(PlayerPrefs.GetInt("ShowHUD")==1){
				PlayerPrefs.SetInt("ShowHUD",0);
				SetLabelText(showhudtext, "off");
			}
		}

		// the playerprefs variable that is checked to enable mobile sfx while in game
		public void MobileSFXMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteSfx")==0){
				PlayerPrefs.SetInt("Mobile_MuteSfx",1);
				SetLabelText(mobileSFXtext, "on");
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteSfx")==1){
				PlayerPrefs.SetInt("Mobile_MuteSfx",0);
				SetLabelText(mobileSFXtext, "off");
			}
		}

		public void MobileMusicMute (){
			if(PlayerPrefs.GetInt("Mobile_MuteMusic")==0){
				PlayerPrefs.SetInt("Mobile_MuteMusic",1);
				SetLabelText(mobileMusictext, "on");
			}
			else if(PlayerPrefs.GetInt("Mobile_MuteMusic")==1){
				PlayerPrefs.SetInt("Mobile_MuteMusic",0);
				SetLabelText(mobileMusictext, "off");
			}
		}

		// show tool tips like: 'How to Play' control pop ups
		public void ToolTips (){
			if(PlayerPrefs.GetInt("ToolTips")==0){
				PlayerPrefs.SetInt("ToolTips",1);
				SetLabelText(tooltipstext, "on");
			}
			else if(PlayerPrefs.GetInt("ToolTips")==1){
				PlayerPrefs.SetInt("ToolTips",0);
				SetLabelText(tooltipstext, "off");
			}
		}

		public void NormalDifficulty (){
			SetIndicatorActive(difficultyhardcoretextLINE, false);
			SetIndicatorActive(difficultynormaltextLINE, true);
			PlayerPrefs.SetInt("NormalDifficulty",1);
			PlayerPrefs.SetInt("HardCoreDifficulty",0);
		}

		public void HardcoreDifficulty (){
			SetIndicatorActive(difficultyhardcoretextLINE, true);
			SetIndicatorActive(difficultynormaltextLINE, false);
			PlayerPrefs.SetInt("NormalDifficulty",0);
			PlayerPrefs.SetInt("HardCoreDifficulty",1);
		}

		public void ShadowsOff (){
			PlayerPrefs.SetInt("Shadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			SetIndicatorActive(shadowofftextLINE, true);
			SetIndicatorActive(shadowlowtextLINE, false);
			SetIndicatorActive(shadowhightextLINE, false);
		}

		public void ShadowsLow (){
			PlayerPrefs.SetInt("Shadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			SetIndicatorActive(shadowofftextLINE, false);
			SetIndicatorActive(shadowlowtextLINE, true);
			SetIndicatorActive(shadowhightextLINE, false);
		}

		public void ShadowsHigh (){
			PlayerPrefs.SetInt("Shadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			SetIndicatorActive(shadowofftextLINE, false);
			SetIndicatorActive(shadowlowtextLINE, false);
			SetIndicatorActive(shadowhightextLINE, true);
		}

		public void MobileShadowsOff (){
			PlayerPrefs.SetInt("MobileShadows",0);
			QualitySettings.shadowCascades = 0;
			QualitySettings.shadowDistance = 0;
			SetIndicatorActive(mobileShadowofftextLINE, true);
			SetIndicatorActive(mobileShadowlowtextLINE, false);
			SetIndicatorActive(mobileShadowhightextLINE, false);
		}

		public void MobileShadowsLow (){
			PlayerPrefs.SetInt("MobileShadows",1);
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowDistance = 75;
			SetIndicatorActive(mobileShadowofftextLINE, false);
			SetIndicatorActive(mobileShadowlowtextLINE, true);
			SetIndicatorActive(mobileShadowhightextLINE, false);
		}

		public void MobileShadowsHigh (){
			PlayerPrefs.SetInt("MobileShadows",2);
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowDistance = 500;
			SetIndicatorActive(mobileShadowofftextLINE, false);
			SetIndicatorActive(mobileShadowlowtextLINE, false);
			SetIndicatorActive(mobileShadowhightextLINE, true);
		}

		public void vsync (){
			if(QualitySettings.vSyncCount == 0){
				QualitySettings.vSyncCount = 1;
				SetLabelText(vsynctext, "on");
			}
			else if(QualitySettings.vSyncCount == 1){
				QualitySettings.vSyncCount = 0;
				SetLabelText(vsynctext, "off");
			}
		}

		public void InvertMouse (){
			if(PlayerPrefs.GetInt("Inverted")==0){
				PlayerPrefs.SetInt("Inverted",1);
				SetLabelText(invertmousetext, "on");
			}
			else if(PlayerPrefs.GetInt("Inverted")==1){
				PlayerPrefs.SetInt("Inverted",0);
				SetLabelText(invertmousetext, "off");
			}
		}

		public void MotionBlur (){
			if(PlayerPrefs.GetInt("MotionBlur")==0){
				PlayerPrefs.SetInt("MotionBlur",1);
				SetLabelText(motionblurtext, "on");
			}
			else if(PlayerPrefs.GetInt("MotionBlur")==1){
				PlayerPrefs.SetInt("MotionBlur",0);
				SetLabelText(motionblurtext, "off");
			}
		}

		public void AmbientOcclusion (){
			if(PlayerPrefs.GetInt("AmbientOcclusion")==0){
				PlayerPrefs.SetInt("AmbientOcclusion",1);
				SetLabelText(ambientocclusiontext, "on");
			}
			else if(PlayerPrefs.GetInt("AmbientOcclusion")==1){
				PlayerPrefs.SetInt("AmbientOcclusion",0);
				SetLabelText(ambientocclusiontext, "off");
			}
		}

		public void CameraEffects (){
			if(PlayerPrefs.GetInt("CameraEffects")==0){
				PlayerPrefs.SetInt("CameraEffects",1);
				SetLabelText(cameraeffectstext, "on");
			}
			else if(PlayerPrefs.GetInt("CameraEffects")==1){
				PlayerPrefs.SetInt("CameraEffects",0);
				SetLabelText(cameraeffectstext, "off");
			}
		}

		public void TexturesLow (){
			PlayerPrefs.SetInt("Textures",0);
			QualitySettings.globalTextureMipmapLimit = 2;
			SetIndicatorActive(texturelowtextLINE, true);
			SetIndicatorActive(texturemedtextLINE, false);
			SetIndicatorActive(texturehightextLINE, false);
		}

		public void TexturesMed (){
			PlayerPrefs.SetInt("Textures",1);
			QualitySettings.globalTextureMipmapLimit = 1;
			SetIndicatorActive(texturelowtextLINE, false);
			SetIndicatorActive(texturemedtextLINE, true);
			SetIndicatorActive(texturehightextLINE, false);
		}

		public void TexturesHigh (){
			PlayerPrefs.SetInt("Textures",2);
			QualitySettings.globalTextureMipmapLimit = 0;
			SetIndicatorActive(texturelowtextLINE, false);
			SetIndicatorActive(texturemedtextLINE, false);
			SetIndicatorActive(texturehightextLINE, true);
		}
	}
}