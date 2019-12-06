using System;
using System.Collections.Generic;
using Core.Services.Data;
using Core.Services.Services;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GainzChangeFeed
{
    public static class SessionFeedTrigger
    {
        [FunctionName("SessionFeedTrigger")]
        public static async System.Threading.Tasks.Task RunAsync([CosmosDBTrigger(
            databaseName: "SouthCosmosDb",
            collectionName: "Sessions",
            ConnectionStringSetting = "CosmosDBConnection",
            LeaseCollectionName = "leases")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                //Add session to User         
                log.LogInformation("Documents modified " + input.Count);
                log.LogInformation("First document Id " + input[0].Id);

                var sessionRepo = new DocumentDbRepository<Session>();
                var emplRepo = new DocumentDbRepository<Employee>();

                foreach (var doc in input)
                {
                    var partitionKey = doc.GetPropertyValue<string>("SessionType");
                    var tonyDemoUser = new Employee();
                    var session = new Session();

                    // Get our user
                    var users = await emplRepo.GetItemsAsync(t => t.Name.Contains("Tony") && t.Cityname == "Chandler", "Employee");
                    foreach (var user in users)
                    {
                        tonyDemoUser = user;
                    }

                    // Get the sessions
                    var sessions = await sessionRepo.GetItemsAsync(d => d.Id == doc.Id && d.SessionType == partitionKey, "Sessions");
                    foreach (var ses in sessions)
                    {
                        session = ses;
                    }

                    if (tonyDemoUser.Sessions == null)
                        tonyDemoUser.Sessions = new List<UserSession>();
                    //validate the users doesnt have session
                    if (!tonyDemoUser.Sessions.Exists(s => s.Id == session.Id))
                    {
                        //add session
                        tonyDemoUser.Sessions.Add(new UserSession {  Id = session.Id, SessionType = session.SessionType });
                        await emplRepo.UpdateItemAsync(tonyDemoUser.Id, tonyDemoUser, "Employee");
                    }
                }
            }
        }
    }
}
