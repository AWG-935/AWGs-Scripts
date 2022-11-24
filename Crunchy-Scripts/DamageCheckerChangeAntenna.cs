/*
 * R e a d m e
 * -----------
 * 
 * In this file you can include any instructions or other comments you want to have injected onto the 
 * top of your final script. You can safely delete this file if you do not want any such comments.
 */


string AntennaToFind = "Antenna 1";
string AntennaToSwitchTo = "Antenna 2";

string PrematureDestructionMessage = " missing! Repairs required!";





//DON'T TOUCH BELOW HERE//

IMyRadioAntenna FindAntenna;
IMyRadioAntenna SwitchAntenna;

public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;
    Echo("If you can read this\nclick the 'Run' button!");
}

public void Main(string argument)
{

    //------------------------------------------

    FindAntenna = GridTerminalSystem.GetBlockWithName(AntennaToFind) as IMyRadioAntenna;
    SwitchAntenna = GridTerminalSystem.GetBlockWithName(AntennaToSwitchTo) as IMyRadioAntenna;

    if (argument == "reset")
    {
        try
        {
            SwitchAntenna.ShowInInventory = false;
            Echo("Switch Antenna Success!");
        }
        catch
        {
            Echo ("Switch Antenna Reset Failed");
        }

        try
        {
            FindAntenna.ShowInInventory = false;
            Echo("Find Antenna Success!");
        }
        catch
        {
            Echo("Switch Antenna Reset Failed");
        }
    }

    try
    {
        if (FindAntenna == null && SwitchAntenna != null) //if Antenna 1 is broken/not found, and Antenna 2 exists, turn it on.             Woo hockey sticks!
        {
            if (SwitchAntenna.ShowInInventory == false)
            {
                SwitchAntenna.Enabled = true;
                SwitchAntenna.ShowInInventory = true;//Make sure this doesn't retrigger.
            }

        }
        else if (SwitchAntenna == null)//in any situation where you're missing the switch antenna
        {
            Echo(AntennaToSwitchTo + "missing! Repair ASAP!"); //Yell out of the program

            if (FindAntenna != null && FindAntenna.ShowInInventory == false)//if you still have a comms method/are broadcasting on the original system.
            {
                FindAntenna.Enabled = true;
                FindAntenna.HudText = AntennaToSwitchTo + PrematureDestructionMessage;
                FindAntenna.ShowInInventory = true;
            }
        }
        else
        {
            return;
        }
    }
    catch
    {
        Echo("had some kind of problem in the block null/grabbing phase");
    }

}