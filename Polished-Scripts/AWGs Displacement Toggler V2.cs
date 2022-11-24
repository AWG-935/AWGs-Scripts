/*
 * R e a d m e
 * -----------
 * 
 * Basic rotor displacement script for groups
 * Allows you to toggle between two settings, or
 * a specific displacement via argument. Much cleaner
 * than the original! Smooth displacement coming in future.
 */

const float Displacement_Max = //Maximum rotor displacement when no argument is input.
0.5f;

const float Displacement_Min = //Minimum rotor displacement when no argument is input.
0f;

const float Displacement_Start = //What does the program start with on first run, other than arguments?
Displacement_Max;

const string Name_Group_DisplacementRotors =        //Light group that will change colour based on system status
"Displacement Rotors";

const string Name_StatusPanel =
"Status Panel";


//---------------------//

const string Name_MaxDisplacementTimer =
"Timer Max Displacement";

const string Name_MinDisplacementTimer =
"Timer Min Displacement";

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
//Message examples
const string Default_Message_LoadedTimerRun = "Load Timer Set\n";
const string Default_Message_UnloadedTimerRun = "Unload Timer Set\n";
//
//---------------------//

//NO TOUCH BELOW
bool HasStatusPanel;
bool HasRotorGroup;
bool ErrorThrownThisRun;

bool SmoothDisplacement;
float Displacement_Argument;
float DisplacementCycleCode = 0;
//0 means not started
//1 means min
//2 means max

//bool OneCompleteCycle;//Triggered when the program has displaced once, will auto-cut it, then reset.

IMyTextPanel Block_StatusPanel;
IMyTimerBlock Block_MinDisplacementTimer;
IMyTimerBlock Block_MaxDisplacementTimer;

//---------------------//
//Default messages:
const string Linebreak = "\n";
const string MissingCheckOwnership = "missing!\nCheck naming\nand ownership!\n";
const string Blank = "";
//---------------------//

/// <summary>
        ///
        /// Expected operation:
        /// Flip between max and min displacement unless an argument is input.
        /// Basically have a thing to decide whether it's max or min when ran
        /// using a variable? rather than rotor displacement check.
        ///
        /// If run with argument, send that instead to the displacement function.
        ///
        /// Prep for smooth running? So it runs every tick but immediately stops itself when it runs once.
        /// Unless smooth running is turned on, where it instead slides between displacements?
        ///
        /// </summary>

//public Program()
//{
//    Runtime.UpdateFrequency = UpdateFrequency.Update1;
//}

public void Main(string argument, UpdateType updateSource)
{


    //----------------------------------------------------//
    //
    //
    //
    //----------------------------------------------------//
    Block_StatusPanel = GridTerminalSystem.GetBlockWithName(Name_StatusPanel) as IMyTextPanel;
    Block_MinDisplacementTimer = GridTerminalSystem.GetBlockWithName(Name_MinDisplacementTimer) as IMyTimerBlock; //optional for triggering actions when block is empty
    Block_MaxDisplacementTimer = GridTerminalSystem.GetBlockWithName(Name_MaxDisplacementTimer) as IMyTimerBlock; //optional for triggering actions when block is refilled
    //----------------------------------------------------//
    //   BLOCK SETUP ^^^
    //
    //
    //                      |
    //   GROUP SETUP       \|/
    //----------------------------------------------------//

    IMyBlockGroup Group_DisplacementRotors = GridTerminalSystem.GetBlockGroupWithName(Name_Group_DisplacementRotors); //Self explanatory
    List<IMyMotorStator> List_DisplacementRotors = new List<IMyMotorStator>();

    if (Group_DisplacementRotors != null)
    {
        HasRotorGroup = true;
        Group_DisplacementRotors.GetBlocksOfType(List_DisplacementRotors);//(This is just here to make sure you're working with the rotors in the group, looks in the group for the block type from the list specified)
    }
    else
    {
        HasRotorGroup = false;
    }

    //----------------------------------------------------//
    //   GROUP SETUP ^^^
    //
    //
    //
    //   SCREEN &           |
    //   ERROR HANDLING    \|/
    //----------------------------------------------------//


    string ErrorLog = ""; //Add to this throughout the script using +=
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
        ErrorThrownThisRun = true;
        Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_StatusPanel);
    }
    else
    {
        HasStatusPanel = true;
    }

    if (HasRotorGroup == false)
    {
        ErrorThrownThisRun = true;
        Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_Group_DisplacementRotors);
        return;
    }
    //ERRORS DONE
    //------------------------------

    //ARGUMENT
    else if (argument != "")//If there is a rotor group, and there is an argument submitted.
    {
        Displacement_Argument = float.Parse(argument);
        Function_DisplaceRotors(List_DisplacementRotors, Displacement_Argument, ref ScreenPrint);
    }

    //CYCLE
    else//if no argument, here's a container for everything that happens for min/max
    {
        if (DisplacementCycleCode == 0)//if not started yet.
        {
            if (Displacement_Start == Displacement_Min || DisplacementCycleCode == 2)
            {
                DisplacementCycleCode = 1;
            }
            else if (Displacement_Start == Displacement_Max || DisplacementCycleCode == 1)
            {
                DisplacementCycleCode = 2;
            }

            Function_DisplaceRotors(List_DisplacementRotors, Displacement_Start, ref ScreenPrint);

        }
        if (DisplacementCycleCode == 1)
        {
            Function_DisplaceRotors(List_DisplacementRotors, Displacement_Min, ref ScreenPrint);
            DisplacementCycleCode = 2;
            //For the next cycle.

            if (Block_MinDisplacementTimer != null)
            {
                Block_MinDisplacementTimer.Trigger();
            }
        }
        else if (DisplacementCycleCode == 2)
        {
            Function_DisplaceRotors(List_DisplacementRotors, Displacement_Max, ref ScreenPrint);
            DisplacementCycleCode = 1;
            //For the next cycle..

            if (Block_MaxDisplacementTimer != null)
            {
                Block_MaxDisplacementTimer.Trigger();
            }
        }
    }












    if (ErrorThrownThisRun == true)//Basically only run the error event if one of the other blocks tells you to
    {
        Event_ErrorThrown(ErrorLog, Block_StatusPanel, HasStatusPanel);
    }
}



public void Function_DisplaceRotors(List<IMyMotorStator> List_DisplacementRotors, float RotorDisplacement, ref string ScreenPrint)//Instead of repeating a bulky segment twice, I pass a colour into this, and run it once depending on what's going
{
    if (HasRotorGroup)
    {
        foreach (var Group_DisplacementRotors in List_DisplacementRotors)//for every rotor in the DisplacementRotors list made from the DisplacementRotors group, recognised as a MotorStator
        {
            Group_DisplacementRotors.Displacement = RotorDisplacement;
            Function_ScreenStatusConstruction(ref ScreenPrint, Group_DisplacementRotors.DisplayNameText, Group_DisplacementRotors.Displacement.ToString());
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




public void Function_ScreenStatusConstruction(ref string ScreenPrint, string Name_UsedBlock, string Message_Status)
{
    //make 'error block' string name?

    ScreenPrint
    +=
    Name_UsedBlock //Rotor displacement
    +
    Linebreak
    +
    Message_Status //whether it is ready or unready
    +
    Linebreak
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