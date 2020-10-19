using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;

namespace OpenH2.Core.Parsing
{
    public static class PropertyAccessorParser
    {
        public static List<PropertyAccessData> ExtractProperties(ReadOnlySpan<char> input)
        {
            var props = new List<PropertyAccessData>();

            var tokenStart = 0;
            var elementArgStart = 0;

            var curr = 0;

            var token = new PropertyAccessData();

            while (curr < input.Length)
            {
                if (input[curr] == '[')
                {
                    elementArgStart = curr;
                    token.AccessType = PropertyAccessType.ElementAccess;
                }

                if (input[curr] == ']')
                {
                    // +1 and -1 to eat brackets
                    var elementArg = input.Slice(elementArgStart + 1, curr - elementArgStart - 1);

                    if (elementArg.Length != 0)
                    {
                        if (int.TryParse(elementArg, out var intArg))
                        {
                            token.ElementArgument = intArg;
                        }
                        else
                        {
                            token.ElementArgument = new string(elementArg);
                        }
                    }
                }

                if (input[curr] == '.' || curr == input.Length-1)
                {
                    var tokenLength = curr - tokenStart;

                    // If we're at the last char, artificially extend token length to ensure last char is included
                    if (curr == input.Length - 1)
                    {
                        tokenLength++;
                    }

                    if (token.AccessType == PropertyAccessType.ElementAccess)
                    {
                        tokenLength = elementArgStart - tokenStart;
                    }

                    token.PropertyName = new string(input.Slice(tokenStart, tokenLength));

                    if (SyntaxFacts.IsValidIdentifier(token.PropertyName) == false)
                    {
                        throw new NotSupportedException($"Invalid property name '{token.PropertyName}'");
                    }

                    props.Add(token);
                    token = new PropertyAccessData();

                    // Eat the '.'
                    curr++;

                    // Start next token here
                    tokenStart = curr;
                }

                curr++;
            }

            return props;
        }

        public class PropertyAccessData
        {
            public string PropertyName { get; set; }
            public PropertyAccessType AccessType { get; set; }
            public object ElementArgument { get; set; }
        }

        public enum PropertyAccessType
        {
            Normal = 0,
            ElementAccess = 1
        }
    }
}
