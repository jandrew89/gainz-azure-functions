using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Core.Services.Services;
using Core.Services.Data;
using Core.Services.Interfaces;

namespace CityFunction
{
    public static class GetAll
    {
        [FunctionName("GetAll")]
        public static async Task<IEnumerable<Employee>> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Get")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            IDocumentDbRepository<Employee> Repository = new DocumentDbRepository<Employee>();
            return await Repository.GetItemsAsync("Employee");
        }
    }

    public static class GetSingle
    {
        [FunctionName("GetSingle")]
        public static async Task<Employee> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Get/{id}/{cityName}")] HttpRequest req, ILogger log, string id, string cityName)
        {
            log.LogInformation("C# HTTP log single data from cosmos.");

            IDocumentDbRepository<Employee> Repository = new DocumentDbRepository<Employee>();
            var employees = await Repository.GetItemsAsync(d => d.Id == id && d.Cityname == cityName, "Employee");
            var employee = new Employee();
            foreach (var emp in employees)
            {
                employee = emp;
            }
            return employee;
        }
    }

    public static class CreateOrUpdate
    {
        [FunctionName("CreateOrUpdate")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "CreateOrUpdate")] HttpRequest req, ILogger log)
        {
            log.LogInformation("C# trigger function to create a record into cosmos.");
            try
            {
                IDocumentDbRepository<Employee> Repository = new DocumentDbRepository<Employee>();
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var employee = JsonConvert.DeserializeObject<Employee>(requestBody);
                if (req.Method == "POST")
                {
                    employee.Id = null;
                    await Repository.CreateItemAsync(employee, "Employee");
                }
                else
                {
                    await Repository.UpdateItemAsync(employee.Id, employee, "Employee");
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

    public static class Delete
    {
        [FunctionName("Delete")]
        public static async Task<bool> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Delete/{id}/{cityName}")] HttpRequest req, ILogger log, string id, string cityName)
        {
            log.LogInformation("C# HTTP delete data from cosmos.");

            try
            {
                IDocumentDbRepository<Employee> Repository = new DocumentDbRepository<Employee>();
                await Repository.DeleteItemAsync(id, "Employee", cityName);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
