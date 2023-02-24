using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
#if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
using System.Text.Json;
#endif
using SimpleJSON;

namespace Puppitor
{

    /*
     * interior class for use as part of parsing a Puppitor rule file into a useable format by C#
     * affectName should correspond to the key where the AffectEntry instance is stored
     * adjacent_affects may be empty
     * actions, modifiers, and equilibrium_point are the primary elements that should be accessed by an Affecter
     * NOTE: AffectEntry uses snake_case for portability with JSON across languages, if you have a problem with that it's on you
     */
    public class AffectEntry
    {
        //public string affectName { get; set; }
        public Dictionary<string, double> actions { get; set; }
        public Dictionary<string, double> modifiers { get; set; }
        public Dictionary<string, int> adjacent_affects { get; set; }
        public double equilibrium_point { get; set; }

        public override string ToString()
        {
            string result = "";

            //result += "\naffect: " + affectName;

            result += "\n\tactions:";
            foreach (KeyValuePair<string, double> kvp in actions)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tmodifiers:";
            foreach (KeyValuePair<string, double> kvp in modifiers)
            {
                result += "\n\t\t" + kvp.Key + ": " + kvp.Value;
            }

            result += "\n\tadjacent affects:";
            foreach (KeyValuePair<string, int> affect in adjacent_affects)
            {
                result += "\n\t\t" + affect.Key + ": " + affect.Value;
            }

            result += "\n\tequilibrium point: " + equilibrium_point +"\n";

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
        public Random randomInstance;
        public List<string> prevailingAffects;
        public List<string> connectedAffects;
        public List<string> affectList;

        public Affecter(string affectRulesJSON, double affectFloor = 0.0, double affectCeiling = 1.0, string equilibriumAction = "resting")
        {

            #if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                Console.WriteLine(affectRulesJSON);
                affectRules = JsonSerializer.Deserialize<Dictionary<string, AffectEntry>>(affectRulesJSON);
            #else
                Console.WriteLine("Falling Back to SimpleJSON");
                JSONClass affectRulesTemp = JSON.Parse(affectRulesJSON).AsObject;

                Console.WriteLine(affectRulesTemp.ToString());

                affectRules = new Dictionary<string, AffectEntry>();

                ConvertRules(affectRulesTemp);
            #endif

            floorValue = affectFloor;
            ceilValue = affectCeiling;
            equilibriumClassAction = equilibriumAction;
            currentAffect = null;

            foreach (KeyValuePair<string, AffectEntry> kvp in affectRules)
            {
                double entryEquilibrium = kvp.Value.equilibrium_point;
                if(currentAffect == null)
                {
                    currentAffect = kvp.Key;
                }
                else if (entryEquilibrium > affectRules[currentAffect].equilibrium_point)
                {
                    currentAffect = kvp.Key;
                }
            }
            randomInstance = new Random();

            foreach (KeyValuePair<string, AffectEntry> affectEntry in affectRules)
            {
                Console.WriteLine(affectEntry.Value.ToString());
            }

            // list of affects to be updated to avoid using elementAt()
            affectList = new List<string>();

            foreach(KeyValuePair<string, AffectEntry> entry in affectRules)
            {
                affectList.Add(entry.Key);
            }

            // choice functions lists
            connectedAffects = new List<string>();

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
                AffectEntry affectEntry = new AffectEntry();
                //affectEntry.affectName = nodeEntry.Key;
                affectEntry.equilibrium_point = Convert.ToDouble(nodeEntry.Value["equilibrium_point"]);
                affectEntry.adjacent_affects = new Dictionary<string, int>();
                affectEntry.actions = new Dictionary<string, double>();
                affectEntry.modifiers = new Dictionary<string, double>();

                // populate each container with their corresponding data from the JSON file stored in affectRulesTemp
                foreach (KeyValuePair<string, JSONNode> adjacencyEntry in nodeEntry.Value["adjacent_affects"].AsObject)
                {
                    int tempIntValue = Convert.ToInt32(adjacencyEntry.Value);
                    affectEntry.adjacent_affects.Add(adjacencyEntry.Key, tempIntValue);
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

            #if NET5_0_OR_GREATER || NETCOREAPP3_1_OR_GREATER
                affectRules = JsonSerializer.Deserialize<Dictionary<string, AffectEntry>>(affectRuleFile);
            #else
                Console.WriteLine("Falling Back to SimpleJSON");
                JSONClass affectRulesTemp = JSON.Parse(affectRuleFile).AsObject;

                affectRules = new Dictionary<string, AffectEntry>();

                ConvertRules(affectRulesTemp);
            #endif

            // update the affect list with the new rule file domain
            affectList.Clear();

            foreach (KeyValuePair<string, AffectEntry> entry in affectRules)
            {
                affectList.Add(entry.Key);
            }

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
        public void UpdateAffect(Dictionary<string, double> affectVector, string currentAction, string currentModifier, double valueMultiplier = 1.0, double valueAdd = 0.0)
        {
            // using a raw for loop here because the values within the affectVector can be changed
            //for (int i = 0; i < affectVector.Count; i++)
            foreach(string affectName in affectList)
            {

                double currentActionUpdateValue = affectRules[affectName].actions[currentAction];
                double currentModifierUpdateValue = affectRules[affectName].modifiers[currentModifier];
                double currentEquilibriumValue = affectRules[affectName].equilibrium_point;
                double currentAffectValue = affectVector[affectName];

                double valueToAdd = valueMultiplier * (currentModifierUpdateValue * currentActionUpdateValue) + valueAdd;

                // while performing the resting action, move values towards the given equilibrium point
                if (currentAction.Equals(equilibriumClassAction))
                {
                    if (currentAffectValue > currentEquilibriumValue)
                    {
                        affectVector[affectName] = UpdateAndClampValues(currentAffectValue, -1 * Math.Abs(valueToAdd), currentEquilibriumValue, ceilValue);
                    }
                    else if (currentAffectValue < currentEquilibriumValue)
                    {
                        affectVector[affectName] = UpdateAndClampValues(currentAffectValue, Math.Abs(valueToAdd), floorValue, currentEquilibriumValue);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    affectVector[affectName] = UpdateAndClampValues(currentAffectValue, valueToAdd, floorValue, ceilValue);
                }
            }
            return;
        }

        /*
         * returns a list of the affects with the highest strength of expression in the given affectVector
         */
        public static List<string> GetPossibleAffects(Dictionary<string, double> affectVector)
        {
            List<string> possibleAffects = new List<string>();

            foreach (KeyValuePair<string, double> affectEntry in affectVector)
            {
                if (affectEntry.Value == affectVector.Values.Max())
                {
                    possibleAffects.Add(affectEntry.Key);
                }
            }

            return possibleAffects;
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
        public static string ChoosePrevailingAffect(Affecter affecter, List<string> possibleAffects, int randomFloor = 0, int randomCeil = 101)
        {
            affecter.connectedAffects.Clear();
            if (possibleAffects.Count == 1)
            {
                affecter.currentAffect = possibleAffects[0];
                return affecter.currentAffect;
            }
            if (possibleAffects.Contains(affecter.currentAffect))
            {
                return affecter.currentAffect;
            }

            Dictionary<string, int> currAdjacencyWeights = affecter.affectRules[affecter.currentAffect].adjacent_affects;

            foreach(string affect in possibleAffects)
            {
                if (currAdjacencyWeights.ContainsKey(affect))
                {
                    affecter.connectedAffects.Add(affect);
                }
            }

            
            if (affecter.connectedAffects.Count > 0)
            {
                int randomNum = affecter.randomInstance.Next(randomFloor, randomCeil);
                // weighted random choice of the connected affects to the current affect
                // a weight of 0 is ignored
                foreach (string affect in affecter.connectedAffects)
                {
                    int currAffectWeight = currAdjacencyWeights[affect];
                    if (currAffectWeight > 0 && randomNum <= currAffectWeight)
                    {
                        affecter.currentAffect = affect;
                        return affecter.currentAffect;
                    }
                    randomNum -= currAffectWeight;
                }

                // if all weights are 0, just pick randombly
                randomNum = affecter.randomInstance.Next(affecter.connectedAffects.Count);

                affecter.currentAffect = affecter.connectedAffects[randomNum];
                return affecter.currentAffect;
            }
            else
            {
                int randomIndex = affecter.randomInstance.Next(possibleAffects.Count);
                affecter.currentAffect = possibleAffects[randomIndex];
                return affecter.currentAffect;
            }

        }

        /*
         * wrapper function around the GetPossibleAffects() to ChoosePrevailingAffect() pipeline to allow for easier, more fixed integration into other code
         * NOTE: this function is not intended to supercede the useage of both GetPossibleAffects() and ChoosePrevailingAffect()
         *  it is here for convenience and if the default behavior of immediately using the list created by GetPossibleAffects() in ChoosePrevailingAffect()
         *  is the desired functionality
         */
        public static string GetPrevailingAffect(Affecter affecter, Dictionary<string, double> affectVector)
        {
            List<string> possibleAffects = GetPossibleAffects(affectVector);
            string prevailingAffect = ChoosePrevailingAffect(affecter, possibleAffects);
            return prevailingAffect;
        }

        // evaluates a given affectVector based on the difference in values between the goalEmotion and the highest valued affects
        public static double EvaluateAffectVector(string currentAffect, Dictionary<string, double> affectVector, string goalEmotion)
        {
            double score = 0;
            double goalEmotionValue = affectVector[goalEmotion];

            List<string> maxAffects = GetPossibleAffects(affectVector);

            if (currentAffect.CompareTo(goalEmotion) == 0)
            {
                score += 1;
            }
            else if (maxAffects.Count > 1 && maxAffects.Contains(goalEmotion) && currentAffect.CompareTo(goalEmotion) != 0)
            {
                score -= 1;
            }
            else
            {
                foreach (KeyValuePair<string, double> affectEntry in affectVector)
                {
                    if (affectEntry.Key.CompareTo(goalEmotion) != 0)
                    {
                        score += goalEmotionValue - affectVector[affectEntry.Key];
                    }
                }
            }

            return score;
        }

        // provided function for formatting dictionaries for use with an Affecter
        // NOTE: it is recommended you make an Affecter instance THEN make the corresponding AffectVector to make sure the keys match
        public static Dictionary<string, double> MakeAffectVector(Dictionary<string, AffectEntry> referenceAffecter)
        {
            List<string> affectNames = referenceAffecter.Keys.ToList();
            Dictionary<string, double> affectVector = new Dictionary<string, double>();

            foreach(string affect in affectNames)
            {
                affectVector.Add(affect, referenceAffecter[affect].equilibrium_point);
            }

            return affectVector;
        }

    }

}
