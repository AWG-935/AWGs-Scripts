/*
 * R e a d m e
 * -----------
 * V3 AWG
 * 22/10/19
 * 
 * Adds degrees to a rotor, with POS###, NEG### (### being digits)
 * This is unpolished currently, as it cannot go past 360, so I recommend
 * setting a limit, and instead running 'NEG360' as an argument when it hits the end.
 * 
 * It will not consider arguments that send it over 360 either, also account for this in
 * your calculations. Future work will rectify this. It can activate timers based on different
 * scenarios.
 */

const string Name_MovingRotor1 = "Loader Angle";// Rotor to be moved's name.
float MoveSpeed_MovingRotor1 = 15f; //Move speed of the rotor in RPM. Tweak to perfection.
float AccurateSpeed_MovingRotor1 = 3f; //Self-correcting speed of the rotor.
float RotorErrorMargin_MovingRotor1 = 3f; //How close to your expected degrees, should the rotor slow down, to become more accurate?

string AngleAdditionKeyword = "POS";
string AngleSubtractionKeyword = "NEG";

string Name_OutputPanel_MovingRotor1 = "Angle Panel"; //Where should this rotor angle be sent?
string Name_MovementTimer1Start = "Timer Rotor Start";//Timer that triggers when you start moving
string Name_MovementTimer1Stop = "Timer Rotor Stop";//Timer that triggers when you stop moving

//FORMAT YOUR ARGUMENTS AS '15POS' or '30NEG' for adding 15 degrees, or taking away 30 degrees.

//-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x-x
// ============== Config Finished ===============
//------------------------------------------------------

//use this area to load block defs, grab blocks (both sides of grabbing the block, so program recompile is necessary to refresh)

IMyMotorStator Block_MovingRotor1;
IMyTextPanel Block_OutputPanel_MovingRotor1;
IMyTimerBlock Block_Timer1Start;
IMyTimerBlock Block_Timer1Stop;

float RotorAddedAngle; //the angle of the rotor + whatever the argument has been taken as
float InterpretedAngleArgument; //the argument container in the first place
string ArgumentStore; //stores the argument for length analysis.
int AmendedLength;
string Status_OutputPanel_MovingRotor1;
string ArgumentSave; //Different from ArgumentStore, in that it's a short term memory stopgap to save argument and use this instead.



public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
    Echo("Running Started...");
}

public void Main(string argument)
{
    Block_MovingRotor1 = GridTerminalSystem.GetBlockWithName(Name_MovingRotor1) as IMyMotorStator;
    Block_OutputPanel_MovingRotor1 = GridTerminalSystem.GetBlockWithName(Name_OutputPanel_MovingRotor1) as IMyTextPanel;

    Block_Timer1Start = GridTerminalSystem.GetBlockWithName(Name_MovementTimer1Start) as IMyTimerBlock;
    Block_Timer1Stop = GridTerminalSystem.GetBlockWithName(Name_MovementTimer1Stop) as IMyTimerBlock;

    //sort out timer to start/stop shen moving the rotor

    //use program custom data to store argument, use rotor to store rotor's added-to-angle
    //(since you don't need to store the original angle, it will be calculated at runtime, something like 'rotor.angle = rotor.angle + argument')
    Echo("Runcheck");
    if (Block_MovingRotor1 == null)
    {
        Echo("Rotor missing!");
    }

    if (Block_MovingRotor1.CustomData == "") //if the custom data is empty, take the current angle and fill with that. Prevents crash.
    {
        Block_MovingRotor1.CustomData = MathHelper.ToDegrees(Block_MovingRotor1.Angle).ToString();
    }

    if (argument != "")
    {
        Echo("argued");
        ArgumentSave = argument;



        if (ArgumentSave.Contains(AngleAdditionKeyword))
        {
            Echo(AngleAdditionKeyword + " DETECT");
            Block_MovingRotor1.CustomData = ArgumentSave;
            ArgumentStore = ArgumentSave;

            //AmendedLength = ArgumentStore.Length - 3; //Looks at the length of the argument inputted then takes away from the end (POS or NEG)
            InterpretedAngleArgument = float.Parse(ArgumentStore.Replace(AngleAdditionKeyword, ""));

            RotorAddedAngle = (MathHelper.ToDegrees(Block_MovingRotor1.Angle) + InterpretedAngleArgument); //due to being negative, the interpreted argument will be subtracted
            Echo(RotorAddedAngle.ToString());

            if (RotorAddedAngle > 360)//accounts for over-360 degrees wrap around
            {
                RotorAddedAngle -= 360; //TAKES AWAY 360!
            }
            else if (RotorAddedAngle < 0)
            {
                RotorAddedAngle += 360;
            }

            Block_MovingRotor1.CustomData = RotorAddedAngle.ToString("f2"); //print to CD, I wonder what happens to negatives?

            Block_MovingRotor1.TargetVelocityRPM = 0;
            Block_MovingRotor1.TargetVelocityRPM += MoveSpeed_MovingRotor1;

            if (Block_Timer1Start != null && Block_Timer1Start.ShowInInventory==true)
            {
                Block_Timer1Start.Trigger();
                Block_Timer1Start.ShowInInventory = false;
                Block_Timer1Stop.ShowInInventory = true;
            }

            Block_MovingRotor1.ShowInInventory = true; //default, moving forwards

        }
        else if (argument.Contains(AngleSubtractionKeyword))
        {
            Echo(AngleSubtractionKeyword + " DETECT");
            Block_MovingRotor1.CustomData = ArgumentSave;
            ArgumentStore = Block_MovingRotor1.CustomData;


            //AmendedLength = ArgumentStore.Length - 3; //Looks at the length of the argument inputted then takes away from the end (POS or NEG)
            InterpretedAngleArgument = float.Parse(ArgumentStore.Replace(AngleSubtractionKeyword, ""));//This replaces the term 'POS' or 'NEG' with blank space in the Interpreted Angle Argument, leaving just numbers to use! Thanks Zapper.

            RotorAddedAngle = (MathHelper.ToDegrees(Block_MovingRotor1.Angle) - InterpretedAngleArgument); //due to being negative, the interpreted argument will be subtracted

            Block_MovingRotor1.CustomData = RotorAddedAngle.ToString("f2"); //print to CD

            Block_MovingRotor1.TargetVelocityRPM = 0;
            Block_MovingRotor1.TargetVelocityRPM -= MoveSpeed_MovingRotor1;

            if (Block_Timer1Start != null && Block_Timer1Start.ShowInInventory == true)
            {
                Block_Timer1Start.Trigger();
                Block_Timer1Start.ShowInInventory = false;
                Block_Timer1Stop.ShowInInventory = true;
            }

            if (Block_Timer1Stop != null)
            {
                Block_Timer1Stop.ShowInInventory = true;
            }

            Block_MovingRotor1.ShowInInventory = false; //different, moving backwards
        }

        else //Interpret as positive movement, just as a float
        {
            Echo("Remember the format '120POS' or similar\n");
        }


    }

    if (Block_MovingRotor1.ShowInInventory == true) // Moving up
    {


        if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) > (float.Parse(Block_MovingRotor1.CustomData) - RotorErrorMargin_MovingRotor1) && (MathHelper.ToDegrees(Block_MovingRotor1.Angle) < (float.Parse(Block_MovingRotor1.CustomData))))
        {
            Block_MovingRotor1.TargetVelocityRPM = AccurateSpeed_MovingRotor1;
        }
        if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) >= float.Parse(Block_MovingRotor1.CustomData))//if the rotor angle greater than or equal to the new added angle
        {

            Block_MovingRotor1.TargetVelocityRPM = 0f;

            if (Block_Timer1Stop != null && Block_Timer1Stop.ShowInInventory == true)
            {
                Block_Timer1Stop.Trigger();
                Block_Timer1Stop.ShowInInventory = false;
                Block_Timer1Start.ShowInInventory = true;
            }
            // if inaccurate, reverse very slowly, invert limiting settings.

            //if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) > float.Parse(Block_MovingRotor1.CustomData))
            //{
            //    Block_MovingRotor1.TargetVelocityRPM = ReturnSpeed_MovingRotor1 * -1;
            //
            //}

        }

        //This checks to see if you're in 'catch'/reverse mode by looking at rotor velocity and turning off the rotor from there
        //if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) <= float.Parse(Block_MovingRotor1.CustomData) && Block_MovingRotor1.TargetVelocityRPM == ReturnSpeed_MovingRotor1)
        //{
        //    Block_MovingRotor1.TargetVelocityRPM = 0f;
        //}

    }//PUT WRAP AROUND CLAUSE IN FOR ARGUMENT RESULT OR WHATEVER IT IS, SO YOU CAN INPUT WHEN YOU NEAR 350 DEGREES OR SIMILAR, SAME WITH BELOW IT
    else
    {

        if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) < (float.Parse(Block_MovingRotor1.CustomData) - RotorErrorMargin_MovingRotor1) && (MathHelper.ToDegrees(Block_MovingRotor1.Angle) < (float.Parse(Block_MovingRotor1.CustomData))))
        {
            Block_MovingRotor1.TargetVelocityRPM = AccurateSpeed_MovingRotor1*-1;
        }
        if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) <= float.Parse(Block_MovingRotor1.CustomData))//if the rotor angle is smaller than or equal to the new added angle
        {
            Block_MovingRotor1.TargetVelocityRPM = 0f;

            if (Block_Timer1Stop != null && Block_Timer1Stop.ShowInInventory == true)
            {
                Block_Timer1Stop.Trigger();
                Block_Timer1Stop.ShowInInventory = false;
                Block_Timer1Start.ShowInInventory = true;
            }
            // if inaccurate, reverse very slowly, invert limiting settings, this acts as a catch in case the rotor is being naughty at high speeds or the program didn't update fast enough
            //if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) < float.Parse(Block_MovingRotor1.CustomData))
            //{
            //    Block_MovingRotor1.TargetVelocityRPM = ReturnSpeed_MovingRotor1;
            //
            //}
        }

        //if (MathHelper.ToDegrees(Block_MovingRotor1.Angle) >= float.Parse(Block_MovingRotor1.CustomData) && Block_MovingRotor1.TargetVelocityRPM == ReturnSpeed_MovingRotor1)
        //{
        //    Block_MovingRotor1.TargetVelocityRPM = 0f;
        //}
    }


    //I need to make a bracket, and have to have it specific to whether it's going down or going up to make it more accurate so that it stops closer to given angle



    if (Block_OutputPanel_MovingRotor1 != null)//prints the angle to a panel, sleeps when unused
    {
        Status_OutputPanel_MovingRotor1 = Math.Round(MathHelper.ToDegrees(Block_MovingRotor1.Angle), 1).ToString("f2") + "°";
        Block_OutputPanel_MovingRotor1.WriteText(Name_MovingRotor1 + ":\n\n" + Status_OutputPanel_MovingRotor1);
    }//set up a status system to have multiple angles at once

    //Block_MovingRotor1.Angle();
}