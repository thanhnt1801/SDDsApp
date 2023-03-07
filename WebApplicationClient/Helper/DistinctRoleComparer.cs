using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebApplicationClient.Models;

namespace WebApplicationClient.Models
{
    public class DistinctRoleComparer : IEqualityComparer<Role>
    {
        public bool Equals(Role x, Role y)
        {
            return x.Id == y.Id && x.Name == y.Name;
        }

        public int GetHashCode([DisallowNull] Role obj)
        {
            return obj.Id.GetHashCode() ^ obj.Name.GetHashCode();
        }
    }
}
