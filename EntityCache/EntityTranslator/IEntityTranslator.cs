using System.Collections.Generic;
using EntityCache.Entity;

namespace EntityCache.EntityTranslator
{
    internal interface IEntityTranslator<T> where T : class, IEntity
    {
        T ParseEntity(Dictionary<string, string> entityProps);
        Dictionary<string, string> MapEntity(T person);
    }
}