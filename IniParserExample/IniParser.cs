
using System.Text;
using GenericParsingLibrary;

using IniKeyValueType = System.Collections.Generic.Dictionary<string, object>;
using IniFileType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;
namespace IniParserExample;

// Better way to do this?
public class IniParser : EasyParser
{

    public IniFileType? IniFile { get; private set; }

    /// <summary>
    /// Instantiate with a list of tokens.
    /// </summary>
    /// <param name="source"></param>
    public IniParser(List<GenericToken> tokens)
    {
        Tokens = tokens;
    }

    protected override bool ParseTop()
    {
        //<file>    ::= { <section> }+
        // We must parse one or more <section>

        var file = new IniFileType();

        OneOrMore(() =>
        {
            (var sectionName, IniKeyValueType sectionKeys) = ParseSection();
            file.Add(sectionName, sectionKeys);
        });
        IniFile = file;
        return true;
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
            KeyValuePair<string, object> pair = ParsePair();
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
            () =>
            {
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

    private KeyValuePair<string, object> ParsePair()
    {
        //<id> , "=" , <value>
        // This rule method handles both <id> and <value> rules itself.
        var key = Eat(TokenType.Identifier).Value;
        Eat(TokenType.Symbol, "=");
        var value = EitherOr<object>(
            () =>
            {
                return Eat(TokenType.String).Value;
            }
            ,
            () =>
            {
                return Eat(TokenType.Identifier).Value;
            }
            ,
            () =>
            {
                // whole and decimal numbers are double for our ini file
                return double.Parse(Eat(TokenType.Number).Value);
            }
            ,
            () =>
            {
                return Eat(TokenType.Keyword).Value.ToLower() == "true";
            }
            );
        return new KeyValuePair<string, object>(key, value);
    }
}

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
