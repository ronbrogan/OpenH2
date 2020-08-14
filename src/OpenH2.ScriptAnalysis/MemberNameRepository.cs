using System;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis
{
    public class MemberNameRepository
    {

        private Dictionary<string, string> mappings = new Dictionary<string, string>();

        private Dictionary<string, HashSet<string>> scopedNames = new Dictionary<string, HashSet<string>>();

        public string CreateName(string scope, string desiredName, string type, int index)
        {
            var key = KeyFromDesired(scope, desiredName, type, index);

            while (mappings.ContainsKey(key))
            {
                throw new Exception("Name was already added");
            }

            string name = SyntaxUtil.SanitizeIdentifier(desiredName);

            var attempt = 0;

            while (true)
            {
                if (scopedNames.TryGetValue(scope, out var scopeNames))
                {
                    var attemptName = name;

                    if(attempt > 0)
                    {
                        attemptName += attempt;
                    }

                    if(scopeNames.Contains(attemptName))
                    {
                        attempt++;
                        continue;
                    }

                    scopeNames.Add(attemptName);
                    mappings.Add(key, attemptName);
                    return attemptName;
                }
                else
                {
                    scopedNames.Add(scope, new HashSet<string>()
                    {
                        { name }
                    });

                    mappings.Add(key, name);
                    return name;
                }
            }
        }

        public bool TryGetName(string scope, string desiredName, string type, int index, out string result)
        {
            var key = KeyFromDesired(scope, desiredName, type, index);
            if(mappings.TryGetValue(key, out result))
            {
                return true;
            }

            var fallbackKey = KeyFromDesired(scope, desiredName, type);
            return mappings.TryGetValue(fallbackKey, out result);
        }

        private string KeyFromDesired(string scope, string desired, string typeInfo, int index = 0)
        {
            return (scope + "@" + desired + "<" + typeInfo + ">#" + index).ToUpperInvariant();
        }
    }
}
