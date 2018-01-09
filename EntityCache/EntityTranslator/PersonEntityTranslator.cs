using System.Collections.Generic;
using EntityCache.Entity;

namespace EntityCache.EntityTranslator
{
    internal class PersonEntityTranslator : IEntityTranslator<Person>
    {
        public Person ParseEntity(Dictionary<string, string> entityProps)
        {
            int.TryParse(entityProps["Id"], out var id);
            int.TryParse(entityProps["Age"], out var age);
            string name = entityProps["Name"];
            return new Person(id, name, age);
        }
        
        public Dictionary<string, string> MapEntity(Person person)
        {
            if (person == null)
            {
                return null;
            }
            
            return new Dictionary<string, string>
            {
                {"Id", person.Id.ToString()},
                {"Name", person.Name },
                {"Age", person.Age.ToString() }
            };
        }
    }
}
