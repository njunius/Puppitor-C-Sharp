using System;
using System.Collections.Generic;
using System.Linq;

namespace Puppitor
{
    /*
     * action_key_map contains the interface for storing keybindings and performing actions
     * the dictionary is modeled on Ren'Py's keymap
     * the class also wraps the flags for detecting if an action is being done
     *
     * the possible_action_states dict is used to keep track of all interpreted actions being done and is updated based on keys and buttons pressed
     *
     * the actual_action_states is broken into modifier states and actions
     * only one action can be happening at a time
     * only one modifier can be active at a time
     * modifiers update independently from actions
     * actions update independently from modifiers
     *
     */
    public class ActionKeyMap<InputT>
    {

        public Dictionary<string, Dictionary<string, List<InputT>>> keyMap;

        public Dictionary<string, string> defaultStates;
        public Dictionary<string, string> currentStates;

        public Dictionary<string, bool> possibleActionStates;
        public Dictionary<string, Dictionary<string, bool>> actualActionStates;

        private Dictionary<string, List<string>> updatableStates;

        public List<Tuple<string, string>> moves;

        public ActionKeyMap(Dictionary<string, Dictionary<string, List<InputT>>> keyMap, string defaultAction = "resting", string defaultModifier = "neutral")
        {
            // this dictionary and values should not be modified ever and are generally for internal use only
            defaultStates = new Dictionary<string, string>();
            defaultStates.Add("actions", defaultAction);
            defaultStates.Add("modifiers", defaultModifier);

            // this dictionary and values should only be modified internally and are used to access the current state of the actions being performed 
            currentStates = new Dictionary<string, string>();
            currentStates.Add("actions", defaultStates["actions"]);
            currentStates.Add("modifiers", defaultStates["modifiers"]);

            moves = new List<Tuple<string, string>>();

            // expected format of keyMap
            /*
            {
                "actions": {
                            "open_flow": [InputT n], 
                            'closed_flow': [InputT m], 
                            'projected_energy': [InputT b]
                },
                "modifiers": {
                            "tempo_up": [InputT c],
                            "tempo_down": [InputT z]
                }
            }
            */

            this.keyMap = keyMap;

            // flags corresponding to actions being specified by the user input
            // FOR INPUT DETECTION USE ONLY
            // MULTIPLE ACTIONS AND MODIFIER STATES CAN BE TRUE
            // structure of possibleActionStates
            /*
                possibleActionStates = {
                            "open_flow" : false,
                            "closed_flow" : false,
                            "projected_energy" : false,
                            "tempo_up" : false,
                            "tempo_down" : false
            */

            possibleActionStates = new Dictionary<string, bool>();

            foreach (string action in keyMap["actions"].Keys)
            {
                possibleActionStates.Add(action, false);
            }

            foreach (string modifier in keyMap["modifiers"].Keys)
            {
                possibleActionStates.Add(modifier, false);
            }

            // flags used for specifying the current state of actions for use in updating a character's physical affect
            // FOR SEMANTIC USE
            // ONLY ONE ACTION AND MODIFIER STATE CAN BE TRUE

            /*actualActionStates = {
                            "actions" : {"resting" : true, "open_flow" : false, "closed_flow" : false, "projected_energy" : false},
                            "modifiers" : {"tempo_up" : false, "tempo_down" : false, "neutral" : true}
            */

            Dictionary<string, bool> tempActualActionDict = new Dictionary<string, bool>();
            tempActualActionDict[defaultAction] = true;
            foreach (string action in keyMap["actions"].Keys)
            {
                tempActualActionDict[action] = false;
            }

            Dictionary<string, bool> tempActualModifierDict = new Dictionary<string, bool>();
            tempActualModifierDict[defaultModifier] = true;
            foreach (string modifier in keyMap["modifiers"].Keys)
            {
                tempActualModifierDict[modifier] = false;
            }

            actualActionStates = new Dictionary<string, Dictionary<string, bool>>();
            actualActionStates.Add("actions", tempActualActionDict);
            actualActionStates.Add("modifiers", tempActualModifierDict);

            tempActualActionDict = null;
            tempActualModifierDict = null;

            updatableStates = new Dictionary<string, List<string>>();
            updatableStates.Add("actions", actualActionStates["actions"].Keys.ToList());
            updatableStates.Add("modifiers", actualActionStates["modifiers"].Keys.ToList());

            foreach(string action in actualActionStates["actions"].Keys)
            {
                foreach(string modifier in actualActionStates["modifiers"].Keys)
                {
                    moves.Add(new Tuple<string, string>(action, modifier));
                }
            }

            Console.Write(this);
        }

        // USED FOR UPDATING BASED ON KEYBOARD INPUTS    
        public void UpdatePossibleStates(string stateToUpdate, bool newValue)
        {

            if (possibleActionStates.ContainsKey(stateToUpdate))
            {
                possibleActionStates[stateToUpdate] = newValue;
            }

            return;
        }

        /* USED FOR UPDATING THE INTERPRETABLE STATE BASED ON WHICH ACTION IS DISPLAYED
         * updates a specified action or modifier to a new boolean value
         * UPDATING AN ACTION WILL SET ALL OTHER ACTIONS TO FALSE
         * UPDATING A MODIFIER WILL SET ALL OTHER MODIFIERS TO FALSE

         * MODIFERS AND ACTIONS ARE ASSUMED TO BE MUTUALLY EXCLUSIVE WHEN UPDATING
         */
        public void UpdateActualStates(string stateToUpdate, string classOfAction, bool newValue)
        {

            List<string> states = updatableStates[classOfAction];

            if (states.Contains(stateToUpdate))
            {
                // go through each of the possible actions or modifiers
                // and set all but the one being explicitly changed to false
                // and use the given value (newValue) to update the value of the
                // specified action/modifier
                foreach (string state in states)
                {

                    if (stateToUpdate.Equals(state))
                    {
                        actualActionStates[classOfAction][state] = newValue;
                        currentStates[classOfAction] = state;
                    }
                    else
                    {
                        actualActionStates[classOfAction][state] = false;
                    }
                }

                if (newValue == false)
                {
                    // return to doing the default behavior
                    currentStates[classOfAction] = defaultStates[classOfAction];
                    actualActionStates[classOfAction][defaultStates[classOfAction]] = true;
                }
            }

            return;
        }

        // makes a copy of moves to allow search algorithms like MCTS to easily store lists of available moves
        public List<Tuple<string, string>> GetMoves()
        {
            List<Tuple<string, string>> moveList = new List<Tuple<string, string>>();

            foreach(Tuple<string, string> move in moves)
            {
                moveList.Add(new Tuple<string, string>(move.Item1, move.Item2));
            }

            return moveList;
        }

        // switches the default action or modifier to the specified new default
        // newDefault must be an action or modifier in the existing set of actions and modifiers contained in ActionKeyMap
        // classOfAction must be either "actions" or "modifiers"
        public void ChangeDefault(string newDefault, string classOfAction)
        {
            if (!defaultStates.ContainsKey(classOfAction))
            {
                Console.WriteLine(classOfAction + " is not an \"action\" or \"modifier\"");
                return;
            }

            if (!keyMap[classOfAction].ContainsKey(newDefault))
            {
                Console.WriteLine(newDefault + " is not in " + classOfAction);
                return;
            }

            string oldDefault = defaultStates[classOfAction];
            List<InputT> oldNonDefaultKeys = keyMap[classOfAction][newDefault];

            Console.WriteLine("original default: " + oldDefault + ", newDefault original keys: ");

            foreach(InputT key in oldNonDefaultKeys)
            {
                Console.WriteLine(key);
            }

            defaultStates[classOfAction] = newDefault;
            keyMap[classOfAction].Remove(newDefault);
            possibleActionStates.Remove(newDefault);
            keyMap[classOfAction][oldDefault] = oldNonDefaultKeys;
            possibleActionStates[oldDefault] = false;

            Console.WriteLine("keyMap: " + this);
        }

        public override string ToString()
        {

            string result = "\n";

            result += "defaultAction = " + defaultStates["actions"] + "\ndefaultModifier = " + defaultStates["modifiers"] + "\n\n";

            result += "currentAction = " + currentStates["actions"] + "\ncurrentModifier = " + currentStates["modifiers"] + "\n";

            result += "\nKeyMap:\n";

            foreach (KeyValuePair<string, Dictionary<string, List<InputT>>> kvp in keyMap)
            {
                result += "\t" + kvp.Key + "\n";

                foreach (KeyValuePair<string, List<InputT>> kvpInner in kvp.Value)
                {
                    result += "\t\t" + kvpInner.Key + " = ";

                    foreach (InputT key in kvpInner.Value)
                    {
                        result += key + "\n";
                    }
                }
            }

            result += "\nPossibleActionStates:\n";

            foreach (KeyValuePair<string, bool> kvp in possibleActionStates)
            {
                result += "\t" + kvp.Key + " = " + kvp.Value + "\n";
            }

            result += "\nActualActionStates:\n";

            foreach (KeyValuePair<string, Dictionary<string, bool>> kvp in actualActionStates)
            {
                result += "\t" + kvp.Key + "\n";

                foreach (KeyValuePair<string, bool> kvpInner in kvp.Value)
                {
                    result += "\t\t" + kvpInner.Key + " = " + kvpInner.Value + "\n";

                }
            }

            result += "\nMoves:";

            foreach(Tuple<string, string> move in moves)
            {
                result += "(" + move.Item1 + ", " + move.Item2 + ") ";
            }

            result += "\n";

            return result;

        }

    }
}