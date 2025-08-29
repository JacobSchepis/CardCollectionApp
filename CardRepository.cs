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
                set_name TEXT NOT NULL,
                collector_number INTEGER NOT NULL,
                color_identity TEXT,
                rarity TEXT,
                mana_cost TEXT,
                cmc INTEGER,
                game_change BOOLEAN DEFAULT 0,
                price FLOAT,
                price_foil FLOAT,
                
            );";
        cmd.ExecuteNonQuery();
        connection.Close();
    }

    public Task AddAsync(ScryfallCard card)
    {
        float price = 0;

        if (!float.TryParse(card.Prices?.Usd, out price))
            price = -1f;

        float price_foil = 0;

        if (!float.TryParse(card.Prices?.UsdFoil, out price_foil))
            price_foil = -1f;

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var cmd = connection.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO cards   ( name,   set_code,   set_name,   collector_number,   color_identity,     rarity,     mana_cost,  cmc,    game_change,    price,  price_foil)
            VALUES              ($name,  $setCode,   $set_name,  $collectorNumber,   $color_identity,    $rarity,    $mana_cost, $cmc,   $game_change,   $price, $price_foil);";
        cmd.Parameters.AddWithValue("$name", card.Name);
        cmd.Parameters.AddWithValue("$setCode", card.SetCode);
        cmd.Parameters.AddWithValue("$set_name", card.SetName);
        cmd.Parameters.AddWithValue("$collectorNumber", card.CollectorNumber);
        cmd.Parameters.AddWithValue("$color_identity", card.ColorIdentity);
        cmd.Parameters.AddWithValue("$rarity", card.Rarity);
        cmd.Parameters.AddWithValue("$mana_cost", card.Cmc);
        cmd.Parameters.AddWithValue("$cmc", card.Cmc);
        cmd.Parameters.AddWithValue("$game_change", card.GameChanger);
        cmd.Parameters.AddWithValue("$price", (price == -1 ? DBNull.Value : price));
        cmd.Parameters.AddWithValue("$price_foil", (price_foil == -1 ? DBNull.Value : price));

        cmd.ExecuteNonQuery();

        connection.Close();

        return Task.CompletedTask;
    }

    public List<CardRecord> GetScryfallCards(string query, int pageSize = 175)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        string sql = $"SELECT * FROM cards {query}";

        using var command = new SqliteCommand(sql, connection);
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

        Console.WriteLine($"Fetched {cards.Count} cards from database.");

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