// See https://aka.ms/new-console-template for more information
using IniParserExample;

var test1 = new FileInfo("../../../testFiles/test1.ini");

Console.WriteLine(test1.FullName);
var p = new IniFile(test1);
//var result = p.TryParse();
//Console.WriteLine($"TryParse result: {result}");
Console.WriteLine(p.ExceptionMessage);

Console.WriteLine();
Console.WriteLine(p.Length);
foreach(var section in p.Sections)
{
    Console.WriteLine();
    Console.WriteLine($"[{section.Key}] ({section.Value.Count})");
    Console.WriteLine();
    foreach(var pair in section.Value)
    {
        Console.WriteLine($"{pair.Key} = {pair.Value} ({(pair.Value is double ? "double" : "string")})");
    }
}

//var kwds = new string[] { "true", "false" };
//Console.WriteLine(string.Concat(kwds).Distinct().ToArray());
//Console.WriteLine("Hello, World!");
