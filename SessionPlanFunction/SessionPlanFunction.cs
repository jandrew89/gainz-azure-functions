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
using System.Linq;

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
            log.LogInformation("C# HTTP trigger function getting filtered session plans.");

            IDocumentDbRepository<SessionPlan> Repository = new DocumentDbRepository<SessionPlan>();
            var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");
            return await Repository.GetItemsAsync(sp => sp.SessionType == sessionType, collectionId);
        }
    }

    public static class GetSessionPlanBySessionPlanId
    {
        [FunctionName("GetSessionPlanBySessionPlanId")]
        public static async Task<SessionPlan> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSessionPlanBySessionPlanId/{sessionPlanId}/{sessionType}")] HttpRequest req, string sessionPlanId, string sessionType, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function getting filtered session plans.");

            IDocumentDbRepository<SessionPlan> Repository = new DocumentDbRepository<SessionPlan>();
            var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");
            var sessionPlans = await Repository.GetItemsAsync(sp => sp.SessionType == sessionType && sp.Id == sessionPlanId, collectionId);
            var plan = new SessionPlan();
            foreach (var sp in sessionPlans)
            {
                plan = sp;
            }
            return plan;
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
                    //if name matches just update
                    var possibleSessionPlans = await repo.GetItemsAsync(s => s.SessionPlanName == sessionPlan.SessionPlanName && s.SessionType == sessionPlan.SessionType, collectionId);

                    if (!possibleSessionPlans.Any())
                    {
                        sessionPlan.Id = null;
                        document = await repo.CreateItemAsync(sessionPlan, collectionId);
                    } else
                    {
                        //set plan id back to session
                        sessionPlan.Id = possibleSessionPlans.First().Id;
                        document = await repo.UpdateItemAsync(sessionPlan.Id, sessionPlan, collectionId);
                    }
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

    public static class DeleteSessionPlan
    {
        [FunctionName("DeleteSessionPlan")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteSessionPlan/{planId}/{sessionType}")] HttpRequest req, ILogger log, string planId, string sessionType)
        {
            log.LogInformation("C# HTTP delete plan from cosmos.");
            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionPlanCollectionId");

                IDocumentDbRepository<SessionPlan> Repository = new DocumentDbRepository<SessionPlan>();
                await Repository.DeleteItemAsync(planId, collectionId, sessionType);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
