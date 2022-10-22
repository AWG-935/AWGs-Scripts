/*
 * R e a d m e
 * -----------
 * Tells you when a block with an inventory is empty, runs a timer then, and when it is filled again
 * also changes light colour. Great for activating autoloaders dynamically or notifying people of 
 * resource shortages! - may modify for groups in future.
 */

const string Name_InventoryBlock =              //Set of blocks that will be checked for being filled.
"Main Gun";

const float Count_ItemThreshold =                 //the count of items to trigger actions if it drops below/fills above this.
1f;

const string Name_Group_StatusLight =        //Light group that will change colour based on system status
"Ammo Load Status";

const string Name_StatusPanel =
"Loading Status Panel";


const string Message_Loaded =
"READY";

const string Message_Unloaded =
"NOT RDY";

//---------------------//

const string Name_LoadedTimer =
"Timer Load";

const string Name_UnloadedTimer =
"Timer Unload";

//---------------------------------//
//  OPERATING COLOURS  //
//---------------------------------//

Color Colour_StatusLight_Operation = new Color
(230, 230, 190);                                            //Regular operation colours

Color Colour_StatusLight_Empty = new Color
(255, 25, 25);                                              //What it switches to when storage is empty

Color Colour_StatusLight_Loaded = new Color
(25, 255, 25);                                              //What it switches to when storage is filled, for a moment.

//---------------------//

const string Default_Message_LoadedTimerRun = "Load Timer Set\n";
const string Default_Message_UnloadedTimerRun = "Unload Timer Set\n";

//NO TOUCH BELOW

bool HasLightGroup;
bool HasStatusPanel;
bool HasInventoryBlock;
bool HasLoadedTimer;
bool HasUnloadedTimer;

bool InventoryThresholdSurpassed = true;
bool OneCompleteCycle;

IMyTimerBlock Block_UnloadedTimer;
IMyTimerBlock Block_LoadedTimer;
IMyTextPanel Block_StatusPanel;
IMyInventoryOwner Block_InventoryBlock; //Using generic check so 'Main gun' may actually be any inventory containing block!


//Default messages:
const string Linebreak = "\n";
const string MissingCheckOwnership = "missing!\nCheck naming\nand ownership!\n";
const string Blank = "";


public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

}

public void Main(string argument, UpdateType updateType)
{


    //----------------------------------------------------//
    //
    //
    //
    //----------------------------------------------------//

    Block_UnloadedTimer = GridTerminalSystem.GetBlockWithName(Name_UnloadedTimer) as IMyTimerBlock; //optional for triggering actions when block is empty
    Block_LoadedTimer = GridTerminalSystem.GetBlockWithName(Name_LoadedTimer) as IMyTimerBlock; //optional for triggering actions when block is refilled
    Block_StatusPanel = GridTerminalSystem.GetBlockWithName(Name_StatusPanel) as IMyTextPanel;
    Block_InventoryBlock = GridTerminalSystem.GetBlockWithName(Name_InventoryBlock) as IMyInventoryOwner;


    //----------------------------------------------------//
    //   BLOCK SETUP ^^^
    //
    //
    //                      |
    //   GROUP SETUP       \|/
    //----------------------------------------------------//



    IMyBlockGroup Group_StatusLight = GridTerminalSystem.GetBlockGroupWithName(Name_Group_StatusLight); //Self explanatory
    List<IMyLightingBlock> List_StatusLights = new List<IMyLightingBlock>();

    if (Group_StatusLight != null)
    {
        HasLightGroup = true;
        Group_StatusLight.GetBlocksOfType(List_StatusLights);//(This is just here to make sure you're working with the lights in the group, looks in the group for the block type from the list specified)
    }
    else
    {
        HasLightGroup = false;
    }


    //----------------------------------------------------//
    //   GROUP SETUP ^^^
    //
    //
    //
    //   SCREEN &           |
    //   ERROR HANDLING    \|/
    //----------------------------------------------------//


    string ErrorLog = ""; //Add to this throughout the script using += (append sign)
    string ScreenPrint = ""; //The eventual result on the screen. You can append to this if you want, but look at other programs for help.

    string Dynamic_Message_LoadedTimerRun = Default_Message_LoadedTimerRun;
    string Dynamic_Message_UnloadedTimerRun = Default_Message_UnloadedTimerRun;
    //The above are here so that if the timers are not, they can be set to blanks, and cut out.

    //----------------------------------------------------//
    //   ERROR HANDLING ^^^
    //
    //
    //
    //   FUNCTION           |
    //   AND BODY          \|/
    //----------------------------------------------------//

    if (Block_StatusPanel == null)//in the function, ask if the text panel exists. If it does, print to it. If not, echo ErrorLog to Me.
    {
        HasStatusPanel = false;

        //This function creates your error message for you passing the name of the block in
        Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_StatusPanel);
    }
    else
    {
        HasStatusPanel = true;
    }

    if (Block_InventoryBlock == null)
    {
        Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_InventoryBlock);


        Event_ErrorThrown(ErrorLog, Block_StatusPanel, HasStatusPanel);
        return;
    }



    //DIS BITCH EMPTY//
    //               //

    //When the inventory has an item count below the user defined threshold, run the empty inventory event at the bottom of the script!
    if (Block_InventoryBlock.GetInventory(0).ItemCount < Count_ItemThreshold)
    {
        //Part to be run once
        if (InventoryThresholdSurpassed) //if it was previously past the 'full' mark and is now empty
        {
            Event_InventoryEmpty(Block_InventoryBlock, Block_LoadedTimer, Block_UnloadedTimer, List_StatusLights, ref Dynamic_Message_UnloadedTimerRun);
            Function_ScreenStatusConstruction(ref ScreenPrint, Name_InventoryBlock, Message_Loaded, Dynamic_Message_UnloadedTimerRun);
            InventoryThresholdSurpassed = false;


        }
        else
        {
            //runs repeatedly when empty - prints the screen with the unloaded message - perhaps put something in here for timer use?
            Function_ScreenStatusConstruction(ref ScreenPrint, Name_InventoryBlock, Message_Unloaded, Blank);

        }
    }
    else // DIS BITCH FULL
    {
        if (!InventoryThresholdSurpassed)//if it was previously empty and is now full, runs once, on fill after being empty
        {
            Event_InventoryFilled(Block_InventoryBlock, Block_LoadedTimer, Block_UnloadedTimer, List_StatusLights, ref Dynamic_Message_LoadedTimerRun);

            InventoryThresholdSurpassed = true;

            OneCompleteCycle = true;

            Function_ScreenStatusConstruction(ref ScreenPrint, Name_InventoryBlock, Message_Loaded, Dynamic_Message_LoadedTimerRun);

        }
        else
        {
            if (OneCompleteCycle == true)
            {
                Function_ChangeLightsColour(List_StatusLights, Colour_StatusLight_Operation);
                OneCompleteCycle = false;
            }
            //Repeated run when full
            Function_ScreenStatusConstruction(ref ScreenPrint, Name_InventoryBlock, Message_Loaded, Blank);
        }

    }









    //Event_ErrorThrown(ErrorLog, Block_StatusPanel, HasStatusPanel);
}










//----------------------------------------------------//
//   METHODS           |
//    :)              \|/
//----------------------------------------------------//

public void Event_InventoryEmpty(IMyInventoryOwner Block_InventoryBlock, IMyTimerBlock Block_LoadedTimer, IMyTimerBlock Block_UnloadedTimer, List<IMyLightingBlock> List_StatusLights, ref string Dynamic_Message_UnloadedTimerRun)
{
    //this is where you activate lights, timers, so on

    Function_ChangeLightsColour(List_StatusLights, Colour_StatusLight_Empty);

    if (Block_UnloadedTimer != null)
    {
        Block_UnloadedTimer.Trigger();
        Dynamic_Message_UnloadedTimerRun = Default_Message_UnloadedTimerRun;
    }
    else
    {
        Dynamic_Message_UnloadedTimerRun = Blank;//Makes the string effectively invisible if the timer is unused and does not bother the text panel
    }


}




public void Event_InventoryFilled(IMyInventoryOwner Block_InventoryBlock, IMyTimerBlock Block_LoadedTimer, IMyTimerBlock Block_UnloadedTimer, List<IMyLightingBlock> List_StatusLights, ref string Dynamic_Message_LoadedTimerRun)
{
    //switch the lights and timers to the other user presets

    Function_ChangeLightsColour(List_StatusLights, Colour_StatusLight_Loaded);//Sends StatusLight_Loaded to take place of the CurrentColor container

    if (Block_LoadedTimer != null)
    {
        Block_LoadedTimer.Trigger();
        Dynamic_Message_LoadedTimerRun = Default_Message_LoadedTimerRun;
    }
    else
    {
        Dynamic_Message_LoadedTimerRun = Blank;//Makes the string effectively invisible if the timer is unused and does not bother the text panel
    }

}





public void Function_ChangeLightsColour(List<IMyLightingBlock> List_StatusLights, Color CurrentColour)//Instead of repeating a bulky segment twice, I pass a colour into this, and run it once depending on what's going
{
    if (HasLightGroup)
    {
        foreach (var Group_StatusLight in List_StatusLights)//for every Status Light in the StatusLight list made from the StatusLight group, recognised as light providing blocks.
        {
            Group_StatusLight.Color = CurrentColour;
        }
    }
}

public void Event_ErrorThrown(string ErrorLog, IMyTextPanel Block_StatusPanel, bool HasStatusPanel)
{
    //If the StatusPanel exists, make sure it can display, and print to it. Otherwise, send to the PB!

    if (HasStatusPanel)
    {
        if (Block_StatusPanel.ContentType == ContentType.NONE)
        {
            Block_StatusPanel.ContentType = ContentType.TEXT_AND_IMAGE;
        }

        Block_StatusPanel.WriteText(ErrorLog);
    }
    else
    {
        Echo(ErrorLog);
    }
}

public void Function_CompileDisplayedErrorMessage(ref string ErrorLog, string Name_ErrorBlock)//this is how I construct the error messages, done because it is repeated multiple times but with different blocks
{
    //make 'error block' string name?

    ErrorLog //Modular error log, woo! - TURN THIS INTO A FUNCTION, WHERE THIS IS ALL DONE, THE NAME IS PASSED TO IT AS A VARIABLE, SO YOU DON'T HAVE TO REPEAT
            +=
            Name_ErrorBlock
            +
            Linebreak
            +
            MissingCheckOwnership
            +
            Linebreak;

}




public void Function_ScreenStatusConstruction(ref string ScreenPrint, string Name_UsedBlock, string Message_Status, string Timer_Status)
{
    //make 'error block' string name?

    ScreenPrint
    +=
    Name_UsedBlock //main gun
    +
    Linebreak
    +
    Message_Status //whether it is ready or unready
    +
    Linebreak
    +
    Timer_Status
    ;
    //READY OR UNREADY
    //HOW MUCH AMMO (?)

    Event_ScreenOperation(ScreenPrint, Block_StatusPanel, HasStatusPanel);

}
public void Event_ScreenOperation(string ScreenPrint, IMyTextPanel Block_StatusPanel, bool HasStatusPanel)
{
    //If the StatusPanel exists, make sure it can display, and print to it. Otherwise, send to the PB!

    if (HasStatusPanel)
    {
        Block_StatusPanel.WriteText(ScreenPrint);
    }
}