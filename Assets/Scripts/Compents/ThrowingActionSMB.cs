using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ThrowingActionSMB : StateMachineBehaviour 
{
    private BattlePlayer            battlePlayer;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if( battlePlayer == null )
        {
            battlePlayer = animator.GetComponent<BattlePlayer>();
        }

        if( battlePlayer != null )
        {
            battlePlayer.EnterATKAction();
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
