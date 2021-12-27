///////////////////////////////////////////////////////////////////////////
//  ThrowingActionSMB                                                    //
//  Kevin Iglesias - https://www.keviniglesias.com/       			     //
//  Contact Support: support@keviniglesias.com                           //
///////////////////////////////////////////////////////////////////////////

/* This Scripts is needed for other scripts (IdleThrowTrick, ChangeSpear, 
ThrowProp, ThrowMultipleProps & ThrowBigAxe) to work */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KevinIglesias {
    public class ThrowingActionSMB : StateMachineBehaviour {

        //Which action will be used
        public int action;
    
        //Point in the animation in which the prop will be thrown (0.5 means middle of the animation)
        public float timePoint;
    
        //Scripts for the different actions
        private ThrowProp tPScript;
        
        //Needed for avoiding multiple throwns
        private bool actionDone;
        
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
           
            //Get the script of the assigned action
            switch(action)
            {
                case 2:
                case 3:
                case 4:
                    if(tPScript == null)
                    {
                        tPScript = animator.GetComponent<ThrowProp>();
                    }
                    
                    actionDone = false;
                    
                    if(tPScript == null)
                    {
                        actionDone = true;
                    }
                break;
            }
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) 
        {
            //Do the action if it wasn't done yet at the assigned point
            if(!actionDone)
            {
                if(stateInfo.normalizedTime >= timePoint)
                {
                    switch(action)
                    {
                        case 2:
                            tPScript.ThrowSpear();
                        break;
                        case 3:
                            tPScript.RecoverProp();
                        break;
                    }
                    actionDone = true;
                }
            }

        }
    }
}
