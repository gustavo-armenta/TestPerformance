using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TestHttpPerformance472
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync(args[0]).GetAwaiter().GetResult();
        }

        static async Task RunAsync(string endpoint)
        {
            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, x509Certificate2, x509Chain, sslPolicyErrors) => { return true; };
            var client = new HttpClient(handler);
            var map = new Dictionary<HttpStatusCode, List<long>>();
            map[HttpStatusCode.OK] = new List<long>();
            var watch = Stopwatch.StartNew();
            while (map[HttpStatusCode.OK].Count < 100)
            {
                try
                {
                    watch.Restart();
                    var response = await client.GetAsync(endpoint);
                    watch.Stop();
                    var duration = watch.ElapsedMilliseconds;
                    Console.WriteLine($"statusCode={response.StatusCode} duration={duration}");
                    if (duration > 3000)
                    {
                        continue;
                    }
                    if (!map.ContainsKey(response.StatusCode))
                    {
                        map[response.StatusCode] = new List<long>();
                    }
                    map[response.StatusCode].Add(duration);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }

            foreach (var k in map.Keys)
            {
                map[k].Sort();
                int count = map[k].Count;
                int p95 = (int)(count * 0.95 - 1);
                int p99 = (int)(count * 0.99 - 1);
                Console.WriteLine($"count={count} statusCode={k} p95={map[k][p95]} p99={map[k][p99]}");
            }
        }
    }
}
