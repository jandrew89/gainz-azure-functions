using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Core.Services.Interfaces
{
    public interface IDocumentDbRepository<T> where T : class
    {
        Task<Document> CreateItemAsync(T item, string collectionId);
        Task DeleteItemAsync(string id, string collectionId, string partitionKey);
        Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, string collectionId);
        Task<IEnumerable<T>> GetItemsAsync(string collectionId);
        Task<Document> UpdateItemAsync(string id, T item, string collectionId);
        Task<dynamic[]> GetItemsBySqlQuery(SqlQuerySpec sqlSpec, string collectionId);
    }
}