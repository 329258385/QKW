using System;
using Solarmax;

public class EffectPool : SimplePool<EffectNode>, Lifecycle2
{
	public EffectPool ()
	{
	
	}
	
	public virtual bool Init ()
	{

		return true;
	}

	public virtual void Tick (int frame, float interval)
	{
		for (int i = mBusyObjects.Count - 1; i >= 0; --i) {
			EffectNode effect = mBusyObjects [i];
			effect.Tick (frame, interval);
		}
	}

	public virtual void Destroy ()
	{
		for (int i = mBusyObjects.Count - 1; i >= 0; --i) {
			EffectNode effect = mBusyObjects [i];
			Recycle (effect);
		}

		EffectNode[] array = mFreeObjects.ToArray ();
		for (int i = 0, max = array.Length; i < max; ++i) {
			EffectNode effect = array [i];
			effect.Destroy ();
		}

		// 清空池
		base.Clear ();
	}
}

