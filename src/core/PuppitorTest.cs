using System;
using System.Collections.Generic;

class Tester{
    static void Main(){
                
        Dictionary<string, List<string>> modifierDict = new Dictionary<string, List<string>>(){
            {"tempo up", new List<string>{"c"}},
            {"tempo down", new List<string>{"z"}}
        };
        
        Dictionary<string, List<string>> actionDict = new Dictionary<string, List<string>>(){
            {"projected energy", new List<string>{"n"}},
            {"open flow", new List<string>{"m"}},
            {"closed flow", new List<string>{"b"}}
        };
        
        Dictionary<string, Dictionary<string, List<string>>> keyMap = new Dictionary<string, Dictionary<string, List<string>>>(){
            {"actions", actionDict},
            {"modifiers", modifierDict}
        
        };

        /*foreach(KeyValuePair<string, Dictionary<string, List<string>>> kvp in keyMap){
            Console.Write("Category = {0}\n", kvp.Key);
            
            foreach(KeyValuePair<string, List<string>> kvpInner in kvp.Value){
                Console.Write("\t{0} = {1}\n", kvpInner.Key, kvpInner.Value[0]);
            }
        }*/
        
        ActionKeyMap<string> test = new ActionKeyMap<string>(keyMap);
        
        //Console.Write(test.PrintKeyMap());
        
        test.UpdatePossibleStates("projected energy", true);
        
        Console.Write(test);
        
        test.UpdateActualStates("projected energy", "actions", true);
        
        Console.Write(test);
    }
}