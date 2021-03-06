﻿using Core.Services.Interfaces;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Services
{
    public class DocumentDbRepository<T> : IDocumentDbRepository<T> where T : class
    {
        private readonly string Endpoint = Environment.GetEnvironmentVariable("DocumentEndpoint");
        private readonly string Key = Environment.GetEnvironmentVariable("DocumentKey");
        private readonly string databaseId = Environment.GetEnvironmentVariable("DocumentDatabaseId");
        private DocumentClient client;
        public DocumentDbRepository()
        {
            client = new DocumentClient(new Uri(Endpoint), Key);
        }

        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate, string collectionId)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                    .Where(predicate)
                    .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        public TKEY[] GetItemsBySqlQuery<TKEY>(SqlQuerySpec sqlSpec, string collectionId)
        {
            return client.CreateDocumentQuery<TKEY>(
                    UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), sqlSpec,
                    new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true }).ToArray();
        }

        public async Task<IEnumerable<T>> GetItemsAsync(string collectionId)
        {
            IDocumentQuery<T> query = client.CreateDocumentQuery<T>(
                    UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                    new FeedOptions { MaxItemCount = -1 })
                    .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }

        public async Task<IEnumerable<T>> GetItemsAsync<TKey>(string collectionId, int itemCount, Expression<Func<T, TKey>> orderBy)
        {
            var query = client.CreateDocumentQuery<T>(
                        UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
                        new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                        .OrderByDescending(orderBy)
                        .Take(itemCount)
                        .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }
        public async Task<Document> CreateItemAsync(T item, string collectionId)
        {
            return await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);
        }

        public async Task<Document> UpdateItemAsync(string id, T item, string collectionId)
        {
            return await client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);
        }

        public async Task DeleteItemAsync(string id, string collectionId, string partitionKey)
        {
            await client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id),
                new RequestOptions() { PartitionKey = new PartitionKey(partitionKey) });
        }

        private async Task CreateDatabaseIfNotExistAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDatabaseAsync(new Database { Id = databaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync(string collectionId)
        {
            try
            {
                await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await client.CreateDocumentCollectionAsync(UriFactory.CreateDatabaseUri(databaseId),
                        new DocumentCollection { Id = collectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
