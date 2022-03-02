using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;

namespace Puppitor
{

    public struct AffectEntry
    {
        public string affectName;
        public Dictionary<string, double> actions;
        public Dictionary<string, double> modifiers;
        public List<string> adjacentAffects;
        public double equilibriumPoint;

        // TODO: ToString Function

    }

    public class Affecter
    {
        public Dictionary<string, AffectEntry> affectRules;
        public double floorValue;
        public double ceilValue;
        public string equilibriumClassAction;
        public string? currentAffect;

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
        }

        private void ConvertRules(JSONClass affectRulesTemp)
        {
            foreach(KeyValuePair<string, JSONNode> nodeEntry in affectRulesTemp)
            {
                AffectEntry affectEntry;
                affectEntry.affectName = nodeEntry.Key;
                affectEntry.equilibriumPoint = Convert.ToDouble(nodeEntry.Value["equilibrium_point"]);
                affectEntry.adjacentAffects = new List<string>();
                affectEntry.actions = new Dictionary<string, double>();
                affectEntry.modifiers = new Dictionary<string, double>();

                foreach (JSONData entry in nodeEntry.Value["adjacent_affects"].AsArray)
                {
                    affectEntry.adjacentAffects.Add(entry.Value);
                    Console.WriteLine(affectEntry.adjacentAffects[affectEntry.adjacentAffects.Count - 1]);
                }

                foreach(KeyValuePair<string, JSONNode> actionEntry in nodeEntry.Value["actions"].AsObject)
                {
                    double tempDoubleVal = Convert.ToDouble(actionEntry.Value);
                    affectEntry.actions.Add(actionEntry.Key, tempDoubleVal);
                    Console.WriteLine("{0}: {1}", actionEntry.Key, affectEntry.actions[actionEntry.Key]);
                }

                foreach (KeyValuePair<string, JSONNode> modEntry in nodeEntry.Value["modifiers"].AsObject)
                {
                    double tempDoubleVal = Convert.ToDouble(modEntry.Value);
                    affectEntry.modifiers.Add(modEntry.Key, tempDoubleVal);
                    Console.WriteLine("{0}: {1}", modEntry.Key, affectEntry.modifiers[modEntry.Key]);
                }

                affectRules.Add(nodeEntry.Key, affectEntry);

                return;

            }
        }

    }
}
