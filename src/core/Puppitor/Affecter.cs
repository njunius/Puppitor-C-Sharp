using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace Puppitor
{

    /*
     * interior struct for use as part of parsing a Puppitor rule file into a useable format by C#
     * affectName should correspond to the key where the AffectEntry instance is stored
     * adjacentAffects may be empty
     * actions, modifiers, and equilibriumPoint are the primary elements that should be accessed by an Affecter
     */
    public struct AffectEntry
    {
        public string affectName;
        public Dictionary<string, double> actions;
        public Dictionary<string, double> modifiers;
        public Dictionary<string, int> adjacentAffects;
        public double equilibriumPoint;

        public override string ToString()
        {
            string result = "";

            result += "\naffect: " + affectName;

            result += "\n\tactions";
            foreach (KeyValuePair<string, double> kvp in actions)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tmodifiers";
            foreach (KeyValuePair<string, double> kvp in modifiers)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tadjacent affects";
            foreach (KeyValuePair<string, int> affect in adjacentAffects)
            {
                result += "\n\t\t" + affect.Key + ": " + affect.Value;
            }

            result += "\n\tequilibrium point: " + equilibriumPoint +"\n";

            return result;
        }

    }

    /*
     Affecter is a wrapper around a JSON object based dictionary of affects (see contents of the affect_rules directory for formatting details)
 
     By default Affecter clamps the values of an affect vector (dictionaries built using make_affect_vector) in the range of 0.0 to 1.0 and uses theatrical terminology, consistent with 
     the default keys in action_key_map.py inside of the actual_action_states dictionary in the Action_Key_Map class
    */

    public class Affecter
    {
        public Dictionary<string, AffectEntry> affectRules;
        public double floorValue;
        public double ceilValue;
        public string equilibriumClassAction;
        public string? currentAffect;
        private Random randomInstance;

        public Affecter(string affectRulesJSON, double affectFloor = 0.0, double affectCeiling = 1.0, string equilibriumAction = "resting")
        {

            JSONClass affectRulesTemp = JSON.Parse(affectRulesJSON).AsObject;

            Console.WriteLine(affectRulesTemp.ToString());

            affectRules = new Dictionary<string, AffectEntry>();

            ConvertRules(affectRulesTemp);

            floorValue = affectFloor;
            ceilValue = affectCeiling;
            equilibriumClassAction = equilibriumAction;
            currentAffect = null;

            foreach (KeyValuePair<string, AffectEntry> kvp in affectRules)
            {
                double entryEquilibrium = kvp.Value.equilibriumPoint;
                if(currentAffect == null)
                {
                    currentAffect = kvp.Key;
                }
                else if (entryEquilibrium > affectRules[currentAffect].equilibriumPoint)
                {
                    currentAffect = kvp.Key;
                }
            }
            randomInstance = new Random();

            foreach (KeyValuePair<string, AffectEntry> affectEntry in affectRules)
            {
                Console.WriteLine(affectEntry.Value.ToString());
            }
            
        }

        /* 
         * helper function for use when loading a Puppitor rule file
         * converts a raw JSONClass into a dictionary of <string, AffectEntry> pairs to sandbox the usage of SimpleJSON to this file
         * also to convert data to its proper format
        */
        private void ConvertRules(JSONClass affectRulesTemp)
        {
            foreach (KeyValuePair<string, JSONNode> nodeEntry in affectRulesTemp)
            {
                // make the new affect entry and setup containers
                AffectEntry affectEntry;
                affectEntry.affectName = nodeEntry.Key;
                affectEntry.equilibriumPoint = Convert.ToDouble(nodeEntry.Value["equilibrium_point"]);
                affectEntry.adjacentAffects = new Dictionary<string, int>();
                affectEntry.actions = new Dictionary<string, double>();
                affectEntry.modifiers = new Dictionary<string, double>();

                // populate each container with their corresponding data from the JSON file stored in affectRulesTemp
                foreach (KeyValuePair<string, JSONNode> adjacencyEntry in nodeEntry.Value["adjacent_affects"].AsObject)
                {
                    int tempIntValue = Convert.ToInt32(adjacencyEntry.Value);
                    affectEntry.adjacentAffects.Add(adjacencyEntry.Key, tempIntValue);
                    //Console.WriteLine("{0}: {1}",adjacencyEntry.Key, affectEntry.adjacentAffects[adjacencyEntry.Key]);
                }

                foreach (KeyValuePair<string, JSONNode> actionEntry in nodeEntry.Value["actions"].AsObject)
                {
                    double tempDoubleVal = Convert.ToDouble(actionEntry.Value);
                    affectEntry.actions.Add(actionEntry.Key, tempDoubleVal);
                    //Console.WriteLine("{0}: {1}", actionEntry.Key, affectEntry.actions[actionEntry.Key]);
                }

                foreach (KeyValuePair<string, JSONNode> modEntry in nodeEntry.Value["modifiers"].AsObject)
                {
                    double tempDoubleVal = Convert.ToDouble(modEntry.Value);
                    affectEntry.modifiers.Add(modEntry.Key, tempDoubleVal);
                    //Console.WriteLine("{0}: {1}", modEntry.Key, affectEntry.modifiers[modEntry.Key]);
                }

                affectRules.Add(nodeEntry.Key, affectEntry);

            }

            return;
        }

        // discards the stored affect_rules and replaces it with a new rule file
        public void LoadOpenRuleFile(string affectRuleFile)
        {
            JSONClass affectRulesTemp = JSON.Parse(affectRuleFile).AsObject;

            affectRules = new Dictionary<string, AffectEntry>();

            ConvertRules(affectRulesTemp);

            return;
        }

        // helper function to do arithmetic with affect values and clamp the results between the floor and ceiling values as specified in an Affecter
        private double UpdateAndClampValues(double affectValue, double affectUpdateValue, double floorValue, double ceilValue)
        {
            // using max and min for Math library version compatibility
            return Math.Max(Math.Min(affectValue + affectUpdateValue, ceilValue), floorValue);
        }

        /*
         * to make sure affectVectors are in the correct format, use the MakeAffectVector function
         * the floats correspond to the strength of the expressed affect
         * current_action corresponds to the standard action expressed by an ActionKeyMap instance in its actual_action_states
         * NOTE: clamps affect values between floorValue and ceilValue
         * NOTE: while performing the equilibriumAction the affect values will move toward the equilibriumValue of the Affecter
         */
        public void UpdateAffect(Dictionary<string, double> affectVector, string currentAction, string currentModifier)
        {
            // using a raw for loop here because the values within the affectVector can be changed
            for (int i = 0; i < affectVector.Count; i++)
            {
                KeyValuePair<string, double> affect = affectVector.ElementAt(i);

                double currentActionUpdateValue = affectRules[affect.Key].actions[currentAction];
                double currentModifierUpdateValue = affectRules[affect.Key].modifiers[currentModifier];
                double currentEquilibriumValue = affectRules[affect.Key].equilibriumPoint;
                double currentAffectValue = affectVector[affect.Key];

                double valueToAdd = currentModifierUpdateValue * currentActionUpdateValue;

                // while performing the resting action, move values towards the given equilibrium point
                if (currentAction.Equals(equilibriumClassAction))
                {
                    if (currentAffectValue > currentEquilibriumValue)
                    {
                        affectVector[affect.Key] = UpdateAndClampValues(currentAffectValue, -1 * Math.Abs(valueToAdd), currentEquilibriumValue, ceilValue);
                    }
                    else if (currentAffectValue < currentEquilibriumValue)
                    {
                        affectVector[affect.Key] = UpdateAndClampValues(currentAffectValue, Math.Abs(valueToAdd), floorValue, currentEquilibriumValue);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    affectVector[affect.Key] = UpdateAndClampValues(currentAffectValue, valueToAdd, floorValue, ceilValue);
                }
            }
            return;
        }

        /*
         * returns a list of the affects with the highest strength of expression in the given affectVector
         * allowableError is used for dealing with the approximate value of floats
         */
        public static List<string> GetPossibleAffects(Dictionary<string, double> affectVector, double allowableError = 0.00000001)
        {
            List<string> prevailingAffects = new List<string>();

            foreach (KeyValuePair<string, double> affectEntry in affectVector)
            {
                double currentAffectValue = affectVector[affectEntry.Key];

                if( prevailingAffects.Count < 1)
                {
                    prevailingAffects.Add(affectEntry.Key);
                }
                else
                {
                    double highestValueSeen = affectVector[prevailingAffects[0]];

                    if (highestValueSeen < currentAffectValue)
                    {
                        prevailingAffects.Clear();
                        prevailingAffects.Add(affectEntry.Key);
                    }
                    else if (Math.Abs(highestValueSeen - currentAffectValue) < allowableError)
                    {
                        prevailingAffects.Add(affectEntry.Key);
                    }
                }

                
            }

            return prevailingAffects;
        }

        /*
         * chooses the next current affect
         * possibleAffects must be a list of strings of affects defined in the .json file loaded into the Affecter instance
         * possibleAffects can be generated using the GetPossibleAffects() function
         * the choice logic is as follows:
         *      pick the only available affect
         *      if there is more than one and the currentAffect is in the set of possible affects pick it
         *      if the currentAffect is not in the set but there is at least one affect connected to the current affect, pick from that subset, with weights if any are specified
         *      otherwise randomly pick from the disconnected set of possible affects
         */
        public string ChoosePrevailingAffect(List<string> possibleAffects, int randomFloor = 0, int randomCeil = 101)
        {
            List<string> connectedAffects = new List<string>();
            if (possibleAffects.Count == 1)
            {
                currentAffect = possibleAffects[0];
                return currentAffect;
            }
            if (possibleAffects.Contains(currentAffect))
            {
                return currentAffect;
            }

            Dictionary<string, int> currAdjacencyWeights = affectRules[currentAffect].adjacentAffects;

            foreach(string affect in possibleAffects)
            {
                if (currAdjacencyWeights.ContainsKey(affect))
                {
                    connectedAffects.Add(affect);
                }
            }

            
            if (connectedAffects.Count > 0)
            {
                int randomNum = randomInstance.Next(randomFloor, randomCeil);
                // weighted random choice of the connected affects to the current affect
                // a weight of 0 is ignored
                foreach (string affect in connectedAffects)
                {
                    int currAffectWeight = currAdjacencyWeights[affect];
                    if (currAffectWeight > 0 && randomNum <= currAffectWeight)
                    {
                        currentAffect = affect;
                        return currentAffect;
                    }
                    randomNum -= currAffectWeight;
                }

                // if all weights are 0, just pick randombly
                randomNum = randomInstance.Next(connectedAffects.Count);

                currentAffect = connectedAffects[randomNum];
                return currentAffect;
            }
            else
            {
                int randomIndex = randomInstance.Next(possibleAffects.Count);
                currentAffect = possibleAffects[randomIndex];
                return currentAffect;
            }

        }

        /*
         * wrapper function around the GetPossibleAffects() to ChoosePrevailingAffect() pipeline to allow for easier, more fixed integration into other code
         * NOTE: this function is not intended to supercede the useage of both GetPossibleAffects() and ChoosePrevailingAffect()
         *  it is here for convenience and if the default behavior of immediately using the list created by GetPossibleAffects() in ChoosePrevailingAffect()
         *  is the desired functionality
         */
        public string GetPrevailingAffect(Dictionary<string, double> affectVector, double allowableError = 0.00000001)
        {
            List<string> possibleAffects = GetPossibleAffects(affectVector, allowableError);
            string prevailingAffect = ChoosePrevailingAffect(possibleAffects);
            return prevailingAffect;
        }

        // provided function for formatting dictionaries for use with an Affecter
        // NOTE: it is recommended you make an Affecter instance THEN make the corresponding AffectVector to make sure the keys match
        public static Dictionary<string, double> MakeAffectVector(List<string> affectNames, Dictionary<string, AffectEntry> equilibriumValues)
        {
            Dictionary<string, double> affectVector = new Dictionary<string, double>();

            foreach(string affect in affectNames)
            {
                affectVector.Add(affect, equilibriumValues[affect].equilibriumPoint);
            }

            return affectVector;
        }

    }

}
