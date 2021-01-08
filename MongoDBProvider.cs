using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sakont.MongoCRUD
{
    public class MongoDBProvider
    {
        private readonly IMongoDatabase db;
        private readonly IMongoClient client;
        private readonly IClientSessionHandle sessionHandle;

        public MongoDBProvider(string database, string replicaSetName, string host, int port, string username, string password, string clientApplicationName = null)
        {
            string mongoDbAuthMechanism = "SCRAM-SHA-1";
            MongoInternalIdentity internalIdentity = new MongoInternalIdentity(database, username);
            PasswordEvidence passwordEvidence = new PasswordEvidence(password);
            MongoCredential mongoCredential = new MongoCredential(mongoDbAuthMechanism, internalIdentity, passwordEvidence);
            var settings = new MongoClientSettings
            {
                ReplicaSetName = replicaSetName,
                Server = new MongoServerAddress(host, port),
                ApplicationName = clientApplicationName,
                Credential = mongoCredential,
                UseTls = false,
                Compressors = new List<CompressorConfiguration>()
                {
                    new CompressorConfiguration(CompressorType.Snappy)
                }
            };

            client = new MongoClient(settings);

            db = client.GetDatabase(database);
            sessionHandle = client.StartSession();
        }

        public MongoDBProvider(string database, string replicaSetName, string host, int port, string clientApplicationName = null)
        {
            var settings = new MongoClientSettings
            {
                ReplicaSetName = replicaSetName,
                Server = new MongoServerAddress(host, port),
                ApplicationName = clientApplicationName,
                UseTls = false,
                Compressors = new List<CompressorConfiguration>()
                {
                    new CompressorConfiguration(CompressorType.Snappy)
                }
            };

            client = new MongoClient(settings);

            db = client.GetDatabase(database);
            sessionHandle = client.StartSession();
        }

        //-----------------------------------------------------//
        public void StartTransaction()
        {
            sessionHandle.StartTransaction();
        }

        public async Task CommitTransactionAsync()
        {
            await sessionHandle.CommitTransactionAsync();
        }

        public async Task AbortTransactionAsync()
        {
            if (sessionHandle.IsInTransaction)
            {
                await sessionHandle.AbortTransactionAsync();
            }
        }

        //-----------------------------------------------------//
        public IMongoDatabase GetDatabase()
        {
            return db;
        }

        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return db.GetCollection<T>(collectionName);
        }

        //-----------------------------------------------------//

        #region Create View

        public void CreateView<TLocal, TForeign, TOutput>(string localCollection, string foreignCollection, string viewName, Expression<Func<TLocal, object>> field, Expression<Func<TForeign, object>> foreignField, Expression<Func<TOutput, object>> outputField)
        {
            var foreign = db.GetCollection<TForeign>(foreignCollection);
            //var projection = new ProjectionDefinitionBuilder().M
            var pipeline = PipelineDefinitionBuilder.Lookup(new EmptyPipelineDefinition<TLocal>(), foreign, field, foreignField, outputField);

            try
            {
                db.CreateView(sessionHandle, viewName, localCollection, pipeline);
            }
            catch
            {
            }
        }

        public async Task CreateViewAsync<TLocal, TForeign, TOutput>(string localCollection, string foreignCollection, string viewName, Expression<Func<TLocal, object>> field, Expression<Func<TForeign, object>> foreignField, Expression<Func<TOutput, object>> outputField)
        {
            var foreign = db.GetCollection<TForeign>(foreignCollection);
            var pipeline = PipelineDefinitionBuilder.Lookup(new EmptyPipelineDefinition<TLocal>(), foreign, field, foreignField, outputField);

            try
            {
                await db.CreateViewAsync(sessionHandle, viewName, localCollection, pipeline);
            }
            catch
            {
            }
        }

        #endregion Create View

        //-----------------------------------------------------//

        #region Watch One Collection for any kind of a change

        public bool WatchDatabase()
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match(x =>
                x.OperationType == ChangeStreamOperationType.Delete ||
                x.OperationType == ChangeStreamOperationType.Insert ||
                x.OperationType == ChangeStreamOperationType.Update ||
                x.OperationType == ChangeStreamOperationType.Replace

            );
            var change = db.Watch(pipeline).ToEnumerable().GetEnumerator();
            return change.MoveNext();
        }

        public async Task<bool> WatchDatabaseAsync()
        {
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match(x =>
                x.OperationType == ChangeStreamOperationType.Delete ||
                x.OperationType == ChangeStreamOperationType.Insert ||
                x.OperationType == ChangeStreamOperationType.Update ||
                x.OperationType == ChangeStreamOperationType.Replace

            );
            var change = await db.WatchAsync(pipeline);
            var enumerator = change.ToEnumerable().GetEnumerator();
            return enumerator.MoveNext();
        }

        public bool WatchCollection<T>(string collectionName)
        {
            var collection = db.GetCollection<T>(collectionName);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>().Match(x =>
                x.OperationType == ChangeStreamOperationType.Delete ||
                x.OperationType == ChangeStreamOperationType.Insert ||
                x.OperationType == ChangeStreamOperationType.Update ||
                x.OperationType == ChangeStreamOperationType.Replace

            );

            var changeQueueStream = collection.Watch(sessionHandle, pipeline).ToEnumerable().GetEnumerator();
            return changeQueueStream.MoveNext();
        }

        public async Task<bool> WatchCollectionAsync<T>(string collectionName)
        {
            var collection = db.GetCollection<T>(collectionName);
            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>().Match(x =>
                x.OperationType == ChangeStreamOperationType.Delete ||
                x.OperationType == ChangeStreamOperationType.Insert ||
                x.OperationType == ChangeStreamOperationType.Update ||
                x.OperationType == ChangeStreamOperationType.Replace
            );

            var changeQueueStream = await collection.WatchAsync(sessionHandle, pipeline);
            var enumerator = changeQueueStream.ToEnumerable().GetEnumerator();

            return enumerator.MoveNext();
        }

        #endregion Watch One Collection for any kind of a change

        //-----------------------------------------------------//

        #region Watch One Collection for a change

        public bool WatchCollection<T>(string collectionName, Expression<Func<ChangeStreamDocument<T>, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<ChangeStreamDocument<T>>.Filter.Where(filter);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>().Match(filterDefinition);

            var changeQueueStream = collection.Watch(pipeline).ToEnumerable().GetEnumerator();
            return changeQueueStream.MoveNext();
        }

        public async Task<bool> WatchCollectionAsync<T>(string collectionName, Expression<Func<ChangeStreamDocument<T>, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<ChangeStreamDocument<T>>.Filter.Where(filter);

            var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<T>>().Match(filterDefinition);

            var changeQueueStream = await collection.WatchAsync(pipeline);

            return changeQueueStream.ToEnumerable().GetEnumerator().MoveNext();
        }

        #endregion Watch One Collection for a change

        //-----------------------------------------------------//

        #region List Collections

        public async Task<List<string>> ListCollectionsAsync()
        {
            return await db.ListCollectionNames().ToListAsync();
        }

        #endregion List Collections

        //-----------------------------------------------------//

        #region Drop Collection

        public void DropCollection(string collection)
        {
            db.DropCollection(sessionHandle, collection);
        }

        public async Task DropCollectionAsync(string collection)
        {
            await db.DropCollectionAsync(sessionHandle, collection);
        }

        #endregion Drop Collection



        //-----------------------------------------------------//

        #region Create One Collection

        public void CreateCollection(string collection)
        {
            db.CreateCollection(sessionHandle, collection);
        }

        public async Task CreateCollectionAsync(string collection)
        {
            await db.CreateCollectionAsync(sessionHandle, collection);
        }

        #endregion Create One Collection

        //-----------------------------------------------------//

        #region Insert Many Documents

        public void InsertRecords<T>(string collectionName, IEnumerable<T> records)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.InsertMany(sessionHandle, records);
        }

        public async Task InsertRecordsAsync<T>(string collectionName, IEnumerable<T> records)
        {
            var collection = db.GetCollection<T>(collectionName);

            await collection.InsertManyAsync(sessionHandle, records);
        }

        #endregion Insert Many Documents

        //-----------------------------------------------------//

        #region Insert One Document

        public void InsertRecord<T>(string collectionName, T record)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.InsertOne(sessionHandle, record);
        }

        public async Task InsertRecordAsync<T>(string collectionName, T record)
        {
            var collection = db.GetCollection<T>(collectionName);
            await collection.InsertOneAsync(sessionHandle, record);
        }

        #endregion Insert One Document

        //-----------------------------------------------------//

        public IMongoQueryable<T> QueryRecords<T>(string collectionName, AggregateOptions aggregateOptions = null)
        {
            var collection = db.GetCollection<T>(collectionName);
            return collection.AsQueryable(aggregateOptions);
        }

        public async Task<IMongoQueryable<T>> QueryRecordsAsync<T>(string collectionName, AggregateOptions aggregateOptions = null)
        {
            return await Task.Run<IMongoQueryable<T>>(() =>
           {
               var collection = db.GetCollection<T>(collectionName);
               return collection.AsQueryable(aggregateOptions);
           });
        }

        public IMongoQueryable<T> QueryRecordsWhere<T>(string collectionName, Expression<Func<T, bool>> filter, AggregateOptions aggregateOptions = null)
        {
            var collection = db.GetCollection<T>(collectionName);
            return collection.AsQueryable(aggregateOptions).Where(filter);
        }

        public async Task<IMongoQueryable<T>> QueryRecordsWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter, AggregateOptions aggregateOptions = null)
        {
            return await Task.Run<IMongoQueryable<T>>(() =>
            {
                var collection = db.GetCollection<T>(collectionName);
                return collection.AsQueryable(aggregateOptions).Where(filter);
            });
        }

        #region Load Many Documents

        public List<T> LoadRecords<T>(string collectionName)
        {
            var collection = db.GetCollection<T>(collectionName);
            return collection.Find(new BsonDocument()).ToList();
        }

        public async Task<List<T>> LoadRecordsAsync<T>(string collectionName)
        {
            var collection = db.GetCollection<T>(collectionName);
            return await collection.Find(new BsonDocument()).ToListAsync();
        }

        #endregion Load Many Documents

        //-----------------------------------------------------//

        #region Load Many Documents by Ids

        public List<T> LoadRecords<T>(string collectionName, IEnumerable<Guid> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            var idList = ids.AsParallel().Select(x => new BsonBinaryData(x, GuidRepresentation.Standard)).ToList();

            var filter = Builders<T>.Filter.AnyEq("_id", idList);

            return collection.Find(filter).ToList();
        }

        public async Task<List<T>> LoadRecordsAsync<T>(string collectionName, IEnumerable<Guid> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            //var idList = ids.AsParallel().Select(x => new BsonBinaryData(x.ToByteArray(),BsonBinarySubType.Binary)).ToList();

            var filter = Builders<T>.Filter.AnyEq("Id", ids.ToList());

            return await collection.FindAsync(filter).GetAwaiter().GetResult().ToListAsync();
        }

        #endregion Load Many Documents by Ids

        //-----------------------------------------------------//

        #region Load Many Documents in Batch

        public IAsyncCursor<T> LoadRecords<T>(string collectionName, int batchSize)
        {
            var collection = db.GetCollection<T>(collectionName);

            return collection.Find(new BsonDocument(),
            new FindOptions()
            {
                BatchSize = batchSize
            }
            ).ToCursor();
        }

        public async Task<IAsyncCursor<T>> LoadRecordsAsync<T>(string collectionName, int batchSize)
        {
            var collection = db.GetCollection<T>(collectionName);

            return await collection.FindAsync(new BsonDocument(),
            new FindOptions<T>()
            {
                BatchSize = batchSize
            }
            );
        }

        #endregion Load Many Documents in Batch

        //-----------------------------------------------------//

        #region Load a Document with a filter

        public T LoadRecordWhere<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<T>.Filter.Where(filter);

            return collection.Find<T>(filterDefinition).FirstOrDefault();
        }

        public async Task<T> LoadRecordWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<T>.Filter.Where(filter);

            return await collection.FindAsync<T>(filterDefinition).Result.FirstOrDefaultAsync();
        }

        #endregion Load a Document with a filter

        //-----------------------------------------------------//

        #region Load Many Documents with a filter

        public List<T> LoadRecordsWhere<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<T>.Filter.Where(filter);

            return collection.FindAsync(filterDefinition).Result.ToList();
        }

        public async Task<List<T>> LoadRecordsWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filterDefinition = Builders<T>.Filter.Where(filter);

            return await collection.FindAsync<T>(filterDefinition).Result.ToListAsync();
        }

        #endregion Load Many Documents with a filter

        //-----------------------------------------------------//

        #region Increment a field value in One Document

        public void IncrementRecord<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> incrementField, TField incrementValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Inc(incrementField, incrementValue);

            collection.FindOneAndUpdate(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public async Task IncrementRecordAsync<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> incrementField, TField incrementValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Inc(incrementField, incrementValue);

            var result = await collection.FindOneAndUpdateAsync(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public void IncrementRecord<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> incrementField, TField incrementValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Inc(incrementField, incrementValue);

            collection.FindOneAndUpdate(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public async Task IncrementRecordAsync<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> incrementField, TField incrementValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Inc(incrementField, incrementValue);

            var result = await collection.FindOneAndUpdateAsync(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        #endregion Increment a field value in One Document

        //-----------------------------------------------------//

        #region Load One Document

        public T LoadRecordById<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return collection.Find(filter).FirstOrDefault();
        }

        public async Task<T> LoadRecordByIdAsync<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        public T LoadRecordById<T, TDocumentID>(string collectionName, TDocumentID id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return collection.Find(filter).FirstOrDefault();
        }

        public async Task<T> LoadRecordByIdAsync<T, TDocumentID>(string collectionName, TDocumentID id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            return await collection.Find(filter).FirstOrDefaultAsync();
        }

        #endregion Load One Document

        //-----------------------------------------------------//

        #region Upsert One Document

        public void UpsertRecord<T>(string collectionName, Guid id, T record)
        {
            var collection = db.GetCollection<T>(collectionName);

            BsonBinaryData bsonBinaryData = new BsonBinaryData(id, GuidRepresentation.Standard);

            collection.FindOneAndReplace<T>(sessionHandle,
                new BsonDocument("_id", bsonBinaryData),
                record,
                new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
        }

        public async Task UpsertRecordAsync<T>(string collectionName, Guid id, T record)
        {
            var collection = db.GetCollection<T>(collectionName);

            BsonBinaryData bsonBinaryData = new BsonBinaryData(id, GuidRepresentation.Standard);

            await collection.FindOneAndReplaceAsync<T>(sessionHandle,
                new BsonDocument("_id", bsonBinaryData),
                record,
                new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
        }

        public void UpsertRecord<T, TDocumentID>(string collectionName, TDocumentID id, T record)
        {
            var collection = db.GetCollection<T>(collectionName);

            BsonBinaryData bsonBinaryData = new BsonBinaryData(id.ToBson());

            collection.FindOneAndReplace<T>(sessionHandle,
                new BsonDocument("_id", bsonBinaryData),
                record,
                new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
        }

        public async Task UpsertRecordAsync<T, TDocumentID>(string collectionName, TDocumentID id, T record)
        {
            var collection = db.GetCollection<T>(collectionName);

            BsonBinaryData bsonBinaryData = new BsonBinaryData(id.ToBson());

            await collection.FindOneAndReplaceAsync<T>(sessionHandle,
                new BsonDocument("_id", bsonBinaryData),
                record,
                new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
        }

        #endregion Upsert One Document

        //-----------------------------------------------------//

        #region Update One Document

        public void UpdateRecord<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> field, TField fieldValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Set(field, fieldValue);

            collection.FindOneAndUpdate(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public async Task UpdateRecordAsync<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> field, TField fieldValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Set(field, fieldValue);

            await collection.FindOneAndUpdateAsync(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public void UpdateRecord<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> field, TField fieldValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Set(field, fieldValue);

            collection.FindOneAndUpdate(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        public async Task UpdateRecordAsync<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> field, TField fieldValue)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            var update = Builders<T>.Update.Set(field, fieldValue);

            await collection.FindOneAndUpdateAsync(sessionHandle, filter, update,
                new FindOneAndUpdateOptions<T>
                {
                    BypassDocumentValidation = false,
                    IsUpsert = true
                });
        }

        #endregion Update One Document

        //-----------------------------------------------------//

        #region Delete One Document

        public void DeleteRecord<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            collection.FindOneAndDelete(sessionHandle, filter);
        }

        public async Task DeleteRecordAsync<T>(string collectionName, Guid id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            await collection.FindOneAndDeleteAsync(sessionHandle, filter);
        }

        public void DeleteRecord<T, TDocumentID>(string collectionName, TDocumentID id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            collection.FindOneAndDelete(sessionHandle, filter);
        }

        public async Task DeleteRecordAsync<T, TDocumentID>(string collectionName, TDocumentID id)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);

            await collection.FindOneAndDeleteAsync(sessionHandle, filter);
        }

        #endregion Delete One Document

        //-----------------------------------------------------//

        #region Delete Many Documents

        public void DeleteRecords<T>(string collectionName, IEnumerable<Guid> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.AnyEq("Id", ids);
            collection.DeleteMany(sessionHandle, filter);
        }

        public async Task DeleteRecordsAsync<T>(string collectionName, IEnumerable<Guid> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.AnyEq("Id", ids);
            await collection.DeleteManyAsync(sessionHandle, filter);
        }

        public void DeleteRecords<T, TDocumentID>(string collectionName, IEnumerable<TDocumentID> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.AnyEq("Id", ids);

            collection.DeleteMany(filter);
        }

        public async Task DeleteRecordsAsync<T, TDocumentID>(string collectionName, IEnumerable<TDocumentID> ids)
        {
            var collection = db.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.AnyEq("Id", ids);

            await collection.DeleteManyAsync(sessionHandle, filter);
        }

        #endregion Delete Many Documents

        //-----------------------------------------------------//

        #region Upsert Many Records

        public void UpsertRecords<T>(string collectionName, IEnumerable<T> records, Expression<Func<T, Guid>> idFilter)
        {
            var collection = db.GetCollection<T>(collectionName);
            records.AsParallel().ForAll(record =>
            {
                var func = idFilter.Compile();
                var id = func(record);
                BsonBinaryData bsonBinaryData = new BsonBinaryData(id, GuidRepresentation.Standard);

                collection.FindOneAndReplace<T>(sessionHandle, new BsonDocument("_id", bsonBinaryData),
                   record,
                   new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
            });
        }

        public async Task UpsertRecordsAsync<T>(string collectionName, IEnumerable<T> records, Expression<Func<T, Guid>> idFilter)
        {
            var collection = db.GetCollection<T>(collectionName);

            //var filter = Builders<T>.Filter.AnyEq("Id", ids);
            await Task.Run(() =>
            {
                records.AsParallel().ForAll(async record =>
                {
                    var func = idFilter.Compile();
                    var id = func(record);
                    BsonBinaryData bsonBinaryData = new BsonBinaryData(id, GuidRepresentation.Standard);

                    await collection.FindOneAndReplaceAsync<T>(sessionHandle,
                       new BsonDocument("_id", bsonBinaryData),
                       record,
                       new FindOneAndReplaceOptions<T> { IsUpsert = true, BypassDocumentValidation = false });
                });
            });
        }

        #endregion Upsert Many Records

        //-----------------------------------------------------//

        #region Delete One Document with a filter

        public void DeleteRecordWhere<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.FindOneAndDelete(sessionHandle, filter);
        }

        public async Task DeleteRecordWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            await collection.FindOneAndDeleteAsync(sessionHandle, filter);
        }

        #endregion Delete One Document with a filter

        //-----------------------------------------------------//

        #region Delete Many Documents with a filter

        public void DeleteRecordsWhere<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            collection.DeleteMany(sessionHandle, filter);
        }

        public async Task DeleteRecordsWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            var collection = db.GetCollection<T>(collectionName);
            await collection.DeleteManyAsync(sessionHandle, filter);
        }

        #endregion Delete Many Documents with a filter
    }
}