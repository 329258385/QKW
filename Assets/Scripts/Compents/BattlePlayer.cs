using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePlayer : MonoBehaviour
{
    public EntityMember entity;

    public void EnterATKAction()
    {

    }


    public void StopATKAction()
    {
        if( entity != null )
        {
            //entity.IsEndAttack = true;
        }
    }
}
