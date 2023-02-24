using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Puppitor;
using AStarSearch;

Tuple<string, string> UpdateFromPath (List<Tuple<string, string>> path)
{
    Tuple<string, string> node = path[path.Count - 1];
    path.RemoveAt(path.Count - 1);
    return node;
}

void MaximizeMinimizeAV (Dictionary<string, double> affectVector, Affecter affecter, bool zeroEven = true){
    int count = 0;
    double even;
    double odd;
    if (zeroEven)
    {
        even = 0.0;
        odd = 1.0;
    }
    else
    {
        even = 1.0;
        odd = 0.0;
    }
    foreach (string affectName in affectVector.Keys.ToList())
    {
        if (count % 2 == 0)
        {
            affectVector[affectName] = even;
        }
        else
        {
            affectVector[affectName] = odd;
        }
        count++;
    }
    Affecter.GetPrevailingAffect(affecter, affectVector);

    return;
}

void ApplyPrintPath (List<Tuple<string, string>> path, Affecter affecter, Dictionary<string, double> affectVector, bool verbose)
{
    while (path.Count > 0)
    {
        Tuple<string, string> step = UpdateFromPath(path);
        affecter.UpdateAffect(affectVector, step.Item1, step.Item2);

        if (verbose)
        {
            Console.WriteLine("\naction and modifier: " + step.Item1 + ", " + step.Item2);
            Console.Write("\ncharacter affect vector: ");
            PrintAffectVector(affectVector);
        }
    }
    return;
}

void PrintRunInfo(double stepValue, string emotionalGoal, Dictionary<string, double> affectVector)
{
    Console.WriteLine("\nstep value: {0} \nemotional goal: {1}", stepValue, emotionalGoal);
    Console.Write("\nstarting affect vector: ");
    PrintAffectVector(affectVector);
    return;
}

void PrintAffectVector (Dictionary<string, double> affectVector)
{
    Console.Write("{");
    foreach (KeyValuePair<string, double> entry in affectVector)
    {
        Console.Write("(" + entry.Key + ": " + entry.Value + ") ");
    }
    Console.WriteLine("}\n");
    return;
}

bool verbose = false;

string fileName = @"affect_rules\test_passions_rules.json";
string jsonString = File.ReadAllText(fileName);

Affecter affecterTest = new Affecter(jsonString);
Dictionary<string, double> affectVector = Affecter.MakeAffectVector(affecterTest.affectRules);

Dictionary<string, List<string>> modifierDict = new Dictionary<string, List<string>>(){
            {"tempo_up", new List<string>{"c"}},
            {"tempo_down", new List<string>{"z"}}
        };

Dictionary<string, List<string>> actionDict = new Dictionary<string, List<string>>(){
            {"projected_energy", new List<string>{"n"}},
            {"open_flow", new List<string>{"m"}},
            {"closed_flow", new List<string>{"b"}}
        };

Dictionary<string, Dictionary<string, List<string>>> keyMap = new Dictionary<string, Dictionary<string, List<string>>>(){
            {"actions", actionDict},
            {"modifiers", modifierDict}

        };

ActionKeyMap<string> testKeyMap = new ActionKeyMap<string>(keyMap);

Console.WriteLine();

double stepValue = 90;

string emotinalGoal = "anger";
string action = "resting";
string modifier = "neutral";

foreach(KeyValuePair<string, double> entry in affectVector)
{
    emotinalGoal = entry.Key;
    action = testKeyMap.defaultStates["actions"];
    modifier = testKeyMap.defaultStates["modifiers"];

    MaximizeMinimizeAV(affectVector, affecterTest);

    PrintRunInfo(stepValue, emotinalGoal, affectVector);

    List<Tuple<string, string>> actionPath = new List<Tuple<string, string>>();
    Tuple<Dictionary<string, double>, string, string, string> startNode = new Tuple<Dictionary<string, double>, string, string, string>(affectVector, action, modifier, affecterTest.currentAffect);
    actionPath = AStarThink.A_Star_Think(affecterTest, testKeyMap, startNode, emotinalGoal, stepValue);

    ApplyPrintPath(actionPath, affecterTest, affectVector, verbose);

    Console.Write("\nfinal affectVector: ");
    PrintAffectVector(affectVector);
}
