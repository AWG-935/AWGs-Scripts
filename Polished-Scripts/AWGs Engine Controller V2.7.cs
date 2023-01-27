/*
 * R e a d m e
 * -----------
 * 
 * //V2.7 AWG
 * //2023-01-27
 * //-Added base speed limiting, speed multipliers. More customisation for game modes. Way harder than imagined. Ughhhhh.
 * 
 * Tag your driver, don't worry about other blocks. This isn't properly polished yet but does everything it should.
 * Meant to work with my Electrotransmission script, otherwise will just advise you on speeds.
 *  
 * This /does/ adapt to player inventory so PLEASE empty it of items you don't intend for in a vehicle.
 * 
 * Pay attention to the program output in the control panel, it will flash with your details occasionally,
 * or read custom data for your speed. It includes screen support.
 * 
 */

//FOR CUSTOM RULES, DO NOT CHANGE OTHERWISE

const float Rule_SpeedMultiplier
    = 1f;
//An overall speed multiplier that affects all calculations.

bool OnOff_SpeedLimit
    = false;
//Activates the code regarding speed limit and multiplier calculation!

const float Rule_DesiredSpeedClamp
    = 1;
//This will limit your BASE speed, not hydrogen boosted speed. Adjust as necessary. 1 = off.

const float Rule_SpeedLimit
    = 130f;
//km/h

const float Rule_WeightLimit
    = 100f;
//Kind of obvious. Try not to break the scales.

const float Rule_MaxHydroBoost
    = 25f;
//Clamp for max amount hydrogen can push you.

const float Multiplier_Weight
    = 1f;
//Modify these for universal speed impacts, like weight's relation to speed

const float Multiplier_Hydro
    = 1f;
//or the level of hydroboost you get!


const string Name_ControllerTag = //Include this in your controlling block to have it be used
"Driver";


const string Name_ProgramTag = //Tag for the transmission script to find and set the wheel speed with /this/ result instead.
"#EC";            //MAKE SURE the tags line up in both scripts!

const string Name_StatusPanel =
"Engine Panel";

const string Arg_ClearCustomData =
"ClearCD";

//Below are engine tags, only for debug outputs.
const string Name_EngineLight =
"Light Engine";

const string Name_EngineMedium =
"Medium Engine";

const string Name_EngineHeavy =
"Heavy Engine";

const string Name_EngineSuperHeavy =
"Super Heavy Engine";

const string Name_EngineRadial =
"Radial Engine";



//---------------------//

bool GrabBlocks()//Check for power, then engine subtypes, return false if not. Collapse this later, and finally move it back to the bottom when finished with script.
{

    Echo("GrabBlocksRun");

    GridTerminalSystem.GetBlocksOfType(List_PowerProducingBlocks);//re-check all power producing blocks on grid
    GridTerminalSystem.GetBlocksOfType(AllControlBlocks);

    if (List_PowerProducingBlocks.Count == 0)//check if there's any engines when it first runs
    {
        Echo("Error: No power producing blocks found, somehow. Impressive!");
        return false;
    }
    else//if there are power producing blocks
    {
        foreach (var PowerProducer in List_PowerProducingBlocks)//for each power producer,
        {
            foreach (var RecognisedSubtypeIdBlock in List_EngineSubtypeIdToLookFor)//look through the list of recognised subtypes,
            {
                if (PowerProducer.BlockDefinition.SubtypeId.Contains(RecognisedSubtypeIdBlock) && !PowerProducer.CustomData.Contains(EngineRecognitionTag))//, then if the block contains a given subtype, and the checked block has NOT already been recognised
                {
                    List_RecognisedEngines.Add(PowerProducer);//Confirmed engine
                    PowerProducer.CustomData = (EngineRecognitionTag);//tag it to make sure

                    Echo(PowerProducer.CustomName);
                    Echo("Successful engine add");

                }
                else if (PowerProducer.BlockDefinition.SubtypeId.Contains(RecognisedSubtypeIdBlock) && PowerProducer.CustomData.Contains(EngineRecognitionTag))//previously recognised (ONLY FOR RECOMPILE version)
                {
                    List_RecognisedEngines.Add(PowerProducer);//Confirmed engine
                    Echo("Existing engine added");
                }

            }

        }
        if (List_RecognisedEngines.Count == 0)
        {
            Echo("Setup failed; no recognised engines.");
            return false;
        }
        else
        {
            Function_EngineCounting();
            return true;

        }

    }
    Echo("Setup failed; no recognised engines.");
    return false;
}                                           //What it switches to when storage is filled, for a moment.

//---------------------//
//Message examples
string Result_SpeedDisplay;
//
//---------------------//

//NO TOUCH BELOW
List<IMyPowerProducer> List_PowerProducingBlocks = new List<IMyPowerProducer>();
List<string> List_EngineSubtypeIdToLookFor = new List<string>();
List<IMyPowerProducer> List_RecognisedEngines = new List<IMyPowerProducer>();


//Debug Start
bool isSetup = false;
bool HasStatusPanel;
bool ErrorThrownThisRun;
//Debug Over


float Count_EngineLight;
int Count_EngineMedium;
int Count_EngineHeavy;
int Count_EngineSuperHeavy;
int Count_EngineRadial;
int Count_RunTimer = 0;

float WeightReal_Grid;
float Result_BaseSpeed;
float Result_BoostSpeed;
float Result_SpeedDifference;
float Result_Total;
float Limited_Result;

string EngineRecognitionTag = "Registered";

//0 means not started
//1 means min
//2 means max

IMyTextPanel Block_StatusPanel;

//---------------------//
//Default messages:
const string Linebreak = "\n";
const string MissingCheckOwnership = "missing!\nCheck naming\nand ownership!\n";
const string Between = ": ";
const string Blank = "";
//---------------------//
/// <summary>
        ///
        /// Expected operation:
        ///
        /// Detect light, medium, heavy, radial engines, and calculate speed + hydrogen boost from them.
        /// Output a speed variable. Send this to the transmission. Perhaps integrate the two?
        ///
        /// Can make them interlocking in two ways, either Engine Heavy or Trans Heavy
        /// Engine Heavy: Where Engine/Speed calc controls wheel speed, and transmission 'advises' speed as just an outside
        /// variable rather than original, sent to Engine. Acts as true gear selector. May be worth building into Electrotransmission in this case.
        ///
        /// Transmission Heavy: Where transmission controls wheel speed, Engine Controller acts as an advisor and continuously sends
        /// updates to it. Would need to plug in an input to the transmission to also take that.

//DEFAULT SPEED SETUP, DO NOT MODIFY OUTSIDE OF YOUR OWN CUSTOM RULES//

const float EfficiencyMod_Light
                     = (0.69f * 1f) * Multiplier_Hydro;
const float EfficiencyMod_Medium
                     = (2f * 1f) * Multiplier_Hydro;
const float EfficiencyMod_Heavy
                     = (4f * 1f) * Multiplier_Hydro;
const float EfficiencyMod_SuperHeavy
                     = (8f * 1f) * Multiplier_Hydro;
//--------------------------------//
const float WeightDiv_Lighter
            = (2280f * 1f) * Multiplier_Weight;
const float WeightDiv_Light
            = (2330f * 1f) * Multiplier_Weight;
const float WeightDiv_Intermediate
            = (2440f * 1f) * Multiplier_Weight;
const float WeightDiv_Medium
            = (2475f * 1f) * Multiplier_Weight;
const float WeightDiv_Heavy
            = (2275f * 1f) * Multiplier_Weight;
const float WeightDiv_Super
            = (2125f * 1f) * Multiplier_Weight;

const float Hydrogen_DivisionLevel = 2;//What the collected EngineCount/EfficiencyMod are divided by

float MultiplierRuntime_Hydrogen;
float MultiplierRuntime_Speed;

//Look below to the 'Calculate' function for the rest.






public Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;

    Function_ClearEngineCD(List_PowerProducingBlocks);
    ///Firstly, for the global running pre-recompile, clear all engine custom data in case there are any.
            ///Then add the given engine subtypes to the subtype list
    Function_LoadEngineSubtypes();
}
float LimitedSpeed = 0f;
public void Main(string argument, UpdateType updateSource)
{
    //Gets the amount needed to divide the top speed by, as to obtain the wanted limited speed.

    string ErrorLog = ""; //Add to this throughout the script using +=
    string ScreenPrint = ""; //The eventual result on the screen. You can append to this if you want, but look at other programs for help.

    if (!isSetup)//If you aren't setup yet, run the grab blocks system, if true, allow program to continue
    {
        isSetup = GrabBlocks();
    }

    if (!isSetup)
        return;


    //COUNTER that refreshes the power list to check for added/removed engines
    Count_RunTimer += 1;
    Echo("\nEngine Refresh: "+(Count_RunTimer*20f+"%\n"));

    if (Count_RunTimer == 4)
    {
        Count_RunTimer = 1;

        Function_RefreshPowerProducersAndEngines(List_PowerProducingBlocks);

        Function_EngineCountReset();



        foreach (var PowerProducer in List_PowerProducingBlocks)//for each block on the grid that is a power producer
        {
            foreach (var RecognisedSubtypeIdBlock in List_EngineSubtypeIdToLookFor)//trying to make sure there are no duplicates.
            {
                if (PowerProducer.BlockDefinition.SubtypeId.Contains(RecognisedSubtypeIdBlock) && !PowerProducer.CustomData.Contains(EngineRecognitionTag) && PowerProducer.IsWorking == true)//, then if the block contains a given subtype, and the checked block has NOT already been recognised
                {
                    List_RecognisedEngines.Add(PowerProducer);//Confirmed engine
                    PowerProducer.CustomData = (EngineRecognitionTag);//tag it to make sure

                    Echo(PowerProducer.BlockDefinition.SubtypeId);
                    Echo("Successful engine add");
                }
            }
        }

        if (!Me.CustomName.Contains(Name_ProgramTag))//if the program doesn't have Name_ControllerTag, add it
        {
            Me.CustomName += (" " + Name_ProgramTag);
        }

        Controller = ScanControlBlocks(AllControlBlocks);

        Function_EngineCounting();

        Function_CalculateSpeed(ref ScreenPrint, LimitedSpeed);

    }
    //COUNTER OVER
    //----------------------------------------------------//
    //
    //
    //
    //----------------------------------------------------//
    Block_StatusPanel = GridTerminalSystem.GetBlockWithName(Name_StatusPanel) as IMyTextPanel;
    //----------------------------------------------------//
    //   BLOCK SETUP ^^^
    //
    //
    //
    //   SCREEN &           |
    //   ERROR HANDLING    \|/
    //----------------------------------------------------//

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

    //ARGUMENT
    if (argument.Contains(Arg_ClearCustomData))//If there is a rotor group, and there is an argument submitted.
    {
        Function_ClearEngineCD(List_RecognisedEngines);
    }

    Function_EngineEchoPrinting(ref ScreenPrint);//tells me current engine status

    Function_CompareEngineLists();//this is my check for power system modification

    if (ErrorThrownThisRun == true)//Basically only run the error event if one of the other blocks tells you to
    {
        Event_ErrorThrown(ErrorLog, Block_StatusPanel, HasStatusPanel);
    }


    Function_ScreenStatusConstruction(ref ScreenPrint, Result_SpeedDisplay);
}


public void Function_CalculateSpeed(ref string ScreenPrint, float LimitedSpeed)
{
    ///Alright let's get this done gamers it's calculating time
            ///
            /// Hydrogen Efficiency Modifier
            ///


    WeightReal_Grid = Controller.CalculateShipMass().TotalMass;
    WeightReal_Grid = WeightReal_Grid / 1000f;
    WeightReal_Grid = (float)Math.Round(WeightReal_Grid, 1);
    Echo("\nWeight:"+WeightReal_Grid+"t");

    //Multiplying the individual hydrogen count by its efficiency multiplier, then adding.
    float Hydrogen_Multiplier =
        (Count_EngineLight      * EfficiencyMod_Light)    +
        (Count_EngineMedium     * EfficiencyMod_Medium)   +
        (Count_EngineHeavy      * EfficiencyMod_Heavy)    +
        (Count_EngineRadial     * EfficiencyMod_Heavy)    +
        (Count_EngineSuperHeavy * EfficiencyMod_SuperHeavy)/Hydrogen_DivisionLevel;

    if (WeightReal_Grid < 10)//9.99t range
    {
        MultiplierRuntime_Hydrogen = 1.3f;
        MultiplierRuntime_Speed = (WeightReal_Grid * (WeightReal_Grid / 21) * 113);
    }
    else if (WeightReal_Grid < 20)//10-19.99t range
    {
        MultiplierRuntime_Hydrogen = 1.19f;
        MultiplierRuntime_Speed = (WeightReal_Grid * (WeightReal_Grid / 21) * 113);
    }
    else if (WeightReal_Grid < 28)//20-28t
    {
        MultiplierRuntime_Hydrogen = 1.11f;
        MultiplierRuntime_Speed = WeightDiv_Light;
    }
    else if (WeightReal_Grid < 35)//28-35t
    {
        MultiplierRuntime_Hydrogen = 1.15f;
        MultiplierRuntime_Speed = WeightDiv_Intermediate;
    }
    else if (WeightReal_Grid < 55)//35-55t
    {
        MultiplierRuntime_Hydrogen = 1.17f;
        MultiplierRuntime_Speed = WeightDiv_Medium;
    }
    else if (WeightReal_Grid < 75)//55-75t (probably introduce another category around here)
    {
        MultiplierRuntime_Hydrogen = 1.15f;
        MultiplierRuntime_Speed = WeightDiv_Heavy;
    }
    else if (WeightReal_Grid < 100)//75-100t
    {
        MultiplierRuntime_Hydrogen = 1.11f;
        MultiplierRuntime_Speed = WeightDiv_Super;
    }
    else if (WeightReal_Grid <= 200)//100-200t
    {
        MultiplierRuntime_Hydrogen = 1.05f;
        MultiplierRuntime_Speed = WeightDiv_Super;
    }
    if (WeightReal_Grid > Rule_WeightLimit)
    {
        Echo("Over weight limit by " + (WeightReal_Grid - Rule_WeightLimit) + "t");
    }

    Result_BaseSpeed = 1/WeightReal_Grid*MultiplierRuntime_Speed;



    if (OnOff_SpeedLimit == true && Rule_DesiredSpeedClamp != 1)//Stops silly players forgetting to modify their programs
    {
        LimitedSpeed = Result_BaseSpeed / Rule_DesiredSpeedClamp;
        Limited_Result = Result_BaseSpeed * Rule_SpeedMultiplier / LimitedSpeed;

        Result_BoostSpeed = Limited_Result * ((MultiplierRuntime_Hydrogen * Hydrogen_Multiplier) - Hydrogen_Multiplier + 1);
        Result_SpeedDifference = (float)Math.Round(Result_BoostSpeed - Limited_Result, 0);
    }
    else
    {
        Result_BoostSpeed = Result_BaseSpeed * ((MultiplierRuntime_Hydrogen * Hydrogen_Multiplier) - Hydrogen_Multiplier + 1);
        Result_SpeedDifference = (float)Math.Round(Result_BoostSpeed - Result_BaseSpeed, 0);
    }


    if (Result_SpeedDifference > Rule_MaxHydroBoost)
        {
            Result_SpeedDifference = Rule_MaxHydroBoost;
        }

    Result_Total = ((float)Math.Round(Result_BaseSpeed + Result_SpeedDifference, 1));



    //if (OnOff_SpeedLimit == true && Rule_DesiredSpeedClamp != 1)//Stops silly players forgetting to modify their programs
    //{
    //    if (Limited_Result <= Result_Total)//NO CHEATING YA NASTY BUGGERS
    //    {
    //        Result_Total = Limited_Result;
    //    }
    //}
    if (OnOff_SpeedLimit == true && Rule_DesiredSpeedClamp == 1)
    {
        Echo("\nChange your speed limiting, honey!\nSetting to default calc result.");
        Result_Total = Result_Total * Rule_SpeedMultiplier;
    }
    else
    {
        Result_Total = Result_Total * Rule_SpeedMultiplier;
    }

    if (Result_Total > Rule_SpeedLimit)
    {
        Result_Total = Rule_SpeedLimit * Rule_SpeedMultiplier;
    }

    Me.CustomData = Result_Total.ToString();

    Result_SpeedDisplay = ("Top Speed: " + Me.CustomData + " km/h");
    Echo(Result_SpeedDisplay);

}

//New control method detector
List<IMyShipController> AllControlBlocks = new List<IMyShipController>();
IMyShipController Controller = null;
IMyShipController ScanControlBlocks(List<IMyShipController> List_Controllers)
{
    foreach (IMyShipController ThisController in List_Controllers)
    {
        if (ThisController.IsUnderControl && ThisController.CanControlShip && ThisController.DisplayNameText.Contains(Name_ControllerTag))
        {

        }

        return ThisController;
    }

    return List_Controllers[0];//return the first of the array as the result
}
public void Function_RefreshPowerProducersAndEngines(List<IMyPowerProducer> List_PowerProducingBlocks)
{
    GridTerminalSystem.GetBlocksOfType(List_PowerProducingBlocks);

}

public void Function_EngineCounting()
{


    foreach (var Engine in List_RecognisedEngines) //for each engine in the recognised engines list
    {
        if (Engine.IsFunctional != true || Engine.IsWorking != true)//check if they are non functional or not power producing
        {
            Echo((Engine.DisplayNameText)+" non-functional!\n");
        }
        else//if they are, count.
        {
            if (Engine.BlockDefinition.SubtypeId == "AWGSmallHydro")
            {
                Count_EngineLight += 1;
            }

            if (Engine.BlockDefinition.SubtypeId == "AWGModularHydro")
            {
                Count_EngineLight += 2f;
            }

            if (Engine.BlockDefinition.SubtypeId == "AWGMediumHydro" || Engine.BlockDefinition.SubtypeId == "SmallHydrogenEngine")
            {
                Count_EngineMedium += 1;
            }

            if (Engine.BlockDefinition.SubtypeId == "AWGHydroEngine")
            {
                Count_EngineHeavy += 1;
            }

            if (Engine.BlockDefinition.SubtypeId == "AWGRadial")
            {
                Count_EngineRadial += 1;
            }

            if (Engine.BlockDefinition.SubtypeId == "AWGSHEngine")
            {
                Count_EngineSuperHeavy += 1;
            }
        }

        //Echo("Testcount Light: "+Count_EngineLight);



        //for each engine containing given subtype,
        //Count_EngineLight = List_RecognisedEngines.Count(List_Engines => Engine.BlockDefinition.SubtypeId.Contains("AWGSmallHydro"));//get the amount of times this subtype shows up in the recognised engine list
        //if (Count_EngineLight != 0)//If the light engine exists
        //{
        //}

    }


}

public void Function_CompareEngineLists()
{
    foreach (var RecognisedEngine in List_RecognisedEngines)//checks through the virtual recognised engine list
    {
        if (!List_PowerProducingBlocks.Contains(RecognisedEngine))//if the power producers don't contain an engine from the recognised engine list
        {

            Function_EngineCountReset();

            Echo("Engine Removed!");//that means the engine is gone, and only exists in our memory now rip please donate to our funding drive praise be

            List<IMyPowerProducer> RemainderEngine;
            RemainderEngine = List_RecognisedEngines.Except(List_PowerProducingBlocks).ToList();//subtracts power producing blocks from the recognised engines. If a recognised engine is gone physically, the virtual one cannot be removed. Ergo, there will be a remainder.
            List_RecognisedEngines = List_RecognisedEngines.Except(RemainderEngine).ToList();//Take the remainder away from the true recognised engine list and save.

            Function_EngineCounting();
            //get the remainder engine, the one that is gone, then take it away from the original. The third list can be used for this

            //write a way to recount engines into the 'count engine' function. Do that when I transfer the light engine code to the rest.

        }
    }
}

public void Function_LoadEngineSubtypes()
{
    List_EngineSubtypeIdToLookFor.Add("AWGSmallHydro");
    List_EngineSubtypeIdToLookFor.Add("AWGModularHydro");
    List_EngineSubtypeIdToLookFor.Add("AWGMediumHydro");
    List_EngineSubtypeIdToLookFor.Add("SmallHydrogenEngine");
    List_EngineSubtypeIdToLookFor.Add("AWGHydroEngine");
    List_EngineSubtypeIdToLookFor.Add("AWGRadial");
    List_EngineSubtypeIdToLookFor.Add("AWGSHEngine");
}

public void Function_EngineCountReset()
{
    Count_EngineLight = 0;
    Count_EngineMedium = 0;
    Count_EngineHeavy = 0;
    Count_EngineRadial = 0;
    Count_EngineSuperHeavy = 0;
}

public void Function_EngineEchoPrinting(ref string ScreenPrint)
{
    Echo(List_RecognisedEngines.Count.ToString() + " engines.\n");

    Echo(
        Count_EngineLight.ToString() + Between + Name_EngineLight +

        Linebreak
        +
        Count_EngineMedium.ToString() + Between + Name_EngineMedium +

        Linebreak
        +
        Count_EngineHeavy.ToString() + Between + Name_EngineHeavy +

        Linebreak
        +
        Count_EngineRadial.ToString() + Between + Name_EngineRadial +

        Linebreak
        +
        Count_EngineSuperHeavy.ToString() + Between + Name_EngineSuperHeavy
        +
        Linebreak
        );

    ScreenPrint +=
        Count_EngineLight.ToString() + Between + Name_EngineLight +

        Linebreak
        +
        Count_EngineMedium.ToString() + Between + Name_EngineMedium +

        Linebreak
        +
        Count_EngineHeavy.ToString() + Between + Name_EngineHeavy +

        Linebreak
        +
        Count_EngineRadial.ToString() + Between + Name_EngineRadial +

        Linebreak
        +
        Count_EngineSuperHeavy.ToString() + Between + Name_EngineSuperHeavy
        +
        Linebreak;
}

public void Function_ClearEngineCD(List<IMyPowerProducer> List_Engines)
{
    if (List_Engines == null)
    {
        return;
    }
    foreach (var Engine in List_Engines)
    {
        Engine.CustomData = "";
        Echo("CD Cleared");
    }
}



//Screenops, structure
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

public void Function_ScreenStatusConstruction(ref string ScreenPrint, string Result_SpeedDisplay)
{
    //make 'error block' string name?

        ScreenPrint
        += Linebreak +
        "Weight: " +
        WeightReal_Grid+"t" +
        Linebreak +
        Result_SpeedDisplay +
        Linebreak +
        "Engines added "+Result_SpeedDifference+"km/h"


        ;

    Event_ScreenOperation(ScreenPrint, Block_StatusPanel, HasStatusPanel);

}
public void Event_ScreenOperation(string ScreenPrint, IMyTextPanel Block_StatusPanel, bool HasStatusPanel)
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

        Block_StatusPanel.WriteText(ScreenPrint);
    }
}