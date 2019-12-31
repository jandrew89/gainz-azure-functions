using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Core.Services.Data;
using Core.Services.Services;
using Core.Services.Interfaces;
using Microsoft.Azure.Documents;
using System.IO;
using Newtonsoft.Json;

namespace SessionPlanFunction
{
    public static class GetSessionPlans
    {
        [FunctionName("GetSessionPlans")]
        public static async Task<IEnumerable<SessionPlan>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSessionPlans")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function geting all session plans.");

            IDocumentDbRepository<SessionPlan> Repository = new DocumentDbRepository<SessionPlan>();
            var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");
            return await Repository.GetItemsAsync(collectionId);
        }
    }

    public static class GetSessionPlansBySessionType
    {
        [FunctionName("GetSessionPlansBySessionType")]
        public static async Task<IEnumerable<SessionPlan>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSessionPlansBySessionType/{sessionType}")] HttpRequest req, string sessionType, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function geting filtered session plans.");

            IDocumentDbRepository<SessionPlan> Repository = new DocumentDbRepository<SessionPlan>();
            var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");
            return await Repository.GetItemsAsync(sp => sp.SessionType == sessionType, collectionId);
        }
    }

    public static class UpsertSessionPlan
    {
        [FunctionName("UpsertSessionPlan")]
        public static async Task<SessionPlan> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "UpsertSessionPlan")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function add an activity to a session.");
            var returnSessionPlan = new SessionPlan();

            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");
                var repo = new DocumentDbRepository<SessionPlan>();
                var document = new Document();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var sessionPlan = JsonConvert.DeserializeObject<SessionPlan>(requestBody);

                if (req.Method == "POST")
                {
                    sessionPlan.Id = null;
                    document = await repo.CreateItemAsync(sessionPlan, collectionId);
                }
                else
                {
                    document = await repo.UpdateItemAsync(sessionPlan.Id, sessionPlan, collectionId);
                }

                returnSessionPlan.SessionType = document.GetPropertyValue<string>("SessionType");
                returnSessionPlan.Id = document.Id;
                return returnSessionPlan;
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return returnSessionPlan;
            }
        }
    }
}
