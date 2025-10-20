using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SetFilter : ICardFilter
{
    public string Identifier => "set";
    public string HelpDescription => "Filters cards by set code (e.g., set:eoe)";

    public bool ApplySql(SqlWhereBuilder builder, string value)
    {
        builder.Add($"set_code = \"{value}\"");
        return true;
    }
}
