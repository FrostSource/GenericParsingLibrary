using System.Collections;
using System.Collections.ObjectModel;

namespace IniParserExample;

public class IniFile : IEnumerable<KeyValuePair<string, Dictionary<string, object>>>
{
    public int Length => Sections.Count;
    public string ExceptionMessage { get; } = string.Empty;
    public bool Success { get; }

    private Dictionary<string, Dictionary<string, object>> Sections { get; }

    public IniFile(string path, bool fast = false)
    {
        // Tokenizing needs to be done first.
        var tokenizer = new IniTokenizer(File.ReadAllText(path));
        if (!tokenizer.TryTokenize())
        {
            ExceptionMessage = tokenizer.ExceptionMessage;
            Sections = new();
            return;
        }
        if (fast)
        {
            var parser = new IniFastParser(tokenizer.Tokens);
            // Checking not null here is just for C# compiler
            // is there a way to tell it that IniFile definitely won't
            // be null after TryParse?
            if (parser.TryParse() && parser.IniFile != null)
            {
                Sections = parser.IniFile;
                Success = true;
            }
            else
            {
                Sections = new();
                Success = false;
                ExceptionMessage = parser.ExceptionMessage;
            }
        }
        else
        {
            var parser = new IniParser(tokenizer.Tokens);
            if (parser.TryParse() && parser.IniFile != null)
            {
                Sections = parser.IniFile;
                Success = true;
            }
            else
            {
                Sections = new();
                Success = false;
                ExceptionMessage = parser.ExceptionMessage;
            }
        }
    }
    public IniFile(FileInfo path, bool fast = false) : this(path.FullName, fast)
    {
    }

    /// <summary>
    /// Gets the dictionary of keys for a given section.
    /// </summary>
    /// <param name="section"></param>
    /// <returns></returns>
    public ReadOnlyDictionary<string, object> this[string section] => new(Sections[section]);
    /// <summary>
    /// Gets the value of a given key in a section.
    /// </summary>
    /// <param name="section"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public object this[string section, string key] => Sections[section][key];

    public IEnumerator<KeyValuePair<string, Dictionary<string, object>>> GetEnumerator()
    {
        return Sections.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}