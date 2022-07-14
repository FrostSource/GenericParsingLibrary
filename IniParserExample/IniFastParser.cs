
using System.Text;
using GenericParsingLibrary;

// Better way to do this?
using IniKeyValueType = System.Collections.Generic.Dictionary<string, object>;
using IniFileType = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, object>>;
namespace IniParserExample;

public class IniFastParser : FastParser
{

    public IniFileType? IniFile { get; private set; }

    /// <summary>
    /// Instantiate with a list of tokens.
    /// </summary>
    /// <param name="source"></param>
    public IniFastParser(List<GenericToken> tokens) : base(tokens) { }
    public IniFastParser(IniTokenizer tokenizer) : this(tokenizer.Tokens) { }


    protected override bool ParseTop()
    {
        //<file>    ::= { <section> }+
        // We must parse one or more <section>

        var file = new IniFileType();
        OneOrMore(() =>
        {
            if (!ParseSection()) return false;
            (var sectionName, IniKeyValueType sectionKeys) = ((string sectionName, IniKeyValueType sectionKeys))GetCache();
            file.Add(sectionName, sectionKeys);
            return true;
        });
        IniFile = file;
        return true;
    }

    private bool ParseSection()
    {
        //(string sectionName, IniKeyValueType sectionKeys)
        //<section> ::= "[" , <id> , "]" , <EOL> , { <pair> , <EOL> }*

        var sectionKeys = new IniKeyValueType();

        // Parse the section syntax
        if (!Eat(TokenType.Symbol, "[")) return false;
        // This part also handles the <sctn_name> rule
        // without creating a new method.
        var nameBuilder = new StringBuilder();
        if (!OneOrMore(() =>
        {
            if (!ParseId()) return false;
            nameBuilder.Append((string)GetCache());
            return true;
        })) return false;

        var sectionName = nameBuilder.ToString();
        if (!Eat(TokenType.Symbol, "]")) return false;

        // Parse end of line
        if (!ParseEOL()) return false;

        // Zero or more pairs
        ZeroOrMore(() =>
        {
            if (!ParsePair()) return false;
            var pair = (KeyValuePair<string, object>)GetCache();
            if (!ParseEOL()) return false;
            // Add the pair after finishing all parsing in this delegate
            sectionKeys.Add(pair.Key, pair.Value);
            return true;
        });

        // Return the data for the top method to handle
        CacheValue((sectionName, sectionKeys));
        return true;
    }

    private bool ParseId()
    {
        //<id>      ::= [ \W | \D | "_" ] , { [ \W | \D | "_" ] }*
        // The specifics are taken care of in the tokenizer
        if (!Eat(TokenType.Identifier)) return false;
        CacheValue(PreviousToken.Value);
        return true;
    }

    private bool ParseEOL()
    {
        //<EOL>       ::= [ { "\n" }+ | EOF ]
        return EitherOr(
            () =>
            {
                // Multiple lines are allowed between values.
                return OneOrMore(() => { return Eat(TokenType.Symbol, "\n"); });
            }
            ,
            () => { return NextToken == null; }
            );
    }

    private bool ParsePair()
    {
        //KeyValuePair<string, object>
        //<id> , "=" , <value>
        // This rule method handles both <id> and <value> rules itself.
        if (!Eat(TokenType.Identifier)) return false;
        var key = PreviousToken.Value;
        if (!Eat(TokenType.Symbol, "=")) return false;
        if (!EitherOr(
            () =>
            {
                if (!Eat(TokenType.String)) return false;
                CacheValue(PreviousToken.Value);
                return true;
            }
            ,
            () =>
            {
                if (!Eat(TokenType.Identifier)) return false;
                CacheValue(PreviousToken.Value);
                return true;
            }
            ,
            () =>
            {
                // whole and decimal numbers are double for our ini file
                if (!Eat(TokenType.Number)) return false;
                CacheValue(double.Parse(PreviousToken.Value));
                return true;
            }
            ,
            () =>
            {
                if (!Eat(TokenType.Keyword)) return false;
                CacheValue(PreviousToken.Value.ToLower() == "true");
                return true;
            }
            ))
            return false;
        CacheValue(new KeyValuePair<string, object>(key, GetCache()));
        return true;
    }

    ///// <summary>
    ///// Parse the source content.
    ///// </summary>
    //public override void Parse()
    //{

    //    // Then the top level method searches the nodes to generate an ini file dictionary.
    //    ParseTop();
    //}
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
