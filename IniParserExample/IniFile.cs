

using System.Collections.ObjectModel;



namespace IniParserExample
{
    using IniKeyValueType = System.Collections.Generic.Dictionary<string, dynamic>;
    using IniFileType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, dynamic>>;
    public class IniFile
    {
        private readonly IniFileType section;

        public readonly IReadOnlyDictionary<string, IniKeyValueType> Sections;
        public readonly int Length;
        public readonly string ExceptionMessage;
        public IniFile(FileInfo path)
        {
            var parser = new IniParser(path);
            // Checking not null here is just for C# parser
            // is there a way to tell it that IniFile definitely won't
            // be null after TryParse?
            if (parser.TryParse() && parser.IniFile != null)
            {
                section = parser.IniFile;
            }
            else
            {
                section = new();
            }

            Sections = new ReadOnlyDictionary<string, IniKeyValueType>(section);
            ExceptionMessage = parser.LastExceptionMessage;
            Length = section.Count;
        }

        /// <summary>
        /// Get the dictionary of keys for a given section.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public IniKeyValueType this[string section] => this.section[section];
        /// <summary>
        /// Get the value of a given key in a section.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string this[string section, string key] => this.section[section][key];
    }
}