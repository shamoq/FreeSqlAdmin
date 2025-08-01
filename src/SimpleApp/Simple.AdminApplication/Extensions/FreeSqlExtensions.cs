using FreeSql;
using FreeSql.Internal.Model;

namespace Simple.AdminApplication.Extensions;

public static class FreeSqlExtensions
{
    public static Task<T> FindAsync<T>(this IBaseRepository<T> table, Guid id) where T : DefaultEntity
    {
        return table.Where(t => t.Id == id).FirstAsync();
    }
 
}