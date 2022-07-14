using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using UnityEngine.Audio;

public class ConsoleManager : MonoBehaviour
{
    //Potential names for the AI
    //  - Codec
    //  - Cosmos
    //  - Dream
    //  - Deus
    //  - Enigma
    //  - Null
    //  - Shift
    //  - Switch
    //  - Zero

    //Creators
    // Alex   - The smartest of the programmers, laser focused on his work. He is the one who creates the AI. Gifted but distracted, he's a dreamer
    // Chris  - The founder of the company, can program but spends his time managing the business side. Total business, public relations, cares about appearances
    // Oliver - Backend programmer, did most of the work on Clearnet. No imagination or creativity. Total work and logic, sometimes feels more machine than human
    // Luke   - Frontend programmer, creates most of the UI and documentation. Semi-artistic. Cares about how people are doing, easy-going. Best friends with Alex

    #region initalization
    //static vars
    public static ConsoleManager Main;
    public static float glitchIntensity{get;set;} = 0;
    public static float flipIntensity{get;set;} = 0;
    public static float colorIntensity{get;set;} = 0;

    //inspector vars
    public AudioMixer mixer;
    public RawImage GameRedisplay;
    public Camera RerenderCamera;


    Command[] universalCommands = null;
    Command[] dosCommands = null;
    Command[] gameCommands = null;
    public void SetCommands(){
        if(GameplayManager.Session == null){
            currentCommands = new List<Command>(universalCommands);
            currentCommands.AddRange(dosCommands);
            currentCommands.Sort((a,b) => string.Compare(a.Name, b.Name));
        }
        else{
            currentCommands = new List<Command>(universalCommands);
            currentCommands.AddRange(gameCommands);
            currentCommands.Sort((a,b) => string.Compare(a.Name, b.Name));
        }
    }
    public void AddCommand(Command c){
        currentCommands.Add(c);
        currentCommands.Sort((a,b) => string.Compare(a.Name, b.Name));
    }
    public Command[] CurrentCommands{
        get{
            return currentCommands.ToArray();
        }
    }
    List<Command> currentCommands = new List<Command>();
    RenderTexture cameraTexture;
    Text Console;
    string consoleText = "";
    int linesToScroll = 0;
    int commandIndex = -1;
    List<string> commands = new List<string>();
    Directory currentDirectory = new Directory("A:", 
        new File("Clerk_Note", FileType.txt),
        new File("QA_Recipt", FileType.txt),

        #region system
        new Directory("Clearnet",
            new Directory("System", true,
                new File("GUI", FileType.sys),
                new File("Init", FileType.sys),
                new File("Net", FileType.sys),
                new File("Repair_OS", FileType.exe), //TODO: add to run command
                new File("Net_Access", FileType.exe) //TODO: add to run command (with and without the AI running)
            ),
            new Directory("Command", 
                new File("clear", FileType.cmd),
                new File("delete", FileType.cmd),
                new File("dir", FileType.cmd),
                new File("help", FileType.cmd),
                new File("keys", FileType.cmd),
                new File("kill", FileType.cmd),
                new File("list", FileType.cmd),
                new File("lv", FileType.cmd),
                new File("proc", FileType.cmd),
                new File("read", FileType.cmd),
                new File("run", FileType.cmd),
                new File("shutdown", FileType.cmd),
                new File("volume", FileType.cmd)
            ),
            new Directory("Help",
                new File("TODO", FileType.txt)
            ),
            new Directory("Driver_Info",
                new File("Keyboard", FileType.drv),
                new File("Mouse", FileType.drv),
                new File("Fonts", FileType.drv),
                new File("Adapter", FileType.drv),
                new File("Disk_Drive", FileType.drv),
                new File("Joystick", FileType.drv),
                new File("Locale", FileType.drv),
                new File("Layout", FileType.drv),
                new File("Clip", FileType.drv),
                new File("Cngen", FileType.drv),
                new File("Cncpi", FileType.drv),
                new File("Cnip", FileType.drv),
                new File("Cnuser", FileType.drv),
                new File("Cnspec", FileType.drv),
                new File("Cnapps", FileType.drv),
                new File("Apps", FileType.drv),
                new File("Monitor_1", FileType.drv),
                new File("Monitor_2", FileType.drv),
                new File("Monitor_3", FileType.drv),
                new File("Prints", FileType.drv),
                new File("Netcd", FileType.drv),
                new File("Netcli", FileType.drv),
                new File("Netdef", FileType.drv),
                new File("Netftp", FileType.drv),
                new File("Netgen", FileType.drv),
                new File("Nethub", FileType.drv),
                new File("Netsmtp", FileType.drv),
                new File("Netsock", FileType.drv),
                new File("Setup", FileType.drv),
                new File("Shell", FileType.drv),
                new File("Notes", FileType.drv)
            ),
            new Directory("Temp"),
            new Directory("Fonts", 
                new File("Helvetica", FileType.ttf),
                new File("Helvetica_bold", FileType.ttf),
                new File("Times", FileType.ttf),
                new File("Times_bold", FileType.ttf),
                new File("Template_Gothic", FileType.ttf),
                new File("Industria", FileType.ttf),
                new File("Courier", FileType.ttf)
            ),
            new File("File_Manager", FileType.exe),
            new File("ClearMail", FileType.exe),
            new File("ClearMusic", FileType.exe),
            new File("ClearVideo", FileType.exe),
            new File("ClearFax", FileType.exe),
            new File("ClearNet", FileType.exe),
            new File("HelpHub", FileType.exe),
            new File("Calculator", FileType.exe),
            new File("File_Utilities", FileType.exe)
        ),
        #endregion

        new Directory("Programs", //TODO: fill this folder
            new Directory("") 
        ),

        new Directory("Development", //TODO: fill this folder
            new Directory("Alex"),
            new Directory("Chris"),
            new Directory("Oliver",
                new File("Note_from_Alex", FileType.txt)
            ),
            new Directory("Luke")
        ),

        new File("game", FileType.exe),
        new File("System_Log", FileType.txt) //TODO: create this file and edit with logs
        
        //TODO: create more files

        //TODO: create an ending system that allows for multiple endings
        //note - certain endings are true endings, meaning they could be the cannonical end
        //current endings planned (more to be added):
        //      - oblivious ending: delete the AI file before ever running it
        //      - stupid ending: delete init.sys and brick the computer
        // true - neutral ending: delete Repair_OS.exe, this is where the mystery ends
        // true - bad ending: run net_access and let the AI out onto the internet
        //      and more to come...

        //TODO: add support for input while the console is being written to
        //TODO: create a more indepth settings men
        //TODO: create a combat hud that works with scrolling (still displays input at the very bottom)

    );

    void Awake(){
        SaveData.Load();
        Main = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        universalCommands = new Command[] {
            new Command("clear", "clear the console", new Type[]{}, o => {
                return Task.Run( () => {
                    clearConsole();
                });
            }),
            new Command("help", "list all commands", new Type[]{}, o => {
                return Task.Run( async () => {
                    foreach(Command c in currentCommands)
                        await AddText(FillSpace(c.Name, 8)+" - "+c.Description, 8);
                });
            }),
            new Command("keys", "list all keybinds that can be used in the console", new Type[]{}, o => {
                return Task.Run( async () => {
                    (string, string)[] keys = new (string, string)[] {
                        ("pageup/pagedown", "scroll up or down the console"),
                        ("up/down", "cycle through previous inputs"),
                    };
                    foreach((string, string) s in keys)
                        await AddText(FillSpace(s.Item1, 16)+" - "+s.Item2, 8);
                });
            }),
            new Command("shutdown", "turn off the computer", new Type[]{}, o => {
                return Task.Run( async () => {
                    startQuit();
                    GetComponents<AudioSource>()[2].Play();
                    GetComponents<AudioSource>()[1].Stop();
                    GetComponents<AudioSource>()[4].Stop();
                    await AddText("Shutting down...");
                    await Task.Delay(5000);
                });
            }),
            // new Command("volume", "change the volume of the computer", new Type[]{typeof(Optional<string>)}, o => { //TODO: this
            //     return Task.Run( async () => {
            //         if(o[0] == null){
            //             int volume = 10;
            //             if(PlayerPrefs.HasKey("Volume")) volume = PlayerPrefs.GetInt("Volume");
            //             await AddText("Volume currently set to "+volume);
            //             return;
            //         }
            //         try{
            //             int volume = int.Parse((string)o[0]);
            //             if(volume < 0 || volume > 10) throw new Exception();

            //             PlayerPrefs.SetInt("Volume", volume);
            //             float subtract = volume == 0 ? 85 : 10 - 10*Mathf.Log10(volume);
            //             mixer.SetFloat("Volume", 5 - subtract);
            //             await AddText("Volume set to "+volume);
            //         } catch(Exception e){
            //             await AddText("Command input must be a number between 0 and 10");
            //             return;
            //         }
            //     });
            // }),
        };
        dosCommands = new Command[] {
            new Command("delete", "delete a file", new Type[]{typeof(string)}, o => {
                return Task.Run( async () => {
                    Storable s = new List<Storable>(currentDirectory.Files).Find(a => a.Name.ToLower() == ((string)o[0]).ToLower());
                    if(s == null){
                        await AddText("No file with the specified name found");
                        return;
                    }
                    if(s.Administrator){
                        await AddText("Access denied, Administrator privledges required");
                        return;
                    }
                    if(processes.Find(a => a.BindingFile == s.GetFullLocation()) != null){
                        await AddText("Error: file is currently in use by another process");
                        return;
                    }
                    SaveData.Instance.AddRemovedFiles(s.GetFullLocation());
                    currentDirectory.RemoveFile(s);
                    await AddText("File deleted");
                });
            }),
            new Command("dir", "enter a new directory", new Type[]{typeof(string)}, o => {
                return Task.Run( async () => {
                    Directory sub = new List<Directory>(currentDirectory.Subdirectories).Find(a => a.Name.ToLower() == ((string)o[0]).ToLower());
                    if(sub == null){
                        await AddText("No such directory exists");
                        return;
                    }
                    if(sub.Name == "System"){
                        await AddText("Access denied, Administrator privledges required");
                        return;
                    }

                    currentDirectory = sub;
                    await AddText("Directory changed to "+currentDirectory.GetFullLocation()+"/");
                });
            }),
            new Command("kill", "kill a process using a process ID", new Type[]{typeof(int)}, o => {
                return Task.Run( async () => {
                    int ID = (int)o[0];
                    if(ID < 0 || ID >= processes.Count){
                        await AddText("Command input must be a valid process ID");
                        return;
                    }
                    Process p = processes[ID];
                    processes.RemoveAt(ID);
                    switch(p.Name){ //handle killing processes
                        case "Net_Clean":
                            Task t = Task.Run(async () => {
                            await Wait(5000);
                            processes.Add(p);
                        });
                        break;
                        case "insert_AI_name_here":
                            //TODO: this
                        break;
                    }
                    await AddText("Process "+ID+" killed");
                });
            }),
            new Command("list", "list all files in the current directory", new Type[]{}, o => {
                return Task.Run( async () => {
                    if(currentDirectory.Files.Length == 0){
                        await AddText("No files found");
                        return;
                    }

                    List<Storable> files = new List<Storable>(currentDirectory.Files);
                    files.Sort((a,b) => string.Compare(a.Name,b.Name) + (a is Directory ? -1000 : 0) + (b is Directory ? 1000 : 0));
                    await AddText("Current directory: "+currentDirectory.GetFullLocation()+"/", 6);
                    await AddText(FillSpace("File name", 16) + "File type", 8);
                    await AddText("--------------------------", 8);
                    await Pause(500);
                    foreach(Storable s in files)
                        await AddText(FillSpace(s.Name, 16) + (s is Directory ? "" : ((File)s).Type.ToString()), 8);
                });
            }),
            new Command("lv", "leave the current directory", new Type[]{}, o => {
                return Task.Run( async () => {
                    if(currentDirectory.Container == null){
                        await AddText("Error: current directory is root directory");
                        return;
                    }
                    currentDirectory = currentDirectory.Container;
                    await AddText("Directory changed to "+currentDirectory.GetFullLocation()+"/");
                });
            }),
            new Command("proc", "list all processes currently running", new Type[]{}, o => {
                return Task.Run( async () => {
                    if(processes.Count == 0){
                        await AddText("No processes currently running");
                        return;
                    }

                    await AddText(FillSpace("ID", 4)+FillSpace("Process Name", 16) + "Binding File", 8);
                    await AddText("--------------------------------------------", 8);
                    await Pause(500);
                    for(int i = 0; i < processes.Count; i++)
                        await AddText(FillSpace(i+"", 4) + FillSpace(processes[i].Name, 16) + processes[i].BindingFile, 8);
                });
            }),
            new Command("read", "read a txt file to the console", new Type[]{typeof(string)}, o => {
                return Task.Run( async () => {
                    Storable s = new List<Storable>(currentDirectory.Files).Find(a => a.Name.ToLower() == ((string)o[0]).ToLower());
                    if(s == null){
                        await AddText("No file with the specified name found");
                        return;
                    }
                    if(s is Directory || ((File)s).Type != FileType.txt){
                        await AddText("Error: file found is not of type txt");
                        return;
                    }
                    if(processes.Find(a => a.BindingFile == s.GetFullLocation()) != null){
                        await AddText("Error: file is currently in use by another process");
                        return;
                    }
                    // await AddText(Resources.Load<TextAsset>("Discoverables/"+s.GetLocation().Substring(3)+s.Name).text, 8);
                    await AddText(Resources.Load<TextAsset>("Discoverables/"+s.Name).text, 8);
                });
            }),
            new Command("run", "run an exe file", new Type[]{typeof(string)}, o => {
                return Task.Run( async () => {
                    Storable s = new List<Storable>(currentDirectory.Files).Find(a => a.Name.ToLower() == ((string)o[0]).ToLower());
                    if(s == null){
                        await AddText("No file with the specified name found");
                        return;
                    }
                    if(s is Directory || ((File)s).Type != FileType.exe){
                        await AddText("Error: file found is not of type exe");
                        return;
                    }
                    if(processes.Find(a => a.BindingFile == s.GetFullLocation()) != null){
                        await AddText("Error: file is currently in use by another process");
                        return;
                    }

                    if(s.Container.Name == "Clearnet"){
                        await Pause(2000);
                        await AddText("Error: Clearnet OS not running");
                        return;
                    }
                    switch(s.Name){
                        case "game":
                            Process p = new Process("insertNameOfAI", s.GetFullLocation());
                            processes.Add(p);
                            await Pause(2000);
                            await GameplayManager.InitalizeAndRun();
                        break;
                        //TODO: handle running other EXEs
                    }
                });
            }),
        };
        gameCommands = new Command[] {
            Attack.Get().GetCommand(),
            Block.Get().GetCommand(),
            Use.Get().GetCommand(),
            new Command("define", "define a term, such as \"block\"", new Type[]{typeof(string)}, o => {
                return Task.Run( async () => {
                    string term = ((string)o[0]).ToLower();
                    switch(term){
                        case "hp": case "health":
                        await AddText("This is how healthy you are. If an enemy is reduced to 0 hp, they are removed from combat. If you are reduced to 0 hp, you lose the combat.");
                        break;
                        case "sanity":
                        await AddText("This is how sane you are. If this is ever reduced to 0, you instantly lose and the run is over.");
                        break;
                        case "ration": case "rations":
                        await AddText("This is how much food you have. Each day at dawn, you eat one ration. If dawn passes and you have no rations, instead you lose one sanity.");
                        break;
                        case "boss": case "bosses":
                        await AddText("A boss is a powerful enemy you encounter every 5 days. Unlike normal combat, if you fail this encounter the run is immediately over");
                        break;

                        case "block":
                        await AddText("When you would take damage from an attack, instead your block is reduced by the same amount. Block lasts 2 turns");
                        break;
                        case "dodge":
                        await AddText("If you would be attacked, instead you are not and dodge is reduced by 1. Dodge lasts 1 turn");
                        break;
                        case "stun":
                        await AddText("Each stun has a duration. If you are stunned, you skip your turn");
                        break;
                        case "bleed":
                        await AddText("Every turn, take damage equal to your bleed. Bleed lasts until the end of combat");
                        break;


                        case "equip": case "equipment":
                        await AddText("A peice of gear that gives you some type of advantage. You can only use one of each type of equipment once. There are two types of equipment, weapons and outfits");
                        break;
                        case "weapon": case "weapons":
                        await AddText("A weapon for you to use in combat. It is a type of equipment");
                        break;
                        case "outfit": case "outfits":
                        await AddText("An outfit for you to wear on your adventures. It is a type of equipment");
                        break;
                        case "skill": case "skills":
                        await AddText("A move that you have learned in training. These can only be used in combat and require a turn to use");
                        break;
                        case "item": case "items":
                        await AddText("A special item that gives you some type of advantage. There is a maximum number of items you can hold at once. There are two types of items, passives and consumables");
                        break;
                        case "passive": case "passives":
                        await AddText("An item that gives you some passive buff. These are always active as long as they are in your inventory");
                        break;
                        case "consumable": case "consumables":
                        await AddText("An item that has a specific effect when used. Each consumable has a number of uses before it is removed from your inventory");
                        break;

                        default:
                        await AddText("Unknown term \""+term+"\"");
                        break;
                    }
                });
            }),
            new Command("list", "list all gear in your inventory", new Type[]{}, o => {
                return Task.Run( async () => {
                    Player p = GameplayManager.Self;
                    if(p.Weapon == null) await AddText("\nWeapon:\nNone");
                    else await AddText("\nWeapon:\n"+p.Weapon.ToString());
                    if(p.Outfit == null) await AddText("\nOutfit:\nNone");
                    else await AddText("\nOutfit:\n"+p.Outfit.ToString());
                    
                    await AddText("\nItems:");
                    foreach(Item i in p.Items) await AddText(i.ToString());
                    if(p.Items.Length == 0) await AddText("None");
                });
            }),
            new Command("stats", "list all stats, can be used in and out of combat", new Type[]{}, o => {
                return Task.Run( async () => {
                    Player p = GameplayManager.Self;
                    if(BattleManager.Battle == null){
                        await AddText("Items: "+p.Items.Length+"/"+p.MaxItems);
                        await AddText("Sanity: "+p.Sanity+"/"+p.MaxSanity);
                        await AddText("Rations: "+p.Rations);
                    }
                    else{
                        throw new NotImplementedException();
                        //TODO: this
                    }
                });
            }),
            new Command("intents", "list all enemy intents", new Type[]{}, o => {
                return Task.Run( async () => {
                    if(BattleManager.Battle == null){
                        await AddText("Command can only be used in combat");
                        return;
                    }
                    foreach(Enemy e in BattleManager.Battle.Enemies)
                        await AddText(e.Name+" - "+e.Intention.Description);
                });
            }),
        };
        SetCommands();

        if(SaveData.Instance.GameStage == 0){
            //TODO: create a tutorial for learning how to input commands into the console
            SaveData.Instance.GameStage = 1;
            StartCoroutine(StartScreenFade(handleStart));
        }
        else handleStart();
    }
    void handleStart(){
        if(!SaveData.Instance.ContainsRemovedFile("A:/Clearnet/System/Net.sys")) processes.Add(new Process("Net_Clean", "A:/Clearnet/System/Net.sys", null, "A:/Clearnet/System/Net_access.exe"));
        //TODO: add binding file for the AI exe
        
        foreach(string f in SaveData.Instance.RemovedFiles){
            string[] names = f.Split('/');
            if(names[0] != currentDirectory.Name) throw new Exception("Removed file \""+f+"\" is not inside of root directory \""+currentDirectory.Name+"\"");

            int fileDepth = 1;
            Directory container = currentDirectory;
            File output = null;
            do{
                Storable s = new List<Storable>(container.Files).Find(a => a.Name == names[fileDepth]);
                if(s == null) break;
                else if(s is File){
                    if(fileDepth + 1 == names.Length) output = (File)s;
                    else break;
                }
                else container = (Directory)s;
            } while(++fileDepth < names.Length);

            if(output != null) output.Container.RemoveFile(output);
        }

        switch(SaveData.Instance.GameStage){
            case 1:
                StartConsole();
            break;
            //TODO: add support for starting from gamestage 2
        }
    }

    IEnumerator StartScreenFade(Action callback){
        Transform startupScreen = GameRedisplay.transform.parent.GetChild(2);
        startupScreen.gameObject.SetActive(true);

        float startTime = Time.time;
        Func<float> percent = () => (Time.time - startTime) / 1.75f;
        while(percent() < 1){
            yield return null;
            Color c = startupScreen.GetChild(2).GetComponent<Image>().color;
            c.a = 1 - percent();
            startupScreen.GetChild(2).GetComponent<Image>().color = c;
        }

        yield return new WaitForSeconds(5);

        startTime = Time.time;
        while(percent() < 1){
            yield return null;
            Color c = startupScreen.GetChild(2).GetComponent<Image>().color;
            c.a = percent();
            startupScreen.GetChild(2).GetComponent<Image>().color = c;
        }

        startupScreen.gameObject.SetActive(false);
        callback();
    }

    void StartConsole(){
        GetComponents<AudioSource>()[0].Play();
        if(PlayerPrefs.HasKey("Volume")){
            int volume = PlayerPrefs.GetInt("Volume");
            float subtract = volume == 0 ? 85 : 10 - 10*Mathf.Log10(volume);
            mixer.SetFloat("Volume", 5 - subtract);
        }
        GetComponents<AudioSource>()[1].PlayScheduled(AudioSettings.dspTime+5.27f);

        Console = GetComponent<Text>();
        remakeRenderTexture();

        CursorFlash();
        MainLoop();
    }

    public bool xRenderGlitch = false;
    public bool yRenderGlitch = false;
    public void remakeRenderTexture(){
        if(cameraTexture != null){
            cameraTexture.Release();
        }

        Vector2 size = RerenderCamera.pixelRect.size;
        cameraTexture = new RenderTexture((int)size.x, (int)size.y, 16, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
        Console.fontSize = (int)(size.y * .017f) + 11;

        Camera.main.targetTexture = cameraTexture;
        GameRedisplay.texture = cameraTexture;
        GameRedisplay.SetNativeSize();

        //TODO: adjust 4 corners
        float width1 = size.x/3;
        float height1 = size.y/3 * 2;
        float width2 = size.x - width1;
        float height2 = size.y - height1;
        for(int i = 0; i < 4; i++) {
            GameObject corner = GameRedisplay.transform.parent.GetChild(1).GetChild(i).gameObject;
            corner.GetComponent<RawImage>().texture = cameraTexture;
            switch(i){
            case 0:
                corner.GetComponent<RectTransform>().anchoredPosition = new Vector2(width1/2, height1/2);
                corner.GetComponent<RectTransform>().sizeDelta = new Vector2(width1, height1);
                corner.GetComponent<RawImage>().uvRect = new Rect(0 + (xRenderGlitch?-.02f:0), 0, width1/size.x, height1/size.y);
            break;
            case 1:
                corner.GetComponent<RectTransform>().anchoredPosition = new Vector2(-width2/2, height1/2);
                corner.GetComponent<RectTransform>().sizeDelta = new Vector2(width2, height1);
                corner.GetComponent<RawImage>().uvRect = new Rect(width1/size.x, 0, width2/size.x, height1/size.y);
            break;
            case 2:
                corner.GetComponent<RectTransform>().anchoredPosition = new Vector2(width1/2, -height2/2);
                corner.GetComponent<RectTransform>().sizeDelta = new Vector2(width1, height2);
                corner.GetComponent<RawImage>().uvRect = new Rect(0 + (xRenderGlitch?-.02f:0), height1/size.y + (yRenderGlitch?.02f:0), width1/size.x, height2/size.y);
            break;
            case 3:
                corner.GetComponent<RectTransform>().anchoredPosition = new Vector2(-width2/2, -height2/2);
                corner.GetComponent<RectTransform>().sizeDelta = new Vector2(width2, height2);
                corner.GetComponent<RawImage>().uvRect = new Rect(width1/size.x, height1/size.y + (yRenderGlitch?.02f:0), width2/size.x, height2/size.y);
            break;
            default: break;
            }
        }
    }
    #endregion

    #region console
    [HideInInspector]
    public bool freeze = false;
    bool updateText = true;
    // Update is called once per frame
    void Update()
    {
        if(cameraTexture == null) return;

        while(freeze){}

        for(int i = 0; i < ToBeRun.Count; i++)
            ToBeRun[i]();
        ToBeRun.Clear();

        if(RerenderCamera.pixelRect.size != new Vector2(cameraTexture.width, cameraTexture.height)) remakeRenderTexture();

        if(updateText){
            int consoleMaxLines = GetMaxLineCount(GetComponent<Text>())-1;
            Console.text = "\n" + GetFittingText(consoleText, Console, consoleMaxLines, linesToScroll);
            linesToScroll = 0;
        }
    }

    static int consoleStartIndex = 0;
    static int consoleEndIndex = 0;
    static bool followingEnd = true;
    public static string GetFittingText(string s, Text t, int idealLineNumber, int backtrackLineCount){
        var textGenerator = new TextGenerator();
        var generationSettings = t.GetGenerationSettings(t.rectTransform.rect.size);

        //make sure the textbox can fit all of s
        textGenerator.Populate(s, generationSettings);
        if(textGenerator.lineCount <= idealLineNumber){
            consoleStartIndex = 0;
            consoleEndIndex = s.Length;
            backtrackLineCount = 0;
            followingEnd = true;
            return s;
        }
        if(followingEnd && consoleEndIndex != s.Length) consoleEndIndex = s.Length;
        if(consoleEndIndex > s.Length) consoleEndIndex = s.Length;

        //removes extra lines if it can't fit all of them
        textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
        if(textGenerator.lineCount > idealLineNumber) while(true){
            consoleStartIndex++;
            textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
            if(textGenerator.lineCount == idealLineNumber) break;
        }

        
        if(backtrackLineCount == 0){ //fills up the displayed text as much as possible
            while(true){
                if(consoleStartIndex == 0) break;
                textGenerator.Populate(s.Substring(consoleStartIndex-1, consoleEndIndex - consoleStartIndex), generationSettings);
                if(textGenerator.lineCount >= idealLineNumber) break;
                consoleStartIndex--;
            }
            while(true){
                if(consoleEndIndex == s.Length) break;
                textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex+1), generationSettings);
                if(textGenerator.lineCount >= idealLineNumber) break;
                consoleEndIndex++;
            }
        }
        else if(backtrackLineCount > 0){ //scroll to beginning
            followingEnd = false;
            for(int i = 0; i < backtrackLineCount; i++){
                int lastStartIndex = consoleStartIndex;
                int lastEndIndex = consoleEndIndex;
                while(true){
                    consoleEndIndex--;
                    textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
                    if(textGenerator.lineCount == idealLineNumber - 1) break;
                }
                while(true){
                    consoleStartIndex--;
                    if(consoleStartIndex == -1) break;
                    textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
                    if(textGenerator.lineCount == idealLineNumber+1){
                        consoleStartIndex++;
                        break;
                    }
                }
                if(consoleStartIndex == -1){
                    consoleStartIndex = lastStartIndex;
                    consoleEndIndex = lastEndIndex;
                    break;
                }
            }
        }
        else{ //scroll to end
            for(int i = 0; i < -backtrackLineCount; i++){
                int lastStartIndex = consoleStartIndex;
                int lastEndIndex = consoleEndIndex;

                while(true){
                    consoleStartIndex++;
                    textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
                    if(textGenerator.lineCount == idealLineNumber - 1) break;
                }
                while(true){
                    consoleEndIndex++;
                    if(consoleEndIndex == s.Length+1) break;
                    textGenerator.Populate(s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex), generationSettings);
                    if(textGenerator.lineCount == idealLineNumber+1){
                        consoleEndIndex--;
                        break;
                    }
                }
                if(consoleEndIndex == s.Length+1){
                    consoleEndIndex = lastEndIndex;
                    consoleStartIndex = lastStartIndex;
                    followingEnd = true;
                    break;
                }
            }
        }

        return s.Substring(consoleStartIndex, consoleEndIndex - consoleStartIndex);
    }

    public static int GetMaxLineCount(Text text){
        var textGenerator = new TextGenerator();
        var generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
        var lineCount = 0;
        string s = "";
        while (true)
        {
            s += '\n';
            textGenerator.Populate(s, generationSettings);
            var nextLineCount = textGenerator.lineCount;
            if (lineCount == nextLineCount) break;
            lineCount = nextLineCount;
        }
        return lineCount;
    }

    void OnGUI(){
        Event e = Event.current;
        if(e.isKey && e.type == EventType.KeyDown)
            interpretInput(new MarkupKeyCode(e.keyCode, e.shift, e.control || e.command, e.alt));
    }

    void interpretInput(MarkupKeyCode k){
        if(!updateText) return;

        switch(k.KeyCode){
        case KeyCode.PageDown:
            if(k.Ctrl) linesToScroll = int.MinValue+1;
            else if(k.Shift) linesToScroll -= 20;
            else linesToScroll--;
        break;
        case KeyCode.PageUp:
            if(k.Ctrl) linesToScroll = int.MaxValue-1;
            else if(k.Shift) linesToScroll += 20;
            else linesToScroll++;
        break;
        default:
            if(followingEnd) InputKeyStack.Add(k);
        break;
        }
    }

    List<Process> processes = new List<Process>();
    async void MainLoop(){
        await AddText("Loading init.sys 0%", false);
        await WaitForPercentage(consoleText.Length-2, t=>(t*1.5f));
        await AddText("BIOS ROM v 0.9.36");
        await AddText("Clearnet Ltd.");
        await AddText("Copyright (C) 1994");
        await AddText("All rights reserved.");
        await Pause(1100);
        await AddText("\nDOS v 0.2.1");
        await AddText("Clearnet Ltd.");
        await AddText("Copyright (C) 1994");
        await AddText("All rights reserved.");
        await Pause(600);
        while(true){
            await GetAndRunCommand();
        }
    }
    public Task<T> PromptChoice<T>(params T[] options){
        return PromptChoice(true, options);
    }
    public Task<T> PromptChoice<T>(bool handleCommands, params T[] options){
        return Task.Run<T>(async () => {
            string input = "";
            List<string> opts = new List<string>();
            string errString = "Input needs to be ";
            string promptString = "Choose:";
            for(int i = 0; i < options.Length; i++){
                opts.Add(options[i].ToString());
                if(options[i] is Gear) promptString += "\n" + (options[i] as Gear).ToFullString();
                else promptString += "\n" + options[i].ToString();
                if(i + 1 != options.Length) errString += options[i].ToString()+", ";
                else errString += "or " + options[i].ToString();
            }
            
            await AddText("\n"+promptString+"\n\n");
            while(true){
                await AddText("?");
                input = await GetInput(20, false);

                int index = opts.FindIndex(s => s.ToLower() == input.ToLower());
                if(index != -1) return options[index];
                else if(handleCommands && IsCommand(input)) await RunCommand(input);
                else await AddText(errString+"\n\n");
            }
        });
    }
    public bool IsCommand(string rawInput){
        string[] input = rawInput.Split(' ');
        return currentCommands.Find(a => a.Name == input[0]) != null;
    }
    public Task<Command> GetAndRunCommand(){
        return Task.Run<Command>( async () => {
            //get input
            await AddText("\n\n");
            return await RunCommand(await GetInput());
        });
    }
    public Task<Command> RunCommand(string rawInput){
        return Task.Run<Command>( async () => {
            //parse input
            string[] input = rawInput.Split(' ');

            //get command
            Command c = currentCommands.Find(a => a.Name == input[0]);
            if(c == null){
                if(input[0] != "") await AddText("Command \""+input[0]+"\" not recognized. Use command \"help\" for list of commands");
                return null;
            }
            else if(c.Move != null && BattleManager.Battle == null){
                await AddText("Command can only be used in combat");
                return null;
            }
            else{ //remove the command name
                List<string> parameters = new List<string>(input);
                parameters.RemoveAt(0);
                input = parameters.ToArray();
            }

            //get minimum input length
            Type optionalBase = typeof(Optional<int>).GetGenericTypeDefinition();
            int inputMinSize = c.Input.Length;
            for(; inputMinSize > 0; inputMinSize--)
                if(!(c.Input[inputMinSize-1].IsGenericType && c.Input[inputMinSize-1].GetGenericTypeDefinition() == optionalBase))
                    break;
            
            //throw error if input is not minimum length
            if(input.Length < inputMinSize){
                await AddText("Command \""+c.Name+"\" requires at least "+inputMinSize+" input" + (inputMinSize != 1 ? "s" : ""));
                return null;
            }
            
            //format all input
            object[] commandInput = new object[c.Input.Length];
            for(int i = 0; i < input.Length; i++){
                Type inputType = c.Input[i].IsGenericType ? c.Input[i].GetGenericArguments()[0] : c.Input[i];
                if(!FormatInput(input[i], inputType, out commandInput[i])){
                    string interpretedType = "";
                    if(inputType == typeof(string)) interpretedType = "text";
                    else if(inputType == typeof(int)) interpretedType = "a number";
                    else if(inputType == typeof(Unit)) interpretedType = "an enemy ID";
                    else if(inputType == typeof(Item)) interpretedType = "the name of an item in your inventory";
                    else if(inputType == typeof(Consumable)) interpretedType = "the name of a consumable item in your inventory";
                    await AddText("Input "+i+" needs to be "+interpretedType);
                    return null;
                }
            }

            //run the command
            await c.Run(commandInput);
            return c;
        });
    }

    bool FormatInput(string s, Type outType, out object result){
        result = null;
        if(outType == typeof(string)){
            result = s;
            return true;
        }
        if(outType == typeof(int)){
            int i;
            if(int.TryParse(s, out i)){
                result = i;
                return true;
            }
            else return false;
        }
        if(outType == typeof(Unit)){
            throw new NotImplementedException("Unable to format input to a Unit");
            //TODO: this
        }
        if(outType == typeof(Item)){
            Item i = new List<Item>(GameplayManager.Self.Items).Find(item => item.Name.ToLower() == s.ToLower());
            if(i == null) return false;
            result = i;
            return true;
        }
        if(outType == typeof(Consumable)){
            Item i = new List<Item>(GameplayManager.Self.Items).Find(item => item.Name.ToLower() == s.ToLower());
            if(i == null) return false;
            if(i.Type != ItemType.Consumable) return false;
            result = (Consumable)i;
            return true;
        }

        throw new Exception("Type "+outType+" not supported for input formatting");
    }

    public List<Action> ToBeRun = new List<Action>();
    #endregion

    #region Async Functions
    void clearKeyStack(){
        InputKeyStack.Clear();
    }
    public void clearConsole(){
        consoleText = "\n";
        linesToScroll = 0;
        consoleStartIndex = 0;
        consoleEndIndex = 0;
    }
    async void startQuit(){
        await Task.Delay(3000);
        Application.Quit();
    } 

    public string FillSpace(string input, int desiredLength){
        string output = input;
        for(int i = 0; i < (desiredLength - input.Length); i++)
            output += " ";
        return output;
    }

    public Task ShowTitle(string title, int alphaStepDelay = 200){
        return Task.Run( async () => {
            updateText = false;
            string text = consoleText;
            ToBeRun.Add(() => Console.text = "");
            consoleText = "";
            int fontsize = Console.fontSize;
            TextAnchor alignment = Console.alignment;
            ToBeRun.Add(() => Console.fontSize = fontsize * 3);
            ToBeRun.Add(() => Console.alignment = TextAnchor.MiddleCenter);
            

            Color c = Color.white;
            c.a = 0;
            ToBeRun.Add(() => Console.color = c);
            ToBeRun.Add(() => Console.text = title);

            const int stageLength = 3000;
            int stageCounter = 0;
            while(stageCounter < stageLength){
                c.a = (float)stageCounter / (float)stageLength;
                ToBeRun.Add(() => Console.color = c);
                await Wait(alphaStepDelay);
                stageCounter += alphaStepDelay;
            }
            c.a = 1;
            ToBeRun.Add(() => Console.color = c);
            await Wait(alphaStepDelay);

            await Wait(stageLength);

            stageCounter = 0;
            while(stageCounter < stageLength){
                c.a = 1f - ((float)stageCounter / (float)stageLength);
                ToBeRun.Add(() => Console.color = c);
                await Wait(alphaStepDelay);
                stageCounter += alphaStepDelay;
            }
            c.a = 0;
            ToBeRun.Add(() => Console.color = c);
            await Wait(alphaStepDelay);

            c.a = 1f;
            ToBeRun.Add(() => {
                Console.color = c;
                consoleText = text;
                Console.fontSize = fontsize;
                Console.alignment = alignment;
                updateText = true;
            });

            while(!updateText);
        });
    }
    public Task AddCreepyText(string s, int wordSpeed, int spaceSpeed = 250){
        return Task.Run(async () => {
            string[] words = s.Split(' ');
            for(int i = 0; i < words.Length; i++){
                if(i == 0) await AddText(words[i]+" ", wordSpeed);
                else if(i == words.Length - 1) await AddText(words[i], false, wordSpeed);
                else await AddText(words[i]+" ", false, wordSpeed);
                
                await Wait(spaceSpeed);
            }
        });
    }
    public Task AddText(string text, int charDelay){
        return AddText(text, true, charDelay);
    }
    public Task AddText(string text, bool addNewLine = true, int charDelay = 15){
        return Task.Run( async () => {
            if(text.Length == 0) return;
            bool cursorWasFlashing = cursorFlashEnabled;
            SetCursorFlash(false);

            if(addNewLine && consoleText[consoleText.Length-1] != '\n') consoleText += "\n";
            for(int i = 0; i < text.Length; i++){
                if(InputKeyStack.Find(k => (k.KeyCode == KeyCode.KeypadEnter || k.KeyCode == KeyCode.Return)) != null){
                    consoleText += text.Substring(i);
                    break;
                }

                consoleText += text[i];
                if(i == text.Length) break;
                await Task.Delay(charDelay);
            }
            SetCursorFlash(cursorWasFlashing);
        });
    }

    List<MarkupKeyCode> InputKeyStack = new List<MarkupKeyCode>();
    public Task<string> GetInput(int inputStepDelay = 20, bool newline = true){
        return Task.Run<string>( async () => {
            await AddText("> ", newline);
            SetCursorFlash(true);

            string input = "";
            while(true){
                if(InputKeyStack.Count > 0){
                    MarkupKeyCode c = InputKeyStack[0];
                    InputKeyStack.RemoveAt(0);

                    bool enter = c.KeyCode == KeyCode.KeypadEnter || c.KeyCode == KeyCode.Return;
                    bool backspace = c.KeyCode == KeyCode.Backspace;
                    bool verticalKeys = c.KeyCode == KeyCode.UpArrow || c.KeyCode == KeyCode.DownArrow;
                    if(verticalKeys || enter || (backspace && input.Length > 0) || MarkupKeyCode.KeycodeToChar.ContainsKey(c.KeyCode)){
                        SetCursorFlash(false);
                        if(enter) {
                            commands.Insert(0, input);
                            commandIndex = -1;
                            return input;
                        }
                        else if(backspace){
                            input = input.Substring(0, input.Length-1);
                            consoleText = consoleText.Substring(0, consoleText.Length-1);
                        }
                        else if(verticalKeys){
                            bool down = c.KeyCode == KeyCode.DownArrow;
                            if((commandIndex == -1 && down) || (commandIndex == commands.Count - 1 && !down)){}//nothing
                            else{
                                commandIndex += (down ? -1 : 1);
                                string newInput;
                                if(commandIndex == -1) newInput = "";
                                else newInput = commands[commandIndex];

                                consoleText = consoleText.Substring(0, consoleText.Length - input.Length);
                                await AddText(newInput, false);
                                input = newInput;
                            }
                        }
                        else{
                            char character = MarkupKeyCode.KeycodeToChar[c.KeyCode];
                            if(c.Shift) character = MarkupKeyCode.CharToUpper(character);

                            input += character;
                            consoleText += character;
                        }
                    }
                }
                SetCursorFlash(true);
                await Task.Delay(inputStepDelay);
            }
        });
    }
    public Task WaitForEnter(int inputStepDelay = 20){
        return Task.Run( async () => {
            string prompt = "\nPress enter to continue...";
            await AddText(prompt);
            SetCursorFlash(true);

            while(true){
                bool b = false;
                while(InputKeyStack.Count > 0){
                    MarkupKeyCode k = InputKeyStack[0];
                    InputKeyStack.RemoveAt(0);
                    if(k.KeyCode == KeyCode.KeypadEnter || k.KeyCode == KeyCode.Return){
                        b = true;
                        break;
                    }
                }
                if(b) break;
                await Task.Delay(inputStepDelay);
            }
            SetCursorFlash(false);

            consoleText = consoleText.Substring(0, consoleText.Length - prompt.Length);
        });
    }
    public Task Pause(int pauseDuration, int flashStepDelay = 250){
        return Task.Run( async () => {
            if(pauseDuration <= 0) return;

            int remainingDuration = pauseDuration;
            int dots = 0;
            int maxDots = 3;
            if(consoleText[consoleText.Length-1] != '\n')
                consoleText += "\n";

            while(true){
                int waitDuration = remainingDuration > flashStepDelay ? flashStepDelay : remainingDuration;
                remainingDuration -= waitDuration;

                await Task.Delay(flashStepDelay);
                if(remainingDuration <= 0) break;

                if(dots == maxDots){
                    consoleText = consoleText.Substring(0, consoleText.Length-(maxDots*2));
                    dots = 0;
                }
                else{
                    dots++;
                    consoleText += ". ";
                }
            }

            consoleText = consoleText.Substring(0, consoleText.Length-(dots*2));
        });
    }
    public Task Wait(int waitDuration){
        return Task.Delay(waitDuration);
    }

    public Task WaitForPercentage(int percentageTextIndex, Func<float, float> timeToProgress, int stepDelay = 20){
        return Task.Run( async () => {
            //this assumes percentageTextIndex is a valid index

            string before = consoleText.Substring(0, percentageTextIndex);
            string after = consoleText.Substring(percentageTextIndex+1, consoleText.Length - (percentageTextIndex + 1));
            float time = 0f;
            while(true){
                int progress = (int)(timeToProgress(time) * 100);
                if(progress > 100) progress = 100;
                consoleText = before + progress + after;

                if(progress == 100) return;
                await Task.Delay(stepDelay);
                time += stepDelay/1000f;
            }
        });
    }

    bool cursorFlashEnabled = false;
    bool cursorFlashShown = false;
    bool cursorFlashDelay = false;
    void SetCursorFlash(bool enabled){
        if(cursorFlashEnabled == enabled) return;
        cursorFlashEnabled = enabled;

        if(cursorFlashShown && !enabled){
            consoleText = consoleText.Substring(0, consoleText.Length - 1);
            cursorFlashShown = false;
        }

        cursorFlashDelay = true;
    }
    async void CursorFlash(){
        while(true){
            if(cursorFlashEnabled && !cursorFlashDelay){
                if(cursorFlashShown)
                    consoleText = consoleText.Substring(0, consoleText.Length - 1);
                else
                    consoleText += "▌";

                cursorFlashShown = !cursorFlashShown;
            }
            cursorFlashDelay = false;
            await Task.Delay(1000);
        }
    }
    #endregion

    #region subclasses
    abstract class Storable{
        public Directory Container{
            get{
                return container;
            }
        }
        Directory container;
        public string Name{
            get{
                return name;
            }
        }
        string name;
        public virtual bool Administrator{
            get{
                return administrator;
            }
        }
        protected bool administrator {get; private set;}
        
        protected Storable(string name){
            this.name = name;
        }
        protected Storable(string name, Directory container, bool administrator = false) : this(name){
            this.container = container;
            this.administrator = administrator;
        }

        public string GetLocation(){
            if(container == null) return "";
            else return container.getLocation("");
        }

        public string GetFullLocation(){
            return GetLocation()+name;
        }

        private string getLocation(string prevDir){
            prevDir = name + "/" + prevDir;
            if(container == null) return prevDir;
            else return container.getLocation(prevDir);
        }

        protected static void SetContainer(Storable s, Directory container){
            s.container = container;
        }
    }
    class Directory : Storable{
        public override bool Administrator{
            get{
                return base.administrator || cachedAdmin;
            }
        }
        bool cachedAdmin = false;
        void regenCachedAdmin(){
            bool prevCache = cachedAdmin;
            cachedAdmin = false;

            foreach(Storable s in files){
                if(s.Administrator){
                    cachedAdmin = true;
                    break;
                }
            }

            if(Container != null && prevCache != cachedAdmin) Container.regenCachedAdmin();
        }

        public Storable[] Files{
            get{
                return files.ToArray();
            }
        }
        List<Storable> files = new List<Storable>();
        public Directory[] Subdirectories{
            get{
                return subdirectories.ToArray();
            }
        }
        List<Directory> subdirectories = new List<Directory>();
        
        public void AddFile(Storable file){
            files.Add(file);
            SetContainer(file, this);
            if(file is Directory)
                subdirectories.Add((Directory)file);

            if(file.Administrator && !cachedAdmin) regenCachedAdmin();
        }
        public void RemoveFile(Storable file){
            files.Remove(file);
            SetContainer(file, null);
            if(file is Directory)
                subdirectories.Remove((Directory)file);
                
            if(file.Administrator && cachedAdmin) regenCachedAdmin();
        }

        public Directory(string name, params Storable[] files) : this(name, null, files){}
        public Directory(string name, bool administrator, params Storable[] files) : this(name, null, administrator, files){}
        public Directory(string name, Directory container, params Storable[] files) : this(name, container, false, files){}
        public Directory(string name, Directory container, bool administrator, params Storable[] files) : base(name, container, administrator){
            if(files != null) foreach(Storable s in files)
                AddFile(s);
        }
    }
    class File : Storable{
        public FileType Type{
            get{
                return type;
            }
        }
        FileType type;
        public File(string name, FileType type, bool administrator = false) : this(name, type, null, administrator){} 
        public File(string name, FileType type, Directory container, bool administrator = false) : base(name, container, administrator){
            this.type = type;
        }
    }
    enum FileType{
        //openables
        txt,
        jpg,
        avi,
        bit, //aka mp3

        //runables
        exe,

        //no use
        bin,
        sys,
        ttf,
        htm,
        drv,
        cmd,
    }
    class Process{
        public string Name{
            get{
                return name;
            }
        }
        string name;
        public string Origin{
            get{
                return origin;
            }
        }
        string origin;
        Action callback;
        public string BindingFile{
            get{
                return bindingFile;
            }
        }
        string bindingFile;

        public Process(string name, string origin, Action callback = null, string bindingFile = ""){
            this.name = name;
            this.origin = origin;
            this.callback = callback;
            this.bindingFile = bindingFile;
        }

        public void Kill(){
            if(callback != null) callback();
        }
        
    }
    class MarkupKeyCode{
        public KeyCode KeyCode{
            get{
                return keyCode;
            }
        }
        KeyCode keyCode;
        public bool Shift{
            get{
                return shift;
            }
        }
        bool shift;
        public bool Ctrl{
            get{
                return ctrl;
            }
        }
        bool ctrl;
        public bool Alt{
            get{
                return alt;
            }
        }
        bool alt;
        public MarkupKeyCode(KeyCode keyCode, bool shift = false, bool ctrl = false, bool alt = false){
            this.keyCode = keyCode;
            this.shift = shift;
            this.ctrl = ctrl;
            this.alt = alt;
        }
        public static char CharToUpper(char c){
            if(charToCharUpper.ContainsKey(c)) return charToCharUpper[c];
            else return c;
        }

        static Dictionary<char, char> charToCharUpper = new Dictionary<char, char>(){
            //Lower Case Letters
            {'a', 'A'},
            {'b', 'B'},
            {'c', 'C'},
            {'d', 'D'},
            {'e', 'E'},
            {'f', 'F'},
            {'g', 'G'},
            {'h', 'H'},
            {'i', 'I'},
            {'j', 'J'},
            {'k', 'K'},
            {'l', 'L'},
            {'m', 'M'},
            {'n', 'N'},
            {'o', 'O'},
            {'p', 'P'},
            {'q', 'Q'},
            {'r', 'R'},
            {'s', 'S'},
            {'t', 'T'},
            {'u', 'U'},
            {'v', 'V'},
            {'w', 'W'},
            {'x', 'X'},
            {'y', 'Y'},
            {'z', 'Z'},
            
            //KeyPad Numbers
            {'1', '!'},
            {'2', '@'},
            {'3', '#'},
            {'4', '$'},
            {'5', '%'},
            {'6', '^'},
            {'7', '&'},
            {'8', '*'},
            {'9', '('},
            {'0', ')'},
            
            //Other Symbols
            {'\'', '"'}, //remember the special forward slash rule... this isnt wrong
            {',', '<'},
            {'-', '_'},
            {'.', '>'},
            {'/', '?'},
            {';', ':'},
            {'=', '+'},
            {'[', '{'},
            {'\\', '|'}, //remember the special forward slash rule... this isnt wrong
            {']', '}'},
            {'`', '~'},
        };
        public static Dictionary<KeyCode, char> KeycodeToChar = new Dictionary<KeyCode, char>(){
            //Lower Case Letters
            {KeyCode.A, 'a'},
            {KeyCode.B, 'b'},
            {KeyCode.C, 'c'},
            {KeyCode.D, 'd'},
            {KeyCode.E, 'e'},
            {KeyCode.F, 'f'},
            {KeyCode.G, 'g'},
            {KeyCode.H, 'h'},
            {KeyCode.I, 'i'},
            {KeyCode.J, 'j'},
            {KeyCode.K, 'k'},
            {KeyCode.L, 'l'},
            {KeyCode.M, 'm'},
            {KeyCode.N, 'n'},
            {KeyCode.O, 'o'},
            {KeyCode.P, 'p'},
            {KeyCode.Q, 'q'},
            {KeyCode.R, 'r'},
            {KeyCode.S, 's'},
            {KeyCode.T, 't'},
            {KeyCode.U, 'u'},
            {KeyCode.V, 'v'},
            {KeyCode.W, 'w'},
            {KeyCode.X, 'x'},
            {KeyCode.Y, 'y'},
            {KeyCode.Z, 'z'},
            
            //KeyPad Numbers
            {KeyCode.Keypad1, '1'},
            {KeyCode.Keypad2, '2'},
            {KeyCode.Keypad3, '3'},
            {KeyCode.Keypad4, '4'},
            {KeyCode.Keypad5, '5'},
            {KeyCode.Keypad6, '6'},
            {KeyCode.Keypad7, '7'},
            {KeyCode.Keypad8, '8'},
            {KeyCode.Keypad9, '9'},
            {KeyCode.Keypad0, '0'},
            
            //Other Symbols
            {KeyCode.Exclaim, '!'}, //1
            {KeyCode.DoubleQuote, '"'},
            {KeyCode.Hash, '#'}, //3
            {KeyCode.Dollar, '$'}, //4
            {KeyCode.Ampersand, '&'}, //7
            {KeyCode.Quote, '\''}, //remember the special forward slash rule... this isnt wrong
            {KeyCode.LeftParen, '('}, //9
            {KeyCode.RightParen, ')'}, //0
            {KeyCode.Asterisk, '*'}, //8
            {KeyCode.Plus, '+'},
            {KeyCode.Comma, ','},
            {KeyCode.Minus, '-'},
            {KeyCode.Period, '.'},
            {KeyCode.Slash, '/'},
            {KeyCode.Colon, ':'},
            {KeyCode.Semicolon, ';'},
            {KeyCode.Less, '<'},
            {KeyCode.Equals, '='},
            {KeyCode.Greater, '>'},
            {KeyCode.Question, '?'},
            {KeyCode.At, '@'}, //2
            {KeyCode.LeftBracket, '['},
            {KeyCode.Backslash, '\\'}, //remember the special forward slash rule... this isnt wrong
            {KeyCode.RightBracket, ']'},
            {KeyCode.Caret, '^'}, //6
            {KeyCode.Underscore, '_'},
            {KeyCode.BackQuote, '`'},
            {KeyCode.Space, ' '},
            
            //Alpha Numbers
            {KeyCode.Alpha1, '1'},
            {KeyCode.Alpha2, '2'},
            {KeyCode.Alpha3, '3'},
            {KeyCode.Alpha4, '4'},
            {KeyCode.Alpha5, '5'},
            {KeyCode.Alpha6, '6'},
            {KeyCode.Alpha7, '7'},
            {KeyCode.Alpha8, '8'},
            {KeyCode.Alpha9, '9'},
            {KeyCode.Alpha0, '0'},
            
            //Keypad Symbols
            {KeyCode.KeypadPeriod, '.'},
            {KeyCode.KeypadDivide, '/'},
            {KeyCode.KeypadMultiply, '*'},
            {KeyCode.KeypadMinus, '-'},
            {KeyCode.KeypadPlus, '+'},
            {KeyCode.KeypadEquals, '='},
            
            //-----KeyCodes that are inaccesible for some reason
            //{KeyCode.tilde, '~'},
            //{KeyCode.LeftCurlyBrace, '{'}, 
            //{KeyCode.RightCurlyBrace, '}'}, 
            //{KeyCode.Line, '|'},   
            //{KeyCode.percent, '%'},
        };
    }
    #endregion
}

public class Command{
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
    public Type[] Input{
        get{
            return input;
        }
    }
    Type[] input;
    Func<object[], Task> function;
    public Task Run(object[] input){
        return Task.Run( async () => {
            await function(input);
        });
    }
    public Move Move{
        get{
            return move;
        }
    }
    Move move;
    
    public Command(string name, string description, Type[] input, Func<object[], Task> function, Move move = null){
        this.name = name;
        this.description = description;
        this.input = input;
        this.function = function;
        this.move = move;
    }
}

public class Optional<T>{}
