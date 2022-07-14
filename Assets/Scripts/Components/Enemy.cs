using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Enemy : Unit
{
    protected override int maxHp{
        get{
            return mxhp;
        }
    }
    int mxhp;

    public override string Name{
        get{
            return name;
        }
    }
    string name;
    
    public Intention Intention{
        get{
            return intention;
        }
    }
    protected Intention intention;
    public Task SetIntention(){
        return Task.Run(async () => {
            intention = setIntention(this);
        });
    }
    Func<Enemy, Intention> setIntention;
    
    protected override void die(){
        BattleManager.Battle.RemoveEnemy(this);
        BattleManager.Battle.CheckBattleResult();
    }
    
    public override void StatChanged(StatChange s){
        if(s.Type != StatType.MaxHealth) throw new System.Exception("Enemy cannot handle a stat change of type "+s.Type);
        float max = MaxHp;
        float proportion = (float)Hp / (max - s.Amount);
        hp = (int)(proportion * max);
    }

    public Enemy(string name, int maxHp, Func<Enemy, Intention> setIntention){
        this.name = name;
        this.mxhp = maxHp;
        this.setIntention = setIntention;
    }
}

public class Intention{
    public IntentionType[] Type{
        get{
            return type;
        }
    }
    IntentionType[] type;
    public string Description{
        get{
            return description;
        }
    }
    string description;
    public Func<Enemy, Task> Move{
        get{
            return move;
        }
    }
    Func<Enemy, Task> move;
    public Intention(IntentionType[] type, string description, Func<Enemy, Task> move){
        this.type = type;
        this.description = description;
        this.move = move;
    }
}
public enum IntentionType{
    Attack,
    Defend,
    Buff,
    Debuff,
}





//TODO: make enemy types