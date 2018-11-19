using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace TestSqlDatabase
{
    class Program
    {
        static void Main(string[] args)
        {
            int duration = int.Parse(ConfigurationManager.AppSettings["duration"]);
            int max_workers = int.Parse(ConfigurationManager.AppSettings["max_workers"]);
            string[] stringKeys = ConfigurationManager.AppSettings["keys"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] keys = new int[stringKeys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = int.Parse(stringKeys[i].Trim());
            }

            int workerId = 0;
            for (int i = 0; i < max_workers; i++)
            {
                var task = RunWorker(keys, Interlocked.Increment(ref workerId));
            }

            Task.Delay(TimeSpan.FromSeconds(duration)).GetAwaiter().GetResult();
            Console.WriteLine("Stopping the test");
        }

        static async Task RunWorker(int[] keys, int workerId)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MyContext"].ConnectionString;
            var watch = Stopwatch.StartNew();
            while (true)
            {
                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} started");
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        watch.Restart();
                        await connection.OpenAsync();
                        watch.Stop();
                        if (watch.ElapsedMilliseconds > 1000)
                        {
                            Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} Open={watch.ElapsedMilliseconds}ms");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} failed to open connection. {ex}");
                        continue;
                    }
                    for (int i = 0; i < keys.Length; i++)
                    {
                        try
                        {
                            var command = connection.CreateCommand();
                            command.CommandTimeout = 180;
                            command.CommandText = "select * from [MySchema].[Blogs] where [Key]='{keys[i]}'";
                            watch.Restart();
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                watch.Stop();
                                if (watch.ElapsedMilliseconds > 1000)
                                {
                                    Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} key={keys[i]} ExecuteReaderAsync={watch.ElapsedMilliseconds}ms");
                                }
                                watch.Restart();
                                while (await reader.ReadAsync())
                                {
                                    var data = reader.GetString(1);
                                    if (data.Length != keys[i])
                                    {
                                        Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} key={keys[i]} length={data.Length}");
                                    }
                                }
                                watch.Stop();
                                if (watch.ElapsedMilliseconds > 1000)
                                {
                                    Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} key={keys[i]} ReadAsync={watch.ElapsedMilliseconds}ms");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} workerId={workerId} key={keys[i]} failed to query. {ex}");
                            break;
                        }
                    }
                }
            }
        }
    }
}
