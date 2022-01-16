using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KevinIglesias;


public class ThrowingActionSMB : StateMachineBehaviour 
{
    public  int                     action;
    public  float                   timePoint;
    private ThrowProp               tPScript;
    private bool                    actionDone;
    private BattlePlayer            battlePlayer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        switch( action )
        {
            case 2:
            case 3:
            case 4:
                if( tPScript == null )
                {
                    tPScript = animator.GetComponent<ThrowProp>();
                }

                actionDone = false;
                if( tPScript == null )
                {
                    actionDone = true;
                }
                break;
        }
        if( battlePlayer == null )
        {
            battlePlayer = animator.GetComponent<BattlePlayer>();
        }

        if( battlePlayer != null )
        {
            battlePlayer.EnterATKAction();
        }
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if( !actionDone )
        {
            if( stateInfo.normalizedTime >= timePoint )
            {
                switch( action )
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

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if( battlePlayer != null )
        {
            battlePlayer.StopATKAction();
        }
    }
}
