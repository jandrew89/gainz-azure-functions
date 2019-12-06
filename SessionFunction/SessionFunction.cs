using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Core.Services.Data;
using Core.Services.Interfaces;
using Core.Services.Services;
using Microsoft.Azure.Documents;

namespace SessionFunction
{
    public static class AddOrUpdateSession
    {
        [FunctionName("AddOrUpdateSession")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post","put", Route = "AddOrUpdateSession")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            return name != null
                ? (ActionResult)new OkObjectResult($"Hello, {name}")
                : new BadRequestObjectResult("Please pass a name on the query string or in the request body");
        }
    }

    public static class UpsertSession
    {
        [FunctionName("UpsertSession")]
        public static async Task<Session> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "UpsertSession")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function add an activity to a session.");
            var returnSession = new Session();

            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");
                var repo = new DocumentDbRepository<Session>();
                var document = new Document();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var session = JsonConvert.DeserializeObject<Session>(requestBody);
                if (req.Method == "POST")
                {
                    session.Id = null;
                    document = await repo.CreateItemAsync(session, collectionId);
                }
                else
                {
                    document = await repo.UpdateItemAsync(session.Id, session, collectionId);
                }

                returnSession.Id = document.Id;
                return returnSession;
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return returnSession;
            }
        }
    }

    public static class GetAllActivities
    {
        [FunctionName("GetAllActivities")]
        public static async Task<IEnumerable<Activity>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAllActivities")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            IDocumentDbRepository<Activity> Repository = new DocumentDbRepository<Activity>();
            var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");
            return await Repository.GetItemsAsync(collectionId);
        }
    }

    public static class GetSession
    {
        [FunctionName("GetSession")]
        public static async Task<Session> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSession/{id}/{sessionType}")] HttpRequest req, ILogger log, string id, string sessionType)
        {
            log.LogInformation("C# HTTP log single data from cosmos.");

            IDocumentDbRepository<Session> Repository = new DocumentDbRepository<Session>();
            var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");

            var sessions = await Repository.GetItemsAsync(d => d.Id == id && d.SessionType == sessionType, collectionId);
            var session = new Session();
            foreach (var emp in sessions)
            {
                session = emp;
            }
            return session;
        }
    }

    public static class GetAllSessions
    {
        [FunctionName("GetAllSessions")]
        public static async Task<IEnumerable<Session>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Get")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");

            IDocumentDbRepository<Session> Repository = new DocumentDbRepository<Session>();
            return await Repository.GetItemsAsync(collectionId);
        }
    }
}
