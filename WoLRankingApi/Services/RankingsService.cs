using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;
using HtmlAgilityPack;
using WoLRankingApi.Models;

namespace WoLRankingApi.Services {
    public class RankingsService {
        public async Task<IEnumerable<Ranking>> ScrapeRankingsHtml(int guild, int page) {
            List<Ranking> result = new List<Ranking>();

            var uri = string.Format(@"http://www.worldoflogs.com/guilds/{0}/rankings/players/?page={1}", guild, page);
            HttpClient client = new HttpClient();
            HttpResponseMessage rsp = await client.GetAsync(uri);
            if (rsp.IsSuccessStatusCode) {
                var strm = await rsp.Content.ReadAsStreamAsync();
                var doc = new HtmlDocument();
                doc.Load(strm);

                // Swap this back to LINQ API calls when there are unit tests (SQL is easier to read, harder to debug)
                var rankings = from div in doc.DocumentNode.Descendants("div")
                               where div.Id == "tab-DPS" || div.Id == "tab-healers"
                               from tr in div.Descendants("tr")
                               where tr.GetAttributeValue("class", "") == "odd" || tr.GetAttributeValue("class", "") == "even"
                               select tr;

                foreach (var row in rankings.AsNotNull()) {
                    var td = row.Descendants("td").ToArray();
                    var r = new Ranking() {
                        Rank = Int32.Parse(td[0].Descendants("span").FirstOrDefault().InnerText),
                        RankUri = new Uri(new Uri("http://www.worldoflogs.com"), td[0].Element("a").GetAttributeValue("href", "")),
                        Player = td[1].InnerText,
                        ParseUri = new Uri(new Uri("http://www.worldoflogs.com"), td[1].Element("a").GetAttributeValue("href", "")),
                        Class = Helpers.ParseClass(td[2].Element("div").GetAttributeValue("class", "").Split(' ')[1]),
                        Spec = td[3].Element("div").GetAttributeValue("class", "").Split(' ')[1],
                        Date = td[4].InnerText,
                        Encounter = td[5].InnerText,
                        Size = Int32.Parse(td[6].InnerText),
                        Difficulty = Helpers.ParseDifficulty(td[7].InnerText),
                        OutputRate = Int64.Parse(td[9].InnerText),
                        OutputTotal = Int64.Parse(td[11].InnerText.Replace(" ", ","), NumberStyles.AllowThousands),
                        Contribution = Double.Parse(td[12].InnerText.Split(' ')[0]),
                        Duration = td[13].InnerText
                    };
                    result.Add(r);
                }
            }

            return result;
        }
    }
}