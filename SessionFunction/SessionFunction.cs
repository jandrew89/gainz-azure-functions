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
using System.Linq;
using Core.Services.Data.Dto;

namespace SessionFunction
{
    public static class GetPreviousSet
    {
        [FunctionName("GetPreviousSetByEquipment")]
        public static Task<SetDate[]> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetPreviousSetByEquipment/{equipmentId}/{sessionType}")] HttpRequest req, string equipmentId, string sessionType, ILogger log)
        {
            log.LogInformation("C# HTTP trigger getting the previous activity.");

            //TODO: Set in env
            var recordsToSendBack = 2;

            IDocumentDbRepository<Session> Repository = new DocumentDbRepository<Session>();
            var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");
            var sqlSpec = new SqlQuerySpec("SELECT s.SessionDate, a.Sets " +
                "FROM Sessions s " +
                "JOIN a IN s.Activities WHERE a.Equipment.id = @equipmentId and " +
                "s.SessionType = @sessionType " +
                "ORDER BY s.SessionDate DESC  OFFSET 1 LIMIT @records",
                new SqlParameterCollection(
                    new SqlParameter[] {
                        new SqlParameter { Name = "@records", Value = recordsToSendBack },
                        new SqlParameter { Name = "@equipmentId", Value = equipmentId }, 
                        new SqlParameter { Name = "@sessionType", Value = sessionType } 
                    }));

            return Task.FromResult(Repository.GetItemsBySqlQuery<SetDate>(sqlSpec, collectionId));
        }
    }

    public static class UpsertActivity
    {
        [FunctionName("UpsertActivity")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "UpsertSession/{sessionId}/{sessionType}")] HttpRequest req, string sessionId, string sessionType, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function add an activity to a session.");

            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");
                var repo = new DocumentDbRepository<Session>();
                var activeSessions = await repo.GetItemsAsync(s => s.Id == sessionId && s.SessionType == sessionType, collectionId);
                if (activeSessions == null)
                {
                    log.LogWarning("Could not find a valid session.");
                    return false;
                }

                var activeSession = activeSessions.First();

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var activityToUpdate = JsonConvert.DeserializeObject<Activity>(requestBody);

                activeSession.Activities.RemoveAll(d => d.Id == activityToUpdate.Id);

                activeSession.Activities.Add(activityToUpdate);

                await repo.UpdateItemAsync(activeSession.Id, activeSession, collectionId);
                return true;
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return false;
            }
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

                returnSession.SessionType = document.GetPropertyValue<string>("SessionType");
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
            return (await Repository.GetItemsAsync(collectionId)).ToList().OrderByDescending(r => r.SessionDate);
        }
    }

    public static class DeleteActivity
    {
        [FunctionName("DeleteActivity")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteActivity/{sessionId}/{activityId}/{sessionType}")] HttpRequest req, ILogger log, string sessionId, string activityId, string sessionType)
        {
            log.LogInformation("C# HTTP delete activity from cosmos.");
            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionCollectionId");


                IDocumentDbRepository<Session> repo = new DocumentDbRepository<Session>();
                var session = await repo.GetItemsAsync(s => s.Id == sessionId && s.SessionType == sessionType, collectionId);

                session.First().Activities.RemoveAll(a => a.Id == activityId);

                await repo.UpdateItemAsync(session.First().Id, session.First(), collectionId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
