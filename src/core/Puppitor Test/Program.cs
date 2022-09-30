using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Puppitor;

string fileName = @"affect_rules\test_passions_rules.json";
string jsonString = File.ReadAllText(fileName);

Affecter affecterTest = new Affecter(jsonString);
Dictionary<string, double> affectVector = Affecter.MakeAffectVector(affecterTest.affectRules.Keys.ToList<string>(), affecterTest.affectRules);

Console.WriteLine("\naffectVector");
foreach (KeyValuePair<string, double> affect in affectVector)
{
    Console.WriteLine("{0}: {1}", affect.Key, affect.Value);
}

affecterTest.UpdateAffect(affectVector, "open_flow", "neutral");

Console.WriteLine("\naffectVector updated");
foreach (KeyValuePair<string, double> affect in affectVector)
{
    Console.WriteLine("{0}: {1}", affect.Key, affect.Value);
}

Console.WriteLine("\naffectVector Prevailing Affects");
List<string> prevailingAffects = affecterTest.GetPossibleAffects(affectVector);
foreach(string affect in prevailingAffects)
{
    Console.WriteLine(affect);
}

Console.WriteLine("\naffectVector Current Affect");
string currAffect = affecterTest.ChoosePrevailingAffect(prevailingAffects);
Console.WriteLine(currAffect);
Console.WriteLine("wrapper function result: {0}", affecterTest.GetPrevailingAffect(affectVector));

fileName = @"affect_rules\test_different_rules.json";
jsonString = File.ReadAllText(fileName);
Affecter differentAffecterTest = new Affecter(jsonString);

foreach (KeyValuePair<string, AffectEntry> affect in differentAffecterTest.affectRules)
{
    Console.WriteLine("{0}: {1}", affect.Key, affect.Value.affectName);
}

Dictionary<string, double> differentAffectVector = Affecter.MakeAffectVector(differentAffecterTest.affectRules.Keys.ToList<string>(), differentAffecterTest.affectRules);

Console.WriteLine("\ndifferentAffectVector");
foreach(KeyValuePair<string, double> affect in differentAffectVector)
{
    Console.WriteLine("{0}: {1}", affect.Key, affect.Value);
}

differentAffecterTest.UpdateAffect(differentAffectVector, "cross_arms", "casually");

Console.WriteLine("\ndifferentAffectVector updated");
foreach (KeyValuePair<string, double> affect in differentAffectVector)
{
    Console.WriteLine("{0}: {1}", affect.Key, affect.Value);
}

Console.WriteLine("\ndifferentAffectVector Prevailing Affects");
List<string> diffPrevailingAffects = differentAffecterTest.GetPossibleAffects(differentAffectVector);
foreach (string affect in diffPrevailingAffects)
{
    Console.WriteLine(affect);
}

Console.WriteLine("\ndifferentAffectVector Current Affect");
currAffect = differentAffecterTest.ChoosePrevailingAffect(diffPrevailingAffects);
Console.WriteLine(currAffect);

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


test.UpdatePossibleStates("projected energy", true);

Console.Write(test);

test.UpdateActualStates("projected energy", "actions", true);

Console.Write(test);
