namespace EntityCache.Entity
{
    internal class Person : IEntity
    {
        public string Name { get; }
        public int Id { get; }
        public int Age { get; }

        public Person(int id, string name, int age)
        {
            Id = id;
            Name = name;
            Age = age;
        }

        public override bool Equals(object obj)
        {
            if (obj is Person p)
            {
                return Equals(p);
            }

            return false;
        }
        
        protected bool Equals(Person other)
        {
            return string.Equals(Name, other.Name) && Id == other.Id && Age == other.Age;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ Age;
                return hashCode;
            }
        }
    }
}
