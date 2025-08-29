using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CardRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string SetCode { get; set; } = "";
    public int CollectorNumber { get; set; }
}

public class CardRepository : ICollectionRepository
{
    private readonly string _connectionString;
    public CardRepository(string connection)
    {
        _connectionString = connection;
    }

    public void EnsureSchema()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS cards (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                set_code TEXT NOT NULL,
                collector_number INTEGER NOT NULL
            );";
        cmd.ExecuteNonQuery();
    }

    public Task AddAsync(ScryfallCard card)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO cards (name, set_code, collector_number)
            VALUES ($name, $setCode, $collectorNumber);";
        cmd.Parameters.AddWithValue("$name", card.Name);
        cmd.Parameters.AddWithValue("$setCode", card.SetName);
        cmd.Parameters.AddWithValue("$collectorNumber", card.CollectorNumber);
        cmd.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public IEnumerable<CardRecord> GetAllCards()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT id, name, set_code, collector_number FROM cards";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            yield return new CardRecord
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                SetCode = reader.GetString(2),
                CollectorNumber = reader.GetInt32(3)
            };
        }
    }

    public Task<(IReadOnlyList<CardRecord> rows, int total)> QueryAsync(QueryOptions opts, int pageIndex)
    {
        throw new NotImplementedException();
    }
}

public sealed class SqlWhereBuilder
{
    private readonly List<string> _clauses = new();
    private readonly List<SqliteParameter> _parms = new();
    private int _i;

    public string AddParam(object v)
    {
        var name = $"$p{_i++}";
        _parms.Add(new SqliteParameter(name, v));
        return name;
    }
    public void And(string expr) { _clauses.Add(expr); }

    public (string whereSql, List<SqliteParameter> parms) Build()
        => (_clauses.Count == 0 ? "" : "WHERE " + string.Join(" AND ", _clauses), _parms);
}