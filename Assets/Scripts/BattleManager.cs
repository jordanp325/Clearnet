using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class BattleManager
{
    public static BattleManager Battle;

    public static BattleManager GetBattle(string key){
        switch(key){
            case "Wasteland 1":
            Func<Enemy, Intention> banditMoveset = s => {
                switch(GameplayManager.randomInt(3)){ //TODO: this
                    case 0: return new Intention(new IntentionType[]{IntentionType.Attack}, "intends to jab at you with his knife, dealing 10 damage", e => {
                        return Task.Run( async () => {
                            await s.Attack(10, GameplayManager.Self);
                        });
                    });

                    case 1: return new Intention(new IntentionType[]{IntentionType.Attack, IntentionType.Defend}, "intends to slice at you, guarding against counterattacks, dealing 5 damage and gaining 5 block", e => {
                        return Task.Run( async () => {
                            await s.Attack(5, GameplayManager.Self);
                            s.AddBuff(Condition.Block(5));
                        });
                    });

                    case 2: return new Intention(new IntentionType[]{}, "fumbles his knife, frantically trying to find it in the dirt", e => {
                        return Task.Run( async () => {
                            //nothing
                        });
                    });

                    default: return null;
                }
            };
            return new BattleManager(
                new Enemy("Disfigured bandit", 10, banditMoveset),
                new Enemy("Toothless bandit", 10, banditMoveset)
            );
        }
        return null;
    }

    public Enemy[] Enemies{
        get{
            return enemies.ToArray();
        }
    }
    List<Enemy> enemies;
    public int Round{
        get{
            return round;
        }
    }
    int round = 1;
    
    public void RemoveEnemy(Enemy e){
        enemies.Remove(e);
    }
    public bool Flawless = true;

    public Task<BattleResult> Run(){
        return Task.Run<BattleResult>( async () => {
            Battle = this;
            GameplayManager.Self.ResetUnit();
            foreach(Command c in ConsoleManager.Main.CurrentCommands)
                if(c.Move != null)
                    c.Move.ResetMove();

            BattleResult result;
            try{ while(true){
                foreach(Enemy e in Enemies) await e.SetIntention();
                foreach(Enemy e in Enemies) await ConsoleManager.Main.AddText(e.Name+" "+e.Intention.Description);
                await HandleTurn(GameplayManager.Self);
                List<Enemy> enemies = new List<Enemy>(this.enemies);
                foreach(Enemy e in enemies)
                    if(!e.Dead)
                        await HandleTurn(e);
                round++;
            }}
            catch(BattleResultException b){
                result = b.Result;
            }

            Battle = null;
            return result;
        });
    }

    public Task HandleTurn(Unit u){
        return Task.Run( async () => {
            if(await u.StartTurn()){
                if(u is Player){
                    ConsoleManager m = ConsoleManager.Main;
                    await m.AddText("Your move");
                    Command c = null;
                    do{
                        c = await m.GetAndRunCommand();
                    } while(c == null || c.Move == null || !c.Move.UsesTurn);
                }
                else if(u is Enemy){
                    await (u as Enemy).Intention.Move((u as Enemy));
                }
            }
            await u.EndTurn();
        });
    }

    public void CheckBattleResult(){
        if(enemies.Count == 0){
            if(Flawless) throw new BattleResultException(BattleResult.Flawless);
            else throw new BattleResultException(BattleResult.Victory);
        }
    }

    public BattleManager(params Enemy[] enemies){
        if(enemies.Length == 0) throw new Exception("BattleManager cannot be initalized with 0 enemies");
        this.enemies = new List<Enemy>(enemies);
    }
}
public class BattleResultException : Exception{
    public BattleResult Result{
        get{
            return result;
        }
    }
    BattleResult result;
    
    public BattleResultException(BattleResult result){
        this.result = result;
    }
}
public enum BattleResult{
    Flawless,
    Victory,
    Defeat,
}
public interface Move{
    Task Activate(object[] input);
    void ResetMove();
    void ReduceCooldown();

    Type[] InputTypes{get;}

    int Cooldown{get;}
    int CurrentCooldown{get;}
    int MaxUses{get;} //-1 is unlimited uses
    int CurrentUses{get;}
    bool UsesTurn{get;}
}

public abstract class BasicMove : Move{
    public abstract string Name{get;}
    public abstract Type[] InputTypes{get;}

    protected abstract Action<Player, object[]> onActivate {get;}
    public bool CanActivate(){
        return !(currentCooldown > 0 || currentUses == 0 || firstTurnCooldown);
    }
    public Task Activate(object[] input){
        return Task.Run( () => {
            if(!CanActivate()) return;
            onActivate(GameplayManager.Self, input);
            if(MaxUses > 0) currentUses--;
            if(Cooldown >= 0){
                currentCooldown = Cooldown;
                firstTurnCooldown = true;
            }
        });
    }
    public void ResetMove(){
        currentCooldown = 0;
        currentUses = MaxUses;
        firstTurnCooldown = false;
    }
    public void ReduceCooldown(){
        if(firstTurnCooldown) firstTurnCooldown = false;
        else if(currentCooldown > 0) currentCooldown--;
    }

    public bool firstTurnCooldown;
    public abstract int Cooldown{get;}
    public int CurrentCooldown{
        get{
            return currentCooldown;
        }
    }
    int currentCooldown;
    public abstract int MaxUses{get;}
    public int CurrentUses{
        get{
            return currentUses;
        }
    }
    int currentUses;
    public abstract bool UsesTurn{get;}
    public abstract string Description{get;}

    public Command GetCommand(){
        return new Command(Name, Description, InputTypes, o => Activate(o), this);
    }
}
public class Attack : BasicMove{
    static Attack a = new Attack();
    public static Attack Get(){
        return a;
    }
    public override string Name => "Attack";
    public override string Description => "Deal 5 damage. This can be changed by your weapon";
    public override int Cooldown => GameplayManager.Self.Weapon.Name == "Pistol" ? 1 : 0;
    public override int MaxUses => -1;
    public override bool UsesTurn => true;
    public override Type[] InputTypes => new Type[] {typeof(Unit)};

    bool rifleShot = true;
    public new void ResetMove(){
        rifleShot = true;
        base.ResetMove();
    }
    protected override Action<Player, object[]> onActivate => (p, o) => {
        switch(p.Weapon.Name){
            case "Dual Daggers": 
                p.Attack(3, (Unit)o[0]);
                p.Attack(3, (Unit)o[0]);
            break;
            case "Rifle and Bayonet": 
                if(rifleShot){
                    p.Attack(15, (Unit)o[0]);
                    rifleShot = false;
                }
                else p.Attack(4, (Unit)o[0]);
            break;
            default: p.Attack(5, (Unit)o[0]); break;
        }
    };
    Attack(){}
}
public class Block : BasicMove{
    static Block b = new Block();
    public static Block Get(){
        return b;
    }
    public override string Name => "Block";
    public override string Description => "Gain 10 block";
    public override int Cooldown => 0;
    public override int MaxUses => -1;
    public override bool UsesTurn => true;
    public override Type[] InputTypes => new Type[] {};

    protected override Action<Player, object[]> onActivate => (p, o) => {
        // switch(s.Weapon.Name){
        //     case "Dual Daggers": 
        //         s.Attack(3, (Unit)o[0]);
        //         s.Attack(3, (Unit)o[0]);
        //     break;
        //     case "Rifle and Bayonet": 
        //         if(rifleShot){
        //             s.Attack(15, (Unit)o[0]);
        //             rifleShot = false;
        //         }
        //         else s.Attack(4, (Unit)o[0]);
        //     break;
        //     default: s.Attack(5, (Unit)o[0]); break;
        // }
        int block = 10;
        if(new List<Item>(GameplayManager.Self.Items).Find(a => a.Name == "Bracers") != null)
            block += 3;
        p.AddBuff(Condition.Block(block));
    };
    Block(){}
}
public class Use : BasicMove{
    static Use u = new Use();
    public static Use Get(){
        return u;
    }
    public override string Name => "Use";
    public override string Description => "Use a consumable item (does not require a turn)";
    public override int Cooldown => 0;
    public override int MaxUses => -1;
    public override bool UsesTurn => false;
    public override Type[] InputTypes => new Type[] {typeof(Consumable)};

    protected override Action<Player, object[]> onActivate => (p, o) => {
        // switch(s.Weapon.Name){
        //     case "Dual Daggers": 
        //         s.Attack(3, (Unit)o[0]);
        //         s.Attack(3, (Unit)o[0]);
        //     break;
        //     case "Rifle and Bayonet": 
        //         if(rifleShot){
        //             s.Attack(15, (Unit)o[0]);
        //             rifleShot = false;
        //         }
        //         else s.Attack(4, (Unit)o[0]);
        //     break;
        //     default: s.Attack(5, (Unit)o[0]); break;
        // }
        ((Consumable)o[0]).OnConsume();
    };
    Use(){}
}