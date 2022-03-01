using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Puppitor;

string fileName = @"affect_rules\test_rules.json";
string jsonString = File.ReadAllText(fileName);
Console.WriteLine(jsonString);

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

ActionKeyMap<string> test = new ActionKeyMap<string>(keyMap);

//Console.Write(test.PrintKeyMap());

test.UpdatePossibleStates("projected energy", true);

Console.Write(test);

test.UpdateActualStates("projected energy", "actions", true);

Console.Write(test);
