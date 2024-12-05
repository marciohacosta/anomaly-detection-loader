using System.Collections.Concurrent;
using System.Diagnostics;

namespace AnomalyDetectionLoader
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Load started.");

            // Specify Host IP
            string requestUrl = "http://[hostIP]/analyzer/anomaly/load";

            DateOnly date             = new DateOnly(2022, 10, 1);
            DateOnly endDate          = new DateOnly(2024, 11, 14);
            int users                 = 1_000;
            Random random             = new Random();
            ConcurrentBag<int> counts = new ConcurrentBag<int>();

            // Set users
            string[] catalog = new string[users];

            for (int i = 0; i < users; i++)
            {
                catalog[i] = Guid.NewGuid().ToString().Substring(0, 23);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            using (HttpClient httpClient = new HttpClient())
            {
                while (date <= endDate)
                {
                    string dateRequest = date.ToString("yyyyMMdd");

                    Console.WriteLine(dateRequest);

                    Parallel.For(0, users, u =>
                    {
                        string userRequest = catalog[u];

                        int qty = random.Next(1, 6);

                        for (int i = 1; i <= qty; i++)
                        {
                            string hourRequest = $"{10+i}0000";
                            string request = $"{requestUrl}?user={userRequest}&resource=pix&method=POST&date={dateRequest}&hour={hourRequest}";
                            int tryCount = 0;

                            while (tryCount < 5)
                            {
                                HttpResponseMessage response = httpClient.GetAsync(request).GetAwaiter().GetResult();

                                if (response.IsSuccessStatusCode)
                                {
                                    tryCount = 5;
                                }
                                else
                                {
                                    Console.WriteLine($"Try {tryCount + 1}: {response.StatusCode}");
                                    tryCount++;
                                }
                            }

                            counts.Add(1);
                        }
                    });

                    date = date.AddDays(1);
                }
            }

            Console.WriteLine($"Took {stopwatch.ElapsedMilliseconds / 1000} seconds.");
            Console.WriteLine($"Total records: {counts.Count}");
            Console.WriteLine("Load finished.");
        }
    }
}
