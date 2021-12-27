using UnityEngine;
using System;
using System.Collections;

public class Singleton<T> : Listenner
{
	class SingletonCreator
	{
		internal static readonly T instance = Activator.CreateInstance<T> ();
	}
	
	public static T Get()
	{
		return SingletonCreator.instance;
	}
}
