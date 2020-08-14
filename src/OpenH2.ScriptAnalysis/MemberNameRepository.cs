using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OpenH2.ScriptAnalysis
{
    public class MemberNameRepository
    {

        private Dictionary<string, string> mappings = new Dictionary<string, string>();

        private Dictionary<string, HashSet<string>> scopedNames = new Dictionary<string, HashSet<string>>();


        private List<RegisteredName> registeredNames = new List<RegisteredName>();
        private Dictionary<string, List<RegisteredName>> originalNameLookup = new Dictionary<string, List<RegisteredName>>();

        public string RegisterName(string desiredName, string type, int? index = null)
        {
            var key = SimplifiedKey(desiredName);
            var sanitized = SyntaxUtil.SanitizeMemberAccess(desiredName);

            var name = new RegisteredName()
            {
                OriginalName = desiredName,
                TypeInfo = type,
                Index = index,
            };

            if (originalNameLookup.TryGetValue(key, out var nameSlot))
            {
                // iteratively unique the name
                var attempt = 0;
                var attemptName = sanitized;
                while (nameSlot.Any(n => n.UniqueName == attemptName))
                {
                    attempt++;
                    attemptName = sanitized + attempt;
                }

                name.UniqueName = attemptName;
                nameSlot.Add(name);
                return name.UniqueName;
            }
            else
            {
                name.UniqueName = sanitized;
                originalNameLookup.Add(key, new List<RegisteredName>() { name });
                return name.UniqueName;
            }
        }

        public bool TryGetName(string desiredName, string type, int? index, out string result)
        {
            var universalKey = SimplifiedKey(desiredName);

            if (originalNameLookup.TryGetValue(universalKey, out var nameSlot) == false)
            {
                result = null;
                return false;
            }

            if(nameSlot.Count == 1)
            {
                result = nameSlot[0].UniqueName;
                return true;
            }
            else
            {
                // iteratively find correct name via type and index 
                var typeNames = nameSlot.Where(n => n.TypeInfo == type);

                if(typeNames.Count() == 0)
                {
                    result = string.Empty;
                    return false;
                }
                else if(typeNames.Count() == 1)
                {
                    result = typeNames.First().UniqueName;
                    return true;
                }
                else
                {
                    throw new Exception("DOes this happen?");
                    result = null;
                    return false;
                }
            }
        }

        private string SimplifiedKey(string name)
        {
            return name.Replace('/', '.').ToUpperInvariant();
        }

        private string KeyFromDesired(string scope, string desired, string typeInfo, int index = 0)
        {
            return (scope + "@" + desired + "<" + typeInfo + ">#" + index).ToUpperInvariant();
        }

        private class RegisteredName
        {
            public string OriginalName { get; set; }
            public string UniqueName { get; set; }
            public string TypeInfo { get; set; }
            public int? Index { get; set; }
        }
    }
}
