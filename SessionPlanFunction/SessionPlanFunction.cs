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
using Core.Services.Services;
using Core.Services.Interfaces;

namespace SessionPlanFunction
{
    public static class SessionPlanFunction
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
    }
}
