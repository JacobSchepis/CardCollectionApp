using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class CardRecord
{
    public string Name { get; set; } = "";
    public string SetCode { get; set; } = "";
    public string SetName { get; set; } = "";
    public int CollectorNumber { get; set; }
    public string ColorIdentity { get; set; }
    public string Rarity { get; set; } = "";
    public string ManaCost { get; set; } = "";
    public int Cmc { get; set; }
    public bool GameChanger { get; set; } = false;
    public bool IsFoil { get; set; } = false;
    public bool IsPromo { get; set; } = false;
    public float Price { get; set; } = -1f;

    public int Quantity { get; set; } = 1;

    public CardRecord() { }

    public CardRecord(ScryfallCard card, bool isFoil, bool isPromo)
    {
        Name = card.Name;
        SetCode = card.SetCode;
        SetName = card.SetName;
        CollectorNumber = int.Parse(card.CollectorNumber);
        ColorIdentity = string.Join("", card.ColorIdentity);
        Rarity = card.Rarity;
        Cmc = (int)card.Cmc;
        GameChanger = card.GameChanger;
        IsFoil = isFoil;
        IsPromo = isPromo;
        Price = card.Prices is null ? -1f : (isFoil ? float.Parse(card.Prices.Usd_Foil ?? "-1") : float.Parse(card.Prices.Usd ?? "-1"));
    }
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
                cmc INTEGER NOT NULL,
                game_change BOOLEAN DEFAULT 0,
                price FLOAT NOT NULL,
                quantity INTEGER DEFAULT 1,
                is_foil BOOLEAN DEFAULT 0,
                UNIQUE(set_code, collector_number, is_foil)
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
            INSERT INTO cards   ( name,   set_code,   set_name,   collector_number,   color_identity,     rarity,     mana_cost,  cmc,    game_change,    price, is_foil, quantity)
            VALUES              ($name,  $setCode,   $set_name,  $collectorNumber,   $color_identity,    $rarity,    $mana_cost, $cmc,   $game_change,   $price, $is_foil, 1)
            ON CONFLICT(set_code, collector_number, is_foil)
            DO UPDATE SET quantity = quantity + 1;";
        cmd.Parameters.AddWithValue("$name", card.Name);
        cmd.Parameters.AddWithValue("$setCode", card.SetCode);
        cmd.Parameters.AddWithValue("$set_name", card.SetName);
        cmd.Parameters.AddWithValue("$collectorNumber", card.CollectorNumber);
        cmd.Parameters.AddWithValue("$color_identity", card.ColorIdentity);
        cmd.Parameters.AddWithValue("$rarity", card.Rarity);
        cmd.Parameters.AddWithValue("$mana_cost", card.Cmc);
        cmd.Parameters.AddWithValue("$cmc", card.Cmc);
        cmd.Parameters.AddWithValue("$game_change", card.GameChanger);
        cmd.Parameters.AddWithValue("$is_foil", card.IsFoil);
        cmd.Parameters.AddWithValue("$price", card.Price);
        cmd.Parameters.AddWithValue("$quantity", card.Quantity);

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
                SetName = reader["set_name"].ToString(),
                CollectorNumber = reader.GetInt32(reader.GetOrdinal("collector_number")),
                ColorIdentity = reader["color_identity"].ToString(),
                Rarity = reader["rarity"].ToString(),
                ManaCost = reader["mana_cost"].ToString(),
                Cmc = reader.GetInt32(reader.GetOrdinal("cmc")),
                GameChanger = reader.GetBoolean(reader.GetOrdinal("game_change")),
                Price = reader.GetFloat(reader.GetOrdinal("price")),
                IsFoil = reader.GetBoolean(reader.GetOrdinal("is_foil")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity"))
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