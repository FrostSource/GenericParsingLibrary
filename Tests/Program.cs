// See https://aka.ms/new-console-template for more information
using IniParserExample;
using System.Diagnostics;

bool GENERATE        = false;
bool GENERATE_INSANE = true;
bool PARSE           = true;
bool PRINT_SECTIONS  = false;
bool PRINT_VALUES    = false;
bool FAST            = true;
bool COMPARE_PARSERS = true;

var t = new IniTokenizer("[ ]");
t.Tokenize();


if (GENERATE)
{
    var sw = Stopwatch.StartNew();
    Generator.GenerateIni(30, "generated_tiny");
    Generator.GenerateIni(100, "generated_small");
    Generator.GenerateIni(1000, "generated_med");
    Generator.GenerateIni(2500, "generated_large");
    Generator.GenerateIni(10000, "generated_giant"); // about 2mb file
    if (GENERATE_INSANE)
        Generator.GenerateIni(100000, "generated_insane"); // about 23mb file
    sw.Stop();
    Console.WriteLine($"Ini generation took {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
}

if (PARSE)
{
    var path = @"../../../testFiles/{0}.ini";

    string[] files =
    {
        //string.Format(path, "test1"),
        string.Format(path, "generated_tiny"),
        string.Format(path, "generated_small"),
        string.Format(path, "generated_med"),
        string.Format(path, "generated_large"),
        string.Format(path, "generated_giant"),
        string.Format(path, "generated_insane"),
    };

    foreach (var file in files)
    {
        if (!File.Exists(file)) continue;

        Console.WriteLine($"Parsing {file}...");
        var sw = Stopwatch.StartNew();
        var ini = new IniFile(file, FAST);
        sw.Stop();
        Console.WriteLine($"\t{(FAST ? "Fast" : "Normal")} parsing took {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
        if (COMPARE_PARSERS)
        {
            sw.Restart();
            _ = new IniFile(file, !FAST);
            sw.Stop();
            Console.WriteLine($"\t{(!FAST ? "Fast" : "Normal")} parsing took {sw.Elapsed} ({sw.ElapsedMilliseconds}ms)");
        }
        if (ini.Success)
        {
            Console.WriteLine($"\tNumber of sections: {ini.Length}");
            if (PRINT_SECTIONS)
            {
                Console.WriteLine("\tPrinting sections:");
                foreach (var section in ini)
                {
                    Console.WriteLine($"\t\t[{section.Key}] ({section.Value.Count})");
                    if (PRINT_VALUES)
                    {
                        foreach (var pair in section.Value)
                        {
                            //(pair.Value is double ? "double" : "string")
                            Console.WriteLine($"\t\t\t{pair.Key} = {pair.Value} ({pair.Value.GetType().ToString()[7..].ToLower()})");
                        }
                    }
                }
            }
        }
        else
        {
            Console.WriteLine(ini.ExceptionMessage);
        }
        Console.WriteLine();
    }
}

