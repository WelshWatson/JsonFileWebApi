using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;

namespace JsonFileWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class JsonController : ApiController
    {
        [HttpGet]
        [Route("search/{searchField}")]
        public List<JObject> GetFileById(string searchField)
        {
            var response = new List<JObject>();
            var allFileInfos = GetFileInfos();

            Parallel.ForEach(allFileInfos, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file =>
            {
                using (JsonTextReader reader = new JsonTextReader(File.OpenText(file.FullName)))
                {
                    var jsonObject = (JObject)JToken.ReadFrom(reader);
                    foreach (KeyValuePair<string, JToken> property in jsonObject)
                    {
                        if (KeyOrValueMatch(property, searchField))
                        {
                            response.Add(jsonObject);
                            break;
                        }
                    }
                }
            });

            return response;
        }

        private FileInfo[] GetFileInfos()
        {
            return new DirectoryInfo(System.Web.Hosting.HostingEnvironment.MapPath("~/jsonfiles")).GetFiles("*.json");
        }

        private bool KeyOrValueMatch(KeyValuePair<string, JToken> keyValuePair, string searchField)
        {
            return keyValuePair.Key.ToLower().Contains(searchField.ToLower()) || keyValuePair.Value.ToString().ToLower().Contains(searchField.ToLower());
        }
    }
}