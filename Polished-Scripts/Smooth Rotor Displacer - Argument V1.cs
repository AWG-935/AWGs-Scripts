/*
 * R e a d m e
 * -----------
 * 
 * In this file you can include any instructions or other comments you want to have injected onto the 
 * top of your final script. You can safely delete this file if you do not want any such comments.
 */

float Displacement_Speed = //Speed of displacement
0.0015f;

float DisplacementErrorMarginDistance = //Distance where the rotor slows down to get more accurate when displacing
0.02f;

float StoppingPrecision = 4f; //What the ErrorMargin distance is divided by when calculating how precisely to stop. (Higher = more precise)

const float Displacement_SlowDownDivision = //Divides your rotor speed by this when it gets within the error margin distance
6f;

const int Rounding = //Amount of decimal places the text panel rounds the rotor display
4;

const string Name_Group_DisplacementRotors =        //Light group that will change colour based on system status
"Displacement Rotors";



const string Name_MaxDisplacementTimer =
"Timer Rotor";

//---------------------//
//NO TOUCH BELOW
List<IMyMotorStator> List_DisplacementRotors = new List<IMyMotorStator>();

const string Name_StatusPanel =
"Rotor Panel";

bool HasStatusPanel;
bool CheckedStatusPanel;
bool HasRotorGroup;
bool ErrorThrownThisRun;

bool isRunning = false;
bool RotorDirection = true;//true = POS, false = NEG
bool DisplacementTargetSatisfied;

bool TimerTriggered;

float DisplacementTarget;

string ArgumentSave;
string Message_ActionComplete = "Complete";

//0 means not started
//1 means min
//2 means max

//bool OneCompleteCycle;//Triggered when the program has displaced once, will auto-cut it, then reset.

IMyTextPanel Block_StatusPanel;
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

string ErrorLog = ""; //Add to this throughout the script using +=
string ScreenPrint = ""; //The eventual result on the screen. You can append to this if you want, but look at other programs for help.

//public Program()
//{
//    Runtime.UpdateFrequency = UpdateFrequency.Update1;
//}
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main(string argument, UpdateType updateSource)
{

    //if (!isRunning)//If you aren't setup yet, run the grab blocks system, if true, allow program to continue
    {
        isRunning = Bool_GrabBlocks(ref ErrorLog);
    }

    if (!isRunning)
        return;

    if (Block_StatusPanel == null && CheckedStatusPanel == false)//in the function, ask if the text panel exists. If it does, print to it. If not, echo ErrorLog to Me.
    {

        CheckedStatusPanel = true;
        HasStatusPanel = false;

        //This function creates your error message for you passing the name of the block in
        //ErrorThrownThisRun = true;
        //Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_StatusPanel);
    }
    else if (Block_StatusPanel != null)
    {
        CheckedStatusPanel = false;
        HasStatusPanel = true;
    }

    //if rotor displacement does not match +/- error margin of displacement target, DisplacementTargetSatisfied = false.
    //if DisplacementTargetSatisfied = false, keep moving? not sure how I'll do that part.





    //The above are here so that if the timers are not, they can be set to blanks, and cut out.

    //----------------------------------------------------//
    //   ERROR HANDLING ^^^
    //
    //
    //
    //   FUNCTION           |
    //   AND BODY          \|/
    //----------------------------------------------------//



    if (argument != "")
    {

        Me.CustomData = Blank;
        DisplacementTargetSatisfied = false;
        ArgumentSave = argument;
        TimerTriggered = false;

        if (float.Parse(argument) > Math.Round(List_DisplacementRotors[0].Displacement - (DisplacementErrorMarginDistance / 2), 4))
        {
            Echo("POS Arg recieved");
            DisplacementTarget = float.Parse(argument.Replace("POS", ""));
            Displacement_Speed = Math.Abs(Displacement_Speed);//forces it to be positive
        }
        //(Math.Round(Group_DisplacementRotors.Displacement, 4) < DisplacementTarget + (DisplacementErrorMargin / 2)

        if (float.Parse(argument) < Math.Round(List_DisplacementRotors[0].Displacement - (DisplacementErrorMarginDistance / 2), 4))
        {
            Echo("NEG Arg recieved");
            DisplacementTarget = float.Parse(argument.Replace("NEG", ""));
            Displacement_Speed = Math.Abs(Displacement_Speed) * (-1);//makes displacement speed positive
        }
    }


    if (HasRotorGroup == false)
    {
        Echo("NO GROUP");
        ErrorThrownThisRun = true;
        Function_CompileDisplayedErrorMessage(ref ErrorLog, Name_Group_DisplacementRotors);
        return;
    }
    else if (DisplacementTargetSatisfied == false) //basically have you just switched something around
    {
        Function_DisplaceRotors(List_DisplacementRotors, DisplacementTarget, ref ScreenPrint);

    }
    else//since the target has been satisfied
    {
        if(TimerTriggered == false && Block_MaxDisplacementTimer != null)
        {
            Block_MaxDisplacementTimer.Trigger();
            TimerTriggered = true;
        }

    }





    //ARGUMENT













    if (ErrorThrownThisRun == true)//Basically only run the error event if one of the other blocks tells you to
    {
        Event_ErrorThrown(ErrorLog, Block_StatusPanel, HasStatusPanel);
    }


}

bool Bool_GrabBlocks(ref string ErrorLog)//Check for power, then engine subtypes, return false if not. Collapse this later, and finally move it back to the bottom when finished with script.
{
    IMyBlockGroup Group_DisplacementRotors = GridTerminalSystem.GetBlockGroupWithName(Name_Group_DisplacementRotors); //Self explanatory

    if (Group_DisplacementRotors != null)
    {
        Echo("Rotor Group found");
        HasRotorGroup = true;
        Group_DisplacementRotors.GetBlocksOfType(List_DisplacementRotors);//(This is just here to make sure you're working with the rotors in the group, looks in the group for the block type from the list specified)
    }
    else
    {
        Echo("No Rotor Group found");
        HasRotorGroup = false;
        return false;
    }



    Block_StatusPanel = GridTerminalSystem.GetBlockWithName(Name_StatusPanel) as IMyTextPanel;
    Block_MaxDisplacementTimer = GridTerminalSystem.GetBlockWithName(Name_MaxDisplacementTimer) as IMyTimerBlock; //optional for triggering actions when block is refilled



    return true;
}



public void Function_DisplaceRotors(List<IMyMotorStator> List_DisplacementRotors, float RotorDisplacement, ref string ScreenPrint)//Instead of repeating a bulky segment twice, I pass a colour into this, and run it once depending on what's going
{
    if (Me.CustomData != Message_ActionComplete)
    {
        if (HasRotorGroup)
        {

            foreach (var Group_DisplacementRotors in List_DisplacementRotors)//for every rotor in the DisplacementRotors list made from the DisplacementRotors group, recognised as a MotorStator
            {
                if (Group_DisplacementRotors.Displacement < (DisplacementTarget - DisplacementErrorMarginDistance) || Group_DisplacementRotors.Displacement > (DisplacementTarget + DisplacementErrorMarginDistance))
                {
                    Group_DisplacementRotors.Displacement = Group_DisplacementRotors.Displacement + Displacement_Speed;
                    Echo("High speed");


                }
                else
                {
                    Group_DisplacementRotors.Displacement = Group_DisplacementRotors.Displacement + (Displacement_Speed / Displacement_SlowDownDivision);
                    Echo("Slow speed");
                }

                if (Math.Round(Group_DisplacementRotors.Displacement, 4) < DisplacementTarget + (DisplacementErrorMarginDistance / StoppingPrecision) && Math.Round(Group_DisplacementRotors.Displacement, 3) > DisplacementTarget - (DisplacementErrorMarginDistance / StoppingPrecision))
                {
                    DisplacementTargetSatisfied = true;
                    Me.CustomData = Message_ActionComplete;
                }
            }

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

            Block_StatusPanel.Font = "Monospace";

            Block_StatusPanel.FontSize = 1.2f;
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

        if (Block_StatusPanel.ContentType == ContentType.NONE)
        {
            Block_StatusPanel.ContentType = ContentType.TEXT_AND_IMAGE;

            Block_StatusPanel.Font = "Monospace";

            Block_StatusPanel.FontSize = 1.2f;
        }

    }
}