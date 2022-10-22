/*
 * R e a d m e
 * -----------
 * An unpolished but helpful scrip that can cycle through different rotor displacements. Useful for moving seats, autoloaders or vision blocks.
 * Make sure you have read variable names correctly, this is older so has little handholding.
 * 
 */

const string DisplacementRotorName = "Displacement Rotor"; //Rotor to be toggled

const float DisplacementMin = 0.01f; //Rotor Displacement Min
const float DisplacementMax = 0.1f; //Rotor Displacement Max
const float DisplacementDefault = 0f; //Rotor Displacement if caught outside of either min/max, can be set to one of these variables (DisplacementMin/Max)
const float DisplacementSetting = DisplacementMax; //What the program does the step after setting to default displacement. Use either one of the variable names here, or a number.

const string StatusPanel = "Status Panel"; //Display Panel (OPTIONAL)

//------------------------------------------------------
// ============== Don't touch below here ===============
//------------------------------------------------------

IMyMotorStator DisplacementRotor;
IMyTextPanel echopanel;

public void Main(string argument, UpdateType updateType)
{
    string STATUS = "";
    string ERROR_TXT = "";
    float DisplacementArg = 0f;

    echopanel = GridTerminalSystem.GetBlockWithName(StatusPanel) as IMyTextPanel;
    DisplacementRotor = GridTerminalSystem.GetBlockWithName(DisplacementRotorName) as IMyMotorStator;

    if (DisplacementRotor == null)
    {
        ERROR_TXT += "No rotor named " + DisplacementRotorName + "\n";
        Echo("No rotor named " + DisplacementRotorName + "\n");
    }
    else
    {
        if (DisplacementRotor.Displacement == DisplacementMin) //if the rotor was at min displacement, set to max
        {
            DisplacementRotor.Displacement = DisplacementMax;
        }
        else if (DisplacementRotor.Displacement == DisplacementMax) //if at max, set to min
        {
            DisplacementRotor.Displacement = DisplacementMin;
        }
        else
        {
            DisplacementRotor.Displacement = DisplacementDefault; //Go to the preset default displacement setting
        }

        if (DisplacementRotor.Displacement == DisplacementDefault) //if the rotor was at default displacement, what do?
        {
            DisplacementRotor.Displacement = DisplacementSetting;
        }

        if (argument != "")
        {
            DisplacementArg = float.Parse(argument);
            DisplacementRotor.Displacement = DisplacementArg;
        }

        STATUS = DisplacementRotor.Displacement.ToString();
    }

    if (echopanel == null) //Checks if text panel exists
    {
        Echo("No screen named " + StatusPanel + "\n");
    }
    else
    {
        if (ERROR_TXT != "") //if there is an error reported
        {
            echopanel.WriteText("Error:\n" + ERROR_TXT + "Ownership?\n\n" + STATUS); //report the error to the panel
        }
        else
        {
            echopanel.WriteText(STATUS + "m");
        } //if there are no errors, just report the status!
    }

}