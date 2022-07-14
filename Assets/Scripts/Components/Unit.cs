using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public abstract class Unit
{
    public abstract string Name{get;}
    public int MaxHp{
        get{
            return maxHp + StatChangeTotalAmount(StatType.MaxHealth);
        }
    }
    protected abstract int maxHp{get;}
    public int Hp{
        get{
            return hp;
        }
    }
    protected Task setHp(int value){
        return Task.Run(async () => {
            int temp = hp;
            hp = value;
            if(hp >= MaxHp) hp = MaxHp;
            else BattleManager.Battle.Flawless = false;
            await ConsoleManager.Main.AddText(Name + " " + (hp - temp > 0 ? "gained" : "lost") + " " + Mathf.Abs(hp - temp) + " hp ("+(hp)+"/"+(MaxHp)+")");
            if(hp <= 0 && !dead) {
                dead = true;
                die();
            }
        });
    }
    protected int hp;

    public bool Dead{
        get{
            return dead;
        }
    }
    protected bool dead = false;
    protected abstract void die();



    #region buffs
    List<Buff> buffs = new List<Buff>();
    public void AddBuff(Buff b){
        if(this == GameplayManager.Self && new List<Item>(GameplayManager.Self.Items).Find(a => a.Name == "Treaded Boots") != null && b is Condition && (b as Condition).Type == ConditionType.Block)
            b.IncreaseDuration();

        buffs.Add(b);
        b.SetSelf(this);
        if(b is StatChange)
            StatChanged((b as StatChange));
    }
    public void RemoveBuff(Buff b){
        buffs.Remove(b);
    }
    public abstract void StatChanged(StatChange s);
    public void ReduceBuffsDuration(){
        for(int i = buffs.Count-1; i >= 0; i--){
            buffs[i].ReduceDuration();
        }
    }
    public Task<object> CallEffects(EffectActivation e, params object[] input){
        return Task.Run<object>( async () => {
            Buff[] temp = buffs.ToArray();
            foreach(Buff b in temp){
                if(b is Effect){
                    if(input.Length > 0) input[0] = await (b as Effect).CallEffect(e, input);
                    else await (b as Effect).CallEffect(e, input);
                }
            }
            if(input.Length > 0) return input[0];
            else return null;
        });
    }

    public int ConditionTotalAmount(ConditionType t){
        int amount = 0;
        foreach(Buff b in buffs)
            if(b is Condition && (b as Condition).Type == t)
                amount += (b as Condition).Amount;
        return amount;
    }

    public int StatChangeTotalAmount(StatType t){
        int amount = 0;
        foreach(Buff b in buffs)
            if(b is StatChange && (b as StatChange).Type == t)
                amount += (b as StatChange).Amount;
        return amount;
    }

    public void ReduceConditionAmount(ConditionType t, int amount){
        List<Buff> toRemove = new List<Buff>();
        foreach(Buff b in buffs){
            if(b is Condition && (b as Condition).Type == t){
                Condition c = (b as Condition);
                if(c.Amount >= amount) {
                    c.Amount -= amount;
                    break;
                }
                else{
                    amount -= c.Amount;
                    toRemove.Add(b);
                }
            }
        }

        foreach(Buff b in toRemove) RemoveBuff(b);
    }

    public void ResetUnit(){
        for(int i = buffs.Count-1; i >= 0; i--)
            if(buffs[i].Duration >= -1)
                RemoveBuff(buffs[i]);
        hp = MaxHp;
        dead = false;
    }
    #endregion



    public Task<bool> StartTurn(){
        return Task.Run<bool>( async () => {
            await CallEffects(EffectActivation.OnTurn);
            if(ConditionTotalAmount(ConditionType.Stun) != 0) return false;
            else return true;
        });
    }
    public Task EndTurn(){
        return Task.Run( async () => {
            await CallEffects(EffectActivation.AfterTurn);
            await TakeDamage(ConditionTotalAmount(ConditionType.Bleed));
            ReduceBuffsDuration();
        });
    }
    public Task<int?> Attack(int baseDamage, Unit target){
        return Task.Run<int?> ( async () => {
            if(dead) return null;

            bool hit = true;
            hit = (bool)await CallEffects(EffectActivation.OnTryAttack, hit, target);
            hit = (bool)await target.CallEffects(EffectActivation.OnTryDefend, hit, this);
            if(hit && target.ConditionTotalAmount(ConditionType.Dodge) > 0){
                target.ReduceConditionAmount(ConditionType.Dodge, 1);
                hit = false;
            }

            int? damage = null;
            if(hit){
                await ConsoleManager.Main.AddText(Name+" hit "+target.Name);
                damage = baseDamage;
                damage = (int)await CallEffects(EffectActivation.OnAttack, damage, target);
                damage = (int)await target.CallEffects(EffectActivation.OnDefend, damage, this);
                if(damage < 0) damage = 0;

                int totalBlock = ConditionTotalAmount(ConditionType.Block);
                int usedBlock = damage > totalBlock ? totalBlock : (int)damage;
                damage -= usedBlock;
                ReduceConditionAmount(ConditionType.Block, usedBlock);

                if(damage > 0) damage = await target.TakeDamage((int)damage);

                await target.CallEffects(EffectActivation.AfterDefend, damage, this);
                await CallEffects(EffectActivation.AfterAttack, damage, target);
            }
            else {
                await target.CallEffects(EffectActivation.AfterTryDefend, damage, this);
                await CallEffects(EffectActivation.AfterTryAttack, damage, target);
            }

            return damage;
        });
    }

    public Task<int> TakeDamage(int damage){
        return Task.Run<int> ( async () => {
            damage = (int)await CallEffects(EffectActivation.OnTakeDamage, damage);
            await setHp(Hp - damage);
            await ConsoleManager.Main.AddText(Name+" took "+damage+" damage");
            await CallEffects(EffectActivation.AfterTakeDamage, damage);
            return damage;
        });
    }

    public Task<int> Heal(int health){
        return Task.Run<int>(async ()=>{
            //possibly add buff callbacks here
            int prevHp = Hp;
            await setHp(Hp + health);
            return Hp - prevHp; 
        });
    }
}

public abstract class Buff{
    public int Duration{
        get{
            return duration;
        }
    }
    bool firstTurnCooldown = true;
    protected int duration; //-1 means infinite duration
    public int CurrentDuration{
        get{
            return currentDuration;
        }
    }
    int currentDuration;
    public Unit Self{
        get{
            return self;
        }
    }
    Unit self;
    public void SetSelf(Unit self){
        if(this.self == null) this.self = self;
    }

    public void ReduceDuration(){
        if(firstTurnCooldown) firstTurnCooldown = false;
        else if(duration > 0){
            currentDuration--;
            if(currentDuration == 0) self.RemoveBuff(this);
        }
    }

    public void IncreaseDuration(){
        currentDuration++;
    }

    public Buff(int duration){
        this.duration = duration;
        currentDuration = duration;
    }
}

public class StatChange : Buff{
    public int Amount{
        get{
            return amount;
        }
    }
    int amount;
    public StatType Type{
        get{
            return type;
        }
    }
    StatType type;
    
    public StatChange(int amount, StatType type, int duration = -1) : base(duration){
        this.amount = amount;
        this.type = type;
    }
}
public enum StatType{
    MaxHealth,
    MaxSanity,
    MaxItems,

}

public class Condition : Buff{
    public int Amount{
        get{
            return amount;
        }
        set{
            Amount = value;
        }
    }
    int amount; //-1 for conditions that don't contain an amount, like stun
    public ConditionType Type{
        get{
            return type;
        }
    }
    ConditionType type;
    
    Condition(int duration, ConditionType type, int amount) : base(duration){
        this.type = type;
        this.amount = amount;
    }

    public static Condition Block(int block) => new Condition(2, ConditionType.Block, block);
    public static Condition Dodge(int dodge) => new Condition(1, ConditionType.Dodge, dodge);
    public static Condition Stun(int duration) => new Condition(duration, ConditionType.Stun, -1);
    public static Condition Bleed(int bleed) => new Condition(-1, ConditionType.Bleed, bleed);
}

public class Effect : Buff{
    Dictionary<EffectActivation, Func<Buff, object[], Task<object>>> effects = new Dictionary<EffectActivation, Func<Buff, object[], Task<object>>>();

    public bool AddEffect(EffectActivation a, Func<Buff, object[], Task<object>> f){
        if(effects.ContainsKey(a)) return false;
        else effects.Add(a, f);
        return true;
    }

    public Task<object> CallEffect(EffectActivation a, object[] i){
        return Task.Run<object>( async () => {
            if(!effects.ContainsKey(a)) return i;
            else return await effects[a](this, i);
        });
    }

    public bool HasEffect(EffectActivation a){
        return effects.ContainsKey(a);
    }

    public Effect(int duration, params (EffectActivation, Func<Buff, object[], Task<object>>)[] effects) : base(duration){
        foreach((EffectActivation, Func<Buff, object[], Task<object>>) p in effects)
            AddEffect(p.Item1, p.Item2);
    }
}

public enum EffectActivation{
    //combat
    OnTurn,
    OnTryAttack,
    OnTryDefend,
    OnAttack,
    OnDefend,
    OnTakeDamage,
    AfterTakeDamage,
    AfterAttack,
    AfterDefend,
    AfterTryAttack,
    AfterTryDefend,
    AfterTurn,

    //meta
    AfterSleep,
}

public enum ConditionType{
    Block,
    Dodge,
    Stun,
    Bleed
}
