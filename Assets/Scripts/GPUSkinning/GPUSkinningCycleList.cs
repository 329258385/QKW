/*
-----------------------------------------------------------------------------------------------------
    骨骼动画---工具
    log:  add by ljp 2021--3-29
-----------------------------------------------------------------------------------------------------
*/
using System;
using UnityEngine;



class GPUSkinningCycleList<T>
{
    private GPUSkinningBetterList<T> list = null;

    private int pointer = 0;

    public GPUSkinningCycleList(int bufferIncrement)
    {
        list = new GPUSkinningBetterList<T>(bufferIncrement);
    }

    public void Set(T[] data)
    {
        list.Clear();
        list.AddRange(data);
        pointer = 0;
    }

    public void Next()
    {
        ++pointer;
        if (pointer >= list.size)
        {
            pointer = 0;
        }
    }

    public T Peek()
    {
        if (pointer >= list.size)
        {
            return default(T);
        }
        return list[pointer];
    }
}

