using System;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestEntityFramework472
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

            RunSetup(keys).GetAwaiter().GetResult();
            int workerId = 0;
            for (int i = 0; i < max_workers; i++)
            {
                var task = RunWorker(keys, Interlocked.Increment(ref workerId));
            }

            Task.Delay(TimeSpan.FromSeconds(duration)).GetAwaiter().GetResult();
            Console.WriteLine("Stopping the test");
        }

        static async Task RunSetup(int[] keys)
        {
            using (var db = new MyContext())
            {
                db.Database.CommandTimeout = 180;
                for (int i = 0; i < keys.Length; i++)
                {
                    var blog = await FindAsync(db, keys[i].ToString());
                    if (blog == null)
                    {
                        blog = new Blog();
                        blog.Key = keys[i].ToString();
                        blog.Data = CreateString(keys[i]);
                        db.Blogs.Add(blog);
                        await db.SaveChangesAsync();
                    }
                }
            }
        }

        static async Task RunWorker(int[] keys, int workerId)
        {
            while (true)
            {
                using (var db = new MyContext())
                {
                    db.Database.CommandTimeout = 180;
                    Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} {workerId} started");
                    for (int i = 0; i < keys.Length; i++)
                    {
                        try
                        {
                            var blog = await FindAsync(db, keys[i].ToString());
                            if (blog == null)
                            {
                                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} {workerId} row not found with key {keys[i]}");
                            }
                            else if (blog.Data.Length != keys[i])
                            {
                                Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} {workerId} row with key {keys[i]} has length {blog.Data.Length}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"{DateTime.UtcNow.ToString("HH:mm:ss")} {workerId} key={keys[i]}. {ex}");
                        }
                    }
                }
            }
        }

        static async Task<Blog> FindAsync(MyContext db, string key)
        {
            var effectiveDateUtc = DateTime.UtcNow;
            var blog = await db.Blogs.AsNoTracking()
                            .Where(s => s.Key == key)
                            .FirstOrDefaultAsync();
            return blog;
        }

        static string CreateString(int size)
        {
            var minValue = (int)'0';
            var maxValue = (int)'z';
            var chars = new char[size];
            var random = new Random();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)random.Next(minValue, maxValue);
            }

            return new string(chars);
        }
    }
}
