using System;
using System.Collections.Generic;
using UnityEngine;





/// <summary>
/// buffer 的具体效果对象实现
/// </summary>
public class ApplyEffect
{
    public enum AttrType
    {
        hit_attr_start = 0,
        hit_Hp = 1,
        hit_attr_end,
    };

    public enum State
    {
        Ready,
        Release,
        Finished,
    }


    private BattleMember        _sender;
    private State               _AppleEffect;
    private TechniqueEntiy      _Technique;
    private CTagBufferConfig    _EffectConfig;
    private int                 _hitType;

    /// <summary>
    /// 目标列表
    /// </summary>
    private List<BattleMember>  _targets = new List<BattleMember>();


    public ApplyEffect( TechniqueEntiy technique, CTagBufferConfig config, int hitType )
    {
        _sender                 = null;
        _AppleEffect            = State.Ready;
        _EffectConfig           = config;
        _Technique              = technique;
        _hitType                = hitType;
    }


    public static ApplyEffect Create( TechniqueEntiy technique, CTagBufferConfig config, int hitType )
    {
        return new ApplyEffect(technique, config, hitType);
    }


    /// <summary>
    /// 触发buff的地方设置
    /// </summary>
    public void DoEffect( )
    {
        foreach( var target in _targets )
        {
            if (target == null) continue;
        }
    }


    public virtual void OnFinished( )
    {

    }

    public virtual State Tick( float delayTime )
    {
        return _AppleEffect;
    }


    public virtual bool OnceHit( BattleMember target, float hurtMult, double realHurt = 0 )
    {
        if (target == null) return false;
        if( IsHited( target, hurtMult ) )
        {
            //target.aiPublicy.EventGroup.fireEvent();
            return false;
        }

        int hp              = target.GetAtt(ShipAttr.Hp);

        // 伤害加成与伤害减免
        float dmgAdds       = _sender.GetAtt(ShipAttr.DamageAdds);
        float dmgReduction  = target.GetAtt(ShipAttr.DamageReduction);
        float att           = Mathf.Clamp(dmgAdds - dmgReduction, -0.8f, 4f);

        float attAttack     = _sender.GetAtt(ShipAttr.AttackPower);
        float defense       = target.GetAtt( ShipAttr.Armor);
        float hurt          = attAttack - defense;
        float hurtDefens    = _sender.GetAtt(ShipAttr.AttackPower ) * 0.1d;

        hurt                = hurt > hurtDefens ? hurt : hurtDefens;
        hurt                = hurt * (1.0f + att) * hurtMult + realHurt;


        if (hurt > hp)
            hurt = hp;

        var imd = target.GetAtt(ShipAttr.ImmuneDeadly);
        if( imd > 0 && hurt > hp )
        {
            hurt = hp - 1;
        }

        /// 减血
        target.ChangeAttr(ShipAttr.Hp, -hurt);

        // 反伤
        var cea = target.GetAtt(ShipAttr.Counterattack);
        var ceaHurt = cea / 100f * hurt;
        if( ceaHurt > target.GetAtt( ShipAttr.MaxHp ))
        {
            ceaHurt = target.GetAtt(ShipAttr.MaxHp);
        }

        // 吸血
        float s = _sender.GetAtt(ShipAttr.SuckBlood);
        _sender.ChangeAttr(ShipAttr.Hp, s - ceaHurt);

        // 被击事件
        if( hurt > 0 )
        {
            Solarmax.KBeHit hit = new Solarmax.KBeHit();
            hit.Src         = _sender;
            hit.Target      = target;
            hit.technique   = null;
            hit.hurt        = hurt;
            target.aiPublicy.EventGroup.fireEvent(hit);
        }

        if( !target.isALive )
        {

            Solarmax.KMonster ed = new Solarmax.KMonster();
            ed.killer       = _sender;
            ed.dead         = target;
            _sender.aiPublicy.EventGroup.fireEvent(ed);
        }
    }


    public bool IsHited( BattleMember target, double hurtMult )
    {
        var rate        = 1.0f - _sender.GetAtt(ShipAttr.HitRate) - target.GetAtt(ShipAttr.Dodge);
        var probability = BattleSystem.Instance.battleData.rand.Range( 1, 100);
        var dodge       = probability < rate * 100f;
        dodge           = dodge && hurtMult != 0;
        return dodge;
    }
}

