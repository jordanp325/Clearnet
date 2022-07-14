using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class GameplayManager
{
    public static GameplayManager Session = null;
    public static Player Self;

    public int CurrentDay{
        get{
            return currentDay;
        }
    }
    int currentDay = 1;

    public static Task InitalizeAndRun(){
        return Task.Run( async () => {
            Session = new GameplayManager();
            Self = new Player();
            ConsoleManager.Main.SetCommands();

            if(SaveData.Instance.CompletedTutorial) await Session.RunGame();
            else {
                await Session.RunTutorial();
                SaveData.Instance.CompletedTutorial = true;
            }

            ConsoleManager.glitchIntensity = 0f;
            ConsoleManager.colorIntensity = 0f;
            Self = null;
            Session = null;
            ConsoleManager.Main.SetCommands();
        });
    }
    Task RunTutorial(){
        ConsoleManager c = ConsoleManager.Main;
        return Task.Run( async () => {
            await c.Pause(1000);
            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[3].Play());
            await c.Pause(2000);

            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[1].Stop());
            int glitchDuration = 8;
            for(int i = 0; i < glitchDuration; i ++){
                
                ConsoleManager.glitchIntensity = 1/(float)glitchDuration*(i+1);
                ConsoleManager.colorIntensity = 1/(float)glitchDuration*(i+1);
                await c.Wait(1000);
                if(i == 4){
                    c.xRenderGlitch = true;
                    c.ToBeRun.Add(() => c.remakeRenderTexture());
                }
                if(i == 6){
                    c.yRenderGlitch = true;
                    c.ToBeRun.Add(() => c.remakeRenderTexture());
                }
            }

            c.freeze = true;
            await c.Wait((15 - glitchDuration)*1000);
            c.freeze = false;

            c.clearConsole();
            ConsoleManager.glitchIntensity = .2f;
            ConsoleManager.colorIntensity = .2f;
            c.xRenderGlitch = false;
            c.yRenderGlitch = false;
            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[4].Play());
            c.ToBeRun.Add(() => c.remakeRenderTexture());
            await c.Wait(4000);
            await c.AddCreepyText("Hello there", 150);
            await c.Wait(2000);
            c.clearConsole();
            await c.Wait(250);
            await c.AddText("How ", 150);
            await c.Wait(250);
            await c.AddText("interesting", false, 250);
            await c.Wait(1000);
            //TODO: this
        });
    }
    Task RunGame(){
        ConsoleManager c = ConsoleManager.Main;
        return Task.Run( async () => {
            //commented out because of the sanity meter changing the amount of glitch
            // ConsoleManager.glitchIntensity = .1f;
            // ConsoleManager.colorIntensity = .1f;
            c.clearConsole();
            int pause = 500;
            int step = 20;
            for(int i = 0; i < pause; i += step){
                await c.Wait(step);
                c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[1].volume = (1 - ((float)i / (float)pause)));
            }
            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[1].Stop());
            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[1].volume = 1);
            c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[4].Play());
            //TODO: music needs to be played here, and stopped after playing the tutorial


            //TODO: add secondary AIs


            #region Intro
            await c.AddText("Drifter's Pass. A modest town that's recently been occupied by a battalion of rangers.");
            await c.AddText("The town's goal: investigate the mysterious fog a quarter mile west.");
            await c.AddText("\n\n");
            await c.AddText("The town's horn sounds and you jolt awake. Unfortunately for you, today is the day you venture into the fog.");
            await c.AddText("\"Hey! Ranger!\" Your sergeant calls out to you.");
            await c.AddText("\""+RandomChoice("Todays the day, haven't you forgotten?", "Let's get a move on here.", "Don't think you're getting out of this.")+"\"");
            await c.AddText("He shows you an assortment of weapons. \"Pick one, and make it count.\"");
            await c.WaitForEnter();

            //AI 1
            List<Equip> allWeapons = new List<Equip>(Equip.Get(GearRarity.Starter)).FindAll(e => e.Type == EquipType.Weapon);
            Equip[] weapons = RandomArray(3, allWeapons.ToArray());
            Self.SetWeapon(await c.PromptChoice(weapons));

            await c.AddText("\nYou take your weapon and the sergeant turns to show you a few outfits.");
            await c.AddText("\"Who knows what you'll find in that fog.\"");

            List<Equip> allOutfits = new List<Equip>(Equip.Get(GearRarity.Starter)).FindAll(e => e.Type == EquipType.Outfit);
            Equip[] outfits = RandomArray(3, allOutfits.ToArray());
            Self.SetOutfit(await c.PromptChoice(outfits));

            c.clearConsole();
            await c.AddText("You move on to your instructor. \""+RandomChoice("The first into the fog, may you find peace.", "Your journey may be treacherous, and I wish you good luck", "May the hand of fate guide you on your way")+"\"");
            await c.AddText("You each drawn your weapon and ready your stance.");
            await c.AddText("\"What do you wish to practice today?\"");
            await c.WaitForEnter();

            //AI 2
            List<Skill> allSkills = new List<Skill>(Skill.Get(GearRarity.Starter));
            List<Skill> skills = new List<Skill>(RandomArray(5, allSkills.ToArray()));
            Skill s = await c.PromptChoice(skills.ToArray());
            AddSkill(s);
            skills.Remove(s);
            s = await c.PromptChoice(skills.ToArray());
            AddSkill(s);

            c.clearConsole();
            await c.AddText("Last was the quartermaster. He's already prepared a bag with 3 rations inside.");
            await c.AddText("\""+RandomChoice("Oh! You scared me there.", "I didn't know how much to pack...", "You won't be gone long, right?")+"\"");
            await c.AddText("He pulls out a tray of items.");
            await c.AddText("\"Pick something interesting.\"");
            await c.WaitForEnter();

            //AI 3
            await Self.SetRations(Self.Rations+3);
            List<Item> allPassives = new List<Item>(Item.Get(GearRarity.Starter)).FindAll(i => i.Type == ItemType.Passive);
            Item[] passives = RandomArray(3, allPassives.ToArray());
            Item pas = await c.PromptChoice(passives);
            allPassives.Remove(pas);
            Self.AddItem(pas);

            await c.AddText("\nHe pulls out a second tray.");
            await c.AddText("\"Something to use when things get tough.\"");

            List<Item> allConsumables = new List<Item>(Item.Get(GearRarity.Starter)).FindAll(i => i.Type == ItemType.Consumable);
            Item[] consumables = RandomArray(3, allConsumables.ToArray());
            Item con = await c.PromptChoice(consumables);
            allConsumables.Remove(con);
            Self.AddItem(con);

            await c.AddText("\nThe quartermaster has a worried expression on his face.");
            await c.AddText("\"Just for good measure, let me pack you something extra...\"");

            if(await c.PromptChoice("Rations", "Item") == "Rations"){
                await Self.SetRations(Self.Rations+3);
            }
            else{
                List<Item> allItems = new List<Item>(allPassives);
                allItems.AddRange(allConsumables);
                Item[] items = RandomArray(3, allItems.ToArray());
                Self.AddItem(await c.PromptChoice(items));
            }

            c.clearConsole();
            await c.AddText("Within no time, the town has faded into the distance. In front of you lies an impenetrable wall of smoke.");
            await c.AddText("You approach the wall. Reaching your hand out, the fog dissapates around it");
            await c.AddText("With one final step, you join the fog in its gloom...");
            await c.WaitForEnter();
            c.clearConsole();
            #endregion

            try{
                #region Chapter 1
                await c.ShowTitle("Chapter 1");
                await c.AddText("Your face is greeted by the cold hard floor of the wasteland as you wake up.");
                await c.AddText("You look around to see nothing familiar, you have no idea where you are.");
                await c.AddText("The sun is covered by the complete overcast clouds, with no clue to what time of day it is.");
                await c.AddText("All that's left is the dark and barren landscape that surrounds you.");
                await c.AddText("\nAfter taking a moment to compose yourself, you look around to see a few places of interest.");
                await c.AddText("Off to your right is a number of rocky hills. You see trails leading to a possible settlement there.");
                await c.AddText("To your left is barren flat wasteland. You have a feeling this is where you came from.");
                await c.AddText("Behind you is a small town, barely 30 buildings in size. Whatever town it is, it isn't one you know.");
                await c.AddText("You look into your bag to see "+Self.Rations+" rations.");
                await c.AddText("With no way of knowing when your next meal will be, you must make a choice.");
                await c.WaitForEnter();

                string chapter1Choice = await c.PromptChoice("Hills", "Wasteland", "Town");
                switch(chapter1Choice){
                    case "Hills":


                    break;
                    case "Wasteland":
                    await CombatEncounter(
                        "Surrender",

                        "You walk deep into the wasteland before coming upon a fresh kill.\n"+
                        "Curious, you approach the corpse. Behind rocks to your left and right, two bandits emerge.\n"+
                        "\"It's your bag, or your life.\" You can tell they're after your rations.",

                        "You toss them your rations, and they immediately start fighting over them.\n"+
                        "Before they can think twice, you run.\n\n"+
                        "After a few minutes, the only thing behind you is wasteland. You've lost them.\n"+
                        "Escaping the bandits gives you a sense of hope.",

                        "Instead of surrendering, you draw your weapon. They do the same.\n"+
                        "With a toothy grin, the bandits draw near. You ready yourself for a fight.",

                        "You're knocked to the side, your injuries consume your attention.\n"+
                        "The bandits rifle through your bag to find your rations.\n"+
                        "While they're busy with your food, you decide to run.\n\n"+
                        "You stumble several times before finally making it a safe distance away.\n"+
                        "You look through whats left of your bag to see all of your rations gone.\n"+
                        "With your injured leg burning and hunger starting to set in, you lose hope.",

                        "You clutch at your side from the bruise you aquired durring battle.\n"+
                        "With the bandits knocked out and lying on the ground, you go through their things.\n\n"+
                        "The most useful thing you find is a deck of cards. Something to pass the time with, perhaps.\n"+
                        "You wince as you attempt to walk forwards. The wasteland has not been kind to you.",

                        "You holster your weapon as the bandits fall to the floor.\n"+
                        "Rifling through their posessions, you find a deck of cards. Solitaire should help pass the time.\n"+
                        "Before continuing through the wasteland, you grab one of the bandits' hats.",

                        async () => {
                            await Self.SetRations(0);
                        },

                        async () => {
                            Self.AddItem(new StatPassive("Deck of cards", "+1 max sanity", (StatType.MaxSanity, 1)));
                        },

                        BattleManager.GetBattle("Wasteland 1")
                    );

                    await c.AddText("lorem ipsum");
                    await c.AddText("dolor si amut");
                    await c.WaitForEnter();

                    break;
                    case "Town":


                    break;
                }
                #endregion
            }
            catch(LoseGameException e){

                //TODO: handle losing the game
                //TODO: add lose game text
                //TODO: crash game or something

                c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[4].Stop());
                c.ToBeRun.Add(() => c.GetComponents<AudioSource>()[1].Play());
            }
        });
    }

    public static T[] RandomArray<T>(int length, T[] objects){
        List<T> allObjects = new List<T>(objects);
        T[] output = new T[length];
        for(int i = 0; i < length; i++){
            int r = rand.Next(0, allObjects.Count);
            output[i] = allObjects[r];
            allObjects.RemoveAt(r);
        }
        return output;
    }
    public static T RandomChoice<T>(params T[] choices){
        return choices[rand.Next(choices.Length)];
    }
    public static int randomInt(int max, int min = 0){
        return rand.Next(min, max);
    }
    static System.Random rand = new System.Random();

    public void AddSkill(Skill s){
        Self.AddSkill(s);
        ConsoleManager.Main.AddCommand(s.GetCommand());
    }

    public Task CombatEncounter(string escapeCommand, string introduction, string escapeString, string startFightString, string loseFightString, string winFightString, string flawlessFightString, Func<Task> penality, Func<Task> reward, BattleManager battle){
        ConsoleManager m = ConsoleManager.Main;
        return Task.Run( async () => {
            await m.AddText(introduction);
            await m.AddText("Will you fight or "+escapeCommand+"?");
            string choice = await m.PromptChoice("fight", escapeCommand);
            if(choice == escapeCommand){
                await m.AddText(escapeString);
                await Self.SetSanity(Self.Sanity+1);
                await penality();
            }
            else if(choice == "fight"){
                await m.AddText(startFightString);
                BattleResult res = await battle.Run();
                switch(res){
                    case BattleResult.Flawless:
                    await m.AddText(flawlessFightString);
                    await reward();
                    break;
                    case BattleResult.Victory:
                    await m.AddText(winFightString);
                    await Self.SetSanity(Self.Sanity-1);
                    await reward();
                    break;
                    case BattleResult.Defeat:
                    await m.AddText(loseFightString);
                    await Self.SetSanity(Self.Sanity-2);
                    await penality();
                    break;
                }
            }
            await m.WaitForEnter();
        });
    }

    public Task BossEncounter(string introduction, string startFightString, string loseFightString, string winFightString, Func<Task> reward, BattleManager battle){
        ConsoleManager m = ConsoleManager.Main;
        return Task.Run( async () => {
            await m.AddText(introduction);
            await m.Pause(1000);
            if(startFightString != "") await m.AddText(startFightString);
            BattleResult res = await battle.Run();
            if(res != BattleResult.Defeat){
                await m.AddText(winFightString);
                await Self.SetSanity(Self.Sanity+2);
                await reward();
            }
            else{
                await m.AddText(loseFightString);
                LoseGame();
            }
            await m.WaitForEnter();
        });
    }

    public void LoseGame(){
        throw new LoseGameException();
    }

    public Task PassDay(string dayPassText){
        return Task.Run(async () => {
            await ConsoleManager.Main.AddText(dayPassText);

            if(Self.Rations > 0) {
                await ConsoleManager.Main.AddText(RandomChoice("As you wake, you tear into another ration", "Your waking hunger leads to another eaten ration", "Before you realize, another ration is gone"));
                await Self.SetRations(Self.Rations-1);
            }
            else {
                await ConsoleManager.Main.AddText(RandomChoice("Hunger fills your mind, your stomach slowly driving you mad", "With nothing to eat, a slow agonizing pain occupies your stomach", "In the back of your mind, you feel relentless hunger gnawing at you"));
                await Self.SetSanity(Self.Sanity-1);
            }

            await Self.CallEffects(EffectActivation.AfterSleep);
        });
    }

    //TODO: make code for handling open interraction segments
}

public class LoseGameException : System.Exception{}