using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Encodings;
using System.Text.Json;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using StorySpoilerSystem.Models;


namespace StorySpoilerSystem
{
    [TestFixture]
    public class  StorySpolierSystemTests
    {
        private RestClient client;
        private static string createdStoryId;

        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("test1001", "123456789");

            var options = new RestClientOptions(baseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            this.client = new RestClient(options);
         }
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new {username, password});

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Order(1)]
        [Test]
        public void CreateStorySpoiler_WithRequiredFieldsShouldReturnSuccess()
        {
            var storyRequest = new StoryDTO
            {
                Title = "Story Spoiler Test Title",
                Description = "This is some test description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);

            var response = client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created), "Expect status code to be 201");
            Assert.That(createResponse.StoryId, Is.Not.Null.And.Not.Empty, "Expect to return storyId");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"), "Exepct to return \"Successfull created!\" messege");

            createdStoryId = createResponse.StoryId;
        }

        [Order(2)]
        [Test]
        public void EditStorySpoiler_WithExcistingStoryId_ShouldReturnSuccess()
        {
            var editRquest = new StoryDTO
            {
                Title = "Updated test title",
                Description = "Updated test decription",
                Url = ""
            };

            var requst = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            requst.AddJsonBody(editRquest);

            var response = client.Execute(requst);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"), "Exepct to return \"Successfully edited\" messege");
        }

        [Order(3)]
        [Test]
        public void GetAllStorySpoiler_ShouldReturnSuccess()
        {
            var requst = new RestRequest("/api/Story/All", Method.Get);

            var response = client.Execute(requst);
            var responseList = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(responseList, Is.Not.Null.And.Not.Empty, "Expect response list to not be emtpy");
        }

        [Order(4)]
        [Test]
        public void DeleteStorySpoiler_WithExecistingId_ShouldReturnSuccess()
        {
            var requst = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);

            var response = client.Execute(requst);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expect status code to be 200");
            Assert.That(deleteResponse.Msg, Is.EqualTo("Deleted successfully!"), "Exepct to return \"Deleted successfully!\" messege");
        }

        [Order(5)]
        [Test]
        public void CreateStorySpoiler_WithoutRqueiredFields_ShouldReturnBadRequest()
        {
            var storyRequest = new StoryDTO
            {
                Title = "",
                Description = "",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(storyRequest);

            var response = client.Execute(request);
            var failedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code to be 400");
         }

        [Order(6)]
        [Test]
        public void EditStorySpoiler_WithNonExcistingSpoilerId_ShouldReturnNotFound()
        {
            var fakeStoryId = "not_excisitng";
            var editRquest = new StoryDTO
            {
                Title = "Updated test title",
                Description = "Updated test decription",
                Url = ""
            };

            var requst = new RestRequest($"/api/Story/Edit/{fakeStoryId}", Method.Put);
            requst.AddJsonBody(editRquest);

            var response = client.Execute(requst);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "Expect status code to be 404");
            Assert.That(editResponse.Msg, Is.EqualTo("No spoilers..."), "Exepct to return \"No spoilers...\" messege");
        }

        [Order(7)]
        [Test]
        public void DeleteStorySpoiler_WithNonExcistingStoryId_ShouldReturnBadRequst()
        {
            var fakeStoryId = "not_excisitng";
            var requst = new RestRequest($"/api/Story/Delete/{fakeStoryId}", Method.Delete);

            var response = client.Execute(requst);
            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expect status code to be 400");
            Assert.That(deleteResponse.Msg, Is.EqualTo("Unable to delete this story spoiler!"), "Exepct to return \"Unable to delete this story spoiler!\" messege");
        }

        [OneTimeTearDown]
        public void TearDown() 
        { 
            this.client?.Dispose();
        }
    }
}