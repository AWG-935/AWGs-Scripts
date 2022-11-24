/*
 * R e a d m e
 * -----------
 * 
 * Supports brake steering at the moment. Heavily unpolished. 
 * To do: 
 * -Other types of steering (reverse support, power changes)
 * -Incorporate modernising features from transmission, engine manager updates
 * -Better error checking and screen support
 */

const string DriverSeatName = "*Remote*"; //Driver Seat (currently only one seat at once)

const string StatusPanel = "Status Panel"; //Gear display Panel

const string AllRotorsGroupName = "Wheels All";
const string LeftRotorsGroupName = "Wheels Left";
const string RightRotorsGroupName = "Wheels Right";

const float MaximumRPM = 60f; //The top limit of your vehicle's speed, used as a clamp for all others'
const float ReverseRPM = 60f; //Your top speed when reversing

const float ForwardGearCount = 15f; //The divisions of your MaximumSpeed that make up your forward gears. (100km/h top speed with 4 gears, means gear 1 is 25km/h, 2 is 50km/h, so on)
const float ReverseGearCount = 15f; //The same as above, but for reverse

const string ForwardGearDefault = "3"; //What gear does accelerating put you in by default? (Don't go above max)
const string ReverseGearDefault = "2"; //What gear does reversing put you in by default?    (Don't go above max)

bool NeutralBraking = true; //When not actively driving forwards or backwards, brake?
bool DefaultShifting = false; //Does the vehicle automatically switch to the below gears when going forward/back
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
const string RotorRPMString = " RPM\n";
//Look at modular block naming based on keyword + gear, right idea previously.
//everything up here is saved permenantly, use for adding/error codes and that sort of thing? Great for argument based stuff like switching around variables.

//------------------------------------------------------
// ============== Don't touch below here ===============
//------------------------------------------------------
MyCommandLine _commandLine = new MyCommandLine();
Dictionary<string, Action> _commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);

float VehicleSpeed;
float CurrentGearSpeed;
string ExpectedTopSpeed;

const double updatesPerSecond = 10;
const double updateTime = 1 / updatesPerSecond;

const double refreshInterval = 10;
double timeSinceRefresh = 141;
bool isSetup = false;

bool Reverse = false;

IMyTextPanel echopanel;
IMyCockpit DriverSeat;
IMyProgrammableBlock self;
IMyTimerBlock TimerForwardBlock;
IMyTimerBlock TimerNeutralBlock;
IMyTimerBlock TimerReverseBlock;
IMyRemoteControl RemoteSeat;

bool ReverseMode = false;//Whether the vehicle has reversed below 1, and not pressed W yet
bool TempBraked = false; //Whether you're braked or not
bool LeftKeyPressed = false;
bool RightKeyPressed = false;
public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main(string argument, UpdateType updateType)
{
    string STATUS = "";
    string ERROR_TXT = "";
    float CurrentGearInput = 1f;

    if (TimerUse == true)
    {
        TimerForwardBlock = GridTerminalSystem.GetBlockWithName(TimerForward) as IMyTimerBlock;
        TimerNeutralBlock = GridTerminalSystem.GetBlockWithName(TimerNeutral) as IMyTimerBlock;
        TimerReverseBlock = GridTerminalSystem.GetBlockWithName(TimerReverse) as IMyTimerBlock;
    }

    RemoteSeat = GridTerminalSystem.GetBlockWithName(DriverSeatName) as IMyRemoteControl;
    echopanel = GridTerminalSystem.GetBlockWithName(StatusPanel) as IMyTextPanel;
    DriverSeat = GridTerminalSystem.GetBlockWithName(DriverSeatName) as IMyCockpit; //optional
    self = Me;

    IMyBlockGroup AllRotorsGroup = GridTerminalSystem.GetBlockGroupWithName(AllRotorsGroupName);
    List<IMyMotorStator> AllRotorsList = new List<IMyMotorStator>();
    if (AllRotorsGroup != null)
    {
        AllRotorsGroup.GetBlocksOfType(AllRotorsList);//asigns all the rotors in this group the the list, I think
    }
    else
    {
        Echo(AllRotorsGroupName + " group missing or misnamed");
    }

    IMyBlockGroup LeftRotorsGroup = GridTerminalSystem.GetBlockGroupWithName(LeftRotorsGroupName);
    List<IMyMotorStator> LeftRotorsList = new List<IMyMotorStator>();
    if (LeftRotorsGroup != null)
    {
        LeftRotorsGroup.GetBlocksOfType(LeftRotorsList);//asigns all the rotors in this group the the list, I think
    }
    else
    {
        Echo(LeftRotorsGroupName + " group missing or misnamed");
    }

    IMyBlockGroup RightRotorsGroup = GridTerminalSystem.GetBlockGroupWithName(RightRotorsGroupName);
    List<IMyMotorStator> RightRotorsList = new List<IMyMotorStator>();
    if (RightRotorsGroup != null)
    {
        RightRotorsGroup.GetBlocksOfType(RightRotorsList);//asigns all the rotors in this group the the list, I think
    }
    else
    {
        Echo(RightRotorsGroupName + " group missing or misnamed");
    }


    //make left group gearshift input invert on use

    if (TimerUse == true)
        if (TimerShutoff == true)
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
            else// if reversing!
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
    //------------------------------------------------------------//
    //SETUP
    //------------------------------------------------------------//






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
    //------------------------------------------------------------//
    //SETUP
    //------------------------------------------------------------//





    //Get estimated top speeds for current gear
    if (TargetSpeedDisplay == true)
    {
        if (Reverse == false)//If going forwards
        {
            ExpectedTopSpeed = Convert.ToString(Math.Round((float)(MaximumRPM / ForwardGearCount * CurrentGearInput), 0)) + RotorRPMString;
        }
        else//if reversing
        {
            ExpectedTopSpeed = "-" + Convert.ToString(Math.Round((float)(ReverseRPM / ReverseGearCount * CurrentGearInput), 0)) + RotorRPMString;
        }
    }
    else
    {
        ExpectedTopSpeed = "";
        ExpectedSpeedString = "";
    }


    if (DriverSeat == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (RemoteSeat == null)
        {
            Echo("Make sure there is a seat or control being used with the name " + DriverSeatName + "\n");
            return;
        }
        else
        {
            controller = RemoteSeat;//control methods used by remote
        }
    }
    else
    {
        controller = DriverSeat;//control methods used by cockpit
    }

    try
    {
        var inputVec = controller.MoveIndicator; //USE THESE SECTIONS TO RUN THE GEAR FUNCTIONS, USE WHIPS STEERING SCRIPT HERE AS AN EXAMPLE, GRAB ALL WHEELS

        VehicleSpeed = (float)Math.Round(((float)controller.GetShipSpeed() * 3.6f), 1);
        if (VehicleSpeed < 0)
        {
            VehicleSpeed = (float)Math.Round((((float)controller.GetShipSpeed() * -1) * 3.6f), 1);
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

            if (ReverseMode == false || CurrentGearInput == 1)//if reverse mode is false, or current gear is 1 (implying you're coming out of reverse and moving up){
            {
                CurrentGearInput = (float.Parse(Me.CustomData) + 1f); //Take current gear before it gets broken by the string, then add one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                STATUS += "\nSHIFTING UP";
                GearShift(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearInput); //Shift gears when moving forwards or backwards. Placed here as not to change when not accel/deccel.
                ReverseMode = false;
            }
            else
            {
                CurrentGearInput = (float.Parse(Me.CustomData) - 1f); //Take current gear before it gets broken by the string, then add one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                GearShiftReverse(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearInput);
            }



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
                + (float)Math.Round((((float)controller.GetShipSpeed() * -1) * 3.6f), 1) + "km/h\n" //inverts vehicle speed calc, displayed on text panel
                + ExpectedSpeedString + ExpectedTopSpeed;


            Reverse = true; //You are reversing

            if (ReverseMode == false && CurrentGearInput > 1)
            {
                CurrentGearInput = (float.Parse(Me.CustomData) - 1f); //Take current gear before it gets broken by the string, then subtract one to it.
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                GearShift(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearInput); //Shift gears when moving forwards or backwards. Placed here as not to change when not accel/deccel.
            }
            else //how you get down into reverse mode
            {
                STATUS += "\nreversemode true";
                CurrentGearInput = (float.Parse(Me.CustomData) + 1f); //Take current gear before it gets broken by the string, then add one to it - this is for reversing
                Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                ReverseMode = true;//stops it from 'bouncing' up and down in and out of this
                GearShiftReverse(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearInput); //Shift gears when moving forwards or backwards. Placed here as not to change when not accel/deccel.

            }




            STATUS += "\nSHIFTING DOWN";

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

            //Current Shift on all rotors

        }

        if (QEShifting == true)
        {
            if (controller.RollIndicator != 0)
            {
                GearShift(AllRotorsList,LeftRotorsList,RightRotorsList, CurrentGearInput); //Shift gears when moving forwards or backwards. Placed here as not to change when not accel/deccel.

                if (controller.RollIndicator < 0)//When holding Q, perma brake
                {
                    CurrentGearInput = (float.Parse(Me.CustomData) - 1f); //Take current gear before it gets broken by the string, then add one to it.
                    Me.CustomData = Convert.ToString((CurrentGearInput)); //Then take wipe the string and turn it into the modified number.
                    STATUS += "\nSHIFTING";
                }
                else if (controller.RollIndicator > 0 && TempBraked == false)//When holding E, temp brake
                {
                    BrakeAllRotors(AllRotorsList, out TempBraked);
                    STATUS += "\nBRAKING";

                }///////////////////////////////////////////FIX TEMP BRAKING< IS PERMANENT RN

            }
            else if (TempBraked == true)
            {
                TurnOnAllRotors(AllRotorsList);
                TempBraked = false;
            }

        }


        //Turning


        if (inputVec.X < 0 && LeftKeyPressed == false) //A is pressed (Turning left) and you aren't already turning left
        {
            STATUS += "\nLEFT TURN ACTIVE";


            LeftKeyPressed = true;
            TurnLeftFunction(LeftRotorsList, LeftKeyPressed);
        }
        else if (inputVec.X > 0 && RightKeyPressed == false)
        {
            STATUS += "\nRIGHT TURN ACTIVE";


            RightKeyPressed = true;
            TurnRightFunction(RightRotorsList, RightKeyPressed); //D is pressed(Turning Right)
        }
        else if (inputVec.X == 0)//do this when you stop pressing the keys
        {
            if (LeftKeyPressed == true)
            {
                LeftKeyPressed = false;
                TurnLeftFunction(LeftRotorsList, LeftKeyPressed);
            }

            if (RightKeyPressed == true)
            {
                RightKeyPressed = false;
                TurnRightFunction(RightRotorsList, RightKeyPressed); //Keep it driving
            }
        }

        //during driving/runtime, need some way of knowing "I am driving" so that when turning, you can immediately

        //put conditional in function so if direction key was not pressed, it does something slightly different?

        //Turning end


        if (inputVec.Z == 0) //Nor forwards or backwards
        {
            Echo(GearLetter + ":" + Convert.ToString(CurrentGearInput) + NeutralLetter);//Make constants for Forwards/Backwards/Neutral

            STATUS = GearName + ":\n"
                + CurrentGearInput + NeutralLetter + "\n"
                + VehicleSpeed + "km/h\n"
                + ExpectedSpeedString + ExpectedTopSpeed;

            if (NeutralBraking == true) //If neutral braking is on, auto brake when there is no input.
            {
                controller.HandBrake = true;
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
            controller.HandBrake = false; //TEMPORARY, LATER BRING TO COMFORTABLE STOP OR GEAR DOWN /THEN HANDBRAKE BASED ON VARIABLE/
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

List<IMyShipController> controllers = new List<IMyShipController>();
IMyShipController controller = null;

bool GrabBlocks() //Literally just here to get the blocks or throw errors, I think
{
    GridTerminalSystem.GetBlocksOfType(controllers);
    if (controllers.Count == 0)
    {
        Echo($"Error: No ship controller named found");
        return false;
    }
    controller = GetControlledShipController(controllers);

    if (GridTerminalSystem.GetBlockGroupWithName(LeftRotorsGroupName) == null)
    {
        Echo("Error: No "+LeftRotorsGroupName+" group found!");
        return false;
    }

    else if (GridTerminalSystem.GetBlockGroupWithName(RightRotorsGroupName) == null)
    {
        Echo("Error: No " + RightRotorsGroupName + " group found!");
        return false;
    }

    else if (GridTerminalSystem.GetBlockGroupWithName(AllRotorsGroupName) == null)
    {
        Echo("Error: No " + AllRotorsGroupName + " group found!");
        return false;
    }

    return true;
}




public void GearShift(List<IMyMotorStator> AllRotorsList, List<IMyMotorStator> LeftRotorsList, List<IMyMotorStator> RightRotorsList, float CurrentGearInput)
{
    if (Reverse == false) //If moving forwards
    {
        CurrentGearSpeed = (MaximumRPM / ForwardGearCount * CurrentGearInput); //Set current gear speed to that of top speed divided by set amount of forward gears.
    }
    else //otherwise, if reversing.
    {
        CurrentGearSpeed = (ReverseRPM / ReverseGearCount * CurrentGearInput); //Do the same, but obeying the reverse system.
    }

    SetRotorForwardRPM(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearSpeed); //Whatever current gear is, run the gear shift function.
}

//when driving forwards, you'll want to set up voids like this for steering left/right reversals too (perhaps speed reduction? if you want it to be easier, reversal)
public void SetRotorForwardRPM(List<IMyMotorStator> AllRotorsList, List<IMyMotorStator> LeftRotorsList, List<IMyMotorStator> RightRotorsList, float CurrentGearSpeed) //The function that actually sets wheel speed.
{

    string TextCurrentGearSpeed = CurrentGearSpeed.ToString(); //Converts the gear speed  double into a string that the text panel can read.

    //foreach (var AllRotorsGroup in AllRotorsList)
    //{
    //    AllRotorsGroup.TargetVelocityRPM = CurrentGearSpeed; //Set wheel speed to whatever CurrentGearSpeed is.
    //}

    foreach (var LeftRotorsGroup in LeftRotorsList)
    {
        LeftRotorsGroup.TargetVelocityRPM = CurrentGearSpeed*-1f; //Set wheel speed to whatever CurrentGearSpeed is.
    }

    foreach (var RightRotorsGroup in RightRotorsList)
    {
        RightRotorsGroup.TargetVelocityRPM = CurrentGearSpeed; //Set wheel speed to whatever CurrentGearSpeed is.
    }
}


//When the transmission has reached gear 1 when reversing, figure out some form of reverse you slag!
public void GearShiftReverse(List<IMyMotorStator> AllRotorsList, List<IMyMotorStator> LeftRotorsList, List<IMyMotorStator> RightRotorsList, float CurrentGearInput)
{

    CurrentGearSpeed = (ReverseRPM / ReverseGearCount * CurrentGearInput);

    SetRotorReverseRPM(AllRotorsList, LeftRotorsList, RightRotorsList, CurrentGearSpeed); //Whatever current gear is, run the gear shift function.
}

//when driving forwards, you'll want to set up voids like this for steering left/right reversals too (perhaps speed reduction? if you want it to be easier, reversal)
public void SetRotorReverseRPM(List<IMyMotorStator> AllRotorsList, List<IMyMotorStator> LeftRotorsList, List<IMyMotorStator> RightRotorsList, float CurrentGearSpeed) //The function that actually sets wheel speed.
{

    string TextCurrentGearSpeed = CurrentGearSpeed.ToString(); //Converts the gear speed  double into a string that the text panel can read.

    //foreach (var AllRotorsGroup in AllRotorsList)
    //{
    //    AllRotorsGroup.TargetVelocityRPM = CurrentGearSpeed; //Set wheel speed to whatever CurrentGearSpeed is.
    //}

    foreach (var LeftRotorsGroup in LeftRotorsList)
    {
        LeftRotorsGroup.TargetVelocityRPM = CurrentGearSpeed; //Set wheel speed to whatever CurrentGearSpeed is.
    }

    foreach (var RightRotorsGroup in RightRotorsList)
    {
        RightRotorsGroup.TargetVelocityRPM = CurrentGearSpeed * -1f; //Set wheel speed to whatever CurrentGearSpeed is.
    }
}



public void BrakeAllRotors(List<IMyMotorStator> AllRotorsList, out bool TempBraked)
{
    TempBraked = true;
    foreach (var AllRotorsGroup in AllRotorsList)
    {
        AllRotorsGroup.Enabled = false;
    }

}


public void TurnOnAllRotors(List<IMyMotorStator> AllRotorsList)
{
    foreach (var AllRotorsGroup in AllRotorsList)
    {
        AllRotorsGroup.Enabled = true;
    }
}





public void TurnLeftFunction(List<IMyMotorStator> LeftRotorsList, bool LeftKeyPressed)
{




    if (LeftKeyPressed == true)//Turn left start/during
    {
        foreach (var LeftRotorsGroup in LeftRotorsList)
        {
            LeftRotorsGroup.Enabled = !LeftRotorsGroup.Enabled;
        }
    }
    else//Turn left stop, reactivate braked side
    {
        foreach (var LeftRotorsGroup in LeftRotorsList)
        {
            LeftRotorsGroup.Enabled = true;
        }
    }



}





public void TurnRightFunction(List<IMyMotorStator> RightRotorsList, bool RightKeyPressed)
{




    if (RightKeyPressed == true)//Turn Right start/during
    {
        foreach (var RightRotorsGroup in RightRotorsList)
        {
            RightRotorsGroup.Enabled = !RightRotorsGroup.Enabled;
        }
    }
    else//Turn Right stop, reactivate braked side
    {
        foreach (var RightRotorsGroup in RightRotorsList)
        {
            RightRotorsGroup.Enabled = true;
        }
    }



}

//--------------------------------------------------------------------------------------
IMyShipController GetControlledShipController(List<IMyShipController> SCs) //Method for grabbing seat controller
{
    foreach (IMyShipController thisController in SCs)
    {
        if (thisController.IsUnderControl && thisController.CanControlShip)
            return thisController;
    }
    return SCs[0];
}