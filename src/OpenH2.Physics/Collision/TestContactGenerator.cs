using OpenH2.Foundation.Physics;
using OpenH2.Physics.Abstractions;
using System.Collections.Generic;

namespace OpenH2.Physics.Collision
{
    public class TestContactGenerator : IContactGenerator
    {
        public Contact[] DetectCollisions(IList<IBody> candidates)
        {
            var contacts = new List<Contact>(candidates.Count);

            for (var i = 0; i < candidates.Count / 2; i++)
            {
                var a = candidates[i * 2];
                var b = candidates[i * 2 + 1];

                AddContact(a, b, contacts);
            }

            return contacts.ToArray();
        }

        private void AddContact(IBody a, IBody b, List<Contact> contacts)
        {
            var colA = a.Collider;
            var colB = b.Collider;

            if (colA.Intersects(colB) == false)
            {
                return;
            }

            var generatedContacts = colA.GenerateContacts(colB);

            for(var i = 0; i < generatedContacts.Count; i++)
            {
                var contact = generatedContacts[i];
                contact.A = a;
                contact.B = b;

                contacts.Add(contact);
            }
        }
    }
}
