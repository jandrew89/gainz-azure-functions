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
}
