using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]

namespace JsonPlaceHolderTests
{
    [TestClass]
    public class Tests
    {
        static readonly HttpClient httpClient = new HttpClient();

        //private TestContext testContextInstance;
        //public TestContext TestContext
        //{
        //    get { return testContextInstance; }
        //    set { testContextInstance = value; }
        //}

        // ************* TODO Figure out if I must be using the "USING" keyword on my requests. **************************

        [TestMethod]
        public async Task GetAllResources()
        {
            // The call to GetFromJsonAsync will verify that the response was a success response, and that the body was JSON. 
            // If the API returns the wrong content you will get...
            // System.NotSupportedException: The provided ContentType is not supported; the supported types are 'application/json' and the structured syntax suffix 'application/+json'.
            // Uncomment the following line to see how it throws when a NON JSON body is returned.
            //List<PlaceHolder> placeHolders = await httpClient.GetFromJsonAsync<List<PlaceHolder>>("https://google.com");

            // If the JSON conversion goes wrong you will get an error like this:
            // System.Text.Json.JsonException: The JSON value could not be converted to System.String. Path: $[0].userId | LineNumber: 2 | BytePositionInLine: 15. ---> System.InvalidOperationException: Cannot get the value of a token type 'Number' as a string.
            // If the JSON response has extra data that we were not expecting then we will not know that. From the test perspective we don't care.
            List<PlaceHolder> placeHolders = await httpClient.GetFromJsonAsync<List<PlaceHolder>>("https://jsonplaceholder.typicode.com/posts");

            // Uncomment to see what was returned.
            //placeHolders.ForEach(LogPlaceHolder);
            
            // The following line should have worked, I spent hours trying to work out why it fails, but finally wrote my own version of it.
            //CollectionAssert.AreEqual(Data.AllResources, placeHolders, "Failed to Validate All Resources");
            Helpers.AssertCollectionsAreEqual(Data.AllResources, placeHolders, "Failed to Validate All Resources");
        }

        [DataTestMethod]
        [DataRow("userId=1")]
        [DataRow("id=42")]
        [DataRow("userId=5")]
        public async Task FindAResource(string by)
        {
            Console.WriteLine($"Find By: {by}");
            List<PlaceHolder> placeHolders = await httpClient.GetFromJsonAsync<List<PlaceHolder>>($"https://jsonplaceholder.typicode.com/posts?{by}");
            placeHolders.ForEach(LogPlaceHolder);

            Assert.Inconclusive();
        }

        [DataTestMethod]
        [DataRow(1)]
        //[DataRow(5)]
        //[DataRow(10)]
        //[DataRow(0)]
        public void GetResourceByUserId(int userId)
        {
            GetResourceByUserIdThenValidate(userId);
        }

        [TestMethod]
        public void FindById()
        {
            List<PlaceHolder> placeHolders = Data.GetById(99);
            placeHolders.ForEach(LogPlaceHolder);
        }

        [TestMethod]
        public async Task CreateAResource()
        {
            //https://jsonplaceholder.typicode.com/posts
            var placeHolder = new PlaceHolder 
            { 
                UserId = 300,
                Id = 101,
                Title = "Mr. Superior",
                Body = "Made of Steel"
            };

            HttpResponseMessage postResponse = await httpClient.PostAsJsonAsync("https://jsonplaceholder.typicode.com/posts", placeHolder);

            postResponse.EnsureSuccessStatusCode();
            PlaceHolder placeHolderResponse = await postResponse.Content.ReadFromJsonAsync<PlaceHolder>();
            Assert.AreEqual(placeHolder, placeHolderResponse, "Not the response we were execting.");
            LogPlaceHolder(placeHolderResponse);
        }

        [TestMethod]
        public async Task UpdateAResource()
        {
            var placeHolder = new PlaceHolder
            {
                UserId = 300,
                Id = 7,
                Title = "Mrs. Potter",
                Body = "An author"
            };

            // Id at end of URI needs to be present.
            HttpResponseMessage postResponse = await httpClient.PutAsJsonAsync("https://jsonplaceholder.typicode.com/posts/7", placeHolder);

            postResponse.EnsureSuccessStatusCode();
            PlaceHolder placeHolderResponse = await postResponse.Content.ReadFromJsonAsync<PlaceHolder>();
            Assert.AreEqual(placeHolder, placeHolderResponse, "Not the response we were execting.");
            LogPlaceHolder(placeHolderResponse);
        }

        void LogPlaceHolder(PlaceHolder placeHolder)
        {
            Console.WriteLine($"userId: {placeHolder.UserId}\nid: {placeHolder.Id}\ntitle: {placeHolder.Title}\nbody: {placeHolder.Body}\n\n");
        }

        public void GetResourceByUserIdThenValidate(int userId, List<PlaceHolder> expectedPlaceHolders = null)
        {
            Task<HttpResponseMessage> responseTask = httpClient.GetAsync($"https://jsonplaceholder.typicode.com/posts?userId={userId}");

            // Yield the processor so that the GetAsync will execute, then it will yield while waiting for the response from the server.
            Thread.Sleep(0);

            // Since the server will be busy for a little bit of time before it sends us a response, we can use our idle time 
            // to do a little work that we need to do to validate the result.
            // While in this case the time this work takes is very small, in other cases it could be significant enough
            // to justify the extra complexity we are adding to the code by writing it this way.
            if (expectedPlaceHolders == null)
                expectedPlaceHolders = Data.GetByUserId(userId);

            // Now we will wait for the response and get it once it is available.
            responseTask.Wait();
            HttpResponseMessage response = responseTask.Result;

            response.EnsureSuccessStatusCode();

            Task<List<PlaceHolder>> placeHoldersTask = response.Content.ReadFromJsonAsync<List<PlaceHolder>>();
            placeHoldersTask.Wait();
            List<PlaceHolder> actualPlaceHolders = placeHoldersTask.Result;
            actualPlaceHolders.ForEach(LogPlaceHolder);

            Helpers.AssertCollectionsAreEqual(expectedPlaceHolders, actualPlaceHolders, "Data returned from the server is not what we are expecting");
        }

        [TestMethod]
        public void GetById(int id, PlaceHolder expectedPlaceHolder)
        {
            List<PlaceHolder> placeHolders = Data.GetById(id);
            placeHolders.ForEach(LogPlaceHolder);
        }

    }
}
