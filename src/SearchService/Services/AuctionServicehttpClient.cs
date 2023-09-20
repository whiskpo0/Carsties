using MongoDB.Entities;

namespace SearchService;

public class AuctionSvcHttpClient
{
        private readonly IConfiguration _config;
        public readonly HttpClient _httpClient ;

        public AuctionSvcHttpClient(HttpClient httpClient, IConfiguration config)
        {
                _httpClient = httpClient;
                _config = config;
        }

        public async Task<List<Item>> GetItemsForSearchDb()
        {
                var lastUpdated = await DB.Find<Item, string>()
                        .Sort(x => x.Descending(x => x.UpdateAt))
                        .Project(x => x.UpdateAt.ToString())
                        .ExecuteFirstAsync();
                 return await _httpClient.GetFromJsonAsync<List<Item>>(_config["AuctionServiceUrl"] + "/api/auctions?date=" + lastUpdated);
        }
}
