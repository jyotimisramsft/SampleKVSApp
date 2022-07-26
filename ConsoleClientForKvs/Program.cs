using KvsActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.KVSToRCMigration.Models;
using System.IO;
using System.Text;
using KeyValuePair = Microsoft.ServiceFabric.Actors.KVSToRCMigration.Models.KeyValuePair;
using System.Xml;
using System.Runtime.Serialization;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.FabricTransport;

[assembly: FabricTransportServiceRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace ConsoleClientForKvs
{
    class Program
    {
        public static void Main(string[] args)
        {
            try
            {

                //var res = GetAllAsync().GetAwaiter().GetResult(); 
                /*string url = "http://MININT-U9I0MVN.fareast.corp.microsoft.com:30007/8df902ed-3bba-41f8-a43a-a612c8bf3a17/132929173713133919/50204944-66fa-4c3f-886b-a815bae6872f/";
                var httpClient = new HttpClient()
                {
                BaseAddress = new Uri(url),
                };
                int count = 0;
                var response = httpClient.SendAsync(CreateKvsApiRequestMessage(new Uri(url), 0, 1000), HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();

                var keyvaluepairserializer = new DataContractSerializer(typeof(List<KeyValuePair>));
                //response.EnsureSuccessStatusCode();
                using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        string responseLine = streamReader.ReadLine();
                        while (responseLine != null)
                        {

                            List<KeyValuePair> kvsData = new List<KeyValuePair>();
                            using (Stream memoryStream = new MemoryStream())
                            {
                                byte[] data = Encoding.UTF8.GetBytes(responseLine);
                                memoryStream.Write(data, 0, data.Length);
                                memoryStream.Position = 0;

                                using (var reader = XmlDictionaryReader.CreateTextReader(memoryStream, XmlDictionaryReaderQuotas.Max))
                                {
                                    kvsData = (List<KeyValuePair>)keyvaluepairserializer.ReadObject(reader);
                                    count += kvsData.Count;
                                }
                            }

                            responseLine = streamReader.ReadLine();
                        }
                        Console.WriteLine("count = " + count );
                    }
                }*/


                var myActor = GetActor("a000");
                myActor.SetCountAsync(5, new CancellationToken()).GetAwaiter().GetResult();

                myActor = GetActor("truce");
                myActor.SetCountAsync(5, new CancellationToken()).GetAwaiter().GetResult();

                myActor = GetActor("9ff");
                myActor.SetCountAsync(5, new CancellationToken()).GetAwaiter().GetResult();
                myActor.SetNameAsync("name", new CancellationToken()).GetAwaiter().GetResult();
                myActor.SetByteArrayAsync(new byte[1], new CancellationToken()).GetAwaiter().GetResult();

                myActor = GetActor("Orderlord");
                myActor.SetCountAsync(5, new CancellationToken()).GetAwaiter().GetResult();
                var task = Task.Run(async () => await myActor.GetCountAsync(new CancellationToken())).ConfigureAwait(false);
                var result = task.GetAwaiter().GetResult();

                Thread.Sleep(10000);
                var task2 = Task.Run(async () => await myActor.GetCountAsync(new CancellationToken())).ConfigureAwait(false);
                var result2 = task2.GetAwaiter().GetResult();

                BulkCreateKvsActorAndAddStates(50);

                var res = GetAllAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /*private static EnumerationRequest CreateEnumerationRequestObject(long startSN, long enumerationSize)
        {
            var req = new EnumerationRequest();
            req.StartSN = startSN;
            req.ChunkSize = 100;
            req.NoOfItems = 1000;
            req.IncludeDeletes = false;

            return req;
        }

        private static HttpRequestMessage CreateKvsApiRequestMessage(Uri baseUri, long startSN, long enumerationSize)
        {
            var requestserializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(EnumerationRequest));
            var enumerationRequestContent = CreateEnumerationRequestObject(startSN, enumerationSize);

            var stream = new MemoryStream();
            requestserializer.WriteObject(stream, enumerationRequestContent);

            var content = Encoding.UTF8.GetString(stream.ToArray());

            return new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(baseUri, $"KvsMigration/EnumerateBySequenceNumber"),
                Content = new StringContent(content, Encoding.UTF8, "application/json"),
            };
        }*/

        private static async Task<string> GetAllAsync()
        {
            ActorId partitionKey = new ActorId("1507609b-69f5-452c-bd3d-cc2f9850e058");
            var actorServiceProxy = ActorServiceProxy.Create(new Uri("fabric:/KvsAppForMigration/KvsActorService"), partitionKey);

            ContinuationToken continuationToken = null;
            CancellationToken cancellationToken = new CancellationToken(false);
            List<ActorInformation> activeActors = new List<ActorInformation>();
            string allActorId = "";
            string allMarker = "";

            do
            {
                PagedResult<ActorInformation> page = await actorServiceProxy.GetActorsAsync(continuationToken, cancellationToken);
                activeActors.AddRange(page.Items);
                continuationToken = page.ContinuationToken;
                allMarker += continuationToken == null ? "" : continuationToken.Marker;
            }
            while (continuationToken != null);

            for (int i = 0; i < activeActors.Count; i++)
            {
                allActorId += activeActors[i].ActorId.ToString() + "\n";
            }
            return activeActors.Count().ToString() + allMarker + "\n\n\n\n" + allActorId;
        }

        private static IKvsActor GetActor(string userId)
        {
            return ActorProxy.Create<IKvsActor>(
                new ActorId(userId),
                new Uri("fabric:/KvsAppForMigration/KvsActorService"));
        }

        private static void BulkCreateKvsActorAndAddStates(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var myActor = GetActor("user" + i.ToString());
                myActor.SetCountAsync(5, new CancellationToken()).GetAwaiter().GetResult();
            }
            Console.WriteLine("Created " + count + " actors");
        }
    }
}
