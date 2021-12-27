using System;
using UnityEngine;
using Solarmax;

public class SettingWindow : BaseWindow
{
	public Animator animator;

	public UISprite musicBg;
	public UISprite musicOn;
	public UILabel musicValue;
	public UISprite soundBg;
	public UISprite soundOn;
	public UILabel soundValue;
	public UIToggle[] effectToggles;
	public UIToggle[] fightOptionToggles;

	public UILabel version;
	public UILabel ipLabel;
	public UILabel bundleLabel;

	private Color btn_on_color = new Color (0, 209 / 255.0f, 1, 1);
	private Color btn_off_color = new Color (1, 1, 1, 0.5f);

	private void Awake()
	{
		musicBg.gameObject.GetComponent <UIEventListener> ().onClick = OnMusicClick;
		soundBg.gameObject.GetComponent <UIEventListener> ().onClick = OnSoundClick;
	}

	public override void OnShow ()
	{
		SetPage ();

		//animator.Play ("SettingWindow_in");
	}

	public override void OnHide ()
	{

	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{

	}

	public void SetPage()
	{
		Vector3 onPos = Vector3.zero;
		Vector3 valuePos = Vector3.zero;
		// 背景音
		onPos = musicBg.transform.localPosition;
		valuePos = musicBg.transform.localPosition;
		if (LocalSettingStorage.Get ().music) {
			onPos.x += 65.0f / 2; // musicOn.bg / 2;
			valuePos.x -= 65.0f / 2;
			musicOn.color = musicBg.color = musicValue.color = btn_on_color;
			musicValue.text = DictionaryDataProvider.GetValue (401);
		} else {
			onPos.x -= 65.0f / 2;
			valuePos.x += 65.0f / 2;
			musicOn.color = musicBg.color = musicValue.color = btn_off_color;
			musicValue.text = DictionaryDataProvider.GetValue (402);
		}
		musicOn.transform.localPosition = onPos;
		musicValue.transform.localPosition = valuePos;

		// 音效
		onPos = soundBg.transform.localPosition;
		valuePos = soundBg.transform.localPosition;
		if (LocalSettingStorage.Get ().sound) {
			onPos.x += 65.0f / 2;
			valuePos.x -= 65.0f / 2;
			soundOn.color = soundBg.color = soundValue.color = btn_on_color;
			soundValue.text = DictionaryDataProvider.GetValue (401);
		} else {
			onPos.x -= 65.0f / 2;
			valuePos.x += 65.0f / 2;
			soundOn.color = soundBg.color = soundValue.color = btn_off_color;
			soundValue.text = DictionaryDataProvider.GetValue (402);
		}
		soundOn.transform.localPosition = onPos;
		soundValue.transform.localPosition = valuePos;

		// 效果
		for (int i = 0; i < effectToggles.Length; ++i) {
			if (i == LocalSettingStorage.Get ().effectLevel) {
				effectToggles [i].Set (true, false);
				SetToggleColor (effectToggles [i].gameObject, true);
			} else {
				effectToggles [i].Set (false, false);
				SetToggleColor (effectToggles [i].gameObject, false);
			}
		}

		// 分兵方式
		for (int i = 0; i < fightOptionToggles.Length; ++i) {
			if (i == LocalSettingStorage.Get ().fightOption) {
				fightOptionToggles [i].Set (true, false);
				SetToggleColor (fightOptionToggles [i].gameObject, true);
			} else {
				fightOptionToggles [i].Set (false, false);
				SetToggleColor (fightOptionToggles [i].gameObject, false);
			}
		}

		// version
		version.text = Application.version;
	}

	private void SetToggleColor(GameObject go, bool on)
	{
		UISprite kuang = go.GetComponent <UISprite> ();
		if (on) {
			kuang.color = btn_on_color;
		} else {
			kuang.color = btn_off_color;
		}
	}

	public void OnMusicClick(GameObject go)
	{
		LocalSettingStorage.Get ().music = !LocalSettingStorage.Get ().music;
		SetPage ();

		// 播放/停止背景音
		if (LocalSettingStorage.Get ().music) {
			AudioManger.Get ().MuteBGAudio (false);
			AudioManger.Get ().PlayAudioBG ("Empty");
		} else {
			AudioManger.Get ().MuteBGAudio (true);
		}
	}

	public void OnSoundClick(GameObject go)
	{
		LocalSettingStorage.Get ().sound = !LocalSettingStorage.Get ().sound;
		SetPage ();

		// 播放/停止音效
		if (LocalSettingStorage.Get ().sound) {
			AudioManger.Get ().MuteEffectAudio (false);
			
		} else {
			AudioManger.Get ().MuteEffectAudio (true);
		}
	}

	public void OnToggleValueChanged()
	{
		for (int i = 0; i < effectToggles.Length; ++i) {
			if (effectToggles [i].value) {
				LocalSettingStorage.Get ().effectLevel = i;
			}

			SetToggleColor (effectToggles [i].gameObject, effectToggles [i].value);
		}
	}

	public void OnFightToggleValueChanged()
	{
		for (int i = 0; i < fightOptionToggles.Length; ++i) {
			if (fightOptionToggles [i].value) {
				LocalSettingStorage.Get ().fightOption = i;
			}

			SetToggleColor (fightOptionToggles [i].gameObject, fightOptionToggles [i].value);
		}
	}

	public void OnCloseClick()
	{
		LocalStorageSystem.Get ().NeedSaveToDisk ();
        UISystem.Get().HideAllWindow();
        UISystem.Get ().ShowWindow ("StartWindow");
	}
}
