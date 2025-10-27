using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IScryfallClient {Task<ScryfallCard?> FetchBySetAndNumberAsync(
        string setCode,
        string collectorNumber,
        string? lang = null,
        CancellationToken ct = default);
}

public interface ICollectionRepository
{
    Task AddAsync(CardRecord card);

    List<CardRecord> GetScryfallCards(string query, int pageSize = 175, int offset = 0);

    void DecrementQuantityOrDeleteAsync(string setCode, int collectorNumber, bool isFoil, int quantity = 0);
}
