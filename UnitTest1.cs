using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Text.Json;
using ExamPrep1.Models;

namespace ExamPrep1

{
        [TestFixture]
        public class Tests
        {
            private RestClient client;

            private static string lastCreateadIdeaId;

            private const string BaseUrl = "http://144.91.123.158:82";
            private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJjMDQ5Y2Y3My1jNjdkLTRmNTktYjlhMS1lMjhmYTM2ZjYxYzAiLCJpYXQiOiIwNC8xNS8yMDI2IDE5OjA0OjIzIiwiVXNlcklkIjoiN2I3NDVmOTAtYjM5YS00YTEwLTUzOWMtMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJmdWtpMTIzQG1haWwuY29tIiwiVXNlck5hbWUiOiJmdWtpMTIzNCIsImV4cCI6MTc3NjMwMTQ2MywiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ._z48lVkGgNex4YbkYU5Usu7lvyU9vPpV9HM0ULrNFcs";

            private const string LoginEmail = "fuki123@mail.com";
            private const string LoginPassword = "123456";

            [OneTimeSetUp]
            public void Setup()
            {
                string jwtToken;

                if (!string.IsNullOrWhiteSpace(StaticToken))
                {
                    jwtToken = StaticToken;
                }
                else
                {
                    jwtToken = GetJwtToken(LoginEmail, LoginPassword);
                }

                var options = new RestClientOptions(BaseUrl)
                {
                    Authenticator = new JwtAuthenticator(jwtToken)
                };

                this.client = new RestClient(options);
            }

            private string GetJwtToken(string email, string password)
            {
                var tempClient = new RestClient(BaseUrl);
                var request = new RestRequest("/api/User/Authentication", Method.Post);
                request.AddJsonBody(new { email, password });

                var response = tempClient.Execute(request);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                    var token = content.GetProperty("token").GetString();

                    if (string.IsNullOrWhiteSpace(token))
                    {
                        throw new InvalidOperationException("Token not found in the response.");
                    }
                    return token;
                }
                else
                {
                    throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
                }
            }

        [Order(1)]
        [Test]

        public void CreateIdea_ShouldReturnCreatedIdea()
        {
            var request = new RestRequest("/api/Idea/Create", Method.Post);

            var newIdea = new IdeaDTO
            {
                Title = "Test Idea",
                Description = "This is a test idea.",
                Url = "",
            };

            request.AddJsonBody(newIdea);
            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK");

            var createdIdea = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(createdIdea.Msg, Is.EqualTo("Successfully created!"));
            
        }

        [Order(2)]
        [Test]

        public void GetAllIdeas_ShouldReturnListOfIdeas()
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK");
            Assert.That(response.Content, Is.Not.Null.And.Not.Empty, "Response content should not be null or empty"); 

            lastCreateadIdeaId = responseItems.LastOrDefault()?.Id;
        }

        [Order(3)]
        [Test]

        public void EditLastIdea_ShouldReturnEditedIdea()
        {
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            var editedIdea = new IdeaDTO
            {
                Title = "Edited Test Idea",
                Description = "This is an edited test idea.",
                Url = "",
            };
            
            request.AddQueryParameter("ideaId", lastCreateadIdeaId);
            request.AddJsonBody(editedIdea);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK");
            var editedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(editedResponse.Msg, Is.EqualTo("Edited successfully"));
        }

        [Order(4)]
        [Test]

        public void DeleteLastIdea_ShouldReturnDeletedIdea()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);

            request.AddQueryParameter("ideaId", lastCreateadIdeaId);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), $"Expected status code 200 OK");
            
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]

        public void TrytoCreatedIdeaWithoutTitle_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Idea/Create", Method.Post);
            var newIdea = new IdeaDTO
            {
                Title = "",
                Description = "This is a test idea without title.",
                Url = "",
            };
            request.AddJsonBody(newIdea);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Expected status code 400 Bad Request");
        }

        [Order(6)]
        [Test]

        public void TrytoEditNonExistingIdea_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            var editedIdea = new IdeaDTO
            {
                Title = "Edited Test Idea",
                Description = "This is an edited test idea.",
                Url = "",
            };
            request.AddQueryParameter("ideaId", "non-existing-id");
            request.AddJsonBody(editedIdea);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Expected status code 400 Bad Request");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

        [Order(7)]
        [Test]

        public void TrytoDeleteNonExistingIdea_ShouldReturnBadRequest()
        {
            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", "non-existing-id");
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), $"Expected status code 400 Bad Request");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }

    }
}