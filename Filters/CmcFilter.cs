using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class CmcFilter : ICardFilter
{
    public string Identifier => "cmc";
    public string HelpDescription => "Filters by converted mana cost (e.g., cmc:<=4)";

    public bool ApplySql(SqlWhereBuilder builder, string value)
    {
        var match = Regex.Match(value, @"(<=|>=|<|>|=)(\d+)");
        if (!match.Success) return false;

        string op = match.Groups[1].Value;
        if (!int.TryParse(match.Groups[2].Value, out int cmc)) return false;

        builder.Add($"cmc {op} {cmc}");
        return true;
    }
}