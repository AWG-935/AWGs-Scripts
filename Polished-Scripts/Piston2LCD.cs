
//AWG'S Piston2LCD Script//

//Displays piston extension.
//Triggers timers based on piston extension/retraction



const string ExtendMessage = "Piston\nExtent\n"; //Header of extension, leave blank for nothing. 'Backslash n' makes a linebreak
const int DecimalPoint = 2; //What decimal value should the script round the piston position to?
const string Unit = "m"; //Length unit, the after-number text. Use a linebreak here if you want to display more text.



const bool Using_Cockpit = false; //if using this with a cockpit, look for one, everything below this depends on this.
const string CockpitName = "Operator Cockpit";
const int DisplayNumber = 1; //Which display to use in the cockpit. Starts from 0. 1 is the middle cockpit in some.



const string PistonName = "Stat Piston"; // Measured Piston 1
const string StatusPanelName = "Status Panel"; // Display Panel
const string ProgramName = "Program Piston2LCD"; // Name of PB

//Timer Zone//

const bool Using_Timers = false; //If using the timer system for different triggers on extention/retraction. No error checking.

const string TimerExtendName = "Extend Timer"; // Name of timer triggered once past extension limit
const string TimerRetractName = "Retract Timer"; // Name of timer triggered once past extension limit

const float ExtendTriggerPos = 2f; //What position to trigger the timer on extention
const float RetractTriggerPos = 0f; //What position to trigger the timer on retraction

//------------------------------------------------------
// ============== Don't touch below here ===============
//------------------------------------------------------




IMyProgrammableBlock SelfReferenceProgramBlock;
IMyPistonBase StatPiston;
IMyTextPanel EchoPanel;
IMyTextSurfaceProvider CockpitUser;
IMyTextSurface ScreenVariable;
IMyTimerBlock TimerExtend;
IMyTimerBlock TimerRetract;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update10;//Runs itself every 0.6s, therefore make sure to use custom datas of this program as memory
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main()
{

    StatPiston = GridTerminalSystem.GetBlockWithName(PistonName) as IMyPistonBase;
    EchoPanel = GridTerminalSystem.GetBlockWithName(StatusPanelName) as IMyTextPanel;
    SelfReferenceProgramBlock = GridTerminalSystem.GetBlockWithName(ProgramName) as IMyProgrammableBlock;
    CockpitUser = GridTerminalSystem.GetBlockWithName(CockpitName) as IMyTextSurfaceProvider;
    ScreenVariable = CockpitUser.GetSurface(DisplayNumber); //Gets a text surface from the cockpit with a number from the constants above and saves as a text surface
    TimerExtend = GridTerminalSystem.GetBlockWithName(TimerExtendName) as IMyTimerBlock;
    TimerRetract = GridTerminalSystem.GetBlockWithName(TimerRetractName) as IMyTimerBlock;

    string STATUS = "";
    string ERROR_TXT = "";
    if (SelfReferenceProgramBlock == null) //Checks if text panel exists
    {
        Echo("No PB named " + ProgramName + "\n");
        ERROR_TXT += "No PB named " + ProgramName + "\n"; //report this to the program and panel
        return;
    }

    if (StatPiston != null)//if StatPiston is detectable, check if it's functional
    {
        if (StatPiston.IsFunctional != false)//if StatPiston is not broken, check and report its position
        {

            STATUS += ExtendMessage+Math.Round(StatPiston.CurrentPosition,DecimalPoint)+Unit; //Custom user message, and current position
        }
        else
        {
            Echo(PistonName + " damaged.");//Self explanatory
            STATUS += " damaged.";
        }
    }
    else
    {
        Echo(PistonName + " destroyed.");//Self explanatory
        STATUS += PistonName + " destroyed or missing.";
    }

    //--------------------------------------------------
    //Cockpit screen system
    //--------------------------------------------------

    if (Using_Cockpit == true)
    {
        if (CockpitUser != null)
        {
            if (ERROR_TXT != "") //if there is an error reported
            {
                ScreenVariable.WriteText("Script Errors:\n" + ERROR_TXT + "(Check ownership)\n\n" + STATUS); //report the error to the panel
            }
            else
            {
                ScreenVariable.WriteText(STATUS);
            } //if there are no errors, just report the status!
        }
        else
        {
            ERROR_TXT = CockpitUser+"missing or misnamed.";
        }
    }
    else
    {
        try
        {
            ScreenVariable.WriteText("");
        }
        catch
        {

        }
    }

    if (Using_Timers == true)
    {

        if (Math.Round(StatPiston.CurrentPosition, DecimalPoint) >= ExtendTriggerPos) //If piston current position greater than extension position trigger timer
        {
            if (TimerExtend.CustomData == "")//make sure it triggers timer once
            {
                TimerExtend.Trigger();
                TimerExtend.CustomData = "Triggered";
            }
        }
        else if (TimerExtend.CustomData == "Triggered") //if it's not greater than the trigger point, and has triggered, clear it.
        {
            TimerExtend.CustomData = "";
        }

        if (Math.Round(StatPiston.CurrentPosition, DecimalPoint) <= (RetractTriggerPos)) //if the piston current position is lower than its trigger timer
        {
            if (StatPiston.CurrentPosition != StatPiston.MinLimit)//First check the piston is not at rest
            {
                if (TimerRetract.CustomData == "")//make sure it triggers timer once
                {
                    TimerRetract.Trigger();
                    TimerRetract.CustomData = "Triggered";
                }
            }
            else if (RetractTriggerPos == StatPiston.MinLimit)
            {
                if (TimerRetract.CustomData == "")//make sure it triggers timer once
                {
                    TimerRetract.Trigger();
                    TimerRetract.CustomData = "Triggered";
                }
            }

        }
        else if (TimerRetract.CustomData == "Triggered") //if it's not lower than the trigger point, and has triggered, clear it.
        {
            TimerRetract.CustomData = "";
        }
    }
    else
    {
        try
        {
            TimerRetract.CustomData = "";
        }
        catch
        {

        }
    }
    //--------------------------------------------------
    //Error Checking
    if (EchoPanel == null) //Checks if text panel exists
    {
        Echo("No screen named " + StatusPanelName + "\n");
    }
    else
    {
        if (ERROR_TXT != "") //if there is an error reported
        {
            EchoPanel.WriteText("Script Errors:\n" + ERROR_TXT + "(Check ownership)\n\n" + STATUS); //report the error to the panel
        }
        else
        {
            EchoPanel.WriteText(STATUS);
            Echo(STATUS + "\n");
        } //if there are no errors, just report the status!
    }
    //Error Checking End
}