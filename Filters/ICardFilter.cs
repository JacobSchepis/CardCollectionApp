public interface ICardFilter
{
    string Identifier { get; }                              
    string HelpDescription { get; }                         
    bool ApplySql(SqlWhereBuilder builder, string values);
}
