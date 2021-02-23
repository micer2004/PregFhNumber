using System;
using System.Diagnostics;
using System.Threading;

using PregFhNumber.Constants;
using PregFhNumber.PersonRegistry;

namespace PregFhNumber
{
    internal static partial class Operations
    {
        private static readonly Random Random = new Random();

        public static void RunPerformanceTest(PersonRegistryClient client)
        {
        var sw = new Stopwatch();

        sw.Start();
        client.GetDemographicsAsync(CreateGetDemographicsRequest(new II(IdNumberOid.FhNumber, CreateRandomFhNumber())));
        sw.Stop();
        Console.WriteLine("Initial request: " + sw.Elapsed);

        sw.Restart();
        const int numRequests = 1000;
        for (var i = 0; i < numRequests; ++i)
        {
            client.GetDemographicsAsync(CreateGetDemographicsRequest(new II(IdNumberOid.FhNumber, CreateRandomFhNumber())));
        }
        sw.Stop();
        Console.WriteLine("{0} subsequent requests: {1} ({2} ms per request)", numRequests, sw.Elapsed, sw.ElapsedMilliseconds / numRequests);

        sw.Restart();
        const int numIndividualRequests = 100;
        for (var i = 0; i < numIndividualRequests; ++i)
        {
            using (var c = Program.CreateClient())
            {
                c.GetDemographicsAsync(CreateGetDemographicsRequest(new II(IdNumberOid.FhNumber, CreateRandomFhNumber())));
            }
        }
        sw.Stop();
        Console.WriteLine("{0} individual requests: {1} ({2} ms per request)", numIndividualRequests, sw.Elapsed, sw.ElapsedMilliseconds / numIndividualRequests);

        const int numThreads = 20;
        var threads = new Thread[numThreads];
        for (int i = 0; i < numThreads; ++i)
        {
            threads[i] = new Thread(ThreadTester);
            threads[i].Start(i);
        }
        foreach (var thread in threads)
            thread.Join();
        }

        private static void ThreadTester(object threadId)
        {
            var sw = new Stopwatch();
            sw.Start();
            const int numIndividualRequests = 50;
            using (var c = Program.CreateClient())
            {
                for (int i = 0; i < numIndividualRequests; ++i)
                {
                    c.GetDemographicsAsync(CreateGetDemographicsRequest(new II(IdNumberOid.FhNumber, CreateRandomFhNumber())));
                }
            }
            sw.Stop();
            Console.WriteLine("{0} individual requests from thread #{1}: {2} ({3} ms per request)", numIndividualRequests, (int)threadId, sw.Elapsed, sw.ElapsedMilliseconds / numIndividualRequests);
        }

        private static string CreateRandomFhNumber()
        {
            while (true)
            {
                string fhNumber = Random.Next(800000000, 800009999).ToString();
                if (AppendChecksum(ref fhNumber))
                    return fhNumber;
            }
        }
    }
    
}
