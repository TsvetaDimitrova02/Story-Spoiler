using System;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;
using StorySpoiler.Models;

namespace StorySpoiler
{
    [TestFixture]
    public class StorySpoilerApiTests
    {
        private RestClient client;
        private static string lastCreatedStoryId;
        private object userName;
        private const string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            
            string token = GetJwtToken("tsvetaemdim", "tsvetaemdim135");

            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        
        private string GetJwtToken(string userName, string password)
        {
            var loginClient = new RestClient(baseURL);
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();


        }


        [Order(1)]
        [Test]

        public void Test_CreateStorySpoiler_ShouldReturnSuccess()
        {
            var storyRequest = new StoryDTO
            {
                Title = "Test Story Spoiler",
                Description = "This is a test story.",
                url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            
            request.AddJsonBody(storyRequest);

            var response = client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(createResponse.StoryId, Is.Not.Null);

            lastCreatedStoryId = createResponse.StoryId;

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));


        }

        [Order(2)]
        [Test]
        public void Test_EditStorySpoiler_ShouldReturn200AndSuccessMessage()
        {


            Assert.That(lastCreatedStoryId, Is.Not.Null.Or.Empty);

            var editRequest = new StoryDTO
                {
                    Title = "Edited Test Story Spoiler",
                    Description = "Updated spoiler description.",
                    url = ""
                };

            var request = new RestRequest($"/api/Story/Edit/{lastCreatedStoryId}", Method.Put);
            request.AddJsonBody(editRequest);


            var response = this.client.Execute(request);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(editResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]
        public void Test_GetAllStorySpoilers_ShouldReturnNonEmptyList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var stories = JsonSerializer.Deserialize<List<StoryDTO>>(response.Content);
            Assert.That(stories, Is.Not.Null);
            Assert.That(stories.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void Test_DeleteStorySpoiler_ShouldReturnSuccess()
        {
            Assert.That(lastCreatedStoryId, Is.Not.Null.Or.Empty);

            var request = new RestRequest($"/api/Story/Delete/{lastCreatedStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(deleteResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void Test_CreateStorySpoiler_MissingRequiredFields_ShouldReturnBadRequest()
        {
            var incompleteStory = new { url = "" }; // Missing Title & Description
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(incompleteStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void Test_EditNonExistingStorySpoiler_ShouldReturnNotFound()
        {
            var editRequest = new StoryDTO
            {
                Title = "Non-existent Story",
                Description = "Should not exist",
                url = ""
            };

            var fakeId = Guid.NewGuid().ToString();
            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(editRequest);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(editResponse.Msg, Does.Contain("No spoilers"));
        }

        [Order(7)]
        [Test]
        public void Test_DeleteNonExistingStorySpoiler_ShouldReturnBadRequest()
        {
            var fakeId = Guid.NewGuid().ToString();
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var deleteResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(deleteResponse.Msg, Does.Contain("Unable to delete this story spoiler"));
        }
    }
}




