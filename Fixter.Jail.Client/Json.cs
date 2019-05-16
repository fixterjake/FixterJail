using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace chat_client
{
    public static class Json
    {
        public static T Parse<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            T obj = null;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                obj = JsonConvert.DeserializeObject<T>(json, settings);
            }
            catch (Exception ex)
            {
                // TODO: Find a way to log the exception (can shared libraries do that?!)
                obj = null;
            }

            return obj;
        }

        public static string Stringify(object data)
        {
            if (data == null) return null;

            string json = null;

            try
            {
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                };

                json = JsonConvert.SerializeObject(data, settings);
            }
            catch (Exception ex)
            {
                // TODO: Find a way to log the exception (can shared libraries do that?!)
                json = null;
            }

            return json;
        }
    }
}
