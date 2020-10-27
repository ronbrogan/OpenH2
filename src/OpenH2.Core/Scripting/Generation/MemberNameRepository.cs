using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Scripting.Generation
{
    public class MemberNameRepository
    {
        private Dictionary<string, List<RegisteredName>> originalNameLookup = new Dictionary<string, List<RegisteredName>>();
        private HashSet<string> finalNames = new HashSet<string>();

        public Dictionary<string, MemberNameRepository> NestedRepos = new Dictionary<string, MemberNameRepository>();

        public string RegisterName(string desiredName, string dataType, int? index = null)
        {
            if(string.IsNullOrWhiteSpace(desiredName))
            {
                desiredName = "Unnamed";
            }

            var sanitized = SyntaxUtil.SanitizeMemberAccess(desiredName);

            var name = new RegisteredName()
            {
                OriginalName = desiredName,
                TypeInfo = dataType,
                Index = index,
            };

            var attempt = 0;
            var attemptName = sanitized;

            var key = SimplifiedKey(desiredName, dataType, index);
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

        public bool TryGetName(string desiredName, string dataType, int? index, out string result)
        {
            if (string.IsNullOrWhiteSpace(desiredName))
            {
                desiredName = "Unnamed";
            }

            var universalKey = SimplifiedKey(desiredName, dataType, index);

            if (originalNameLookup.TryGetValue(universalKey, out var nameSlot) && nameSlot.Count == 1)
            {
                result = nameSlot[0].UniqueName;
                return true;
            }

            result = null;
            return false;
        }

        private string SimplifiedKey(string name, string type, int? index = null)
        {
            return $"{name}<{type}>@{index}".Replace('/', '.').ToUpperInvariant();
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
