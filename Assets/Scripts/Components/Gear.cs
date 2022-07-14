using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public abstract class Gear{
    public string Name{
        get{
            return name;
        }
    }
    string name;
    public string Description{
        get{
            return description;
        }
    }
    string description;
    
    protected Action<Player> onObtain;
    public void OnObtain(){
        if(onObtain != null) onObtain(GameplayManager.Self);
    }
    protected Action<Player> onRemove;
    public void OnRemove(){
         if(onRemove != null) onRemove(GameplayManager.Self);
    }

    public Gear(string name, string description, Action<Player> onObtain = null, Action<Player> onRemove = null){
        this.name = name;
        this.description = description;
        this.onObtain = onObtain;
        this.onRemove = onRemove;
    }

    public override string ToString(){
        return Name;
    }
    public string ToFullString(){
        return Name+" - "+Description;
    }
}
public enum GearRarity{
    Starter,
}

public class Equip : Gear{
    public static Equip[] Get(GearRarity r){
        switch(r){
            case GearRarity.Starter: return new Equip[] { //TODO: survival gear
                new NormalEquip("Pistol", "Deal +4 damage. Attacks now have a cooldown of 1 turn", EquipType.Weapon, (EffectActivation.OnAttack, async (b, o) => {
                    return (int)o[0] + 4;
                })),
                new NormalEquip("Rapier", "If the target has 0 block, deal an additional 5 damage", EquipType.Weapon, (EffectActivation.OnAttack, async (b, o) => {
                    Unit target = (Unit)o[1];
                    if(target.ConditionTotalAmount(ConditionType.Block) == 0) return (int)o[0] + 5;
                    else return o[0];
                })),
                new NormalEquip("Cutlass", "After attacking, gain 5 block", EquipType.Weapon, (EffectActivation.AfterAttack, async (b, o) => {
                    b.Self.AddBuff(Condition.Block(5));
                    return null;
                })),
                new NormalEquip("Long Sword", "Deal +2 damage", EquipType.Weapon, (EffectActivation.OnAttack, async (b, o) => {
                    return (int)o[0]+2;
                })),
                new NormalEquip("Whip", "After attacking, if the target is attacking, gain 1 dodge", EquipType.Weapon, (EffectActivation.AfterAttack, async (b, o) => {
                    if(o[1] is Enemy && new List<IntentionType>((o[1] as Enemy).Intention.Type).Contains(IntentionType.Attack))
                        b.Self.AddBuff(Condition.Dodge(1));
                    return null;
                })),
                new NormalEquip("Dual Daggers", "Attacking instead now deals 3 damage twice", EquipType.Weapon),
                new NormalEquip("Rifle and Bayonet", "The first attack of each combat does 15 damage, all subsequent attacks do 4", EquipType.Weapon),

                
                new NormalEquip("Metal Armor", "All attacks do -2 damage against you", EquipType.Outfit, (EffectActivation.OnDefend, async (b, o) => {
                    return (int)o[0] - 2;
                })),
                new StatEquip("Hide Armor", "+10 max health", EquipType.Outfit, (StatType.MaxHealth, 10)),
                new NormalEquip("Survival Gear", "Every 3 days, gain 1 ration", EquipType.Outfit, (EffectActivation.AfterSleep, async (b, o) => {
                    if(GameplayManager.Session.CurrentDay % 3 == 0) await GameplayManager.Self.SetRations(GameplayManager.Self.Rations+1);
                    return null;
                })),
                new StatEquip("Travel Clothes", "Carry up to 3 more items", EquipType.Outfit, (StatType.MaxItems, 3)),
                new StatEquip("Comfortable Clothes", "+1 max sanity", EquipType.Outfit, (StatType.MaxSanity, 1)),
                new NormalEquip("Light Clothes", "Dodge instead dodges all attacks from the same attacker", EquipType.Outfit, (EffectActivation.AfterTryDefend, async (b, o) => {
                    Unit attacker = (Unit)o[1];
                    if((int?)o[0] == null){
                        b.Self.AddBuff(new Effect(1, (EffectActivation.OnTryDefend, async (buff, obj) => {
                            if((Unit)obj[1] == attacker) return false;
                            else return obj[0];
                        })));
                    }
                    return null;
                })),
                new NormalEquip("Counter Armor", "Whenever an attack hits you, deal 3 damage to the attacker", EquipType.Outfit, (EffectActivation.AfterDefend, async (b, o) => {
                    await b.Self.Attack(3, (Unit)o[1]);
                    return null;
                })),
            };
            default: throw new Exception("Gear rarity \""+r+"\" is invalid");
        }
    }

    
    public EquipType Type{
        get{
            return type;
        }
    }
    EquipType type;

    public Equip(string name, string description, EquipType type, Action<Player> onObtain = null, Action<Player> onRemove = null) : base(name, description, onObtain, onRemove){
        this.type = type;
    }
}
public class NormalEquip : Equip{
    public NormalEquip(string name, string description, EquipType type, params (EffectActivation, Func<Buff, object[], Task<object>>)[] effects) : base(name, description, type) {
        Buff[] buffs = new Buff[effects.Length];
        for(int i = 0; i < effects.Length; i++)
            buffs[i] = new Effect(-2, (effects[i].Item1, effects[i].Item2));
        onObtain = p => {
            foreach(Buff b in buffs) p.AddBuff(b);
        };
        onRemove = p => {
            foreach(Buff b in buffs) p.RemoveBuff(b);
        };
    }
}
public class StatEquip : Equip{
    public StatEquip(string name, string description, EquipType type, params (StatType, int)[] statChanges) : base(name, description, type) {
        Buff[] buffs = new Buff[statChanges.Length];
        for(int i = 0; i < statChanges.Length; i++)
            buffs[i] = new StatChange(statChanges[i].Item2, statChanges[i].Item1, -2);
        onObtain = p => {
            foreach(Buff b in buffs) p.AddBuff(b);
        };
        onRemove = p => {
            foreach(Buff b in buffs) p.RemoveBuff(b);
        };
    }
}
public enum EquipType{
    Weapon,
    Outfit,
}
public class Skill : Gear, Move{
    public static Skill[] Get(GearRarity r){
        switch(r){
            case GearRarity.Starter: return new Skill[] {
                new Skill("Flurry", "Attack a target twice (3 turns)", new Type[] {typeof(Unit)}, 3, async (p, o) => {
                    Unit target = (Unit)o[0];
                    Attack.Get().ResetMove();
                    await Attack.Get().Activate(o);
                    Attack.Get().ResetMove();
                    await Attack.Get().Activate(o);
                }, p => Attack.Get().CanActivate()),
                new Skill("Fortify", "Gain 5 block plus 5 per enemy (3 turns)", new Type[] {}, 3, async (p, o) => {
                    p.AddBuff(Condition.Block(5 + BattleManager.Battle.Enemies.Length));
                }),
                new Skill("Berzerk", "Deal 15 damage, take 5 damage (2 turns)", new Type[] {typeof(Unit)}, 2, async (p, o) => {
                    await p.Attack(15, (Unit)o[0]);
                    await p.TakeDamage(5);
                }),
                new Skill("Feint", "Gain 2 dodge (3 turns)", new Type[] {}, 3, async (p, o) => {
                    p.AddBuff(Condition.Dodge(2));
                }),
                new Skill("Concuss", "Stun an enemy for 1 turn (4 turns)", new Type[] {typeof(Unit)}, 4, async (p, o) => {
                    ((Unit)o[0]).AddBuff(Condition.Stun(1));
                }),
                new Skill("Target", "Attack, for each 4 damage done inflict 1 bleed (3 turns)", new Type[] {typeof(Unit)}, 3, async (p, o) => {
                    Unit target = (Unit)o[0];
                    int prevHp = target.Hp;
                    await Attack.Get().Activate(o);
                    int difference = prevHp - target.Hp;
                    if(difference < 0) difference = 0;
                    target.AddBuff(Condition.Bleed(difference / 4));

                }, p => Attack.Get().CanActivate()),
                new Skill("Rest", "Heal 5 hp (3 turns)", new Type[] {}, 3, async (p, o) => {
                    await p.Heal(5);
                }),
                new Skill("Surge", "Take both the Attack action and the Block action (2 uses)", new Type[] {typeof(Unit)}, -1, 2, async (p, o) => {
                    await Attack.Get().Activate(o);
                    await Block.Get().Activate(null);
                }, p => Attack.Get().CanActivate() && Block.Get().CanActivate()),
                new Skill("Bodyslam", "Deal as much damage as block you have (4 turns)", new Type[] {typeof(Unit)}, 4, async (p, o) => {
                    await p.Attack(p.ConditionTotalAmount(ConditionType.Block), (Unit)o[0]);
                }),
                new Skill("Focus", "Deal +1 damage for the rest of combat (3 turns)", new Type[] {}, 3, async (p, o) => {
                    p.AddBuff(new Effect(-1, (EffectActivation.OnAttack, async (b, obj) => {
                        return (int)obj[0] + 1;
                    })));
                }),
                new Skill("Cut", "Inflict 4 bleed (1 use)", new Type[] {typeof(Unit)}, -1, 1, async (p, o) => {
                    ((Unit)o[0]).AddBuff(Condition.Bleed(4));
                }),
                new Skill("Cannibalize", "Deal 5 damage, if this kills gain 1 ration (1 use)", new Type[] {typeof(Unit)}, -1, 1, async (p, o) => {
                    Unit target = (Unit)o[0];
                    bool prevAlive = !target.Dead;
                    await p.Attack(5, target);
                    if(prevAlive && target.Dead) await GameplayManager.Self.SetRations(GameplayManager.Self.Rations+1);
                }),
            };
            default: throw new Exception("Gear rarity \""+r+"\" is invalid");
        }
    }
    
    
    
    Func<Player, object[], Task> onActivate;
    Func<Player, bool> canActivate;
    public bool CanActivate(){
        return !(currentCooldown != 0 || currentUses == 0 || firstTurnCooldown) && (canActivate == null || canActivate(GameplayManager.Self));
    }
    public Task Activate(object[] input){
        return Task.Run( async () => {
            if(!CanActivate()) return;
            await onActivate(GameplayManager.Self, input);
            if(maxUses > 0) currentUses--;
            if(cooldown >= 0){
                currentCooldown = cooldown;
                firstTurnCooldown = true;
            }
        });
    }
    public void ResetMove(){
        currentCooldown = 0;
        currentUses = maxUses;
        firstTurnCooldown = false;
    }
    public void ReduceCooldown(){
        if(firstTurnCooldown) firstTurnCooldown = false;
        else if(currentCooldown > 0) currentCooldown--;
    }

    bool firstTurnCooldown;
    public int Cooldown{
        get{
            return cooldown;
        }
    }
    int cooldown;
    public int CurrentCooldown{
        get{
            return currentCooldown;
        }
    }
    int currentCooldown;
    public int MaxUses{
        get{
            return maxUses;
        }
    }
    int maxUses; //-1 means infiinte uses
    public int CurrentUses{
        get{
            return currentUses;
        }
    }
    int currentUses;
    public bool UsesTurn{
        get{
            return usesTurn;
        }
    }
    bool usesTurn;

    public Type[] InputTypes{
        get{
            return inputTypes;
        }
    }
    Type[] inputTypes;
    
    
    public Skill(string name, string description, Type[] inputTypes, int cooldown, Func<Player, object[], Task> onActivate, Func<Player, bool> canActivate = null, Action<Player> onObtain = null, Action<Player> onRemove = null) : this(name, description, inputTypes, cooldown, -1, onActivate, canActivate, onObtain, onRemove){}
    public Skill(string name, string description, Type[] inputTypes, int cooldown, int maxUses, Func<Player, object[], Task> onActivate, Func<Player, bool> canActivate = null,  Action<Player> onObtain = null, Action<Player> onRemove = null) : this(name, description, inputTypes, cooldown, maxUses, true, onActivate, canActivate, onObtain, onRemove){}
    public Skill(string name, string description, Type[] inputTypes, int cooldown, int maxUses, bool usesTurn, Func<Player, object[], Task> onActivate, Func<Player, bool> canActivate = null, Action<Player> onObtain = null, Action<Player> onRemove = null) : base(name, description, onObtain, onRemove){
        this.cooldown = cooldown < 0 ? cooldown : cooldown + 1;
        currentCooldown = 0;
        this.maxUses = maxUses;
        currentUses = maxUses;
        this.onActivate = onActivate;
        this.usesTurn = usesTurn;
        this.canActivate = canActivate;
    }


    public Command GetCommand(){
        return new Command(Name, Description, inputTypes, o => Task.Run( async () => await Activate(o)), this);
    }
}
public abstract class Item : Gear{
    public static Item[] Get(GearRarity r){
        switch(r){
            case GearRarity.Starter: return new Item[] { //TODO: firecracker
                new Consumable("Bandages", "Heal 10 hp (3 uses)", async p => {
                    await p.Heal(10);
                }, 3),
                new Consumable("Bomb", "All enemies take 10 damage", async p => {
                    for(int i = BattleManager.Battle.Enemies.Length-1; i >= 0; i--){
                        await BattleManager.Battle.Enemies[i].TakeDamage(10);
                    }
                }),
                // new Consumable("Firecracker", "Combat ends and no penalty is applied. Sanity is calculated normally.", p => {
                //     for(int i = BattleManager.Battle.Enemies.Length-1; i >= 0; i--){
                //         BattleManager.Battle.Enemies[i].TakeDamage(10);
                //     }
                // }),
                new Consumable("Throwing Knives", "Deal 3 damage to the lowest health enemy (3 uses)", async p => {
                    Unit target = BattleManager.Battle.Enemies[0];
                    foreach(Unit u in BattleManager.Battle.Enemies)
                        if(u.Hp < target.Hp)
                            target = u;
                    await p.Attack(3, target);
                }, 3),
                new Consumable("Whiskey", "Gain 1 sanity, reduce max hp by 10 for this combat (3 uses)", async p => {
                    await p.SetSanity(p.Sanity+1);
                    p.AddBuff(new StatChange(-10, StatType.MaxHealth));
                }, 3),


                new NormalPassive("Weapon Charm", "Deal +2 damage", (EffectActivation.OnAttack, async (b, o) => {
                    return (int)o[0] + 2;
                })),
                new StatPassive("Armor Padding", "+5 max hp", (StatType.MaxHealth, 5)),
                new NormalPassive("Whetstone", "After dealing damage, if the target has 0 bleed, inflict 1 bleed", (EffectActivation.AfterAttack, async (b, o) => {
                    Unit target = (Unit)o[1];
                    if(target.ConditionTotalAmount(ConditionType.Bleed) == 0) target.AddBuff(Condition.Bleed(1));
                    return null;
                })),
                new NormalPassive("Bracers", "Gain +3 block when blocking"),
                new NormalPassive("Amulet", "Upon taking lethal damage, you instead take 0 and this item breaks", (EffectActivation.OnTakeDamage, async (b, o) => {
                    if(b.Self.Hp <= (int)o[0]) {
                        GameplayManager.Self.RemoveItem(new List<Item>(GameplayManager.Self.Items).Find(a => a.Name == "Amulet"));
                        return 0;
                    }
                    else return o[0];
                })),
                new NormalPassive("Treaded Boots", "Block lasts an extra turn"),
            };
            default: throw new Exception("Gear rarity \""+r+"\" is invalid");
        }
    }
    

    public ItemType Type{
        get{
            return type;
        }
    }
    ItemType type;

    public Item(string name, string description, ItemType type, Action<Player> onObtain = null, Action<Player> onRemove = null) : base(name, description, onObtain, onRemove){
        this.type = type;
    }
}
public class Consumable : Item{
    public new string Description{
        get{
            return base.Description + " ("+uses+" use"+(uses != 1 ? "s" : "")+" left)";
        }
    }

    public int Uses{
        get{
            return uses;
        }
    }
    int uses;
    
    Action<Player> onConsume;
    public void OnConsume(){
        onConsume(GameplayManager.Self);
        uses--;
        if(uses == 0) GameplayManager.Self.RemoveItem(this);
    }

    public Consumable(string name, string description, Action<Player> onConsume, int uses = 1, Action<Player> onObtain = null, Action<Player> onRemove = null) : base(name, description, ItemType.Consumable, onObtain, onRemove){
        this.onConsume = onConsume;
        this.uses = uses;
    }
}
public class Passive : Item{
    public Passive(string name, string description, Action<Player> onObtain = null, Action<Player> onRemove = null) : base(name, description, ItemType.Passive, onObtain, onRemove){}
}
public class NormalPassive : Passive{
    public NormalPassive(string name, string description, params (EffectActivation, Func<Buff, object[], Task<object>>)[] effects) : base(name, description) {
        Buff[] buffs = new Buff[effects.Length];
        for(int i = 0; i < effects.Length; i++)
            buffs[i] = new Effect(-2, (effects[i].Item1, effects[i].Item2));
        onObtain = p => {
            foreach(Buff b in buffs) p.AddBuff(b);
        };
        onRemove = p => {
            foreach(Buff b in buffs) p.RemoveBuff(b);
        };
    }
}
public class StatPassive : Passive{
    public StatPassive(string name, string description, params (StatType, int)[] statChanges) : base(name, description) {
        Buff[] buffs = new Buff[statChanges.Length];
        for(int i = 0; i < statChanges.Length; i++)
            buffs[i] = new StatChange(statChanges[i].Item2, statChanges[i].Item1, -2);
        onObtain = p => {
            foreach(Buff b in buffs) p.AddBuff(b);
        };
        onRemove = p => {
            foreach(Buff b in buffs) p.RemoveBuff(b);
        };
    }
}
public enum ItemType{
    Passive,
    Consumable,
}