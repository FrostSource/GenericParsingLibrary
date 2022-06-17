using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GenericParsingLibrary;

/* This parser described in EBNF and some regex.
* 
* A series of section names enclosed by [ ].
* Each section can have a series of key/value pairs, each on its own line, separated by '='.
* 
* A section may be any string of characters including spaces, except ';' and '='.
* A section will NOT be trimmed of white space.
* 
* Keys and values may be any string of characters including spaces, except ';' and '='.
* Keys and values will be white space trimmed on both sides.
* 
* 
* (*) = Zero or more
* (+) = One or more
* (?) = Zero or one
* (.) = Anything
* 
* \W = Alpha numeric chars
* \D = Digit chars
* 
* <file>      ::= { <section> }+
* <section>   ::= <sctn_name> , <EOL>, { <pair> , <EOL> }*
* <sctn_name> ::= "[" , { <id> }+ , "]"
* <EOL>       ::= [ { "\n" }+ | EOF ]
* <pair>      ::= <id> , "=" , <value>
* <value>     ::= { . }+
* <id>        ::= [ \W | \D | "_" ] , { [ \W | \D | "_" ] }*
* 
* It is not always necessary to create a method for every rule,
* sometimes multiple rules can be handles in a single method.
* 
* Tree structure:
* 
* value = language
* 
* left = null
* right = 
*      value = tokens list
*      left = null
*      right = null
*      
* Tokens list:
* 
* value = null
* left =
*      value = caption token
*      left = null
*      right = null
* right = 
*      value = caption text
*      left = null
*      right = null
*      
*
*/

namespace IniParserExample
{
    // Better way to do this?
    using IniKeyValueType = System.Collections.Generic.Dictionary<string, dynamic>;
    using IniFileType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, dynamic>>;
    public class IniParser : GenericParser
    {

        private IniTokenizer Tokenizer { get; set; }
        /// <summary>
        /// A node tree is a collection of nodes, each with a value and left/right node.
        /// The value could be any type of object so knowing the structure is vital.
        /// </summary>
        private GenericNode? NodeTree { get; set; }

        public string LastExceptionMessage { get; private set; } = string.Empty;

        public IniFileType? IniFile { get; private set; }

        /// <summary>
        /// Instantiate with a source string.
        /// Tokenizer is automatically created.
        /// </summary>
        /// <param name="source"></param>
        public IniParser(string source)
        {
            Tokenizer = new IniTokenizer(source);
        }
        /// <summary>
        /// Instantiate with a source text file.
        /// Tokenizer is automatically created.
        /// </summary>
        /// <param name="file"></param>
        public IniParser(FileInfo file) : this(File.ReadAllText(file.FullName))
        {

        }

        private IniFileType ParseFile()
        {
            //<file>    ::= { <section> }+
            // We must parse one or more <section>

            var file = new IniFileType();

            OneOrMore(() =>
            {
                var result = ParseSection();
                file.Add(result.sectionName, result.sectionKeys);
            });
            return file;
        }

        private (string sectionName, IniKeyValueType sectionKeys) ParseSection()
        {
            //<section> ::= "[" , <id> , "]" , <EOL> , { <pair> , <EOL> }*

            var sectionKeys = new IniKeyValueType();

            // Parse the section syntax
            Eat(TokenType.Symbol, "[");
            // This part also handles the <sctn_name> rule
            // without creating a new method.
            var nameBuilder = new StringBuilder();
            OneOrMore(() =>
            {
                var name = ParseId();
                nameBuilder.Append(name);
            });
            var sectionName = nameBuilder.ToString();
            Eat(TokenType.Symbol, "]");

            // Parse end of line
            ParseEOL();

            // Zero or more pairs
            ZeroOrMore(() =>
            {
                var pair = ParsePair();
                ParseEOL();
                // Add the pair after finishing all parsing in this delegate
                sectionKeys.Add(pair.Key, pair.Value);
            });

            // Return the data for the top method to handle
            return (sectionName, sectionKeys);
        }

        private string ParseId()
        {
            //<id>      ::= [ \W | \D | "_" ] , { [ \W | \D | "_" ] }*
            // The specifics are taken care of in the tokenizer
            return Eat(TokenType.Identifier).Value;
        }

        private void ParseEOL()
        {
            //<EOL>       ::= [ { "\n" }+ | EOF ]
            EitherOr(
                () => {
                    // Multiple lines are allowed between values.
                    OneOrMore(
                        () => { Eat(TokenType.Symbol, "\n"); }
                        );
                    return true;
                    }
                ,
                () => { return NextToken == null; }
                );
        }

        private KeyValuePair<string, dynamic> ParsePair()
        {
            //<id> , "=" , <value>
            // This rule method handles both <id> and <value> rules itself.
            var key = Eat(TokenType.Identifier).Value;
            Eat(TokenType.Symbol, "=");
            dynamic value = "";
            EitherOr(
                () =>
                {
                    value = Eat(TokenType.Identifier).Value;
                    return true;
                }
                ,
                () =>
                {
                    // whole and decimal numbers are double for our ini file
                    value = Double.Parse(Eat(TokenType.Number).Value);
                    return true;
                }
                );
            return new KeyValuePair<string, dynamic>(key, value);
        }

        /// <summary>
        /// Parse the source content.
        /// </summary>
        public void Parse()
        {
            // Tokenizing needs to be done first.
            Tokens = Tokenizer.Tokenize();
            // Then the top level method searches the nodes to generate an ini file dictionary.
            IniFile = ParseFile();
        }

        /// <summary>
        /// Safely try to parse, catching any exceptions, returning the result.
        /// </summary>
        /// <returns></returns>
        public bool TryParse()
        {
            try
            {
                Parse();
                return true;
            }
            catch (ParserException e)
            {
                LastExceptionMessage = e.Message;
                //Console.WriteLine(GenericTokenizer.ReplaceSpecialCharsInString(e.Message));
                return false;
            }
        }
    }
}
