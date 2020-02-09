using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Core.Services.Interfaces;
using Core.Services.Data;
using Core.Services.Services;
using System.Collections.Generic;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Azure.Documents;

namespace EquipmentFunction
{
    public static class GetAllEquipment
    {
        [FunctionName("GetAllEquipment")]
        public static async Task<IEnumerable<Equipment>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Get")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function geting all equipment.");

            IDocumentDbRepository<Equipment> Repository = new DocumentDbRepository<Equipment>();
            var collectionId = Environment.GetEnvironmentVariable("EquipmentCollectionId");
            return await Repository.GetItemsAsync(collectionId);
        }
    }

    public static class GetEquipmentBySessionType
    {
        [FunctionName("GetEquipmentBySessionType")]
        public static async Task<IEnumerable<Equipment>> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetEquipmentBySessionType/{sessionType}")] HttpRequest req,
            ILogger log, string sessionType)
        {
            log.LogInformation("Getting equipment by session type");

            var repo = new DocumentDbRepository<Equipment>();
            var collectionId = Environment.GetEnvironmentVariable("EquipmentCollectionId");

            var results = await repo.GetItemsAsync(e => e.SessionTypes.Any(t => t.Name == sessionType), collectionId);
            return results;
        }
    }

    public static class GetSessionTypes
    {
        [FunctionName("GetSessionTypes")]
        public static async Task<IEnumerable<SessionType>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetSessionTypes")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function geting all equipment.");

            IDocumentDbRepository<SessionType> Repository = new DocumentDbRepository<SessionType>();
            var collectionId = Environment.GetEnvironmentVariable("SessionTypeCollectionId");
            return await Repository.GetItemsAsync(collectionId);
        }
    }

    public static class CreateOrUpdateSessionType
    {
        [FunctionName("CreateOrUpdateSessionType")]
        public static async Task<SessionType> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "CreateOrUpdateSessionType")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# trigger function to create a session type into cosmos.");
            try
            {
                IDocumentDbRepository<SessionType> Repository = new DocumentDbRepository<SessionType>();
                var document = new Document();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var sessionType = JsonConvert.DeserializeObject<SessionType>(requestBody);
                var collectionId = Environment.GetEnvironmentVariable("SessionTypeCollectionId");
                if (req.Method == "POST")
                {
                    sessionType.Id = null;
                    document = await Repository.CreateItemAsync(sessionType, collectionId);
                }
                else
                {
                    document = await Repository.UpdateItemAsync(sessionType.Id, sessionType, collectionId);
                }
                return new SessionType { Id = document.Id, Name = sessionType.Name };
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return new SessionType();
            }
        }
    }

    public static class DeleteSessionType
    {
        [FunctionName("DeleteSessionType")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteSessionType/{sessionTypeId}/{sessionType}")] HttpRequest req, ILogger log, string sessionTypeId, string sessionType)
        {
            log.LogInformation("C# HTTP delete type from cosmos.");
            try
            {
                var collectionId = Environment.GetEnvironmentVariable("SessionTypeCollectionId");
                //todo: filter out of equipment
                IDocumentDbRepository<SessionType> Repository = new DocumentDbRepository<SessionType>();
                await Repository.DeleteItemAsync(sessionTypeId, collectionId, sessionType);
                return true;
            }
            catch (Exception e)
            {
                log.LogInformation("Error deleting session type.", e);
                return false;
            }
        }
    }

    public static class CreateOrUpdateEquipment
    {
        [FunctionName("CreateOrUpdateEquipment")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "CreateOrUpdate")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# trigger function to create an equipment record into cosmos.");
            try
            {
                IDocumentDbRepository<Equipment> Repository = new DocumentDbRepository<Equipment>();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var Equipment = JsonConvert.DeserializeObject<Equipment>(requestBody);
                var collectionId = Environment.GetEnvironmentVariable("EquipmentCollectionId");
                if (req.Method == "POST")
                {
                    Equipment.Id = null;
                    await Repository.CreateItemAsync(Equipment, collectionId);
                }
                else
                {
                    await Repository.UpdateItemAsync(Equipment.Id, Equipment, collectionId);
                }
                return true;
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return false;
            }
        }
    }

    //TODO: Move to seperate user function
    public static class GetUser
    {
        [FunctionName("GetUser")]
        public static async Task<Core.Services.Data.User> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetUser")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function geting users.");

            IDocumentDbRepository<Core.Services.Data.User> Repository = new DocumentDbRepository<Core.Services.Data.User>();
            var collectionId = Environment.GetEnvironmentVariable("UserCollectionId");
            return (await Repository.GetItemsAsync(collectionId)).First();
        }
    }

    //TODO: Move to seperate user function
    public static class CreateOrUpdateEnvironmentSettings
    {
        //TODO: dependent on userId
        [FunctionName("CreateOrUpdateEnvironmentSettings")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "CreateOrUpdateEnvironmentSettings")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function creating users.");
            try
            {
                //**TODO: FIX TEMP Workaround
                IDocumentDbRepository<Core.Services.Data.User> Repository = new DocumentDbRepository<Core.Services.Data.User>();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var user = JsonConvert.DeserializeObject<Core.Services.Data.User>(requestBody);
                var collectionId = Environment.GetEnvironmentVariable("UserCollectionId");
                if (req.Method == "POST")
                {
                    user.Id = null;
                    await Repository.CreateItemAsync(user, collectionId);
                }
                else
                {
                    await Repository.UpdateItemAsync(user.Id, user, collectionId);
                }
                return true;
            }
            catch
            {
                log.LogError("Error occured while creating a record in Cosmos Db");
                return false;
            }
        }
    }
}
