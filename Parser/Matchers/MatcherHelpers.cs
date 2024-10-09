using Newtonsoft.Json.Linq;

namespace Parser.Matchers;

internal class MatcherHelpers
{
    internal static JObject? ParseGreToClientEvent(string line)
    {
        dynamic? data = JObject.Parse(line);
        return data?.greToClientEvent;
    }
}
