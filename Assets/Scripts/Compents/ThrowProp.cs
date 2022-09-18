using System.Collections;
using System.Collections.Generic;
using UnityEngine;






namespace KevinIglesias {

    public class ThrowProp : MonoBehaviour {

        //Prop to move
        public Transform        propToThrow;
        //Hand that holds the prop
        public Transform        hand;

        //Target to throw the prop
        public Vector3          targetPos;

        //Speed of the prop
        public float            speed = 10;
        
        //Maximum arc the prop will make
        public float            arcHeight = 1;
        
        //Needed for checking if prop was thrown or not
        public bool             launched = false;

        //Character root (for parenting when prop is thrown)
        private Transform       characterRoot;

        //Needed for calculate prop trajectory
        private Vector3         startPos; 
        private Vector3         zeroPosition;
        private Quaternion      zeroRotation;
        private Vector3         nextPos;
        
        void Start() 
        {
            characterRoot           = this.transform;
            zeroPosition            = propToThrow.localPosition;
            zeroRotation            = propToThrow.localRotation;
        }
        
        //This will make the prop move when launched
        void Update() 
        {
            if (targetPos == null) return;


            if(launched)
            {
                float nextX          = Mathf.MoveTowards(startPos.x, targetPos.x, speed * Time.deltaTime);
                float nextY          = Mathf.MoveTowards(startPos.y, targetPos.y, speed * Time.deltaTime);
                float nextZ          = Mathf.MoveTowards(startPos.z, targetPos.z, speed * Time.deltaTime);
               
                float x0             = startPos.x;
                float x1             = targetPos.x;
                float dist           = x1 - x0;
                float arc            = arcHeight * (nextX - x0) * (nextX - x1) / (-0.25f * dist * dist);
                Vector3 nextPos      = new Vector3(nextX, nextY, nextZ);
            
                propToThrow.rotation = LookAt2D(targetPos - propToThrow.position);
                propToThrow.position = nextPos;
     
                float currentDistance = Mathf.Abs(targetPos.x - propToThrow.position.x);
                if(currentDistance < 0.5f)
                {
                    launched = false;
                }
            }
        }
        
        static Quaternion LookAt2D(Vector3 forward) 
        {
            return Quaternion.Euler(0, 0, (Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg)-90f);
        }   
        
        //Function called by 'ThrowingActionSMB' script
        public void ThrowSpear()
        {
            startPos        = propToThrow.position;
            propToThrow.SetParent(characterRoot);
            launched        = true;
        }
        
        //Function called by 'ThrowingActionSMB' script
        public void RecoverProp()
        {
            launched        = false;
            propToThrow.SetParent(hand);
            propToThrow.localPosition = zeroPosition;
            propToThrow.localRotation = zeroRotation;
        }
    }

}