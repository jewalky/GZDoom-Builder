using CodeImp.DoomBuilder.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeImp.DoomBuilder.ZDoom
{
    public sealed class ZScriptActorStructure : ActorStructure
    {
        // privates
        private ZScriptParser parser;
        private Stream stream;
        private ZScriptTokenizer tokenizer;
        // ========

        internal static bool ParseGZDBComment(Dictionary<string, List<string>> props, string text)
        {
            text = text.Trim();
            // check if it's a GZDB comment
            if (text[0] != '$')
                return false;
            // check next occurrence of " \t\r\u00A0", then put everything else as property without parsing
            int nextWhitespace = text.IndexOfAny(new char[] { ' ', '\t', '\r', '\u00A0' });

            string propertyname = text;
            string propertyvalue = "";
            if (nextWhitespace >= 0)
            {
                propertyname = propertyname.Substring(0, nextWhitespace);
                propertyvalue = text.Substring(nextWhitespace + 1).Trim();
            }

            props[propertyname.ToLowerInvariant()] = new List<string> { propertyvalue };
            return true;
        }

        private bool ParseDefaultBlock()
        {
            tokenizer.SkipWhitespace();
            ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.OpenCurly);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected {, got " + ((Object)token ?? "<null>").ToString());
                return false;
            }

            ZScriptTokenType[] whitespacetypes = new ZScriptTokenType[] { ZScriptTokenType.Newline, ZScriptTokenType.Whitespace, ZScriptTokenType.BlockComment, ZScriptTokenType.LineComment };

            // todo parse defaults block
            while (true)
            {
                long cpos = stream.Position;
                token = tokenizer.ExpectToken(ZScriptTokenType.Whitespace, ZScriptTokenType.BlockComment, ZScriptTokenType.Newline, ZScriptTokenType.LineComment, ZScriptTokenType.OpAdd, ZScriptTokenType.OpSubtract, ZScriptTokenType.Identifier, ZScriptTokenType.CloseCurly, ZScriptTokenType.Semicolon);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected comment, flag, property, or }, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }

                //if (ClassName == "Enforcer")
                //    parser.LogWarning(token.ToString());

                if (token.Type == ZScriptTokenType.CloseCurly)
                    break;

                switch (token.Type)
                {
                    case ZScriptTokenType.Whitespace:
                    case ZScriptTokenType.BlockComment:
                    case ZScriptTokenType.Newline:
                        break;

                    case ZScriptTokenType.LineComment:
                        ParseGZDBComment(props, token.Value);
                        break;

                    // flag definition (+/-)
                    case ZScriptTokenType.OpAdd:
                    case ZScriptTokenType.OpSubtract:
                        {
                            bool flagset = (token.Type == ZScriptTokenType.OpAdd);
                            string flagname = parser.ParseDottedIdentifier();
                            if (flagname == null) return false;

                            //parser.LogWarning(string.Format("{0}{1}", (flagset ? '+' : '-'), flagname));
                            // set flag
                            flags[flagname] = flagset;
                            break;
                        }

                    // property or combo definition
                    case ZScriptTokenType.Identifier:
                        {
                            stream.Position = cpos;
                            string propertyname = parser.ParseDottedIdentifier();
                            if (propertyname == null) return false;
                            List<string> propertyvalues = new List<string>();

                            // read in property values, until semicolon reached
                            while (true)
                            {
                                tokenizer.SkipWhitespace();
                                List<ZScriptToken> expr = parser.ParseExpression();
                                string exprstring = ZScriptTokenizer.TokensToString(expr);

                                token = tokenizer.ExpectToken(ZScriptTokenType.Comma, ZScriptTokenType.Semicolon);
                                if (token == null || !token.IsValid)
                                {
                                    parser.ReportError("Expected comma or ;, got " + ((Object)token ?? "<null>").ToString());
                                    return false;
                                }

                                propertyvalues.Add(exprstring);
                                if (token.Type == ZScriptTokenType.Semicolon)
                                    break;
                            }

                            //parser.LogWarning(string.Format("{0} = [{1}]", propertyname, string.Join(", ", propertyvalues.ToArray())));
                            // set property
                            // translate "scale" to x and y scale
                            if (propertyname == "scale")
                            {
                                props["xscale"] = props["yscale"] = propertyvalues;
                            }
                            else
                            {
                                props[propertyname] = propertyvalues;
                            }
                            break;
                        }
                }

            }

            return true;
        }

        private bool ParseStatesBlock()
        {
            tokenizer.SkipWhitespace();
            ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.OpenParen, ZScriptTokenType.OpenCurly);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected ( or {, got " + ((Object)token ?? "<null>").ToString());
                return false;
            }

            // we can have some weirdass class name list after States keyword. handle that here.
            if (token.Type == ZScriptTokenType.OpenParen)
            {
                parser.ParseExpression(true);
                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.CloseParen);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected ), got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }

                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.OpenCurly);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected {, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }
            }

            // todo parse states block
            stream.Position--;
            token = tokenizer.ExpectToken(ZScriptTokenType.OpenCurly);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected {, got " + ((Object)token ?? "<null>").ToString());
                return false;
            }

            string statelabel = "";
            while (true)
            {
                // parse a state block.
                // this is a seriously broken approach, but let it be for now.
                StateStructure st = new ZScriptStateStructure(this, parser);
                parser.tokenizer = tokenizer;
                if (parser.HasError) return false;
                states[statelabel] = st;

                tokenizer.SkipWhitespace();
                long cpos = stream.Position;
                token = tokenizer.ExpectToken(ZScriptTokenType.Identifier, ZScriptTokenType.CloseCurly);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected state label or }, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }

                if (token.Type == ZScriptTokenType.CloseCurly)
                    break;

                stream.Position = cpos;
                statelabel = parser.ParseDottedIdentifier();
                if (statelabel == null)
                    return false;

                // otherwise expect a colon
                token = tokenizer.ExpectToken(ZScriptTokenType.Colon);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected :, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }
            }

            return true;
        }

        private string ParseTypeName()
        {
            ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.Identifier);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected type name, got " + ((Object)token ?? "<null>").ToString());
                return null;
            }

            string outs = token.Value;

            long cpos = stream.Position;
            tokenizer.SkipWhitespace();
            token = tokenizer.ReadToken();
            if (token != null && token.Type == ZScriptTokenType.OpLessThan) // <
            {
                string internal_type = ParseTypeName();
                if (internal_type == null)
                    return null;
                token = tokenizer.ExpectToken(ZScriptTokenType.OpGreaterThan);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected >, got " + ((Object)token ?? "<null>").ToString());
                    return null;
                }
                return outs + "<" + internal_type + ">";
            }
            else
            {
                stream.Position = cpos;
                return outs;
            }
        }

        private List<int> ParseArrayDimensions()
        {
            List<int> dimensions = new List<int>();
            while (true)
            {
                tokenizer.SkipWhitespace();
                ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.OpenSquare);
                if (token == null || !token.IsValid) // array dimensions ended.
                    return dimensions;

                // parse identifier or int (identifier is a constant, we don't parse this yet)
                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.Integer, ZScriptTokenType.Identifier);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected integer or const, got " + ((Object)token ?? "<null>").ToString());
                    return null;
                }

                int arraylen = -1;
                if (token.Type == ZScriptTokenType.Integer)
                    arraylen = token.ValueInt;
                dimensions.Add(arraylen);

                // closing square
                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.CloseSquare);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected ], got " + ((Object)token ?? "<null>").ToString());
                    return null;
                }
            }
        }

        private bool ParseProperty()
        {
            // property identifier: identifier, identifier, identifier, ...;
            tokenizer.SkipWhitespace();
            ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.Identifier);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected property name, got " + ((Object)token ?? "<null>").ToString());
                return false;
            }

            tokenizer.SkipWhitespace();
            token = tokenizer.ExpectToken(ZScriptTokenType.Colon);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected :, got " + ((Object)token ?? "<null>").ToString());
                return false;
            }

            while (true)
            {
                // expect identifier, then either a comma or a semicolon.
                // semicolon means end of definition, comma means we parse next identifier.
                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.Identifier);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected variable, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }

                tokenizer.SkipWhitespace();
                token = tokenizer.ExpectToken(ZScriptTokenType.Semicolon, ZScriptTokenType.Comma);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected comma or ;, got " + ((Object)token ?? "<null>").ToString());
                    return false;
                }

                if (token.Type == ZScriptTokenType.Semicolon)
                    break;
            }

            return true;
        }

        private string ParseVersion(bool required)
        {
            // read in the version.
            tokenizer.SkipWhitespace();
            ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.OpenParen);
            if (token == null || !token.IsValid)
            {
                if (required)
                    parser.ReportError("Expected (, got " + ((Object)token ?? "<null>").ToString());
                return null;
            }

            tokenizer.SkipWhitespace();
            token = tokenizer.ExpectToken(ZScriptTokenType.String);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected version, got " + ((Object)token ?? "<null>").ToString());
                return null;
            }

            string version = token.Value.Trim();
            tokenizer.SkipWhitespace();
            token = tokenizer.ExpectToken(ZScriptTokenType.CloseParen);
            if (token == null || !token.IsValid)
            {
                parser.ReportError("Expected ), got " + ((Object)token ?? "<null>").ToString());
                return null;
            }

            return version;
        }

        internal ZScriptActorStructure(ZDTextParser zdparser, DecorateCategoryInfo catinfo, string _classname, string _replacesname, string _parentname)
        {
            this.catinfo = catinfo; //mxd

            parser = (ZScriptParser)zdparser;
            stream = parser.DataStream;
            tokenizer = new ZScriptTokenizer(parser.DataReader);
            parser.tokenizer = tokenizer;

            classname = _classname;
            replaceclass = _replacesname;
            //baseclass = parser.GetArchivedActorByName(_parentname); // this is not guaranteed to work here

            ZScriptToken cls_open = tokenizer.ExpectToken(ZScriptTokenType.OpenCurly);
            if (cls_open == null || !cls_open.IsValid)
            {
                parser.ReportError("Expected {, got " + ((Object)cls_open ?? "<null>").ToString());
                return;
            }

            // in the class definition, we can have the following:
            // - Defaults block
            // - States block
            // - method signature: [native] [action] <type [, type [...]]> <name> (<arguments>);
            // - method: <method signature (except native)> <block>
            // - field declaration: [native] <type> <name>;
            // - enum definition: enum <name> <block>;
            // we are skipping everything, except Defaults and States.
            while (true)
            {
                tokenizer.SkipWhitespace();
                long ocpos = stream.Position;
                ZScriptToken token = tokenizer.ExpectToken(ZScriptTokenType.Identifier, ZScriptTokenType.CloseCurly);
                if (token == null || !token.IsValid)
                {
                    parser.ReportError("Expected identifier, got " + ((Object)cls_open ?? "<null>").ToString());
                    return;
                }
                if (token.Type == ZScriptTokenType.CloseCurly) // end of class
                    break;

                string b_lower = token.Value.ToLowerInvariant();
                switch (b_lower)
                {
                    case "default":
                        if (!ParseDefaultBlock())
                            return;
                        continue;

                    case "states":
                        if (!ParseStatesBlock())
                            return;
                        continue;

                    case "enum":
                        if (!parser.ParseEnum())
                            return;
                        continue;

                    case "const":
                        if (!parser.ParseConst())
                            return;
                        continue;

                    // apparently we can have a struct inside a class, but not another class.
                    case "struct":
                        if (!parser.ParseClassOrStruct(true, false, null))
                            return;
                        continue;

                    // new properties syntax
                    case "property":
                        if (!ParseProperty())
                            return;
                        continue;

                    default:
                        stream.Position = ocpos;
                        break;
                }

                // try to read in a variable/method.
                bool bmethod = false;
                string[] availablemodifiers = new string[] { "static", "native", "action", "readonly", "protected", "private", "virtual", "override", "meta", "transient", "deprecated", "final", "play", "ui", "clearscope", "virtualscope", "version" };
                string[] versionedmodifiers = new string[] { "version", "deprecated" };
                string[] methodmodifiers = new string[] { "action", "virtual", "override", "final" };
                HashSet<string> modifiers = new HashSet<string>();
                List<string> types = new List<string>();
                List<List<int>> typearraylens = new List<List<int>>();
                List<string> names = new List<string>();
                List<List<int>> arraylens = new List<List<int>>();
                List<ZScriptToken> args = null; // this is for the future
                //List<ZScriptToken> body = null;

                while (true)
                {
                    tokenizer.SkipWhitespace();
                    long cpos = stream.Position;
                    token = tokenizer.ExpectToken(ZScriptTokenType.Identifier);
                    if (token == null || !token.IsValid)
                    {
                        parser.ReportError("Expected modifier or name, got " + ((Object)cls_open ?? "<null>").ToString());
                        return;
                    }

                    b_lower = token.Value.ToLowerInvariant();
                    if (availablemodifiers.Contains(b_lower))
                    {
                        if (modifiers.Contains(b_lower))
                        {
                            parser.ReportError("Field/method modifier '" + b_lower + "' was specified twice");
                            return;
                        }

                        if (methodmodifiers.Contains(b_lower))
                            bmethod = true;

                        if (versionedmodifiers.Contains(b_lower))
                        {
                            string version = ParseVersion(b_lower == "version"); // deprecated doesn't require version string for historical reasons. (compatibility with old gzdoom.pk3)
                            if (version == null && b_lower == "version")
                                return;
                        }

                        modifiers.Add(b_lower);
                    }
                    else
                    {
                        stream.Position = cpos;
                        break;
                    }
                }

                // read in the type name(s)
                // type name can be:
                //  - identifier
                //  - identifier<identifier>
                while (true)
                {
                    tokenizer.SkipWhitespace();
                    string typename = ParseTypeName();
                    if (typename == null)
                        return;

                    types.Add(typename.ToLowerInvariant());
                    typearraylens.Add(null);
                    long cpos = stream.Position;
                    tokenizer.SkipWhitespace();
                    token = tokenizer.ExpectToken(ZScriptTokenType.Comma, ZScriptTokenType.Identifier, ZScriptTokenType.OpenSquare);

                    if (token != null && !token.IsValid)
                    {
                        parser.ReportError("Expected comma, identifier or array dimensions, got " + ((Object)token ?? "<null>").ToString());
                        return;
                    }

                    if (token == null || token.Type != ZScriptTokenType.Comma)
                    {
                        stream.Position = cpos;
                        if (token.Type == ZScriptTokenType.OpenSquare)
                        {
                            List<int> typelens = ParseArrayDimensions();
                            if (typelens == null) // error
                                return;
                            typearraylens[typearraylens.Count - 1] = typelens;
                        }
                        break;
                    }
                }

                while (true)
                {
                    string name = null;
                    List<int> lens = null;

                    // read in the method/field name
                    tokenizer.SkipWhitespace();
                    token = tokenizer.ExpectToken(ZScriptTokenType.Identifier);
                    if (token == null || !token.IsValid)
                    {
                        parser.ReportError("Expected field/method name, got " + ((Object)token ?? "<null>").ToString());
                        return;
                    }
                    name = token.Value.ToLowerInvariant();

                    // check the token. if it's a (, then it's a method. if it's a ;, then it's a field, if it's a [ it's an array field.
                    // if it's a field and bmethod=true, report error.
                    tokenizer.SkipWhitespace();
                    long cpos = stream.Position;
                    token = tokenizer.ExpectToken(ZScriptTokenType.Comma, ZScriptTokenType.OpenParen, ZScriptTokenType.OpenSquare, ZScriptTokenType.Semicolon);
                    if (token == null || !token.IsValid)
                    {
                        parser.ReportError("Expected comma, ;, [, or argument list, got " + ((Object)token ?? "<null>").ToString());
                        return;
                    }

                    if (token.Type == ZScriptTokenType.OpenParen)
                    {
                        // if we have multiple names
                        if (names.Count > 0)
                        {
                            parser.ReportError("Cannot have multiple names in a method");
                            return;
                        }

                        bmethod = true;
                        // now, I could parse this properly, but it won't be used anyway, so I'll do it as a fake expression.
                        args = parser.ParseExpression(true);
                        token = tokenizer.ExpectToken(ZScriptTokenType.CloseParen);
                        if (token == null || !token.IsValid)
                        {
                            parser.ReportError("Expected ), got " + ((Object)token ?? "<null>").ToString());
                            return;
                        }

                        // also get the body block, if any.
                        tokenizer.SkipWhitespace();
                        cpos = stream.Position;
                        token = tokenizer.ExpectToken(ZScriptTokenType.Semicolon, ZScriptTokenType.OpenCurly, ZScriptTokenType.Identifier);
                        if (token == null || !token.IsValid)
                        {
                            parser.ReportError("Expected 'const', ; or {, got " + ((Object)token ?? "<null>").ToString());
                            return;
                        }

                        //
                        if (token.Type == ZScriptTokenType.Identifier)
                        {
                            if (token.Value.ToLowerInvariant() != "const")
                            {
                                parser.ReportError("Expected 'const', got " + ((Object)token ?? "<null>").ToString());
                                return;
                            }

                            tokenizer.SkipWhitespace();
                            cpos = stream.Position;
                            token = tokenizer.ExpectToken(ZScriptTokenType.Semicolon, ZScriptTokenType.OpenCurly);
                            if (token == null || !token.IsValid)
                            {
                                parser.ReportError("Expected ; or {, got " + ((Object)token ?? "<null>").ToString());
                                return;
                            }
                        }

                        if (token.Type == ZScriptTokenType.OpenCurly)
                        {
                            stream.Position = cpos;
                            parser.SkipBlock();
                            //body = parser.ParseBlock(false);
                        }

                        break; // end method parsing
                    }
                    else
                    {
                        if (bmethod)
                        {
                            parser.ReportError("Cannot have virtual, override or action fields");
                            return;
                        }

                        // array
                        if (token.Type == ZScriptTokenType.OpenSquare)
                        {
                            stream.Position = cpos;
                            lens = ParseArrayDimensions();
                            if (lens == null) // error
                                return;


                            tokenizer.SkipWhitespace();
                            token = tokenizer.ExpectToken(ZScriptTokenType.Semicolon, ZScriptTokenType.Comma);
                            if (token == null || !token.IsValid)
                            {
                                parser.ReportError("Expected ; or comma, got " + ((Object)token ?? "<null>").ToString());
                                return;
                            }
                        }
                    }

                    names.Add(name);
                    arraylens.Add(lens);

                    if (token.Type != ZScriptTokenType.Comma) // next name
                        break;
                }

                // validate modifiers here.
                // protected and private cannot be combined.
                if (bmethod)
                {
                    if (modifiers.Contains("protected") && modifiers.Contains("private"))
                    {
                        parser.ReportError("Cannot have protected and private on the same method");
                        return;
                    }
                    // virtual and override cannot be combined.
                    int cvirtual = modifiers.Contains("virtual") ? 1 : 0;
                    cvirtual += modifiers.Contains("override") ? 1 : 0;
                    cvirtual += modifiers.Contains("final") ? 1 : 0;
                    if (cvirtual > 1)
                    {
                        parser.ReportError("Cannot have virtual, override and final on the same method");
                        return;
                    }
                    // meta (what the fuck is that?) probably cant be on a method
                    if (modifiers.Contains("meta"))
                    {
                        parser.ReportError("Cannot have meta on a method");
                        return;
                    }
                }

                // finished method or field parsing.
                /*for (int i = 0; i < names.Count; i++)
                {
                    string name = names[i];
                    int arraylen = arraylens[i];

                    string _args = "";
                    if (args != null) _args = " (" + ZScriptTokenizer.TokensToString(args) + ")";
                    else if (arraylen != -1) _args = " [" + arraylen.ToString() + "]";
                    parser.LogWarning(string.Format("{0} {1} {2}{3}", string.Join(" ", modifiers.ToArray()), string.Join(", ", types.ToArray()), name, _args));
                }*/
                
                // update 08.02.17: add user variables from ZScript actors.
                if (args == null && types.Count == 1) // it's a field
                {
                    // we support:
                    //  - float
                    //  - int
                    //  - double
                    //  - bool
                    string type = types[0];
                    UniversalType utype;
                    switch (type)
                    {
                        case "int":
                            utype = UniversalType.Integer;
                            break;
                        /*case "float":
                        case "double":
                            utype = UniversalType.Float;
                            break;
                        case "bool":
                            utype = UniversalType.Integer;
                            break;
                        case "string":
                            utype = UniversalType.String;
                            break;
                            // todo test if class names and colors will work*/
                            // [ZZ] currently only integer variable works.
                        default:
                            continue; // go read next field
                    }

                    for (int i = 0; i < names.Count; i++)
                    {
                        string name = names[i];
                        if (arraylens[i] != null || typearraylens[0] != null)
                            continue; // we don't process arrays
                        if (!name.StartsWith("user_"))
                            continue; // we don't process non-user_ fields (because ZScript won't pick them up anyway)
                        // parent class is not guaranteed to be loaded already, so handle collisions later
                        uservars.Add(name, utype);
                    }
                }
            }

            // parsing done, process thing arguments
            ParseCustomArguments();
        }
    }
}
