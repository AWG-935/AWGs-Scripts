/*
 * AWG's Electrotransmission Script
 * -------------------------------------
 * Read the comments carefully, you'll need them.
 * 
 * Q/E changes gears, but you can do this manually via toolbar to get more responsive and
 * better performance out of your transmission/vehicle. The lower the speed, the higher
 * the traction.
 * 
 * For best acceleration, find a start gear that is balanced between torque and top speed.
 * When you near the predicted top speed of the gear raise it, and repeat the process.
 * You will accelerate faster than a vehicle that has no transmission script.
 * 
 * Can function without a status panel but
 * one is recommended.
 * 
 * Works with my Engine Controller Script, these will automatically find each other and it
 * will override your top speed, and adapt to weight, engine change. Empty your player's inventory of junk
 * lest you be too heavy and impact tank speed!
 * 
 * Quick changelog: Added support for engine controller. Now uses tag system for control seats, no more specific
 * painful naming, better support for remote controls.
 */

//V15 AWG
//2022-11-24

const string Name_ControllerTag = "Driver"; //Driver Seat (currently only one seat at once)

const string StatusPanel = "Status Panel"; //Gear display Panel

float       MaximumSpeed = 80f; //The top limit of your vehicle's speed, used as a clamp for all others'
const float ReverseSpeed = 30f; //Your top speed when reversing
const float ForwardGearCount = 4f; //The divisions of your MaximumSpeed that make up your forward gears. (100km/h top speed with 4 gears, means gear 1 is 25km/h, 2 is 50km/h, so on)
const float ReverseGearCount = 4f; //The same as above, but for reverse

const string ForwardGearDefault = "3"; //What gear does accelerating put you in by default? (Don't go above max)
const string ReverseGearDefault = "2"; //What gear does reversing put you in by default?    (Don't go above max)

const string Name_ProgramTag = //Tag for the transmission script to find the engine controller if it exists
"Engine Controller";            //MAKE SURE the tags line up in both scripts!

bool NeutralBraking = true; //When not actively driving forwards or backwards, brake?
bool DefaultShifting = true; //Does the vehicle automatically switch to the below gears when going forward/back
bool TargetSpeedDisplay = true; //Do you want to show the top speed of your current gear on screen? (This will help you change gear to what fits your speed, can chage label below)
bool QEShifting = true; //Whether Q/E should control gear changing.
bool TimerUse = false; //Does the script trigger timers when you enter forward/neutral/reverse?

const string Keyword = "Gear"; //A keyword to put in arguments to the program recognises them as non-numerical shifts.
const string ArgGearUp = "Up"; //Switch gear up argument
const string ArgGearDown = "Down"; //Switch gear down argument
const string ArgNeutralBrake = "ToggleStopBrake"; //Toggle neutral braking argument
const string ArgDefaultShift = "ToggleReverseShift"; //Toggle default reverse gear shifting argument
const string ArgTargetDisplay = "ToggleTargetSpeed"; //Toggle target speed display argument

//Timer Names// (If 'TimerUse' = true)
//-----------//
bool TimerShutoff = true; //If a timer is destroyed, shut off the entire timer computer system by default.

string TimerForward = "TimerF";
string TimerNeutral = "TimerN";
string TimerReverse = "TimerR";

//Which timers are you using?
bool UsingForwardTimer = true;
bool UsingNeutralTimer = true;
bool UsingReverseTimer = true;

//Status Panel Customisation//
//-------------------------//

const string GearName = "Gear"; //output to the status panel
const string GearLetter = "G"; //Short form version of 'Gear Name' (used on the program readout)
const string AccelerateLetter = "F"; //What the program displays on the Status Panel/readout when doing these actions (going forwards/backwards/nothing)
const string NeutralLetter = "N";
const string ReverseLetter = "R";
const string GearShiftTerm = "SHIFTING";
string ExpectedSpeedString = "GT:"; //Gear Speed Target

//Look at modular block naming based on keyword + gear, right idea previously.
//everything up here is saved permenantly, use for adding/error codes and that sort of thing? Great for argument based stuff like switching around variables.

//------------------------------------------------------
// ============== Don't touch below here ===============
//------------------------------------------------------
MyCommandLine _commandLine = new MyCommandLine();
Dictionary<string, Action> _commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

bool EngineControlOverride;

float VehicleSpeed;
float CurrentGearSpeed;
string ExpectedTopSpeed;

const double updatesPerSecond = 10;
const double updateTime = 1 / updatesPerSecond;

const double refreshInterval = 10;
double timeSinceRefresh = 141;
bool isSetup = false;

bool Reverse = false;
bool EngineCheckedAlready = false;

List<IMyProgrammableBlock> List_ProgrammableBlocksToCheck = new List<IMyProgrammableBlock>();

IMyTextPanel echopanel;
IMyCockpit DriverSeat;
IMyProgrammableBlock self;
IMyTimerBlock TimerForwardBlock;
IMyTimerBlock TimerNeutralBlock;
IMyTimerBlock TimerReverseBlock;
IMyProgrammableBlock EngineController;

string STATUS = "";
string ERROR_TXT = "";


public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main(string argument, UpdateType updateType)
{

    if (EngineController != null)
    {
        MaximumSpeed = float.Parse(EngineController.CustomData);
    }


    float CurrentGearInput = 1f;

    if (TimerUse == true)
    {
        TimerForwardBlock = GridTerminalSystem.GetBlockWithName(TimerForward) as IMyTimerBlock;
        TimerNeutralBlock = GridTerminalSystem.GetBlockWithName(TimerNeutral) as IMyTimerBlock;
        TimerReverseBlock = GridTerminalSystem.GetBlockWithName(TimerReverse) as IMyTimerBlock;
    }

    echopanel = GridTerminalSystem.GetBlockWithName(StatusPanel) as IMyTextPanel;
    self = Me;



    if (TimerUse == true)
        if(TimerShutoff==true)
        {
            if (UsingForwardTimer == true)
            {
                if (TimerForwardBlock == null)
                {
                    TimerUse = false;
                }
            }


            if (UsingNeutralTimer == true)
            {
                if (TimerNeutralBlock == null)
                {
                    TimerUse = false;
                }
            }

            if (UsingReverseTimer == true)
            {
                if (TimerReverseBlock == null)
                {
                    TimerUse = false;
                }
            }
        }



    //Write gear shift links here
    //if gear >range, gear = range max

    if (argument != "")//Checks if argument contains data before flashing it to CD
    {
        if (argument.Contains(Keyword))//Check if the argument given has 'Gear' in it, this allows for a gate to run any other arguments
        {
            if (argument.Contains(ArgGearUp))//Before sending new data, check if it's got a gear argument
            {
                CurrentGearInput = (float.Parse(Me.CustomData) + 1f); //Take current gear before it gets broken by the string, then add one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                STATUS += "\n" + GearShiftTerm;
            }
            else if (argument.Contains(ArgGearDown))//if not gear up, check if geardown
            {
                CurrentGearInput = (float.Parse(Me.CustomData) - 1f);
                Me.CustomData = Convert.ToString((CurrentGearInput));
                STATUS += "\n" + GearShiftTerm;
            }
            else if (argument.Contains(ArgNeutralBrake))
            {
                NeutralBraking = !NeutralBraking;
            }
            else if (argument.Contains(ArgDefaultShift))
            {
                DefaultShifting = !DefaultShifting;
            }
            else if (argument.Contains(ArgTargetDisplay))
            {
                TargetSpeedDisplay = !TargetSpeedDisplay;
            }
        }
        else//if it does not have the keyword: do this
        {
            Me.CustomData = argument; //Translate argument into custom data
            STATUS += "\nSHIFTSPEC";
        }
    }
    else
    {
        STATUS += "\nArgument\nRequired";
    }

    if (Me.CustomData != "") //if there is content in custom data for this tick //Put 'if Up/Down' stuff next, then the else can be for the gear inputs specifically, then later can do Q/E
    {

        CurrentGearInput = float.Parse(Me.CustomData);

        if (CurrentGearInput > 0)//Catch to prevent breaking
        {
            if (Reverse == false) //if you're going forwards or neutral
            {
                if (CurrentGearInput >= ForwardGearCount)//if input higher than given gears, set to max (should account for increments)
                {
                    CurrentGearInput = ForwardGearCount; //Set to max
                    Me.CustomData = Convert.ToString((ForwardGearCount));//Account for overflow of custom data as well
                }

                else //If everything is normal on forwards/neutral, do this
                {
                    CurrentGearInput = float.Parse(Me.CustomData); //current gear (speed setting) is the argument, taken as a float
                }

            }
            else
            {
                if (CurrentGearInput >= ReverseGearCount)//if input higher than given gears, set to max (should account for increments)
                {
                    CurrentGearInput = ReverseGearCount; //Set to max
                    Me.CustomData = Convert.ToString((ReverseGearCount));//Account for overflow of custom data as well
                }
                else //If everything is normal on reverse, not neutral, do this
                {
                    CurrentGearInput = float.Parse(Me.CustomData); //current gear (speed setting) is the argument, taken as a float
                }
            }
        }
        else
        {
            CurrentGearInput = 1f;
            Me.CustomData = Convert.ToString((CurrentGearInput));
        }
    }
    else
    {
        Me.CustomData = "1";
    }


    //------------------------------------------
    if ((Runtime.UpdateFrequency & UpdateFrequency.Update10) == 0)
        Runtime.UpdateFrequency = UpdateFrequency.Update10;
    //------------------------------------------

    if ((updateType & UpdateType.Update10) == 0)
        return;
    //implied else

    //currentTime += 1.0/6.0;
    timeSinceRefresh += 1.0 / 6.0;

    if (!isSetup || timeSinceRefresh >= refreshInterval)
    {
        isSetup = GrabBlocks();
        timeSinceRefresh = 0;
    }

    if (!isSetup)
        return;

    //Get estimated top speeds for current gear
    if (TargetSpeedDisplay == true)
    {
        if (Reverse == false)//If going forwards
        {
            ExpectedTopSpeed = Convert.ToString(Math.Round((float)(MaximumSpeed / ForwardGearCount * CurrentGearInput),0)) + "km/h";
        }
        else//if reversing
        {
            ExpectedTopSpeed = "-" + Convert.ToString(Math.Round((float)(ReverseSpeed / ReverseGearCount * CurrentGearInput),0)) + "km/h";
        }
    }
    else
    {
        ExpectedTopSpeed = "";
        ExpectedSpeedString = "";
    }


    try
    {
        var inputVec = Controller.MoveIndicator; //USE THESE SECTIONS TO RUN THE GEAR FUNCTIONS, USE WHIPS STEERING SCRIPT HERE AS AN EXAMPLE, GRAB ALL WHEELS

        VehicleSpeed = (float)Math.Round(((float)Controller.GetShipSpeed()*3.6f),1);
        if (VehicleSpeed < 0)
        {
            VehicleSpeed = (float)Math.Round((((float)Controller.GetShipSpeed()*-1)*3.6f),1);
        }

        //converting a method group to a float, rounding it with something made specifically for doubles, then converting it to km/h in one go


        if (inputVec.Z < 0) //W if input is positive in the forwards direction
        {
            Echo(GearLetter + ":" + Convert.ToString(CurrentGearInput) + AccelerateLetter);

            STATUS = GearName + ":\n"
                + CurrentGearInput + AccelerateLetter + "\n"
                + VehicleSpeed + "km/h\n"
                + ExpectedSpeedString + ExpectedTopSpeed;

            Reverse = false; //Switches the transmission to the forwards gears

            if (Me.ShowInInventory == false) //This is the area that lets you run things once per W/S
            {
                Me.ShowInInventory = true;//stop this happening again until you've gone forwards

                if (DefaultShifting == true) //if you last went forwards/neutral
                {
                    Me.CustomData = ForwardGearDefault; //Shifts to default reverse gear
                }
            }
            if (TimerUse == true)//Forwards timer systems
            {
                if (TimerForwardBlock != null)//if you can find the block for forward timer
                {
                    if (TimerForwardBlock.ShowInInventory == true)
                    {
                        TimerForwardBlock.ShowInInventory = false;
                        TimerForwardBlock.Trigger();

                        if (TimerNeutralBlock != null)//Act as a catch for the neutral timer if it exists
                        {
                            TimerNeutralBlock.ShowInInventory = true;
                        }
                    }
                }
                else
                {
                    return;//Timer block gone
                }
            }

        }
        else if (inputVec.Z > 0)//S
        {
            Echo(GearLetter + ":" + Convert.ToString(CurrentGearInput) + ReverseLetter); //Program readout, configurable now

            STATUS = GearName + ":\n"
                + CurrentGearInput + ReverseLetter + "\n"
                + (float)Math.Round((((float)Controller.GetShipSpeed() * -1) * 3.6f), 1) + "km/h\n" //inverts vehicle speed calc, displayed on text panel
                + ExpectedSpeedString + ExpectedTopSpeed;


            Reverse = true; //You are reversing

            if (Me.ShowInInventory == true) //This is the area that lets you run things once per W/S
            {
                Me.ShowInInventory = false;//stop this happening again until you've gone forwards

                if (DefaultShifting == true) //if you last went forwards/neutral
                {
                    Me.CustomData = ReverseGearDefault; //Shifts to default reverse gear
                }
            }

        if (TimerUse == true)//Forwards timer systems
        {
            if (TimerReverseBlock != null)//if you can find the block for forward timer
            {
                if (TimerReverseBlock.ShowInInventory == true)
                {
                        TimerReverseBlock.ShowInInventory = false;
                        TimerReverseBlock.Trigger();

                        if (TimerNeutralBlock != null)//Act as a catch for the neutral timer if it exists
                        {
                            TimerNeutralBlock.ShowInInventory = true;
                        }
                    }
            }
        }
        }

        if (QEShifting == true)
        {
            if (Controller.RollIndicator < 0)//When holding Q
            {
                CurrentGearInput = (float.Parse(Me.CustomData) - 1f); //Take current gear before it gets broken by the string, then add one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                STATUS += "\nSHIFTING";
            }
            else if (Controller.RollIndicator > 0)//When holding E
            {
                CurrentGearInput = (float.Parse(Me.CustomData) + 1f); //Take current gear before it gets broken by the string, then add one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                STATUS += "\nSHIFTING";
            }
        }

        GearShift(wheels, CurrentGearInput); //Shift gears when moving forwards or backwards. Placed here as not to change when not accel/deccel.

        if (inputVec.Z == 0) //Nor forwards or backwards
        {
            Echo(GearLetter + ":" + Convert.ToString(CurrentGearInput) + NeutralLetter);//Make constants for Forwards/Backwards/Neutral

            STATUS = GearName + ":\n"
                + CurrentGearInput + NeutralLetter + "\n"
                + VehicleSpeed + "km/h\n"
                + ExpectedSpeedString + ExpectedTopSpeed;

            if (NeutralBraking == true) //If neutral braking is on, auto brake when there is no input.
            {
                Controller.HandBrake = true;
            }

            if (TimerUse == true) //Neutral timer systems, since no 'single use' system exists here, use the neutral timer that may exist as storage with its nonexistent inventory
            {
                if (TimerNeutralBlock != null)//if you can find the block for neutral timer
                {
                    if (TimerNeutralBlock.ShowInInventory == true)//if you were last reversing or accelerating, the 'do once' section for neutral
                    {
                        TimerNeutralBlock.Trigger();
                        TimerNeutralBlock.ShowInInventory = false;

                        if (TimerForwardBlock != null)
                        {
                            TimerForwardBlock.ShowInInventory = true;
                        }
                        if (TimerReverseBlock != null)
                        {
                            TimerReverseBlock.ShowInInventory = true;
                        }
                    }
                }
            }

        }
        else if (NeutralBraking == true) //if neutbrake is on whilst for/back input is going, unbrake
        {
            Controller.HandBrake = false; //TEMPORARY, LATER BRING TO COMFORTABLE STOP OR GEAR DOWN /THEN HANDBRAKE BASED ON VARIABLE/
        }

    }
    catch
    {
        isSetup = false;
    }


    if (echopanel == null) //Checks if text panel exists
    {
        Echo("No screen named " + StatusPanel + "\n");
    }
    else
    {
        if (ERROR_TXT != "") //if there is an error reported
        {
            echopanel.WriteText("Script Errors:\n" + ERROR_TXT + "(Check ownership)\n\n" + STATUS); //report the error to the panel
        }
        else
        {
            echopanel.WriteText(STATUS);
        } //if there are no errors, just report the status!
    }

}

List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();

bool GrabBlocks()
{
    GridTerminalSystem.GetBlocksOfType(AllControlBlocks);
    if (AllControlBlocks.Count == 0)
    {
        Echo($"Error: No ship controller named found");
        return false;
    }
    Controller = ScanControlBlocks(AllControlBlocks);

    GridTerminalSystem.GetBlocksOfType(wheels, block => block.CubeGrid == Controller.CubeGrid);
    if (wheels.Count == 0)
    {
        Echo("Error: No wheels found on same grid as controller");
        return false;
    }

    GridTerminalSystem.GetBlocksOfType(List_ProgrammableBlocksToCheck);//check programs for engine controller

    if(EngineCheckedAlready != true)//this makes sure it only checks through these once as needed
    {
        foreach (var CheckedPrograms in List_ProgrammableBlocksToCheck)//look through the list of recognised subtypes,
        {
            if (CheckedPrograms.CustomName.Contains(Name_ProgramTag))//, then if the block contains a given subtype, and the checked block has NOT already been recognised
            {
                EngineController = CheckedPrograms;
                EngineCheckedAlready = true;
                Echo("Engine Controller Active");
                EngineControlOverride = true;
                break;
            }
        }
    }

    if (List_ProgrammableBlocksToCheck.Count == 0)
    {
        EngineControlOverride = false;
    }
    return true;
}




public void GearShift(List<IMyMotorSuspension> wheels, float CurrentGearInput)
{
    if (Reverse == false) //If moving forwards
    {
        CurrentGearSpeed = (MaximumSpeed / ForwardGearCount * CurrentGearInput); //Set current gear speed to that of top speed divided by set amount of forward gears.
    }
    else //otherwise, if reversing.
    {
        CurrentGearSpeed = (ReverseSpeed / ReverseGearCount * CurrentGearInput); //Do the same, but obeying the reverse system.
    }

    SetWheelSpeed(wheels, CurrentGearSpeed); //Whatever current gear is, run the gear shift function.
}

public void SetWheelSpeed(List<IMyMotorSuspension> wheels, float CurrentGearSpeed) //The function that actually sets wheel speed.
{

    string TextCurrentGearSpeed = CurrentGearSpeed.ToString(); //Converts the gear speed  double into a string that the text panel can read.

    foreach (var wheel in wheels)
    {
        wheel.SetValue("Speed Limit", CurrentGearSpeed); //Set wheel speed to whatever CurrentGearSpeed is.
    }
}


IMyShipController Controller = null;
List<IMyShipController> AllControlBlocks = new List<IMyShipController>();
IMyShipController ScanControlBlocks(List<IMyShipController> List_Controllers)
{

    foreach (IMyShipController ThisController in List_Controllers)
    {
        Echo(ThisController.CustomName+"\n");
        if (ThisController.CustomName.Contains(Name_ControllerTag) && ThisController.IsUnderControl)
        {
            return ThisController;
        }
    }
    return List_Controllers[0];

}