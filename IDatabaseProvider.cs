using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sakont.MongoCRUD
{
    public interface IDatabaseProvider
    {
        /// <summary>
        /// Drop a collection from database
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        void DropCollection(string collection);

        /// <summary>
        /// Async Drop a collection from database
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        Task DropCollectionAsync(string collection);

        //-----------------------------------------------------//
        /// <summary>
        /// Watch a collection for any change
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <returns>return true if something changed</returns>
        bool WatchCollection<T>(string collectionName);

        /// <summary>
        /// Async Watch a collection for any change
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <returns>return true if something changed</returns>
        Task<bool> WatchCollectionAsync<T>(string collectionName);

        //-----------------------------------------------------//
        /// <summary>
        /// Create a new collection in database
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        void CreateCollection(string collection);

        /// <summary>
        /// Async Create a new collection in database
        /// </summary>
        /// <param name="collection">collection name</param>
        /// <returns></returns>
        Task CreateCollectionAsync(string collection);

        //-----------------------------------------------------//

        /// <summary>
        /// Load documents of generic type T from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <returns>return documents of type T</returns>
        List<T> LoadRecords<T>(string collection);

        /// <summary>
        /// Async Load documents of generic type T from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <returns>return documents of type T</returns>
        Task<List<T>> LoadRecordsAsync<T>(string collection);

        //-----------------------------------------------------//
        /// <summary>
        /// Load documents of generic type T that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        /// <returns>return documents of type T</returns>
        T LoadRecordWhere<T>(string collectionName, Expression<Func<T, bool>> filter);

        /// <summary>
        /// Async Load documents of generic type T that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        /// <returns>return documents of type T</returns>
        Task<T> LoadRecordWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter);
        //-----------------------------------------------------//
        /// <summary>
        /// Load documents of generic type T that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        /// <returns>return documents of type T</returns>
        List<T> LoadRecordsWhere<T>(string collectionName, Expression<Func<T, bool>> filter);

        /// <summary>
        /// Async Load documents of generic type T that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        /// <returns>return documents of type T</returns>
        Task<List<T>> LoadRecordsWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter);

        //-----------------------------------------------------//
        /// <summary>
        /// Insert many documents in bulk to a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="records">documents to be inserted</param>
        void InsertRecords<T>(string collection, IEnumerable<T> records);

        /// <summary>
        /// Async Insert many documents in bulk to a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="records">documents to be inserted</param>
        Task InsertRecordsAsync<T>(string collection, IEnumerable<T> records);

        //-----------------------------------------------------//
        /// <summary>
        /// Insert one document to a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="record">record to be inserted</param>
        void InsertRecord<T>(string collection, T record);

        /// <summary>
        /// Async Insert one document to a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="record">record to be inserted</param>
        Task InsertRecordAsync<T>(string collection, T record);

        //-----------------------------------------------------//
        /// <summary>
        /// Increment a field value from a document
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be modified</param>
        /// <param name="incrementField">increment expression</param>
        /// <param name="incrementValue">increment value</param>
        void IncrementRecord<T, TField>(string collection, Guid id, Expression<Func<T, TField>> incrementField, TField incrementValue);

        /// <summary>
        /// Async Increment a field value from a document
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be modified</param>
        /// <param name="incrementField">increment expression</param>
        /// <param name="incrementValue">increment value</param>
        Task IncrementRecordAsync<T, TField>(string collection, Guid id, Expression<Func<T, TField>> incrementField, TField incrementValue);

        /// <summary>
        /// Increment a field value from a document
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be modified</param>
        /// <param name="incrementField">field expression</param>
        /// <param name="incrementValue">increment value</param>
        void IncrementRecord<T, TDocumentID, TField>(string collection, TDocumentID id, Expression<Func<T, TField>> incrementField, TField incrementValue);

        /// <summary>
        /// Async Increment a field value from a document
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be modified</param>
        /// <param name="incrementField">field expression</param>
        /// <param name="incrementValue">increment value</param>
        Task IncrementRecordAsync<T, TDocumentID, TField>(string collection, TDocumentID id, Expression<Func<T, TField>> incrementField, TField incrementValue);

        //-----------------------------------------------------//
        /// <summary>
        /// Load one document from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be loaded</param>
        T LoadRecordById<T>(string collection, Guid id);

        /// <summary>
        /// Async Load one document from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be loaded</param>
        /// <returns>document of type T</returns>
        Task<T> LoadRecordByIdAsync<T>(string collection, Guid id);

        //-----------------------------------------------------//
        /// <summary>
        /// Load one document from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be loaded</param>
        /// <returns>document of type T</returns>
        T LoadRecordById<T, TDocumentID>(string collection, TDocumentID id);

        /// <summary>
        /// Async Load one document from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be loaded</param>
        /// <returns>document of type T</returns>
        Task<T> LoadRecordByIdAsync<T, TDocumentID>(string collection, TDocumentID id);

        //-----------------------------------------------------//
        /// <summary>
        /// Replace one document from a collection, if document doesn't exist creates a new one atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be replaced</param>
        /// <param name="record">updated record</param>
        void UpsertRecord<T>(string collection, Guid id, T record);

        /// <summary>
        /// Async Replace one document from a collection, if document doesn't exist creates a new one atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be replaced</param>
        /// <param name="record">updated record</param>
        Task UpsertRecordAsync<T>(string collection, Guid id, T record);

        /// <summary>
        /// Replace one document from a collection, if document doesn't exist creates a new one atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be replaced</param>
        /// <param name="record">updated record</param>
        void UpsertRecord<T, TDocumentID>(string collection, TDocumentID id, T record);

        /// <summary>
        /// Async Replace one document from a collection, if document doesn't exist creates a new one atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be replaced</param>
        /// <param name="record">updated record</param>
        Task UpsertRecordAsync<T, TDocumentID>(string collection, TDocumentID id, T record);

        //-----------------------------------------------------//
        /// <summary>
        /// Update a field value inside a document atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="id">UUID/ID of document to be updated</param>
        /// <param name="field">field expression</param>
        /// <param name="fieldValue">field value</param>
        void UpdateRecord<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> field, TField fieldValue);

        /// <summary>
        /// Async Update a field value inside a document atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="id">UUID/ID of document to be updated</param>
        /// <param name="field">field expression</param>
        /// <param name="fieldValue">field value</param>
        Task UpdateRecordAsync<T, TField>(string collectionName, Guid id, Expression<Func<T, TField>> field, TField fieldValue);

        /// <summary>
        /// Update a field value inside a document atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="id">UUID/ID of document to be updated</param>
        /// <param name="field">field expression</param>
        /// <param name="fieldValue">field value</param>
        void UpdateRecord<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> field, TField fieldValue);

        /// <summary>
        /// Async Update a field value inside a document atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <typeparam name="TField">field type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="id">UUID/ID of document to be updated</param>
        /// <param name="field">field expression</param>
        /// <param name="fieldValue">field value</param>
        Task UpdateRecordAsync<T, TDocumentID, TField>(string collectionName, TDocumentID id, Expression<Func<T, TField>> field, TField fieldValue);

        //-----------------------------------------------------//
        /// <summary>
        /// Delete one document from a collection atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be deleted</param>
        void DeleteRecord<T>(string collection, Guid id);

        /// <summary>
        /// Async Delete one document from a collection atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be deleted</param>
        Task DeleteRecordAsync<T>(string collection, Guid id);

        /// <summary>
        /// Delete one document from a collection atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be deleted</param>
        void DeleteRecord<T, TDocumentID>(string collection, TDocumentID id);

        /// <summary>
        /// Async Delete one document from a collection atomically
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collection">collection name</param>
        /// <param name="id">UUID/ID of document to be deleted</param>
        Task DeleteRecordAsync<T, TDocumentID>(string collection, TDocumentID id);

        //-----------------------------------------------------//
        /// <summary>
        /// Delete many documents from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="ids">UUID/ID list of documents to be deleted</param>
        void DeleteRecords<T>(string collectionName, IEnumerable<Guid> ids);

        /// <summary>
        /// Async Delete many documents from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="ids">UUID/ID list of documents to be deleted</param>
        Task DeleteRecordsAsync<T>(string collectionName, IEnumerable<Guid> ids);

        /// <summary>
        /// Delete many documents from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="ids">UUID/ID list of documents to be deleted</param>
        void DeleteRecords<T, TDocumentID>(string collectionName, IEnumerable<TDocumentID> ids);

        /// <summary>
        /// Async Delete many documents from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <typeparam name="TDocumentID">document id type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="ids">UUID/ID list of documents to be deleted</param>
        Task DeleteRecordsAsync<T, TDocumentID>(string collectionName, IEnumerable<TDocumentID> ids);

        //-----------------------------------------------------//
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="ids"></param>
        /// <param name="records"></param>
        void UpsertRecords<T>(string collectionName, IEnumerable<Guid> ids, IEnumerable<T> records);

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collectionName"></param>
        /// <param name="ids"></param>
        /// <param name="records"></param>
        /// <returns></returns>
        Task UpsertRecordsAsync<T>(string collectionName, IEnumerable<Guid> ids, IEnumerable<T> records);

        //-----------------------------------------------------//
        /// <summary>
        /// Delete one document that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        void DeleteRecordWhere<T>(string collectionName, Expression<Func<T, bool>> filter);

        /// <summary>
        /// Async Delete one document that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        Task DeleteRecordWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter);

        //-----------------------------------------------------//
        /// <summary>
        /// Delete many documents that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        void DeleteRecordsWhere<T>(string collectionName, Expression<Func<T, bool>> filter);

        /// <summary>
        /// Async Delete many documents that satisfy a filter condition, from a collection
        /// </summary>
        /// <typeparam name="T">document type</typeparam>
        /// <param name="collectionName">collection name</param>
        /// <param name="filter">filter query</param>
        Task DeleteRecordsWhereAsync<T>(string collectionName, Expression<Func<T, bool>> filter);
    }
}