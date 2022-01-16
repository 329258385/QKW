///////////////////////////////////////////////////////////////////////////
//  Archer Animations - BowLoadScript                                    //
//  Kevin Iglesias - https://www.keviniglesias.com/     			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

// This script makes the bow animate when pulling arrow, also it makes thes
// arrow look ready to shot when drawing it from the quivers.

// To do the pulling animation, the bow mesh needs a blenshape named 'Load' 
// and the character needs an empty Gameobject in your Unity scene named 
// 'ArrowLoad' as a child, see character dummies hierarchy from the demo 
// scene as example. More information at Documentation PDF file.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias {
	public class BowLoadScript : MonoBehaviour
	{
		public Transform		arrowLoad;
		
		//Arrow draw & rotation
		public bool				arrowOnHand;
		private bool			bThrow = false;

		public Vector3			startPos = Vector3.zero;
		public Vector3			targetPos = Vector3.zero;
		public float			speed = 10;
		public float			arcHeight = 1;
		public Transform		arrowToDraw;
		public Transform		arrowToShoot; 
		
	   
		void Awake()
		{
			if(arrowToDraw != null)
			{
				arrowToDraw.gameObject.SetActive(false);
			}
			if(arrowToShoot != null)
			{
				arrowToShoot.gameObject.SetActive(false);
			}
		}

		void Update()
		{
			//Draw arrow from quiver and rotate it
			if(arrowToDraw != null && arrowToShoot != null && arrowLoad != null)
			{
				if(arrowLoad.localPosition.y >= 0.4f && !arrowOnHand )
				{
					if(arrowToDraw != null)
					{
						arrowOnHand = true;
						bThrow		= false;
						arrowToDraw.gameObject.SetActive(true);
					}
				}
					
				if(arrowLoad.localPosition.y > 0.5f && arrowOnHand )
				{
					if(arrowToDraw != null && arrowToShoot != null)
					{
						bThrow		= true;
						arrowToDraw.gameObject.SetActive(false);
						arrowToShoot.gameObject.SetActive(true);
						startPos	= arrowToShoot.position;
					}
				}

                if (arrowLoad.localScale.z < 1f)
                {
                    if (arrowToShoot != null)
                    {
                        arrowToShoot.gameObject.SetActive(false);
                    }
                }

                if ( bThrow && arrowToShoot.gameObject.activeSelf )
                {
					Vector3 fireDirection = targetPos - startPos;
					EffectManager.Get().AddLaserLine(startPos, Quaternion.LookRotation(fireDirection.normalized));
					//float x0 = arrowToShoot.position.x;
					//float x1 = targetPos.x;
					//float dist = x1 - x0;
					//float nextX = Mathf.MoveTowards(startPos.x, targetPos.x, speed * Time.deltaTime);
					//float nextY = Mathf.MoveTowards(startPos.y, targetPos.y, speed * Time.deltaTime);
					//float nextZ = Mathf.MoveTowards(startPos.z, targetPos.z, speed * Time.deltaTime);

					//float arc = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
					//Vector3 nextPos		= new Vector3(nextX, nextY, nextZ);
					//arrowToShoot.rotation = LookAt2D(targetPos - arrowToShoot.position);
					//arrowToShoot.position = nextPos;

					//float currentDistance = Mathf.Abs(targetPos.x - arrowToShoot.position.x);
					//if (currentDistance < 0.5f)
					//{
					//    bThrow			  = false;
					//	arrowOnHand		  = false;
					//}
				}
			}
		}

		static Quaternion LookAt2D(Vector3 forward)
		{
			return Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg) - 90f);
		}
	}
}
