using System.Text.Json;
using System.Text.Json.Serialization;



public sealed class ScryfallClient : IScryfallClient
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ScryfallClient(HttpClient? http = null)
    {
        _http = http ?? new HttpClient();
        _http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "CardCollector/1.0 (+local)");
        _http.Timeout = TimeSpan.FromSeconds(15);
    }

    public async Task<ScryfallCard?> FetchBySetAndNumberAsync(
        string setCode,
        string collectorNumber,
        string? lang = null,
        CancellationToken ct = default)
    {
        var path = lang is null
            ? $"https://api.scryfall.com/cards/{Uri.EscapeDataString(setCode)}/{Uri.EscapeDataString(collectorNumber)}"
            : $"https://api.scryfall.com/cards/{Uri.EscapeDataString(setCode)}/{Uri.EscapeDataString(collectorNumber)}/{Uri.EscapeDataString(lang)}";

        using var resp = await _http.GetAsync(path, ct);

        if (resp.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        resp.EnsureSuccessStatusCode();

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        return await JsonSerializer.DeserializeAsync<ScryfallCard>(stream, _json, ct);
    }
}

public sealed class ScryfallCard
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("set")] public string SetCode { get; set; } = "";
    [JsonPropertyName("set_name")] public string SetName { get; set; } = "";
    [JsonPropertyName("collector_number")] public string CollectorNumber { get; set; } = "";
    [JsonPropertyName("color_identity")] public string[]? ColorIdentity { get; set; }
    [JsonPropertyName("rarity")] public string Rarity { get; set; } = "";
    [JsonPropertyName("mana_cost")] public string ManaCost { get; set; } = "";
    [JsonPropertyName("colors")] public string[]? Colors { get; set; }
    [JsonPropertyName("cmc")] public float Cmc { get; set; }
    [JsonPropertyName("game_change")] public bool GameChanger { get; set; } = false;
    [JsonPropertyName("legalities")] public ScryfallLegalities Legalities { get; set; }
    [JsonPropertyName("prices")] public ScryfallPrices? Prices { get; set; }
    [JsonPropertyName("lang")] public string Language { get; set; } = "en";
}

public sealed class ScryfallPrices
{
    public string? Usd { get; set; }
    public string? Usd_Foil { get; set; }
}

public sealed class ScryfallLegalities
{
    public string? standard = "";
    public string? future = "";
    public string? historic = "";
    public string? timeless = "";
    public string? gladiator = "";
    public string? pioneer = "";
    public string? modern = "";
    public string? legacy = "";
    public string? pauper = "";
    public string? vintage = "";
    public string? penny = "";
    public string? commander = "";
    public string? oathbreaker = "";
    public string? standardbrawl = "";
    public string? brawl = "";
    public string? alchemy = "";
    public string? paupercommander = "";
    public string? duel = "";
    public string? oldschool = "";
    public string? premodern = "";
    public string? predh = "";
}