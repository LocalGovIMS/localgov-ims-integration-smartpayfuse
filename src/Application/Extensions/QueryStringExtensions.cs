using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Linq;

namespace Application.Extensions
{
    public static class QueryStringExtensions
    {
        public static Dictionary<string, string> ToDictionary(this QueryString queryString)
        {
            var queryStringValues = QueryHelpers.ParseQuery(queryString.Value);

            return new Dictionary<string, string>(queryStringValues.Select(x => new KeyValuePair<string, string>(x.Key, x.Value.FirstOrDefault() ?? string.Empty)));
        }
    }
}
