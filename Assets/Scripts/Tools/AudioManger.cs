using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 音频管理
/// </summary>
public class AudioManger : Singleton<AudioManger>
{
	/// <summary>
	/// 背景音乐 
	/// </summary>
	/// <value>The background audio.</value>
	AudioSource bgAudio { get; set; }

	/// <summary>
	/// 特效
	/// </summary>
	/// <value>The effect audio.</value>
	AudioSource effectAudio { get; set; }
	private GameObject effectAudioParent;

	/// <summary>
	/// 特效音
	/// </summary>
	List<AudioSource> effectAudioList = new List<AudioSource>();
	int effectAudioIndex = 0;
	const int DefaultEffectNum = 2;
	const int DefaultEffectMax = 6;

	/// <summary>
	/// 当前播放的
	/// </summary>
	private string nowPlayAudioBG = string.Empty;

	/// <summary>
	/// 初始化
	/// </summary>
	public void Init()
	{
		//获取背景音乐播放器
		bgAudio = GameObject.Find("bg").GetComponent<AudioSource>();

		if (bgAudio == null) {
			Debug.LogError ("Find bg audio source is error!");
			return;
		}

		//获取特效播放器
		effectAudio = GameObject.Find("effect").GetComponent<AudioSource>();

		if (bgAudio == null) {
			Debug.LogError ("Find bg audio source is error!");
			return;
		} else {
			effectAudioParent = effectAudio.transform.parent.gameObject;

			for (int i = 0; i < DefaultEffectNum; ++i) {
				CreateNewEffectAudio ();
			}
		}
	}

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="audio">Audio.</param>
	/// <param name="loop">If set to <c>true</c> loop.</param>
	/// <param name="volume">Volume.</param>
	/// <param name="pan">Pan.</param>
	void PlayAudioEffect(string audioName, float volume = 1.0f, float pan = 0.0f)
	{
		if (!LocalSettingStorage.Get ().sound)
			return;

		if (BattleSystem.Instance.battleData.silent)
			return;

		AudioSource audio = GetFreeEffectAudioSource ();

		if (audio == null)
			return;

		AudioClip clip = AssetManager.Get ().GetResources (audioName) as AudioClip;

		if (clip == null)
			return;
		
		//audio.Stop ();
		
		audio.volume = volume;
		audio.panStereo = pan;
		audio.loop = false;
		audio.PlayOneShot (clip);
	}

	/// <summary>
	/// 播放音效
	/// </summary>
	/// <param name="clip">Clip.</param>
	/// <param name="volume">Volume.</param>
	/// <param name="pitch">Pitch.</param>
	private void PlayAudioEffect (AudioClip clip, float volume)
	{
		if (!LocalSettingStorage.Get ().sound)
			return;

		if (clip == null)
			return;

		AudioSource audio = GetFreeEffectAudioSource ();

		if (audio == null)
			return;

		//Debug.Log ("Audio Play:" + clip.name);

		//audio.Stop ();

		audio.volume = volume;
		audio.panStereo = 0;
		audio.loop = false;
		audio.PlayOneShot (clip);
	}

	/// <summary>
	/// 获得一个空闲的音源
	/// </summary>
	private AudioSource GetFreeEffectAudioSource()
	{
		//return effectAudio;
		AudioSource ret = null;
		for (int i = 0, max = effectAudioList.Count; i < max; ++i) {
			if (!effectAudioList [i].isPlaying) {
				ret = effectAudioList [i];
				break;
			}
		}

		if (ret == null) {
			if (effectAudioList.Count < DefaultEffectMax) {
				ret = CreateNewEffectAudio ();
			} else {
				if (effectAudioIndex >= DefaultEffectMax) {
					effectAudioIndex = 0;
				}
				ret = effectAudioList [effectAudioIndex++];
			}
		}
		//Debug.Log ("Free is " + ret.name);

		return ret;
	}

	private AudioSource CreateNewEffectAudio()
	{
		GameObject go = NGUITools.AddChild (effectAudioParent, effectAudio.gameObject);
		go.name = "effect-" + effectAudioList.Count;
		AudioSource audio = go.GetComponent<AudioSource> ();
		effectAudioList.Add (audio);
		return audio;
	}

	/// <summary>
	/// 静音－特效
	/// </summary>
	/// <param name="mute">If set to <c>true</c> mute.</param>
	public void MuteEffectAudio(bool mute)
	{
		if (effectAudio == null)
			return;
		effectAudio.mute = mute;
	}
		
	/// <summary>
	/// 播放背景音乐
	/// </summary>
	/// <param name="audio">Audio.</param>
	public void PlayAudioBG(string audio, float volume=0.5f)
	{
		if (!LocalSettingStorage.Get ().music)
			return;

		if (bgAudio.isPlaying && !bgAudio.mute) {
			if (nowPlayAudioBG.Equals (audio)) {
				bgAudio.volume = volume;
				return;
			}
		}

		nowPlayAudioBG = audio;
		Coroutine.Start (PlayAudioBGA(audio, volume));

	}

	/// <summary>
	/// 异步加载背景音乐
	/// </summary>
	/// <returns>The audio background.</returns>
	/// <param name="audio">Audio.</param>
	/// <param name="volume">Volume.</param>
	IEnumerator PlayAudioBGA(string audio, float volume)
	{
		ResourceRequest request = Resources.LoadAsync (string.Format("sounds/{0}", audio));

		while (!request.isDone)
			yield return 1;

		AudioClip clip = bgAudio.clip;

		bgAudio.clip = request.asset as AudioClip;

		Resources.UnloadAsset (clip);

		if (bgAudio.clip != null) {
			bgAudio.loop = true;
			bgAudio.volume = volume;
			bgAudio.Play ();
		}
	}

	/// <summary>
	/// 静音－背景音乐
	/// </summary>
	public void MuteBGAudio(bool mute)
	{
		if (bgAudio == null)
			return;
		bgAudio.Stop ();
		bgAudio.mute = mute;
	}
		
	////////////////////////////////////////////////////////////////////////////////////////////////////////
	/// 声音播放

	float GetAudioPan(Vector3 pos)
	{
		if (UICamera.currentCamera != null)
			pos = UICamera.currentCamera.WorldToScreenPoint (pos);
		return (pos.x / Screen.width - 0.5f) * 2;
	}

	public void PlayJumpCharge(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_JumpCharge))
			return;

		float pan = GetAudioPan (pos);
		PlayAudioEffect ("jumpCharge", 0.6f, pan);
	}

	public void PlayJumpStart(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_JumpStart))
			return;
		
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("jumpStart", 0.6f, pan);
	}

	public void PlayJumpEnd(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_JumpEnd))
			return;
		
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("jumpEnd", 0.6f, pan);
	}

	public void PlayCapture(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_Capture))
			return;
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("capture", 1.0f, pan);
	}

	public void PlayLaser(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_Laser))
			return;
		
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("laser", 1.0f, pan);
	}

	public void PlayExlposion(Vector3 pos)
	{
		float pan = GetAudioPan (pos);
		int num = UnityEngine.Random.Range (1, 8);
		PlayAudioEffect (string.Format("explosion{0:D2}", num), 0.7f, pan);
	}

	public void PlayWarpCharge(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_WarpCharge))
			return;
		
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("warp_charge", 0.7f, pan);
	}

	public void PlayWarp(Vector3 pos)
	{
		if (!GameTimeManager.Get ().CheckTimer (TimerType.T_Warp))
			return;
		
		float pan = GetAudioPan (pos);
		PlayAudioEffect ("warp", 0.7f, pan);
	}

	public void PlayEffect (string name)
	{
		PlayAudioEffect (name);
	}

	public void PlayEffect (AudioClip clip, float volumn)
	{
		PlayAudioEffect (clip, volumn);
	}

}
