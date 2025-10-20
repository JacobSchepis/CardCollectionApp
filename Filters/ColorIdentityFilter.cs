using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class ColorIdentityFilter : ICardFilter
{
    public string Identifier => "ci";
    public string HelpDescription => "Filters by color identity. Supports && (AND) or || (OR), e.g., ci:{R}&&{G}";

    public bool ApplySql(SqlWhereBuilder builder, string value)
    {
        var colors = Regex.Split(value, @"\s*(\&\&|\|\|)\s*");
        if (colors.Length < 1) return false;

        string logic = colors.Contains("&&") ? "AND" : "OR";
        var conditions = new List<string>();
        int i = 0;

        foreach (var token in colors)
        {
            if (token == "&&" || token == "||") continue;
            var paramName = $"@ci{i++}";
            conditions.Add($"color_identity LIKE '%' || {paramName} || '%'");
            //builder.Parameters.Add((paramName, token));
        }

        builder.Add($"({string.Join($" {logic} ", conditions)})");
        return true;
    }
}
