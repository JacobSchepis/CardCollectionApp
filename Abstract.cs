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

    IEnumerable<CardRecord> GetScryfallCards(string query, int pageSize = 175);
}
