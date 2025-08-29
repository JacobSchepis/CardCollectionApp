using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CardRecord
{
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
        connection.Close();
    }

    public Task AddAsync(CardRecord card)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO cards (name, set_code, collector_number)
            VALUES ($name, $setCode, $collectorNumber);";
        cmd.Parameters.AddWithValue("$name", card.Name);
        cmd.Parameters.AddWithValue("$setCode", card.SetCode);
        cmd.Parameters.AddWithValue("$collectorNumber", card.CollectorNumber);
        cmd.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public IEnumerable<CardRecord> GetScryfallCards(string query, int pageSize = 175)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string sql = $"SELECT * FROM cards {query} LIMIT {pageSize}";

        Console.Write(sql);

        using var command = new SqliteCommand(query, connection);
        using var reader = command.ExecuteReader();

        List<CardRecord> cards = new List<CardRecord>();

        while (reader.Read())
        {
            var card = new CardRecord
            {
                Name = reader["name"].ToString(),
                SetCode = reader["set_code"].ToString(),
                CollectorNumber = reader.GetInt32(reader.GetOrdinal("collector_number")),
            };

            cards.Add(card);
        }

        return cards;
    }
}

public class SqlWhereBuilder
{
    private readonly List<string> _conditions = new();

    public void Add(string condition)
    {
        _conditions.Add(condition);
    }

    public string Build()
    {
        if (_conditions.Count == 0)
            return "";

        return "WHERE " + string.Join(" AND ", _conditions);
    }
}