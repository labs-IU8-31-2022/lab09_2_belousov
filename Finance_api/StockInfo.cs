using Newtonsoft.Json;

namespace Finance_api;

public class StockInfo
{
    //private string Id;
    //public StockInfo(string id) => Id = id;
    
    
    public static string make_request(string id)
    {
        HttpClient client = new HttpClient();
        string str_request = $"https://query1.finance.yahoo.com/v7/finance/download/{id}?" +
                             $"period1={DateTimeOffset.Now.AddYears(-1).ToUnixTimeSeconds()}" +
                             $"&period2={DateTimeOffset.Now.ToUnixTimeSeconds()}" +
                             $"&interval=1d&events=history&includeAdjustedClose=true";
        var response = client.GetAsync(str_request);
        response.Result.EnsureSuccessStatusCode();
        //var dynamicObject = JsonConvert.DeserializeObject<dynamic>(response.Result.Content.ReadAsStringAsync())!;
        //string content = await response.Result.Content.ReadAsStringAsync();
        //var dynamicObject = JsonConvert.DeserializeObject<dynamic>(content)!;
        //Console.WriteLine(content);
        return response.Result.Content.ReadAsStringAsync().Result;
    }

    public static decimal? ParseString(string request)
    {
        var parse = request
            .Split('\n')
            .Select(line => line.Split(','))
            .Where(numbers => numbers[0] != "Date" && numbers[1] != "null")
            .Sum(numbers => ((Convert.ToDecimal(numbers[2].Replace('.', ',')) -
                              (Convert.ToDecimal(numbers[3].Replace('.', ','))))) / 2);
        return parse / request.Split('\n').Count(numbers => !numbers.Contains("null") && !numbers.Contains("Date"));
    }
}