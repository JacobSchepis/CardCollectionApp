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
    Task AddAsync(ScryfallCard card);

    Task<(IReadOnlyList<CardRecord> rows, int total)> QueryAsync(QueryOptions opts, int pageIndex);
}

public interface IMenuAction
{
    string Label { get; protected set; }
    Task<bool> ExecuteAsync();
}
