﻿using Microsoft.Azure.Documents;
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
        Task<IEnumerable<T>> GetItemsAsync<TKey>(string collectionId, int itemCount, Expression<Func<T, TKey>> orderBy);
        Task<Document> UpdateItemAsync(string id, T item, string collectionId);
        TKEY[] GetItemsBySqlQuery<TKEY>(SqlQuerySpec sqlSpec, string collectionId);
    }
}