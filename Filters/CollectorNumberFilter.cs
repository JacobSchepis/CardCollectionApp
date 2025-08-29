public class CollectorNumberFilter : ICardFilter
{
    public string Identifier => "cn";
    public string HelpDescription => "Filter by collector number. Usage: cn:<value>";
    public bool ApplySql(SqlWhereBuilder builder, string value)
    {
        if (!int.TryParse(value, out int parsedValue))
            return false;
        else
            builder.Add("collector_number = " + parsedValue);

        return true;
    }
}