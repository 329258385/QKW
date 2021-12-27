using System;


public class EventArgs_SinVal<T> : EventArgs
{
	public EventArgs_SinVal(T val)
	{
		Val = val;
	}
	public T Val { get; internal set; }
}

public class EventArgs_DouVal<K, V> : EventArgs
{
	public EventArgs_DouVal(K k, V v)
	{
		KVal = k;
		VVal = v;
	}
	public K KVal { get; internal set; }
	public V VVal { get; internal set; }
}

public class EventArgs_ThreeVal<K, V, P> : EventArgs
{
	public EventArgs_ThreeVal(K k, V v, P p)
	{
		KVal = k;
		VVal = v;
		PVal = p;
	}
	public K KVal { get; internal set; }
	public V VVal { get; internal set; }
	public P PVal { get; internal set; }
}

public class EventArgs_FourVal<K, V, P, R> : EventArgs
{
	public EventArgs_FourVal(K k, V v, P p, R r)
	{
		KVal = k;
		VVal = v;
		PVal = p;
		RVal = r;
	}
	public K KVal { get; internal set; }
	public V VVal { get; internal set; }
	public P PVal { get; internal set; }
	public R RVal { get; internal set; }
}

public class EventArgs_SixVal<K, V, P, R, S, T> : EventArgs
{
	public EventArgs_SixVal(K k, V v, P p, R r, S s, T t)
	{
		KVal = k;
		VVal = v;
		PVal = p;

		RVal = r;
		SVal = s;
		TVal = t;

	}
	public K KVal { get; internal set; }
	public V VVal { get; internal set; }
	public P PVal { get; internal set; }
	public R RVal { get; internal set; }
	public S SVal { get; internal set; }
	public T TVal { get; internal set; }

}