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
    [JsonPropertyName("set_code")] public string SetCode { get; set; } = "";
    [JsonPropertyName("set_name")] public string SetName { get; set; } = "";
    [JsonPropertyName("collector_number")] public string CollectorNumber { get; set; } = "";
    [JsonPropertyName("color_identity")] public string ColorIdentity { get; set; } = "";
    [JsonPropertyName("rarity")] public string Rarity { get; set; } = "";
    [JsonPropertyName("cmc")] public string Cmc { get; set; } = "";
    [JsonPropertyName("game_change")] public bool GameChanger { get; set; } = false;
    [JsonPropertyName("prices")] public ScryfallPrices? Prices { get; set; }
    [JsonPropertyName("lang")] public string Language { get; set; } = "en";

    public override string ToString()
    {
        string s = $"{SetName} #{CollectorNumber}: {Name} \n";
        s += Prices is null
            ? "  No price data\n"
            : $"  Prices: USD {Prices.Usd ?? "N/A"}, USD Foil {Prices.UsdFoil ?? "N/A"}\n";
        return s;
    }
}

public sealed class ScryfallPrices
{
    public string? Usd { get; set; }
    public string? UsdFoil { get; set; }
}
