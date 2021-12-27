using UnityEngine;
using System.Collections;
using System.Collections.Generic;






public class Coroutine : MonoBehaviour 
{

	public int                  maxRunningCoroutine = 5;
	private static Coroutine    coroutine;
	private int                 nowRunningCount = 0;
	private Queue<IEnumerator>  enumeratorQueue;

	void Awake()
	{
		coroutine       = this;
		enumeratorQueue = new Queue<IEnumerator> ();
	}

	void Start()
	{

	}

	public static void Start(IEnumerator ie)
	{
		coroutine.enumeratorQueue.Enqueue (ie);

		// 检测
		coroutine.Check ();
	}

	private IEnumerator Runtor(IEnumerator ie)
	{
		yield return ie;

		// 运行完成
		--nowRunningCount;

		// 检测
		Check();
	}

	private void Check()
	{
		for (int i = nowRunningCount; i < maxRunningCoroutine; ++i) 
        {

			if (enumeratorQueue.Count > 0) {
				StartCoroutine (Runtor (enumeratorQueue.Dequeue ()));
				++ nowRunningCount;
			} 
            else 
            {
				break;
			}
		}
	}
}
