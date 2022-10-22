
//AWG's Thruster Throttle Script//

//An old but helpful script that controls thruster usage with W/S, works with planes, amphib tanks, so on


//put a 0 in your custom data at the start, if it is error throwing for no reason. Read config carefully especially regarding screen usage and names.

const string ControlSeatName = "Control Seat"; //Control seat name.
const string ThrusterGroupName = "Thrusters"; //Thruster group name.

const float MinimumThrust = 0.01f; //The minimum % the thrusters will lower to before stopping. (Min 0)
const float MaximumThrust = 1f; //Maximum % the thrusters will raise to before stopping. (Max 1)
const bool MinThrustShutdown = true; //Do thrusters shut down at min thrust?

const bool UsingTimers = false; //Do you want thrust level to trigger timers? Set names and specifics below. ---- Turning this off improves perf.
const string TimerLowerName = "Low Thrust Timer"; //Optional timer triggered for when thrust is lowered to or past this %
const string TimerHigherName = "High Thrust Timer"; //Optional timer triggered for when thrust is lowered to or past this %
const float TimerLowerTrigger = 0.25f; //% amount to be below to trigger the timer
const float TimerHigherTrigger = 0.75f; //% amount to be above to trigger the timer

const bool UsingScreen = false; //Outputting to a screen of some form? This enables that function. ---- Turning this off improves perf.

const bool UsingTextPanel = true; //If you are using a dedicated LCD or text panel set to true, if false, will search for PB or cockpit.
const string LCDName = "Thrust Output Panel"; //The name of the block.
const string LCDPrefix = "Thrust:\n"; //Backslash n is a linebreak. Put before the thrust %
const string LCDAffix = "%"; //put after thrust %
const int DisplayNo = 0; //which display on the block to use if you're using a cockpit (List starts at 0)

const int DecimalPlace = 2; //How accurate is the thrust override percentage?
const float ThrottleDeltaMultiplier = 5f; //raising this changes how fast override changes.

//No touch-y below-y

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main(string argument, UpdateType updateSource)
{
    IMyCockpit ControlSeat;
    IMyTimerBlock TimerLower;
    IMyTimerBlock TimerHigher;
    IMyTextSurfaceProvider TextSurfaceProvider;
    IMyTextSurface TextSurface;
    IMyTextPanel TextPanel;

    if (Me.CustomData == "")
    {
        Me.CustomData = "1";
    }

    float ThrustPercentage = float.Parse(Me.CustomData);//Makes custom data into an integer
    //Now I can make decisions based on whatever thrust is!

    ControlSeat = GridTerminalSystem.GetBlockWithName(ControlSeatName) as IMyCockpit;
    IMyBlockGroup ThrustGroup = GridTerminalSystem.GetBlockGroupWithName(ThrusterGroupName);

    List<IMyThrust> Thrusters = new List<IMyThrust>();

    if (ThrustGroup != null)
    {
        ThrustGroup.GetBlocksOfType(Thrusters);
    }
    else
    {
        Echo(ThrusterGroupName + " group missing or misnamed");
    }

    if (ControlSeat != null)
    {
        if (ControlSeat.IsUnderControl == false)
        {
            return;
        }
        else
        {
            var inputVec = ControlSeat.MoveIndicator;

            if (inputVec.Z < 0) //W is pressed
            {
                Increment(Thrusters, MaximumThrust); //if this is true, run the function to actually make permenant changes in the program
            }
            else if (inputVec.Z > 0)//S is pressed
            {
                Decrement(Thrusters, MinimumThrust, MinThrustShutdown);
            }
            else
            {
                return;
            }

            //Timers

            //Now that the functions have run...
            if (UsingTimers == true)
            {
                TimerLower = GridTerminalSystem.GetBlockWithName(TimerLowerName) as IMyTimerBlock;
                TimerHigher = GridTerminalSystem.GetBlockWithName(TimerHigherName) as IMyTimerBlock;

                if (Me.ShowInInventory == true) //You are increasing thrust
                {
                    if (TimerHigher == null)
                    {
                        Echo(TimerHigher + "missing or misnamed.");
                    }
                    else
                    {
                        if (ThrustPercentage >= TimerHigherTrigger)
                        {
                            //if timerhigher hasn't been triggered above limit, trigger, then add tag to CD
                            if (TimerHigher.CustomData != "Triggered")
                            {
                                TimerHigher.Trigger();
                                TimerHigher.CustomData = "Triggered";
                            }
                        }
                        else if (TimerHigher.CustomData == "Triggered") //if the thrust is not higher than trigger but you're raising, and it previously was triggered, raise it.
                        {
                            TimerHigher.CustomData = "";
                        }
                    }
                }
                else //You are decreasing thrust
                {
                    if (TimerLower == null)
                    {
                        Echo(TimerLower + "missing or misnamed.");
                    }
                    else
                    {
                        if (ThrustPercentage <= TimerLowerTrigger)
                        {
                            if (TimerLower.CustomData != "Triggered")
                            {
                                TimerLower.Trigger();
                                TimerLower.CustomData = "Triggered";
                            }
                        }
                        else if (TimerLower.CustomData == "Triggered")
                        {
                            TimerLower.CustomData = "";
                        }
                    }
                }
            }

            if (UsingScreen == true)//if you're using some kind of screen
            {
                if (UsingTextPanel == false)//if ur not using LCD/text panel
                {
                    TextSurfaceProvider = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextSurfaceProvider;
                    TextSurface = TextSurfaceProvider.GetSurface(DisplayNo);

                    TextSurface.WriteText(LCDPrefix + (ThrustPercentage * 100) + LCDAffix);
                }
                else
                {
                    TextPanel = GridTerminalSystem.GetBlockWithName(LCDName) as IMyTextPanel;

                    TextPanel.WriteText(LCDPrefix + (ThrustPercentage * 100) + LCDAffix);
                }



            }
        }
    }
    else
    {
        Echo(ControlSeatName + " missing or misnamed");
    }
}

public void Increment(List<IMyThrust> Thrusters, float MaximumThrust)
{
    foreach (var ThrustGroup in Thrusters)
    {
        ThrustGroup.Enabled = true;
        ThrustGroup.ThrustOverridePercentage = ThrustGroup.ThrustOverridePercentage + (0.001f * ThrottleDeltaMultiplier);

        if (ThrustGroup.ThrustOverridePercentage > MaximumThrust)//If thrust override below minthrust, set back to min
        {
            ThrustGroup.ThrustOverridePercentage = MaximumThrust;
        }

        Me.CustomData = "" + Math.Round((ThrustGroup.ThrustOverridePercentage), DecimalPlace);
    } //First line wakes thrusters, second increases throttle + multiplies, third edits custom data w/out string conversion

    Me.ShowInInventory = true; //Says that the program should check for the relevant timer
}

public void Decrement(List<IMyThrust> Thrusters, float MinimumThrust, bool MinThrustShutdown)
{
    foreach (var ThrustGroup in Thrusters)
    {
        ThrustGroup.ThrustOverridePercentage = ThrustGroup.ThrustOverridePercentage - (0.001f * ThrottleDeltaMultiplier);

        if (ThrustGroup.ThrustOverridePercentage < MinimumThrust)//If thrust override below minthrust, set back to min
        {
            ThrustGroup.ThrustOverridePercentage = MinimumThrust;

            if (MinThrustShutdown == true) //if minthrustshutdown true, turn off thrusters
            {
                ThrustGroup.Enabled = false;
            }
        }

        Me.CustomData = "" + Math.Round((ThrustGroup.ThrustOverridePercentage), DecimalPlace);
    }

    Me.ShowInInventory = false; //Says that the program should check for the relevant timer
}