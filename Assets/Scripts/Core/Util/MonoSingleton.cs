using UnityEngine;





public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	/// <summary>
	/// 
	/// </summary>
	private static T s_Instance;

	private static bool s_IsDestroyed;

	/// <summary>
	/// singleton property
	/// </summary>
	public static T Instance
	{
		get
		{
		    if (s_IsDestroyed)
		        return null;

		    if (s_Instance == null)
		    {
		        s_Instance = GameObject.FindObjectOfType(typeof(T)) as T;

		        if (s_Instance == null)
		        {
		            GameObject gameObject = new GameObject(typeof(T).Name);
		            GameObject.DontDestroyOnLoad(gameObject);

		            s_Instance = gameObject.AddComponent(typeof(T)) as T;
		        }
		    }

		    return s_Instance;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	protected virtual void OnDestroy()
	{
		if (s_Instance)
		    Destroy(s_Instance);

		s_Instance = null;
		s_IsDestroyed = true;
	}
}