using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Player : Unit
{
    public override string Name => "You";
    protected override int maxHp => 20;

    public int Rations{
        get{
            return rations;
        }
    }
    public Task SetRations(int value){
        return Task.Run(async ()=>{
            int temp = rations;
            rations = value;
            await ConsoleManager.Main.AddText(Name + " " + (rations - temp > 0 ? "gained" : "lost") + " " + Mathf.Abs(rations - temp) + " rations ("+(rations)+")");
        });
    }
    int rations;
    public int MaxSanity{
        get{
            return maxSanity + StatChangeTotalAmount(StatType.MaxSanity);
        }
    }
    int maxSanity = 5;
    const float maxGlitch = .3f;
    public int Sanity{
        get{
            return sanity;
        }
    }
    public Task SetSanity(int value){
        return Task.Run(async ()=>{
            int temp = sanity;
            sanity = value;
            float p = (1 - ((float)sanity / (float)MaxSanity)) * maxGlitch;
            ConsoleManager.glitchIntensity = p;
            ConsoleManager.colorIntensity = p;
            if(sanity > MaxSanity){
                sanity = MaxSanity;
            }
            await ConsoleManager.Main.AddText(Name + " " + (sanity - temp > 0 ? "gained" : "lost") + " " + Mathf.Abs(sanity - temp) + " sanity ("+(sanity)+"/"+(MaxSanity)+")");
            if(sanity <= 0){
                GameplayManager.Session.LoseGame();
            }
        });
    }
    int sanity;
    public int MaxItems{
        get{
            return maxItems + StatChangeTotalAmount(StatType.MaxItems);
        }
    }
    int maxItems = 5;
    public override void StatChanged(StatChange s){
        switch(s.Type){
            case StatType.MaxHealth:{
                float max = MaxHp;
                float proportion = (float)Hp / (max - s.Amount);
                hp = (int)(proportion * max);
            break;}
            case StatType.MaxItems:{
                int max = MaxItems;
                while(items.Count > max)
                    items.RemoveAt(items.Count-1);
            break;}
            case StatType.MaxSanity:{
                float max = MaxSanity;
                float proportion = (float)Sanity / (max - s.Amount);
                sanity = (int)(proportion * max);
            break;}
            default: throw new System.Exception("Stat change of type "+s.Type+" not implemented");
        }
    }
    

    public Equip Weapon{
        get{
            return weapon;
        }
    }
    Equip weapon;
    public Equip Outfit{
        get{
            return outfit;
        }
    }
    Equip outfit;
    public Skill[] Skills{
        get{
            return skills.ToArray();
        }
    }
    List<Skill> skills = new List<Skill>();
    public Item[] Items{
        get{
            return items.ToArray();
        }
    }
    List<Item> items = new List<Item>();

    protected override void die(){
        throw new BattleResultException(BattleResult.Defeat);
    }
    
    public new Task<bool> StartTurn(){
        foreach(Skill s in skills)
            s.ReduceCooldown();
        return base.StartTurn();
    }
    public void SetWeapon(Equip e){
        if(e.Type != EquipType.Weapon) return;

        if(weapon != null) weapon.OnRemove();
        weapon = e;
        weapon.OnObtain();
    }
    public void SetOutfit(Equip e){
        if(e.Type != EquipType.Outfit) return;

        if(outfit != null) outfit.OnRemove();
        outfit = e;
        outfit.OnObtain();
    }
    public void AddSkill(Skill s){
        skills.Add(s);
        s.OnObtain();
    }
    public void RemoveSkill(Skill s){
        skills.Remove(s);
        s.OnRemove();
    }
    public void AddItem(Item i){
        items.Add(i);
        i.OnObtain();
    }
    public void RemoveItem(Item i){
        items.Remove(i);
        i.OnObtain();
    }

    public Player(){
        sanity = maxSanity;
    }
}
