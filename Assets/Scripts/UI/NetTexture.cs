using UnityEngine;
using System.Collections;

[RequireComponent(typeof(UITexture))]
public class NetTexture : MonoBehaviour
{
	public bool pixelPerfect = false;

	private string url = string.Empty;
	private Color color = Color.white;
	private UITexture uiTexture;

	public string picUrl
	{
		get{ return url;}
		set
		{
			url = value;
			Coroutine.Start (SetUrl ());
		}
	}

	public Color picColor
	{
		get{ return color;}
		set
		{
			color = value;
			if (uiTexture != null) {
				uiTexture.color = color;
			}
		}
	}

	void Awake()
	{
		uiTexture = GetComponent<UITexture> ();
	}

	void Start ()
	{
		Coroutine.Start (SetUrl ());
	}

	void OnDestroy ()
	{
		//if (tex != null) Destroy(tex);
	}

	private IEnumerator SetUrl()
	{
		if (!string.IsNullOrEmpty (url))
		{
			if (url.StartsWith ("http")) {
				WWW www = new WWW (url);
				yield return www;
				Texture2D tex = www.texture;

				if (tex != null) {
					uiTexture.mainTexture = tex;
					if (pixelPerfect)
						uiTexture.MakePixelPerfect ();
				}
				www.Dispose ();
			} else {
				ResourceRequest rr = Resources.LoadAsync (url);

				while (!rr.isDone)
					yield return 1;

				Texture2D tex = rr.asset as Texture2D;
				if (tex != null) {
					uiTexture.mainTexture = tex;
					if (pixelPerfect)
						uiTexture.MakePixelPerfect ();
				}
			}
		}
	}
}
