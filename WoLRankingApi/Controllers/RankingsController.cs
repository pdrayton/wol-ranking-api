using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using HtmlAgilityPack;
using WoLRankingApi.Models;

namespace WoLRankingApi.Controllers {

    public class RankingController : ApiController {

        // Other URL schemes to consider supporting:
        //
        // GET api/guilds/
        // GET api/guilds/{0}/rankings
        // GET api/guilds/{0}/reports
        // GET api/guilds/{0}/players
        //
        // GET api/ranking/guild/{0}?player={1} / class={1} / spec={1}
        // GET api/ranking/encounter/{0}?player={1} / class={1} / spec={1}

        // GET api/ranking/guild/{0}
        //      Very simple initial implementation. 
        //      Needs:
        //          1. cache results on the server, with a way to force a cache refresh via URI or HTTP headers
        //          2. support for BSON representations and http gzip if the appropriate headers are sent
        //          3. move the web API calls to be async; explore pipelining successive page requests
        //          4. some of the data returned still needs to be converted to real .NET/JSON data types
        //          5. decide if we want to return enum values (matching blizz api?) or stringified enum names
        [HttpGet]
        public IEnumerable<Ranking> Guild(int id) {

            int page = 0;
            bool moreRanks = true;
            while (moreRanks) {
                moreRanks = false;
                page++;

                var uri = string.Format(@"http://www.worldoflogs.com/guilds/{0}/rankings/players/?page={1}", id, page);
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(uri);

                // Swap this back to LINQ API calls when there are unit tests (SQL is easier to read, harder to debug)
                var rankings = from div in doc.DocumentNode.Descendants("div")
                               where div.Id == "tab-DPS" || div.Id == "tab-healers"
                               from tr in div.Descendants("tr")
                               where tr.GetAttributeValue("class", "") == "odd" || tr.GetAttributeValue("class", "") == "even"
                               select tr;

                foreach (var row in rankings) {
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
                    moreRanks = true;
                    yield return r;
                }
            }
        }

        // GET api/ranking/guildasync/{0}
        [HttpGet]
        public IEnumerable<Ranking> GuildAsync(int id) {
            var result = new List<Ranking>();
            int page = 0;
            bool moreRanks = true;
            while (moreRanks) {
                moreRanks = false;
                page++;

                var uri = string.Format(@"http://www.worldoflogs.com/guilds/{0}/rankings/players/?page={1}", id, page);
                Task<HtmlDocument> t = GetDocument(uri);
                HtmlDocument doc = t.Result;

                // Swap this back to LINQ API calls when there are unit tests (SQL is easier to read, harder to debug)
                var rankings = from div in doc.DocumentNode.Descendants("div")
                                where div.Id == "tab-DPS" || div.Id == "tab-healers"
                                from tr in div.Descendants("tr")
                                where tr.GetAttributeValue("class", "") == "odd" || tr.GetAttributeValue("class", "") == "even"
                                select tr;

                if (rankings.Count() > 0) {
                    foreach (var row in rankings) {
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
                        moreRanks = true;
                        result.Add(r);
                    }
                }
            }
            return result;
        }

        private Task<HtmlDocument> GetDocument(string uri) {
            Task<HtmlDocument> t = new Task<HtmlDocument>(() => {
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(uri);
                return doc;
            });
            t.Start();
            return t;
        }
    }
}