using System.Collections.Generic;

namespace EntityCache.Repository
{
    internal interface IRepositoryProvider
    {
        bool Add(Dictionary<string, string> entity);
        bool Update(Dictionary<string, string> entity);
        bool Remove(int id);
        Dictionary<string, string> Get(int id);

        List<Dictionary<string, string>> GetAllEntries();
    }
}
