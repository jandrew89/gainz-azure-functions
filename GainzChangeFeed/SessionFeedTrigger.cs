using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
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
                //TODO: Added update user session funcationality
            }
        }
    }
}
