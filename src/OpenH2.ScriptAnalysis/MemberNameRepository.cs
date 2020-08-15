using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OpenH2.ScriptAnalysis
{
    public class MemberNameRepository
    {
        private Dictionary<string, List<RegisteredName>> originalNameLookup = new Dictionary<string, List<RegisteredName>>();
        private HashSet<string> finalNames = new HashSet<string>();

        public Dictionary<string, MemberNameRepository> NestedRepos = new Dictionary<string, MemberNameRepository>();

        public string RegisterName(string desiredName, string type, int? index = null)
        {
            var key = SimplifiedKey(desiredName);

            if(string.IsNullOrWhiteSpace(desiredName))
            {
                desiredName = "Unnamed";
            }

            var sanitized = SyntaxUtil.SanitizeMemberAccess(desiredName);

            var name = new RegisteredName()
            {
                OriginalName = desiredName,
                TypeInfo = type,
                Index = index,
            };

            var attempt = 0;
            var attemptName = sanitized;

            if (originalNameLookup.TryGetValue(key, out var nameSlot))
            {
                while (nameSlot.Any(n => n.UniqueName == attemptName) || finalNames.Contains(attemptName))
                {
                    attempt++;
                    attemptName = sanitized + attempt;
                }

                name.UniqueName = attemptName;
                nameSlot.Add(name);
            }
            else
            {
                while (finalNames.Contains(attemptName))
                {
                    attempt++;
                    attemptName = sanitized + attempt;
                }

                name.UniqueName = attemptName;
                originalNameLookup.Add(key, new List<RegisteredName>() { name });
            }

            finalNames.Add(name.UniqueName);
            return name.UniqueName;
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

                    var indexMatch = typeNames.FirstOrDefault(n => n.Index == index);

                    if(indexMatch != null)
                    {
                        result = indexMatch.UniqueName;
                        return true;
                    }
                    else
                    {
                        result = null;
                        return false;
                    }
                }
            }
        }

        private string SimplifiedKey(string name)
        {
            return name.Replace('/', '.').ToUpperInvariant();
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
