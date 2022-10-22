/*
 * R e a d m e
 * -----------
 * 
 * Modular rotor control script (up to 5 rotors) that can key control 
 * them with WS/AD/QE, with variable speed and return velocity, as well as
 * timer triggering based on status.
 * 
 */

//
//
//RotorEntry RotorModular1 Start -------------------------->

const string Name_RM1 = "Rotor Blade Tilt 1"; //Name of rotor.

bool ControlWS_RM1 = true;
bool ControlAD_RM1 = false;       //Put this next to whichever one is true, all others must be false
bool ControlQE_RM1 = false;

float MoveSpeed_RM1 = -4f; //Arbitrary move speed of the rotor. Tweak to perfection.
float StopVelocity_RM1 = 0f; //When you stop moving the rotor, what velocity does it stop at? (0 recommended)

string ControlBlock_RM1 = "Seat Pilot"; //Seat, cockpit or remote control, what is the name?
string OutputPanel_RM1 = "Tilt Panel"; //Where should this rotor angle be sent?
string TimerStartName_RM1 = "Timer Start"; //Optional timer for when you start pressing this
string TimerStopName_RM1 = "Timer Stop"; //Optional timer for when you stop pressing this
string Status_RM1 = "";
//Rotor Entry RM1 End -------------------------->
//
//


//
//
//RotorEntry RotorModular2 Start -------------------------->

const string Name_RM2 = "Rotor Blade Tilt 2"; //Name of rotor.

bool ControlWS_RM2 = true;
bool ControlAD_RM2 = false;       //Put this next to whichever one is true, all others must be false
bool ControlQE_RM2 = false;

float MoveSpeed_RM2 = 4f; //Arbitrary move speed of the rotor. Tweak to perfection.
float StopVelocity_RM2 = 0f; //When you stop moving the rotor, what velocity does it stop at? (0 recommended)

string ControlBlock_RM2 = "Seat Pilot"; //Seat, cockpit or remote control, what is the name?
string OutputPanel_RM2 = "Status Panel"; //Where should this rotor angle be sent?
string TimerStartName_RM2 = "Timer Start"; //Optional timer for when you start pressing this
string TimerStopName_RM2 = "Timer Stop"; //Optional timer for when you stop pressing this
string Status_RM2 = "";

//Rotor Entry RM2 End -------------------------->
//
//


//
//
//RotorEntry RotorModular3 Start -------------------------->

const string Name_RM3 = "Rotor Blade Tilt 3"; //Name of rotor.

bool ControlWS_RM3 = true;
bool ControlAD_RM3 = false;       //Put this next to whichever one is true, all others must be false
bool ControlQE_RM3 = false;

float MoveSpeed_RM3 = 4f; //Arbitrary move speed of the rotor. Tweak to perfection.
float StopVelocity_RM3 = 0f; //When you stop moving the rotor, what velocity does it stop at? (0 recommended)

string ControlBlock_RM3 = "Seat Pilot"; //Seat, cockpit or remote control, what is the name?
string OutputPanel_RM3 = "Status Panel"; //Where should this rotor angle be sent?
string TimerStartName_RM3 = "Timer Start"; //Optional timer for when you start pressing this
string TimerStopName_RM3 = "Timer Stop"; //Optional timer for when you stop pressing this
string Status_RM3 = "";

//Rotor Entry RM3 End -------------------------->
//
//


//
//
//RotorEntry RotorModular4 Start -------------------------->

const string Name_RM4 = "Rotor Blade Tilt 4"; //Name of rotor.

bool ControlWS_RM4 = true;
bool ControlAD_RM4 = false;       //Put this next to whichever one is true, all others must be false
bool ControlQE_RM4 = false;

float MoveSpeed_RM4 = 4f; //Arbitrary move speed of the rotor. Tweak to perfection.
float StopVelocity_RM4 = 0f; //When you stop moving the rotor, what velocity does it stop at? (0 recommended)

string ControlBlock_RM4 = "Seat Pilot"; //Seat, cockpit or remote control, what is the name?
string OutputPanel_RM4 = "Status Panel"; //Where should this rotor angle be sent?
string TimerStartName_RM4 = "Timer Start"; //Optional timer for when you start pressing this
string TimerStopName_RM4 = "Timer Stop"; //Optional timer for when you stop pressing this
string Status_RM4 = "";

//Rotor Entry RM4 End -------------------------->
//
//


//
//
//RotorEntry RotorModular5 Start -------------------------->

const string Name_RM5 = "Blade Forward Shift"; //Name of rotor.

bool ControlWS_RM5 = false;
bool ControlAD_RM5 = true;       //Put this next to whichever one is true, all others must be false
bool ControlQE_RM5 = false;

float MoveSpeed_RM5 = -1f; //Arbitrary move speed of the rotor. Tweak to perfection.
float StopVelocity_RM5 = 0f; //When you stop moving the rotor, what velocity does it stop at? (0 recommended)

string ControlBlock_RM5 = "Seat Pilot"; //Seat, cockpit or remote control, what is the name?
string OutputPanel_RM5 = "Forward Panel"; //Where should this rotor angle be sent?
string TimerStartName_RM5 = "Timer Start"; //Optional timer for when you start pressing this
string TimerStopName_RM5 = "Timer Stop"; //Optional timer for when you stop pressing this
string Status_RM5 = "";

//Rotor Entry RM5 End -------------------------->
//
//








//------------------------------------------------------
// ============== Don't touch below here ===============
//------------------------------------------------------
MyCommandLine _commandLine = new MyCommandLine();
Dictionary<string, Action> _commands = new Dictionary<string, Action>(StringComparer.OrdinalIgnoreCase);


const double updatesPerSecond = 10;
const double updateTime = 1 / updatesPerSecond;

const double refreshInterval = 10;
double timeSinceRefresh = 141;
bool isSetup = false;


//
//RM1----------------------------
IMyMotorStator MotorStator_RM1;
IMyMotorAdvancedStator AdvancedMotorStator_RM1;
IMyCockpit Cockpit_RM1;
IMyRemoteControl Remote_RM1;
IMyTextPanel LCD_RM1;
IMyTimerBlock TimerStart_RM1;
IMyTimerBlock TimerStop_RM1;
//RM1----------------------------
//
//
//
//
//RM2----------------------------
IMyMotorStator MotorStator_RM2;
IMyMotorAdvancedStator AdvancedMotorStator_RM2;
IMyCockpit Cockpit_RM2;
IMyRemoteControl Remote_RM2;
IMyTextPanel LCD_RM2;
IMyTimerBlock TimerStart_RM2;
IMyTimerBlock TimerStop_RM2;
//RM2----------------------------
//
//
//
//
//
//RM3----------------------------
IMyMotorStator MotorStator_RM3;
IMyMotorAdvancedStator AdvancedMotorStator_RM3;
IMyCockpit Cockpit_RM3;
IMyRemoteControl Remote_RM3;
IMyTextPanel LCD_RM3;
IMyTimerBlock TimerStart_RM3;
IMyTimerBlock TimerStop_RM3;
//RM3----------------------------
//
//
//
//
//
//RM4----------------------------
IMyMotorStator MotorStator_RM4;
IMyMotorAdvancedStator AdvancedMotorStator_RM4;
IMyCockpit Cockpit_RM4;
IMyRemoteControl Remote_RM4;
IMyTextPanel LCD_RM4;
IMyTimerBlock TimerStart_RM4;
IMyTimerBlock TimerStop_RM4;
//RM4----------------------------
//
//
//
//
//
//RM5----------------------------
IMyMotorStator MotorStator_RM5;
IMyMotorAdvancedStator AdvancedMotorStator_RM5;
IMyCockpit Cockpit_RM5;
IMyRemoteControl Remote_RM5;
IMyTextPanel LCD_RM5;
IMyTimerBlock TimerStart_RM5;
IMyTimerBlock TimerStop_RM5;
//RM5----------------------------
//
//
//


public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main(string argument, UpdateType updateType)
{
    //
    //RM1----------------------------
    MotorStator_RM1 = GridTerminalSystem.GetBlockWithName(Name_RM1) as IMyMotorStator;
    AdvancedMotorStator_RM1 = GridTerminalSystem.GetBlockWithName(Name_RM1) as IMyMotorAdvancedStator;
    Cockpit_RM1 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM1) as IMyCockpit;
    Remote_RM1 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM1) as IMyRemoteControl;
    LCD_RM1 = GridTerminalSystem.GetBlockWithName(OutputPanel_RM1) as IMyTextPanel;
    TimerStart_RM1 = GridTerminalSystem.GetBlockWithName(TimerStartName_RM1) as IMyTimerBlock;
    TimerStop_RM1 = GridTerminalSystem.GetBlockWithName(TimerStopName_RM1) as IMyTimerBlock;

    if (Cockpit_RM1 == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (Remote_RM1 == null)
        {
            Echo("No control methods found!");
            return;
        }
        else
        {
            controller = Remote_RM1;//control methods used by remote
        }
    }
    else
    {
        controller = Cockpit_RM1;//control methods used by cockpit
    }

    //Remember to consider putting movement in functions, as well as you've still gotta do custom
    //cockpit acquisition and movement!

    if (MotorStator_RM1 == null)
    {
        Echo(Name_RM1 + " missing!");
    }
    else
    {
        if (ControlWS_RM1 == false)
        {

            if (ControlAD_RM1 == false)
            {

                if (ControlQE_RM1 == false)
                {
                    //When you aren't doing Q/E movement
                }
                else
                {
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                    if (controller.RollIndicator < 0)//When holding E
                    {
                        MotorStator_RM1.TargetVelocityRPM = +MoveSpeed_RM1;
                        MotorStator_RM1.ShowInInventory = true;
                    }



                    else if (controller.RollIndicator > 0)//When holding Q
                    {
                        MotorStator_RM1.TargetVelocityRPM = -MoveSpeed_RM1;
                        MotorStator_RM1.ShowInInventory = true;
                    }



                    else
                    {
                        if (MotorStator_RM1.ShowInInventory == true)//Essrntially creates a one stop toggle
                        {
                            MotorStator_RM1.TargetVelocityRPM = StopVelocity_RM1;
                            MotorStator_RM1.ShowInInventory = false;

                        }
                        //implicit else
                    }
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                }
            }



            else
            {
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
                if (controller.MoveIndicator.X < 0)//When holding A
                {
                    MotorStator_RM1.TargetVelocityRPM = -MoveSpeed_RM1;
                    MotorStator_RM1.ShowInInventory = true;
                }



                else if (controller.MoveIndicator.X > 0)//When holding D
                {
                    MotorStator_RM1.TargetVelocityRPM = +MoveSpeed_RM1;
                    MotorStator_RM1.ShowInInventory = true;
                }



                else
                {
                    if (MotorStator_RM1.ShowInInventory == true)//Essrntially creates a one stop toggle
                    {
                        MotorStator_RM1.TargetVelocityRPM = StopVelocity_RM1;
                        MotorStator_RM1.ShowInInventory = false;

                    }
                    //implicit else
                }
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
            }

        }



        else
        {
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
            if (controller.MoveIndicator.Z < 0)//When holding W
            {
                MotorStator_RM1.TargetVelocityRPM = -MoveSpeed_RM1;
                MotorStator_RM1.ShowInInventory = true;
            }


            else if (controller.MoveIndicator.Z > 0)//When holding S
            {
                MotorStator_RM1.TargetVelocityRPM = +MoveSpeed_RM1;
                MotorStator_RM1.ShowInInventory = true;
            }


            else
            {
                if (MotorStator_RM1.ShowInInventory == true)//Essentially creates a one stop toggle
                {


                    MotorStator_RM1.TargetVelocityRPM = StopVelocity_RM1;
                    MotorStator_RM1.ShowInInventory = false;


                }
                //implicit else
            }
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
        }

        if (MotorStator_RM1.ShowInInventory == true)//When moving the rotor
        {


            if (TimerStart_RM1 != null)//if the timer exists
            {


                if (TimerStart_RM1.ShowInInventory != true)//Check if show in inventory is false, which means it has not been triggered this life
                {
                    TimerStart_RM1.Trigger();
                    TimerStart_RM1.ShowInInventory = true;//Set the timer's show in inventory to false when you know it's stopped moving
                }


            }



            if (TimerStop_RM1 != null)
            {
                TimerStop_RM1.ShowInInventory = false;//resets timer so it can be triggered again when the rotors stop moving
            }



            if (LCD_RM1 != null)//prints the angle to a panel, sleeps when unused
            {
                Status_RM1 = Math.Round(MathHelper.ToDegrees(MotorStator_RM1.Angle), 1).ToString("f2") + "°";
                LCD_RM1.WriteText(Name_RM1 + ":\n" + Status_RM1);
            }//set up a status system to have multiple angles at once
        }


        else//You're not moving the rotor any more
        {



            if (TimerStart_RM1 != null)//prevent crash by checking if this block is there
            {
                TimerStart_RM1.ShowInInventory = false;//reset one stop check
            }



            if (TimerStop_RM1 != null)
            {
                if (TimerStop_RM1.ShowInInventory != true)
                {
                    TimerStop_RM1.Trigger();
                    TimerStop_RM1.ShowInInventory = true;//Set the timer's show in inventory to false, it's set to true above when you start moving
                }
            }


        }
    }

    //RM1----------------------------
    //
    //


    //
    //RM2----------------------------
    MotorStator_RM2 = GridTerminalSystem.GetBlockWithName(Name_RM2) as IMyMotorStator;
    AdvancedMotorStator_RM2 = GridTerminalSystem.GetBlockWithName(Name_RM2) as IMyMotorAdvancedStator;
    Cockpit_RM2 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM2) as IMyCockpit;
    Remote_RM2 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM2) as IMyRemoteControl;
    LCD_RM2 = GridTerminalSystem.GetBlockWithName(OutputPanel_RM2) as IMyTextPanel;
    TimerStart_RM2 = GridTerminalSystem.GetBlockWithName(TimerStartName_RM2) as IMyTimerBlock;
    TimerStop_RM2 = GridTerminalSystem.GetBlockWithName(TimerStopName_RM2) as IMyTimerBlock;

    if (Cockpit_RM2 == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (Remote_RM2 == null)
        {
            Echo("No control methods found!");
            return;
        }
        else
        {
            controller = Remote_RM2;//control methods used by remote
        }
    }
    else
    {
        controller = Cockpit_RM2;//control methods used by cockpit
    }

    //Remember to consider putting movement in functions, as well as you've still gotta do custom
    //cockpit acquisition and movement!

    if (MotorStator_RM2 == null)
    {
        Echo(Name_RM2 + " missing!");
    }
    else
    {
        if (ControlWS_RM2 == false)
        {

            if (ControlAD_RM2 == false)
            {

                if (ControlQE_RM2 == false)
                {
                    //When you aren't doing Q/E movement
                }
                else
                {
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                    if (controller.RollIndicator < 0)//When holding E
                    {
                        MotorStator_RM2.TargetVelocityRPM = +MoveSpeed_RM2;
                        MotorStator_RM2.ShowInInventory = true;
                    }
                    else if (controller.RollIndicator > 0)//When holding Q
                    {
                        MotorStator_RM2.TargetVelocityRPM = -MoveSpeed_RM2;
                        MotorStator_RM2.ShowInInventory = true;
                    }
                    else
                    {
                        if (MotorStator_RM2.ShowInInventory == true)//Essrntially creates a one stop toggle
                        {
                            MotorStator_RM2.TargetVelocityRPM = StopVelocity_RM2;
                            MotorStator_RM2.ShowInInventory = false;

                        }
                        //implicit else
                    }
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                }
            }
            else
            {
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
                if (controller.MoveIndicator.X < 0)//When holding A
                {
                    MotorStator_RM2.TargetVelocityRPM = +MoveSpeed_RM2;
                    MotorStator_RM2.ShowInInventory = true;
                }
                else if (controller.MoveIndicator.X > 0)//When holding D
                {
                    MotorStator_RM2.TargetVelocityRPM = -MoveSpeed_RM2;
                    MotorStator_RM2.ShowInInventory = true;
                }
                else
                {
                    if (MotorStator_RM2.ShowInInventory == true)//Essrntially creates a one stop toggle
                    {
                        MotorStator_RM2.TargetVelocityRPM = StopVelocity_RM2;
                        MotorStator_RM2.ShowInInventory = false;

                    }
                    //implicit else
                }
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
            }

        }
        else
        {
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
            if (controller.MoveIndicator.Z < 0)//When holding W
            {
                MotorStator_RM2.TargetVelocityRPM = +MoveSpeed_RM2;
                MotorStator_RM2.ShowInInventory = true;
            }
            else if (controller.MoveIndicator.Z > 0)//When holding S
            {
                MotorStator_RM2.TargetVelocityRPM = -MoveSpeed_RM2;
                MotorStator_RM2.ShowInInventory = true;
            }
            else
            {
                if (MotorStator_RM2.ShowInInventory == true)//Essrntially creates a one stop toggle
                {
                    MotorStator_RM2.TargetVelocityRPM = StopVelocity_RM2;
                    MotorStator_RM2.ShowInInventory = false;

                }
                //implicit else
            }
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
        }

        if (MotorStator_RM2.ShowInInventory == true)//When moving the rotor
        {
            if (TimerStart_RM2 != null)//if the timer exists
            {
                if (TimerStart_RM2.ShowInInventory != true)//Check if show in inventory is false, which means it has not been triggered this life
                {
                    TimerStart_RM2.Trigger();
                    TimerStart_RM2.ShowInInventory = true;//Set the timer's show in inventory to false when you know it's stopped moving
                }
            }

            if (TimerStop_RM2 != null)
            {
                TimerStop_RM2.ShowInInventory = false;//resets timer so it can be triggered again when the rotors stop moving
            }

            if (LCD_RM2 != null)//prints the angle to a panel, sleeps when unused
            {
                Status_RM2 = Math.Round(MathHelper.ToDegrees(MotorStator_RM2.Angle), 1).ToString("f2") + "°";
                LCD_RM2.WriteText(Name_RM2 + ":\n" + Status_RM2);
            }//set up a status system to have multiple angles at once
        }
        else//You're not moving the rotor any more
        {


            if (TimerStart_RM2 != null)//prevent crash by checking if this block is there
            {
                TimerStart_RM2.ShowInInventory = false;//reset one stop check
            }




            if (TimerStop_RM2 != null)
            {
                if (TimerStop_RM2.ShowInInventory != true)
                {
                    TimerStop_RM2.Trigger();
                    TimerStop_RM2.ShowInInventory = true;//Set the timer's show in inventory to false, it's set to true above when you start moving
                }
            }


        }
    }
    //RM2----------------------------
    //
    //



    //
    //RM3----------------------------
    MotorStator_RM3 = GridTerminalSystem.GetBlockWithName(Name_RM3) as IMyMotorStator;
    AdvancedMotorStator_RM3 = GridTerminalSystem.GetBlockWithName(Name_RM3) as IMyMotorAdvancedStator;
    Cockpit_RM3 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM3) as IMyCockpit;
    Remote_RM3 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM3) as IMyRemoteControl;
    LCD_RM3 = GridTerminalSystem.GetBlockWithName(OutputPanel_RM3) as IMyTextPanel;
    TimerStart_RM3 = GridTerminalSystem.GetBlockWithName(TimerStartName_RM3) as IMyTimerBlock;
    TimerStop_RM3 = GridTerminalSystem.GetBlockWithName(TimerStopName_RM3) as IMyTimerBlock;

    if (Cockpit_RM3 == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (Remote_RM3 == null)
        {
            Echo("No control methods found!");
            return;
        }
        else
        {
            controller = Remote_RM3;//control methods used by remote
        }
    }
    else
    {
        controller = Cockpit_RM3;//control methods used by cockpit
    }

    //Remember to consider putting movement in functions, as well as you've still gotta do custom
    //cockpit acquisition and movement!

    if (MotorStator_RM3 == null)
    {
        Echo(Name_RM3 + " missing!");
    }
    else
    {
        if (ControlWS_RM3 == false)
        {

            if (ControlAD_RM3 == false)
            {

                if (ControlQE_RM3 == false)
                {
                    //When you aren't doing Q/E movement
                }
                else
                {
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                    if (controller.RollIndicator < 0)//When holding E
                    {
                        MotorStator_RM3.TargetVelocityRPM = +MoveSpeed_RM3;
                        MotorStator_RM3.ShowInInventory = true;
                    }
                    else if (controller.RollIndicator > 0)//When holding Q
                    {
                        MotorStator_RM3.TargetVelocityRPM = -MoveSpeed_RM3;
                        MotorStator_RM3.ShowInInventory = true;
                    }
                    else
                    {
                        if (MotorStator_RM3.ShowInInventory == true)//Essrntially creates a one stop toggle
                        {
                            MotorStator_RM3.TargetVelocityRPM = StopVelocity_RM3;
                            MotorStator_RM3.ShowInInventory = false;

                        }
                        //implicit else
                    }
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                }
            }
            else
            {
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
                if (controller.MoveIndicator.X < 0)//When holding A
                {
                    MotorStator_RM3.TargetVelocityRPM = +MoveSpeed_RM3;
                    MotorStator_RM3.ShowInInventory = true;
                }
                else if (controller.MoveIndicator.X > 0)//When holding D
                {
                    MotorStator_RM3.TargetVelocityRPM = -MoveSpeed_RM3;
                    MotorStator_RM3.ShowInInventory = true;
                }
                else
                {
                    if (MotorStator_RM3.ShowInInventory == true)//Essrntially creates a one stop toggle
                    {
                        MotorStator_RM3.TargetVelocityRPM = StopVelocity_RM3;
                        MotorStator_RM3.ShowInInventory = false;

                    }
                    //implicit else
                }
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
            }

        }
        else
        {
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
            if (controller.MoveIndicator.Z < 0)//When holding W
            {
                MotorStator_RM3.TargetVelocityRPM = +MoveSpeed_RM3;
                MotorStator_RM3.ShowInInventory = true;
            }
            else if (controller.MoveIndicator.Z > 0)//When holding S
            {
                MotorStator_RM3.TargetVelocityRPM = -MoveSpeed_RM3;
                MotorStator_RM3.ShowInInventory = true;
            }
            else
            {
                if (MotorStator_RM3.ShowInInventory == true)//Essrntially creates a one stop toggle
                {
                    MotorStator_RM3.TargetVelocityRPM = StopVelocity_RM3;
                    MotorStator_RM3.ShowInInventory = false;

                }
                //implicit else
            }
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
        }

        if (MotorStator_RM3.ShowInInventory == true)//When moving the rotor
        {
            if (TimerStart_RM3 != null)//if the timer exists
            {
                if (TimerStart_RM3.ShowInInventory != true)//Check if show in inventory is false, which means it has not been triggered this life
                {
                    TimerStart_RM3.Trigger();
                    TimerStart_RM3.ShowInInventory = true;//Set the timer's show in inventory to false when you know it's stopped moving
                }
            }

            if (TimerStop_RM3 != null)
            {
                TimerStop_RM3.ShowInInventory = false;//resets timer so it can be triggered again when the rotors stop moving
            }

            if (LCD_RM3 != null)//prints the angle to a panel, sleeps when unused
            {
                Status_RM3 = Math.Round(MathHelper.ToDegrees(MotorStator_RM3.Angle), 1).ToString("f2") + "°";
                LCD_RM3.WriteText(Name_RM3 + ":\n" + Status_RM3);
            }//set up a status system to have multiple angles at once
        }
        else//You're not moving the rotor any more
        {


            if (TimerStart_RM3 != null)//prevent crash by checking if this block is there
            {
                TimerStart_RM3.ShowInInventory = false;//reset one stop check
            }




            if (TimerStop_RM3 != null)
            {
                if (TimerStop_RM3.ShowInInventory != true)
                {
                    TimerStop_RM3.Trigger();
                    TimerStop_RM3.ShowInInventory = true;//Set the timer's show in inventory to false, it's set to true above when you start moving
                }
            }


        }
    }
    //RM3----------------------------
    //
    //

    //
    //RM4----------------------------
    MotorStator_RM4 = GridTerminalSystem.GetBlockWithName(Name_RM4) as IMyMotorStator;
    AdvancedMotorStator_RM4 = GridTerminalSystem.GetBlockWithName(Name_RM4) as IMyMotorAdvancedStator;
    Cockpit_RM4 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM4) as IMyCockpit;
    Remote_RM4 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM4) as IMyRemoteControl;
    LCD_RM4 = GridTerminalSystem.GetBlockWithName(OutputPanel_RM4) as IMyTextPanel;
    TimerStart_RM4 = GridTerminalSystem.GetBlockWithName(TimerStartName_RM4) as IMyTimerBlock;
    TimerStop_RM4 = GridTerminalSystem.GetBlockWithName(TimerStopName_RM4) as IMyTimerBlock;

    if (Cockpit_RM4 == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (Remote_RM4 == null)
        {
            Echo("No control methods found!");
            return;
        }
        else
        {
            controller = Remote_RM4;//control methods used by remote
        }
    }
    else
    {
        controller = Cockpit_RM4;//control methods used by cockpit
    }

    //Remember to consider putting movement in functions, as well as you've still gotta do custom
    //cockpit acquisition and movement!

    if (MotorStator_RM4 == null)
    {
        Echo(Name_RM4 + " missing!");
    }
    else
    {
        if (ControlWS_RM4 == false)
        {

            if (ControlAD_RM4 == false)
            {

                if (ControlQE_RM4 == false)
                {
                    //When you aren't doing Q/E movement
                }
                else
                {
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                    if (controller.RollIndicator < 0)//When holding E
                    {
                        MotorStator_RM4.TargetVelocityRPM = +MoveSpeed_RM4;
                        MotorStator_RM4.ShowInInventory = true;
                    }
                    else if (controller.RollIndicator > 0)//When holding Q
                    {
                        MotorStator_RM4.TargetVelocityRPM = -MoveSpeed_RM4;
                        MotorStator_RM4.ShowInInventory = true;
                    }
                    else
                    {
                        if (MotorStator_RM4.ShowInInventory == true)//Essrntially creates a one stop toggle
                        {
                            MotorStator_RM4.TargetVelocityRPM = StopVelocity_RM4;
                            MotorStator_RM4.ShowInInventory = false;

                        }
                        //implicit else
                    }
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                }
            }
            else
            {
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
                if (controller.MoveIndicator.X < 0)//When holding A
                {
                    MotorStator_RM4.TargetVelocityRPM = +MoveSpeed_RM4;
                    MotorStator_RM4.ShowInInventory = true;
                }
                else if (controller.MoveIndicator.X > 0)//When holding D
                {
                    MotorStator_RM4.TargetVelocityRPM = -MoveSpeed_RM4;
                    MotorStator_RM4.ShowInInventory = true;
                }
                else
                {
                    if (MotorStator_RM4.ShowInInventory == true)//Essrntially creates a one stop toggle
                    {
                        MotorStator_RM4.TargetVelocityRPM = StopVelocity_RM4;
                        MotorStator_RM4.ShowInInventory = false;

                    }
                    //implicit else
                }
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
            }

        }
        else
        {
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
            if (controller.MoveIndicator.Z < 0)//When holding W
            {
                MotorStator_RM4.TargetVelocityRPM = +MoveSpeed_RM4;
                MotorStator_RM4.ShowInInventory = true;
            }
            else if (controller.MoveIndicator.Z > 0)//When holding S
            {
                MotorStator_RM4.TargetVelocityRPM = -MoveSpeed_RM4;
                MotorStator_RM4.ShowInInventory = true;
            }
            else
            {
                if (MotorStator_RM4.ShowInInventory == true)//Essrntially creates a one stop toggle
                {
                    MotorStator_RM4.TargetVelocityRPM = StopVelocity_RM4;
                    MotorStator_RM4.ShowInInventory = false;

                }
                //implicit else
            }
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
        }

        if (MotorStator_RM4.ShowInInventory == true)//When moving the rotor
        {
            if (TimerStart_RM4 != null)//if the timer exists
            {
                if (TimerStart_RM4.ShowInInventory != true)//Check if show in inventory is false, which means it has not been triggered this life
                {
                    TimerStart_RM4.Trigger();
                    TimerStart_RM4.ShowInInventory = true;//Set the timer's show in inventory to false when you know it's stopped moving
                }
            }

            if (TimerStop_RM4 != null)
            {
                TimerStop_RM4.ShowInInventory = false;//resets timer so it can be triggered again when the rotors stop moving
            }

            if (LCD_RM4 != null)//prints the angle to a panel, sleeps when unused
            {
                Status_RM4 = Math.Round(MathHelper.ToDegrees(MotorStator_RM4.Angle), 1).ToString("f2") + "°";
                LCD_RM4.WriteText(Name_RM4 + ":\n" + Status_RM4);
            }//set up a status system to have multiple angles at once
        }
        else//You're not moving the rotor any more
        {


            if (TimerStart_RM4 != null)//prevent crash by checking if this block is there
            {
                TimerStart_RM4.ShowInInventory = false;//reset one stop check
            }




            if (TimerStop_RM4 != null)
            {
                if (TimerStop_RM4.ShowInInventory != true)
                {
                    TimerStop_RM4.Trigger();
                    TimerStop_RM4.ShowInInventory = true;//Set the timer's show in inventory to false, it's set to true above when you start moving
                }
            }


        }
    }
    //RM4----------------------------
    //
    //


    //
    //RM5----------------------------
    MotorStator_RM5 = GridTerminalSystem.GetBlockWithName(Name_RM5) as IMyMotorStator;
    AdvancedMotorStator_RM5 = GridTerminalSystem.GetBlockWithName(Name_RM5) as IMyMotorAdvancedStator;
    Cockpit_RM5 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM5) as IMyCockpit;
    Remote_RM5 = GridTerminalSystem.GetBlockWithName(ControlBlock_RM5) as IMyRemoteControl;
    LCD_RM5 = GridTerminalSystem.GetBlockWithName(OutputPanel_RM5) as IMyTextPanel;
    TimerStart_RM5 = GridTerminalSystem.GetBlockWithName(TimerStartName_RM5) as IMyTimerBlock;
    TimerStop_RM5 = GridTerminalSystem.GetBlockWithName(TimerStopName_RM5) as IMyTimerBlock;

    if (Cockpit_RM5 == null)//This little section swaps the register of control to the remote, if the seat isn't found
    {
        if (Remote_RM5 == null)
        {
            Echo("No control methods found!");
            return;
        }
        else
        {
            controller = Remote_RM5;//control methods used by remote
        }
    }
    else
    {
        controller = Cockpit_RM5;//control methods used by cockpit
    }

    //Remember to consider putting movement in functions, as well as you've still gotta do custom
    //cockpit acquisition and movement!

    if (MotorStator_RM5 == null)
    {
        Echo(Name_RM5 + " missing!");
    }
    else
    {
        if (ControlWS_RM5 == false)
        {

            if (ControlAD_RM5 == false)
            {

                if (ControlQE_RM5 == false)
                {
                    //When you aren't doing Q/E movement
                }
                else
                {
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                    if (controller.RollIndicator < 0)//When holding E
                    {
                        MotorStator_RM5.TargetVelocityRPM = +MoveSpeed_RM5;
                        MotorStator_RM5.ShowInInventory = true;
                    }
                    else if (controller.RollIndicator > 0)//When holding Q
                    {
                        MotorStator_RM5.TargetVelocityRPM = -MoveSpeed_RM5;
                        MotorStator_RM5.ShowInInventory = true;
                    }
                    else
                    {
                        if (MotorStator_RM5.ShowInInventory == true)//Essrntially creates a one stop toggle
                        {
                            MotorStator_RM5.TargetVelocityRPM = StopVelocity_RM5;
                            MotorStator_RM5.ShowInInventory = false;

                        }
                        //implicit else
                    }
                    //BORDER----------------------------------------------------------
                    //CURRENTLY NOT A FUNCTION, Q E MOVEMENT
                }
            }
            else
            {
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
                if (controller.MoveIndicator.X < 0)//When holding A
                {
                    MotorStator_RM5.TargetVelocityRPM = +MoveSpeed_RM5;
                    MotorStator_RM5.ShowInInventory = true;
                }
                else if (controller.MoveIndicator.X > 0)//When holding D
                {
                    MotorStator_RM5.TargetVelocityRPM = -MoveSpeed_RM5;
                    MotorStator_RM5.ShowInInventory = true;
                }
                else
                {
                    if (MotorStator_RM5.ShowInInventory == true)//Essrntially creates a one stop toggle
                    {
                        MotorStator_RM5.TargetVelocityRPM = StopVelocity_RM5;
                        MotorStator_RM5.ShowInInventory = false;

                    }
                    //implicit else
                }
                //BORDER----------------------------------------------------------
                //CURRENTLY NOT A FUNCTION, A D MOVEMENT
            }

        }
        else
        {
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
            if (controller.MoveIndicator.Z < 0)//When holding W
            {
                MotorStator_RM5.TargetVelocityRPM = +MoveSpeed_RM5;
                MotorStator_RM5.ShowInInventory = true;
            }
            else if (controller.MoveIndicator.Z > 0)//When holding S
            {
                MotorStator_RM5.TargetVelocityRPM = -MoveSpeed_RM5;
                MotorStator_RM5.ShowInInventory = true;
            }
            else
            {
                if (MotorStator_RM5.ShowInInventory == true)//Essrntially creates a one stop toggle
                {
                    MotorStator_RM5.TargetVelocityRPM = StopVelocity_RM5;
                    MotorStator_RM5.ShowInInventory = false;

                }
                //implicit else
            }
            //BORDER----------------------------------------------------------
            //CURRENTLY NOT A FUNCTION, W S MOVEMENT
        }

        if (MotorStator_RM5.ShowInInventory == true)//When moving the rotor
        {
            if (TimerStart_RM5 != null)//if the timer exists
            {
                if (TimerStart_RM5.ShowInInventory != true)//Check if show in inventory is false, which means it has not been triggered this life
                {
                    TimerStart_RM5.Trigger();
                    TimerStart_RM5.ShowInInventory = true;//Set the timer's show in inventory to false when you know it's stopped moving
                }
            }

            if (TimerStop_RM5 != null)
            {
                TimerStop_RM5.ShowInInventory = false;//resets timer so it can be triggered again when the rotors stop moving
            }

            if (LCD_RM5 != null)//prints the angle to a panel, sleeps when unused
            {
                Status_RM5 = Math.Round(MathHelper.ToDegrees(MotorStator_RM5.Angle), 1).ToString("f2") + "°";
                LCD_RM5.WriteText(Name_RM5 + ":\n" + Status_RM5);
            }//set up a status system to have multiple angles at once
        }
        else//You're not moving the rotor any more
        {


            if (TimerStart_RM5 != null)//prevent crash by checking if this block is there
            {
                TimerStart_RM5.ShowInInventory = false;//reset one stop check
            }




            if (TimerStop_RM5 != null)
            {
                if (TimerStop_RM5.ShowInInventory != true)
                {
                    TimerStop_RM5.Trigger();
                    TimerStop_RM5.ShowInInventory = true;//Set the timer's show in inventory to false, it's set to true above when you start moving
                }
            }


        }
    }
    //RM5----------------------------
    //
    //

    //------------------------------------------
    if ((Runtime.UpdateFrequency & UpdateFrequency.Update1) == 0)
        Runtime.UpdateFrequency = UpdateFrequency.Update1;
    //------------------------------------------

    if ((updateType & UpdateType.Update1) == 0)
        return;

    timeSinceRefresh += 1.0 / 6.0;

    if (!isSetup || timeSinceRefresh >= refreshInterval)
    {
        isSetup = GrabBlocks();
        timeSinceRefresh = 0;
    }

    if (!isSetup)
        return;
}
List<IMyShipController> controllers = new List<IMyShipController>();
IMyShipController controller = null;

bool GrabBlocks()
{
    GridTerminalSystem.GetBlocksOfType(controllers);
    if (controllers.Count == 0)
    {
        Echo($"Error: No ship controller named found");
        return false;
    }
    controller = GetControlledShipController(controllers);
    return true;
}
IMyShipController GetControlledShipController(List<IMyShipController> SCs) //Method for grabbing seat controller
{
    foreach (IMyShipController thisController in SCs)
    {
        if (thisController.IsUnderControl && thisController.CanControlShip)
            return thisController;
    }

    return SCs[0];
}