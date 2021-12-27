using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Solarmax;

public class ScanWindow : BaseWindow
{
	public UITexture bgTexture;
	public UISprite scanTexture;

	private WebCamTexture webCamTexture;
	
	private float scalex;
	private float scaley;

	private int camWidth;
	private int camHeight;
	private int scanWidth;
	private int scanHeight;
	private Color32[] camTextureData;
	private Color32[] scanTextureData;
	private int cutFromI;
	private int cutToI;
	private int cutFromJ;
	private int cutToJ;
	private int cutPosI;
	private int cutPosJ;

	private Queue<Color32[]> texturesQueue;
	private Queue<string> stringQueue;
	private Thread recognizeThread;
	private bool recognizeThreadRun;

	IEnumerator Start()
	{

		int width = bgTexture.width;
		int height = bgTexture.height;

        #if UNITY_EDITOR
        bgTexture.width = 1080;
        bgTexture.height = 1920;
        bgTexture.transform.rotation = Quaternion.Euler (0, 0, 90);
        #else
		bgTexture.width = 1920;
		bgTexture.height = 1080;
		bgTexture.transform.rotation = Quaternion.Euler (0, 0, -90);
		width = Screen.height;
		height = Screen.width;
		#endif

		// get the back camera
		for (int i = 0; i < WebCamTexture.devices.Length; ++ i) {
			if (!WebCamTexture.devices [i].isFrontFacing) {
				webCamTexture = new WebCamTexture (WebCamTexture.devices [i].name, width, height, 12);
				break;
			}
		}

		if (webCamTexture == null) {
			// get front camera
			for (int i = 0; i < WebCamTexture.devices.Length; ++i) {
				if (WebCamTexture.devices [i].isFrontFacing) {
					webCamTexture = new WebCamTexture (WebCamTexture.devices [i].name, width, height,12);
					break;
				}
			}
		}

        if (webCamTexture != null)
        {
            webCamTexture.Play();
            bgTexture.mainTexture = webCamTexture;
            bgTexture.transform.rotation = Quaternion.Euler(0, 0, -webCamTexture.videoRotationAngle);
        }
        else
        {
            UISystem.Get().HideWindow("ScanWindow");
        }
		yield return new WaitForSeconds(1);

		////////////////////
        if (webCamTexture != null)
        {
            camWidth = webCamTexture.width;
            camHeight = webCamTexture.height;

            scalex = bgTexture.width * 1.0f / camWidth;
            scaley = bgTexture.height * 1.0f / camHeight;

            scanWidth = (int)(scanTexture.width / scalex);
            scanHeight = (int)(scanTexture.height / scaley);

            camTextureData = new Color32[camWidth * camHeight];
            scanTextureData = new Color32[scanWidth * scanHeight];
            cutFromI = (int)((camWidth - scanWidth) / 2);
            cutToI = (int)((camWidth + scanWidth) / 2);
            cutFromJ = (int)((camHeight - scanHeight) / 2);
            cutToJ = (int)((camHeight + scanHeight) / 2);

            InvokeRepeating("DecodeInvoke", 0.5f, 0.2f);

            texturesQueue = new Queue<Color32[]>();
            stringQueue = new Queue<string>();
            recognizeThreadRun = true;
            recognizeThread = new Thread(RecognizeThreadFunction);
            recognizeThread.Start();
        }
	}

	public override void OnShow()
	{
		
	}

	public override void OnHide()
	{
		if (webCamTexture != null) {
			webCamTexture.Stop ();
		}
		recognizeThreadRun = false;
	}

	public override void OnUIEventHandler (EventId eventId, params object[] args)
	{
		
	}

	private void DecodeInvoke()
	{
		Scan ();
	}

	private void Scan()
	{
		GetScanAreaPixels ();

		texturesQueue.Enqueue (scanTextureData.Clone () as Color32[]);
	}


	private void GetScanAreaPixels()
	{
//		scanTextureData = webCamTexture.GetPixels (cutFromI, cutFromJ, scanWidth, scanHeight);
		webCamTexture.GetPixels32 (camTextureData);
		for (int j = cutFromJ; j < cutToJ; ++j) {
			cutPosJ = j - cutFromJ;
			for (int i = cutFromI; i < cutToI; ++i) {
				cutPosI = i - cutFromI;
				scanTextureData [scanWidth * cutPosJ + cutPosI] = camTextureData [camWidth * j + i];
			}
		}
	}

	private void RecognizeThreadFunction()
	{
		
	}

	public void OnCloseClick()
	{
		UISystem.Get ().HideWindow ("ScanWindow");
        UISystem.Get().ShowWindow("FriendWindow");
	}
}

