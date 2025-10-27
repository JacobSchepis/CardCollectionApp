using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCollectionApp.Filters
{
    public class PageOffsetFilter : ICardFilter
    {
        public string Identifier => "page";
        public string HelpDescription => "Filters by page offset for pagination (e.g., page:2)";
        public bool ApplySql(SqlWhereBuilder builder, string value)
        {
            if (!int.TryParse(value, out int pageNumber) || pageNumber < 1)
                return false;
            int offset = (pageNumber - 1) * 30; // Assuming 20 items per page
            builder.Add($"LIMIT 30 OFFSET {offset}");
            return true;
        }
    }
}
