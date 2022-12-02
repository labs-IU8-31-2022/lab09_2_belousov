using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Finance_api;


class Program
{
    static async Task Main(string[] args)
    {
        /*StockInfo stock = new StockInfo("BBQ");
        decimal? average = stock.ParseString(stock.make_request().Result);
        Console.WriteLine(average);*/
        var quotations = new List<string>();
        var waitHandler = new AutoResetEvent(true);
        var pathRes = $"/Users/theblindpew/result.txt";
        if (File.Exists(pathRes))
            File.Delete(pathRes);
    
        using (var reader = new StreamReader($"/Users/theblindpew/ticker.txt"))
        {
            while (await reader.ReadLineAsync() is { } line)
            {
                quotations.Add(line);
            }
        }

        var tasks = new Task[quotations.Count];
        var size = 0;
        foreach (var t1 in quotations.Select(action => Task.Run(() =>
                 {
                     var response = "";
                     while (true)
                     {
                         try
                         {
                             response = StockInfo.make_request(action);
                             break;
                         }
                         catch (HttpRequestException e)
                         {
                             if (e.StatusCode == HttpStatusCode.NotFound)
                             {
                                 Console.WriteLine($"404 (Not Found)  {action} may be delisted");
                                 break;
                             }

                             Console.WriteLine($"{e.Message}  {action}");
                             if (e.StatusCode == HttpStatusCode.Unauthorized)
                             {
                                 Thread.Sleep(300000);
                             }

                             Thread.Sleep(10000);
                         }
                     }

                     return KeyValuePair.Create(action, response);
                 })))
        {
            tasks[size++] = t1.ContinueWith(t =>
            {
                var dec = StockInfo.ParseString(t.Result.Value);
                if (dec is null) return;
                SaveToFile(KeyValuePair.Create(t.Result.Key, dec.GetValueOrDefault()));
            });
            Thread.Sleep(11);
        }


        void SaveToFile(KeyValuePair<string, decimal> pair)
        {
            waitHandler.WaitOne();
            File.AppendAllText(pathRes, $"{pair.Key} : {pair.Value:f4}\n");
            waitHandler.Set();
        }

        Task.WaitAll(tasks);
    }
}