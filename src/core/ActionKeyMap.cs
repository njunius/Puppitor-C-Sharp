using System;
using System.Collections.Generic;

public class ActionKeyMap<InputT> {
    
    Dictionary<string, Dictionary<string, List<InputT>>> classKeyMap;
    
    Dictionary<string, string> defaultStates;
    Dictionary<string, string> currentStates;
    
    Dictionary<string, bool> possibleActionStates;
    Dictionary<string, Dictionary<string, bool>> actualActionStates;
    
    public ActionKeyMap(Dictionary<string, Dictionary<string, List<InputT>>> keyMap, string defaultAction = "resting", string defaultModifier = "neutral") {
        
        defaultStates = new Dictionary<string, string>();
        defaultStates.Add("actions", defaultAction);
        defaultStates.Add("modifiers", defaultModifier);
        
        currentStates = new Dictionary<string, string>();
        currentStates.Add("actions", defaultStates["actions"]);
        currentStates.Add("modifiers", defaultStates["modifiers"]);
    
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
        
        classKeyMap = keyMap;

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
        
        foreach(string action in classKeyMap["actions"].Keys){
            possibleActionStates.Add(action, false);
        }
        
        foreach(string modifier in classKeyMap["modifiers"].Keys){
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
        foreach(string action in classKeyMap["actions"].Keys){
            tempActualActionDict[action] = false;
        }
        
        Dictionary<string, bool> tempActualModifierDict = new Dictionary<string, bool>();
        tempActualModifierDict[defaultModifier] = true;
        foreach(string modifier in classKeyMap["modifiers"].Keys){
            tempActualModifierDict[modifier] = false;
        }
        
        actualActionStates = new Dictionary<string, Dictionary<string, bool>>();
        actualActionStates.Add("actions", tempActualActionDict);
        actualActionStates.Add("modifiers", tempActualModifierDict);
        
        tempActualActionDict = null;
        tempActualModifierDict = null;
        
        Console.Write(this);
    }
    
    public override string ToString(){
        
        string result = "\n";
        
        result += "defaultAction = " + defaultStates["actions"] + "\ndefaultModifier = " + defaultStates["modifiers"] + "\n\n";
        
        result += "currentAction = " + currentStates["actions"] + "\ncurrentModifier = " + currentStates["modifiers"] + "\n";
        
        result += "\nKeyMap:\n";
        
        foreach(KeyValuePair<string, Dictionary<string, List<InputT>>> kvp in classKeyMap){
            result += "\t" + kvp.Key + "\n";
            
            foreach(KeyValuePair<string, List<InputT>> kvpInner in kvp.Value){
                result += "\t\t" + kvpInner.Key + " = ";
                
                foreach(InputT key in kvpInner.Value){
                    result += key + "\n";
                }
            }
        }
        
        result += "\nPossibleActionStates:\n";
        
        foreach(KeyValuePair<string, bool> kvp in possibleActionStates){
            result += "\t" + kvp.Key + " = " + kvp.Value + "\n";
        }
        
        result += "\nActualActionStates:\n";
        
        foreach(KeyValuePair<string, Dictionary<string, bool>> kvp in actualActionStates){
            result += "\t" + kvp.Key + "\n";
            
            foreach(KeyValuePair<string, bool> kvpInner in kvp.Value){
                result += "\t\t" + kvpInner.Key + " = " + kvpInner.Value + "\n";
                
            }
        }
        
        result += "\n";
        
        return result;
    
    }
}