using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Solarmax
{
    public interface Lifecycle
    {
        bool Init ();

		void Tick (float interval);

        void Destroy ();
    }

	public interface Lifecycle2
	{
		bool Init ();

		void Tick (int frame, float interval);

		void Destroy ();
	}

	public interface Lifecycle3
	{
		bool Init( GameObject go );

		void Tick(int frame, float interval);

		void Destroy();
	}
}
