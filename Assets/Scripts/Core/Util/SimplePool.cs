using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Solarmax
{
    public class IPoolObject<T> where T : IPoolObject<T>, new ()
    {
        private SimplePool<T> mAssociatedPool;

        public IPoolObject()
        {
            mAssociatedPool = null;
        }

        public void InitPool(SimplePool<T> associatedPool)
        {
            mAssociatedPool = associatedPool;
        }

        protected void Recycle(T t)
        {
            mAssociatedPool.Recycle(t);
        }

		public virtual void OnRecycle( )
		{
			
		}
    }

    public class SimplePool<T> where T : IPoolObject<T>, new()
    {
        private object      mAsyncLocker;
		protected Queue<T>  mFreeObjects;           // modify private symbol to protected
		protected List<T>   mBusyObjects;           // modify private symbol to protected
        protected Dictionary<string, Queue<T>>      mFreeObjectsPool;

        public SimplePool()
        {
            mAsyncLocker        = new object();
            mFreeObjects        = new Queue<T>();
            mBusyObjects        = new List<T>();
            mFreeObjectsPool    = new Dictionary<string, Queue<T>>();
        }

        private T NewOne()
        {
            T t = new T();
            if (null != t)
            {
                t.InitPool(this);
            }

            return t;
        }

		private DerivedT NewOne<DerivedT>() where DerivedT : T, new ()
		{
			DerivedT t = new DerivedT();
			if (null != t)
			{
				t.InitPool(this);
			}

			return t;
		}

        public virtual T Alloc( )
        {
            T ret       = default(T);
            if (mFreeObjects.Count > 0)
            {
                ret     = mFreeObjects.Dequeue();
            }
            else
            {
                ret     = NewOne();
            }
            mBusyObjects.Add(ret);
            return ret;
        }


        public virtual DerivedT Alloc<DerivedT>( string nameKey ) where DerivedT : T, new()
        {
            lock (mAsyncLocker)
            {
                DerivedT ret    = default(DerivedT);
                mFreeObjects    = null;
                if (mFreeObjectsPool.TryGetValue(nameKey, out mFreeObjects))
                {
                    if (mFreeObjects.Count > 0)
                    {
                        ret     = mFreeObjects.Dequeue() as DerivedT;
                    }
                    else
                    {
                        ret     = NewOne<DerivedT>();
                    }
                }
                else
                {
                    ret         = NewOne<DerivedT>();
                }

                mBusyObjects.Add(ret);
                return ret;
            }
        }

        public virtual void Recycle(T t)
        {
			lock(mAsyncLocker)
            {
                EffectNode node     = t as EffectNode;
                if (null != t && node != null )
				{
					t.OnRecycle ();
					mBusyObjects.Remove(t);

                    mFreeObjects = null;
                    if( mFreeObjectsPool.TryGetValue( node.nameKey, out mFreeObjects ) )
                    {
                        mFreeObjects.Enqueue(t);
                    }
                    else
                    {
                        mFreeObjects = new Queue<T>();
                        mFreeObjects.Enqueue(t);
                        mFreeObjectsPool.Add(node.nameKey, mFreeObjects);
                    }
                }
            }
        }

        public int GetFreeCount()
        {
            return mFreeObjects.Count;
        }

		public List<T> GetAllObjects()
		{
			List<T> ret = mFreeObjects.ToList ();
			ret.AddRange (mBusyObjects);
			return ret;
		}

		public void Clear()
		{
			mFreeObjects.Clear ();
			mBusyObjects.Clear ();
		}
    }
}
