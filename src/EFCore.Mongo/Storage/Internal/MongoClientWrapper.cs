// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
//using Microsoft.EntityFrameworkCore.Mongo.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Mongo.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
//using Microsoft.EntityFrameworkCore.Mongo.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore.Mongo.Query.Internal;
//using MongoDB.Driver.Core.Operations;


namespace Microsoft.EntityFrameworkCore.Mongo.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MongoClientWrapper : IMongoClientWrapper
{
    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    ////public static readonly JsonSerializer Serializer;

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    ////public static readonly string DefaultPartitionKey = "__partitionKey";

    private readonly ISingletonMongoClientWrapper _singletonWrapper;
    private readonly string _databaseName;
    private readonly IExecutionStrategy _executionStrategy;
    private readonly IDiagnosticsLogger<DbLoggerCategory.Database.Command> _commandLogger;
    private readonly bool? _enableContentResponseOnWrite;

    static MongoClientWrapper()
    {
        //Serializer = JsonSerializer.Create();
        //Serializer.Converters.Add(new ByteArrayConverter());
        //Serializer.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        //Serializer.DateParseHandling = DateParseHandling.None;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public MongoClientWrapper(
        ISingletonMongoClientWrapper singletonWrapper,
        IDbContextOptions dbContextOptions,
        IExecutionStrategy executionStrategy,
        IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger)
    {
        var options = dbContextOptions.FindExtension<MongoOptionsExtension>();

        _singletonWrapper = singletonWrapper;
        _databaseName = options!.DatabaseName;
        _executionStrategy = executionStrategy;
        _commandLogger = commandLogger;
        _enableContentResponseOnWrite = options.EnableContentResponseOnWrite;
    }

    private MongoClient Client
        => _singletonWrapper.Client;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CreateDatabaseIfNotExists()
        => true;
    //_executionStrategy.Execute(/*(throughput, this)*/this, CreateDatabaseIfNotExistsOnce, null);

    //private static bool CreateDatabaseIfNotExistsOnce(
    //    DbContext? context,
    //    (ThroughputProperties? Throughput, MongoClientWrapper Wrapper) parameters)
    //    => CreateDatabaseIfNotExistsOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateDatabaseIfNotExistsAsync(
        CancellationToken cancellationToken = default)
        => Task.FromResult(true);
        //_executionStrategy.ExecuteAsync(
        //    (throughput, this), CreateDatabaseIfNotExistsOnceAsync, null, cancellationToken);

    //private static async Task<bool> CreateDatabaseIfNotExistsOnceAsync(
    //    DbContext? _,
    //    (ThroughputProperties? Throughput, MongoClientWrapper Wrapper) parameters,
    //    CancellationToken cancellationToken = default)
    //{
    //    var (throughput, wrapper) = parameters;
    //    var response = await wrapper.Client.CreateDatabaseIfNotExistsAsync(
    //            wrapper._databaseId, throughput, cancellationToken: cancellationToken)
    //        .ConfigureAwait(false);

    //    return response.StatusCode == HttpStatusCode.Created;
    //}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool DeleteDatabase()
        => _executionStrategy.Execute(this, DeleteDatabaseOnce, null);

    private static bool DeleteDatabaseOnce(
        DbContext? context,
        MongoClientWrapper wrapper)
        => DeleteDatabaseOnceAsync(context, wrapper).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> DeleteDatabaseAsync(
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(this, DeleteDatabaseOnceAsync, null, cancellationToken);

    private static /*async*/ Task<bool> DeleteDatabaseOnceAsync(
        DbContext? _,
        MongoClientWrapper wrapper,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
        //using var response = await wrapper.Client.GetDatabase(wrapper._databaseId)
        //    .DeleteStreamAsync(cancellationToken: cancellationToken)
        //    .ConfigureAwait(false);
        //if (response.StatusCode == HttpStatusCode.NotFound)
        //{
        //    return false;
        //}

        //response.EnsureSuccessStatusCode();
        //return response.StatusCode == HttpStatusCode.NoContent;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CreateContainerIfNotExists(CollectionProperties properties)
        => true;
    //_executionStrategy.Execute((properties, this), CreateContainerIfNotExistsOnce, null);

    private static bool CreateContainerIfNotExistsOnce(
        DbContext context,
        (CollectionProperties Parameters, MongoClientWrapper Wrapper) parametersTuple)
        => true;
    //CreateContainerIfNotExistsOnceAsync(context, parametersTuple).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateContainerIfNotExistsAsync(
        CollectionProperties properties,
        CancellationToken cancellationToken = default)
        => Task.FromResult(true);
        //_executionStrategy.ExecuteAsync((properties, this), CreateContainerIfNotExistsOnceAsync, null, cancellationToken);

    //private static async Task<bool> CreateContainerIfNotExistsOnceAsync(
    //    DbContext _,
    //    (ContainerProperties Parameters, MongoClientWrapper Wrapper) parametersTuple,
    //    CancellationToken cancellationToken = default)
    //{
    //    var (parameters, wrapper) = parametersTuple;
    //    using var response = await wrapper.Client.GetDatabase(wrapper._databaseId).CreateContainerStreamAsync(
    //            new Azure.Cosmos.ContainerProperties(parameters.Id, "/" + parameters.PartitionKey)
    //            {
    //                PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2,
    //                DefaultTimeToLive = parameters.DefaultTimeToLive,
    //                AnalyticalStoreTimeToLiveInSeconds = parameters.AnalyticalStoreTimeToLiveInSeconds
    //            },
    //            parameters.Throughput,
    //            cancellationToken: cancellationToken)
    //        .ConfigureAwait(false);
    //    if (response.StatusCode == HttpStatusCode.Conflict)
    //    {
    //        return false;
    //    }

    //    response.EnsureSuccessStatusCode();
    //    return response.StatusCode == HttpStatusCode.Created;
    //}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool CreateItem(
        string containerId,
        BsonDocument document,
        IUpdateEntry entry)
        => _executionStrategy.Execute((containerId, document, entry, this), CreateItemOnce, null);

    private static bool CreateItemOnce(
        DbContext context,
        (string CollectionName, BsonDocument Document, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters)
        => CreateItemOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> CreateItemAsync(
        string collectionName,
        BsonDocument document,
        IUpdateEntry updateEntry,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((collectionName, document, updateEntry, this), CreateItemOnceAsync, null, cancellationToken);

    private static async Task<bool> CreateItemOnceAsync(
        DbContext _,
        (string CollectionName, BsonDocument Document, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var client = parameters.Wrapper;
        var collection = client.Client.GetDatabase(client._databaseName).GetCollection<BsonDocument>(parameters.CollectionName);
        await collection.InsertOneAsync(parameters.Document, cancellationToken: cancellationToken).ConfigureAwait(false);
        return true;

        // __jObject?

        //var stream = new MemoryStream();
        //await using var __ = stream.ConfigureAwait(false);
        //var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
        //await using var ___ = writer.ConfigureAwait(false);

        //using var jsonWriter = new JsonTextWriter(writer);
        //Serializer.Serialize(jsonWriter, parameters.Document);
        //await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

        //var entry = parameters.Entry;
        //var wrapper = parameters.Wrapper;
        //var collection = wrapper.Client.GetDatabase(wrapper._databaseName).GetCollection<BsonDocument>(parameters.CollectionName);
        //var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        //var partitionKey = CreatePartitionKey(entry);

        //var response = await collection.CreateItemStreamAsync(
        //        stream,
        //        //partitionKey == null ? PartitionKey.None : new PartitionKey(partitionKey),
        //        itemRequestOptions,
        //        cancellationToken)
        //    .ConfigureAwait(false);

        //wrapper._commandLogger.ExecutedCreateItem(
        //    response.Diagnostics.GetClientElapsedTime(),
        //    response.Headers.RequestCharge,
        //    response.Headers.ActivityId,
        //    parameters.Document["id"].ToString(),
        //    parameters.ContainerId,
        //    partitionKey);

        //ProcessResponse(response, entry);

        //return response.StatusCode == HttpStatusCode.Created;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool ReplaceItem(
        string collectionId,
        string documentId,
        BsonDocument document,
        IUpdateEntry entry)
        => _executionStrategy.Execute((collectionId, documentId, document, entry, this), ReplaceItemOnce, null);

    private static bool ReplaceItemOnce(
        DbContext context,
        (string ContainerId, string ItemId, BsonDocument Document, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters)
        => ReplaceItemOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> ReplaceItemAsync(
        string collectionId,
        string documentId,
        BsonDocument document,
        IUpdateEntry updateEntry,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync(
            (collectionId, documentId, document, updateEntry, this), ReplaceItemOnceAsync, null, cancellationToken);

    private static Task<bool> ReplaceItemOnceAsync(
        DbContext _,
        (string CollectionName, string ResourceId, BsonDocument Document, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        //var stream = new MemoryStream();
        //await using var __ = stream.ConfigureAwait(false);
        //var writer = new StreamWriter(stream, new UTF8Encoding(), bufferSize: 1024, leaveOpen: false);
        //await using var ___ = writer.ConfigureAwait(false);
        //using var jsonWriter = new JsonTextWriter(writer);
        //Serializer.Serialize(jsonWriter, parameters.Document);
        //await jsonWriter.FlushAsync(cancellationToken).ConfigureAwait(false);

        //var entry = parameters.Entry;
        //var wrapper = parameters.Wrapper;
        //var container = wrapper.Client.GetDatabase(wrapper._databaseName).GetCollection<BsonDocument>(parameters.CollectionName);
        //var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        //var partitionKey = CreatePartitionKey(entry);

        //using var response = await container.ReplaceItemStreamAsync(
        //        stream,
        //        parameters.ResourceId,
        //        itemRequestOptions,
        //        cancellationToken)
        //    .ConfigureAwait(false);

        //wrapper._commandLogger.ExecutedReplaceItem(
        //    response.Diagnostics.GetClientElapsedTime(),
        //    response.Headers.RequestCharge,
        //    response.Headers.ActivityId,
        //    parameters.ResourceId,
        //    parameters.ContainerId,
        //    partitionKey);

        //ProcessResponse(response, entry);

        //return response.StatusCode == HttpStatusCode.OK;

        return Task.FromResult(true);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool DeleteItem(
        string containerId,
        string documentId,
        IUpdateEntry entry)
        => _executionStrategy.Execute((containerId, documentId, entry, this), DeleteItemOnce, null);

    private static bool DeleteItemOnce(
        DbContext context,
        (string ContainerId, string DocumentId, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters)
        => DeleteItemOnceAsync(context, parameters).GetAwaiter().GetResult();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Task<bool> DeleteItemAsync(
        string collectionName,
        string documentId,
        IUpdateEntry entry,
        CancellationToken cancellationToken = default)
        => _executionStrategy.ExecuteAsync((collectionName, documentId, entry, this), DeleteItemOnceAsync, null, cancellationToken);

    private static /*async*/ Task<bool> DeleteItemOnceAsync(
        DbContext? _,
        (string CollectionName, string ResourceId, IUpdateEntry Entry, MongoClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var entry = parameters.Entry;
        var wrapper = parameters.Wrapper;
        var items = wrapper.Client.GetDatabase(wrapper._databaseName).GetCollection<BsonDocument>(parameters.CollectionName);

        //var itemRequestOptions = CreateItemRequestOptions(entry, wrapper._enableContentResponseOnWrite);
        //var partitionKey = CreatePartitionKey(entry);

        //using var response = await items.DeleteItemStreamAsync(
        //        parameters.ResourceId,
        //        partitionKey == null ? PartitionKey.None : new PartitionKey(partitionKey),
        //        itemRequestOptions,
        //        cancellationToken: cancellationToken)
        //    .ConfigureAwait(false);

        //wrapper._commandLogger.ExecutedDeleteItem(
        //    response.Diagnostics.GetClientElapsedTime(),
        //    response.Headers.RequestCharge,
        //    response.Headers.ActivityId,
        //    parameters.ResourceId,
        //    parameters.ContainerId,
        //    partitionKey);

        //ProcessResponse(response, entry);

        //return response.StatusCode == HttpStatusCode.NoContent;
        return Task.FromResult(true);
    }

    //private static ItemRequestOptions? CreateItemRequestOptions(IUpdateEntry entry, bool? enableContentResponseOnWrite)
    //{
    //    var etagProperty = entry.EntityType.GetETagProperty();
    //    if (etagProperty == null)
    //    {
    //        return null;
    //    }

    //    var etag = entry.GetOriginalValue(etagProperty);
    //    var converter = etagProperty.GetTypeMapping().Converter;
    //    if (converter != null)
    //    {
    //        etag = converter.ConvertToProvider(etag);
    //    }

    //    bool enabledContentResponse;
    //    if (enableContentResponseOnWrite.HasValue)
    //    {
    //        enabledContentResponse = enableContentResponseOnWrite.Value;
    //    }
    //    else
    //    {
    //        switch (entry.EntityState)
    //        {
    //            case EntityState.Modified:
    //            {
    //                var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
    //                enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnUpdate) == ValueGenerated.OnUpdate;
    //                break;
    //            }
    //            case EntityState.Added:
    //            {
    //                var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
    //                enabledContentResponse = (jObjectProperty?.ValueGenerated & ValueGenerated.OnAdd) == ValueGenerated.OnAdd;
    //                break;
    //            }
    //            default:
    //                enabledContentResponse = false;
    //                break;
    //        }
    //    }

    //    return new ItemRequestOptions { IfMatchEtag = (string?)etag, EnableContentResponseOnWrite = enabledContentResponse };
    //}

    //private static string? CreatePartitionKey(IUpdateEntry entry)
    //{
    //    object? partitionKey = null;
    //    var partitionKeyPropertyName = entry.EntityType.GetPartitionKeyPropertyName();
    //    if (partitionKeyPropertyName != null)
    //    {
    //        var partitionKeyProperty = entry.EntityType.FindProperty(partitionKeyPropertyName)!;
    //        partitionKey = entry.GetCurrentValue(partitionKeyProperty);

    //        var converter = partitionKeyProperty.GetTypeMapping().Converter;
    //        if (converter != null)
    //        {
    //            partitionKey = converter.ConvertToProvider(partitionKey);
    //        }
    //    }

    //    return (string?)partitionKey;
    //}

    //private static void ProcessResponse(ResponseMessage response, IUpdateEntry entry)
    //{
    //    response.EnsureSuccessStatusCode();
    //    var etagProperty = entry.EntityType.GetETagProperty();
    //    if (etagProperty != null && entry.EntityState != EntityState.Deleted)
    //    {
    //        entry.SetStoreGeneratedValue(etagProperty, response.Headers.ETag);
    //    }

    //    var jObjectProperty = entry.EntityType.FindProperty(StoreKeyConvention.JObjectPropertyName);
    //    if (jObjectProperty != null
    //        && jObjectProperty.ValueGenerated == ValueGenerated.OnAddOrUpdate
    //        && response.Content != null)
    //    {
    //        using var responseStream = response.Content;
    //        using var reader = new StreamReader(responseStream);
    //        using var jsonReader = new JsonTextReader(reader);

    //        var createdDocument = Serializer.Deserialize<JObject>(jsonReader);

    //        entry.SetStoreGeneratedValue(jObjectProperty, createdDocument);
    //    }
    //}

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<BsonDocument> ExecuteMqlQuery(
        string collectionName,
        MongoMqlQuery query)
    {
        return new DocumentEnumerable(this, collectionName, query);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IAsyncEnumerable<BsonDocument> ExecuteMqlQueryAsync(
        string containerId,
        MongoMqlQuery query)
    {
        //_commandLogger.ExecutingSqlQuery(containerId, partitionKey, query);

#pragma warning disable CS8603 // Possible null reference return.
        return default;
#pragma warning restore CS8603 // Possible null reference return.
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual BsonDocument? ExecuteReadItem(
        string containerId,
        string? partitionKey,
        string resourceId)
    {
        //_commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

        var response = _executionStrategy.Execute((containerId, partitionKey, resourceId, this), CreateSingleItemQuery, null);

        //_commandLogger.ExecutedReadItem(
        //    response.Diagnostics.GetClientElapsedTime(),
        //    response.Headers.RequestCharge,
        //    response.Headers.ActivityId,
        //    resourceId,
        //    containerId,
        //    partitionKey);

        return JObjectFromReadItemResponseMessage(response);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<BsonDocument?> ExecuteReadItemAsync(
        string containerId,
        string? partitionKey,
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        //_commandLogger.ExecutingReadItem(containerId, partitionKey, resourceId);

        var response = await _executionStrategy.ExecuteAsync(
                (containerId, partitionKey, resourceId, this),
                CreateSingleItemQueryAsync,
                null,
                cancellationToken)
            .ConfigureAwait(false);

        //_commandLogger.ExecutedReadItem(
        //    response.Diagnostics.GetClientElapsedTime(),
        //    response.Headers.RequestCharge,
        //    response.Headers.ActivityId,
        //    resourceId,
        //    containerId,
        //    partitionKey);

        return JObjectFromReadItemResponseMessage(response);
    }

    private static /*ResponseMessage*/ BsonDocument CreateSingleItemQuery(
        DbContext? context,
        (string CollectionName, string? PartitionKey, string ResourceId, MongoClientWrapper Wrapper) parameters)
        => CreateSingleItemQueryAsync(context, parameters).GetAwaiter().GetResult();

    private static Task</*ResponseMessage*/BsonDocument> CreateSingleItemQueryAsync(
        DbContext? _,
        (string CollectionName, string? PartitionKey, string ResourceId, MongoClientWrapper Wrapper) parameters,
        CancellationToken cancellationToken = default)
    {
        var (collectionName, partitionKey, resourceId, wrapper) = parameters;
        var container = wrapper.Client.GetDatabase(wrapper._databaseName).GetCollection<BsonDocument>(collectionName);

        //return container.ReadItemStreamAsync(
        //    resourceId,
        //    string.IsNullOrEmpty(partitionKey) ? PartitionKey.None : new PartitionKey(partitionKey),
        //    cancellationToken: cancellationToken);
        return Task.FromResult(new BsonDocument());
    }

    private static BsonDocument? JObjectFromReadItemResponseMessage(/*ResponseMessage responseMessage*/ BsonDocument responseMessage)
    {
        //if (responseMessage.StatusCode == HttpStatusCode.NotFound)
        //{
        //    return null;
        //}

        //responseMessage.EnsureSuccessStatusCode();

        //var responseStream = responseMessage.Content;
        //using var reader = new StreamReader(responseStream);
        //using var jsonReader = new JsonTextReader(reader);

        //var jObject = Serializer.Deserialize<JObject>(jsonReader);

        return new BsonDocument("c", /*jObject*/ 1);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual /*FeedIterator*/ IAsyncCursor<BsonDocument> CreateQuery(
        string collectionName,
        MongoMqlQuery query)
    {
        var collection = Client.GetDatabase(_databaseName).GetCollection<BsonDocument>(collectionName);
        //var queryDefinition = new QueryDefinition(query.Query);

        //queryDefinition = query.Parameters
        //    .Aggregate(
        //        queryDefinition,
        //        (current, parameter) => current.WithParameter(parameter.Name, parameter.Value));

        //if (string.IsNullOrEmpty(partitionKey))
        //{
        //    return container.GetItemQueryStreamIterator(queryDefinition);
        //}

        //var queryRequestOptions = new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) };

        //return container.GetItemQueryStreamIterator(queryDefinition, requestOptions: queryRequestOptions);
        return collection.FindSync("{}"); // TODO
    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static JsonTextReader CreateJsonReader(TextReader reader)
    //{
    //    var jsonReader = new JsonTextReader(reader);

    //    while (jsonReader.Read())
    //    {
    //        if (jsonReader.TokenType == JsonToken.StartObject)
    //        {
    //            while (jsonReader.Read())
    //            {
    //                if (jsonReader.TokenType == JsonToken.StartArray)
    //                {
    //                    return jsonReader;
    //                }
    //            }
    //        }
    //    }

    //    return jsonReader;
    //}

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //private static bool TryReadJObject(JsonTextReader jsonReader, [NotNullWhen(true)] out JObject? jObject)
    //{
    //    jObject = null;

    //    while (jsonReader.Read())
    //    {
    //        if (jsonReader.TokenType == JsonToken.StartObject)
    //        {
    //            jObject = Serializer.Deserialize<JObject>(jsonReader);
    //            return true;
    //        }
    //    }

    //    return false;
    //}

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public bool CreateCollectionIfNotExists(CollectionProperties properties)
    {
        try
        {
            _singletonWrapper.Client.GetDatabase(_databaseName).CreateCollection(properties.CollectionName);
            return true; // TODO
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> CreateCollectionIfNotExistsAsync(CollectionProperties properties, CancellationToken cancellationToken = default)
    {
        var db = _singletonWrapper.Client.GetDatabase(_databaseName);
        await db.CreateCollectionAsync(properties.CollectionName, cancellationToken: cancellationToken).ConfigureAwait(false);
        return true;
    }

    public bool ReplaceItem(string collectionName, BsonValue id, BsonDocument document, IUpdateEntry entry)
    {
        throw new NotImplementedException();
    }

    public bool DeleteItem(string collectionName, BsonValue id, IUpdateEntry entry)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ReplaceItemAsync(string collectionName, BsonValue id, BsonDocument document, IUpdateEntry updateEntry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteItemAsync(string collectionName, BsonValue id, IUpdateEntry entry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public BsonDocument? ExecuteReadItem(string collectionName)
    {
        throw new NotImplementedException();
    }

    public Task<BsonDocument?> ExecuteReadItemAsync(string collectionName, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    private sealed class DocumentEnumerable : IEnumerable<BsonDocument>
    {
        private readonly MongoClientWrapper _mongoClient;
        private readonly string _collectionName;
        private readonly MongoMqlQuery _mongoMqlQuery;

        public DocumentEnumerable(
            MongoClientWrapper mongoClient,
            string collectionName,
            MongoMqlQuery mongoMqlQuery)
        {
            _mongoClient = mongoClient;
            _collectionName = collectionName;
            _mongoMqlQuery = mongoMqlQuery;
        }

        public IEnumerator<BsonDocument> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        private sealed class Enumerator : IEnumerator<BsonDocument>
        {
            private readonly MongoClientWrapper _mongoClientWrapper;
            private readonly string _collectionName;
            private readonly MongoMqlQuery _mongoMqlQuery;

            private BsonDocument? _current;
            //private ResponseMessage? _responseMessage;
            private BsonDocument? _responseMessage;
            private Stream? _responseStream;
            private StreamReader? _reader;
            //private JsonTextReader? _jsonReader;

            private IAsyncCursor<BsonDocument>? _query;

            public Enumerator(DocumentEnumerable documentEnumerable)
            {
                _mongoClientWrapper = documentEnumerable._mongoClient;
                _collectionName = documentEnumerable._collectionName;
                _mongoMqlQuery = documentEnumerable._mongoMqlQuery;
            }

            public BsonDocument Current
                => _current ?? throw new InvalidOperationException();

            object IEnumerator.Current
                => Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                //if (_jsonReader == null)
                {
                    _query ??= _mongoClientWrapper.CreateQuery(_collectionName, _mongoMqlQuery);

                    if (!_query.MoveNext())
                    {
                        _current = null;
                        return false;
                    }

                    _responseMessage = _query.Current.First();
                        //_query.ReadNextAsync().GetAwaiter().GetResult();

                    //_cosmosClientWrapper._commandLogger.ExecutedReadNext(
                    //    _responseMessage.Diagnostics.GetClientElapsedTime(),
                    //    _responseMessage.Headers.RequestCharge,
                    //    _responseMessage.Headers.ActivityId,
                    //    _containerId,
                    //    _partitionKey,
                    //    _cosmosSqlQuery);

                    //_responseMessage.EnsureSuccessStatusCode();

                    //_responseStream = _responseMessage.Content;
                    //_reader = new StreamReader(_responseStream);
                    //_jsonReader = CreateJsonReader(_reader);
                }

                //if (TryReadJObject(_jsonReader, out var jObject))
                //{
                //    _current = jObject;
                //    return true;
                //}

                ResetRead();

                return MoveNext();
            }

            private void ResetRead()
            {
                //_jsonReader?.Close();
                //_jsonReader = null;
                _reader?.Dispose();
                _reader = null;
                _responseStream?.Dispose();
                _responseStream = null;
            }

            public void Dispose()
            {
                ResetRead();

                //_responseMessage?.Dispose();
                //_responseMessage = null;
            }

            public void Reset()
                => throw new NotSupportedException(CoreStrings.EnumerableResetNotSupported);
        }
    }

    private sealed class DocumentAsyncEnumerable : IAsyncEnumerable<BsonDocument>
    {
        private readonly MongoClientWrapper _mongoClient;
        private readonly string _collectionName;
        private readonly MongoMqlQuery _mongoMqlQuery;

        public DocumentAsyncEnumerable(
            MongoClientWrapper mongoClient,
            string collectionName,
            MongoMqlQuery mongoMqlQuery)
        {
            _mongoClient = mongoClient;
            _collectionName = collectionName;
            _mongoMqlQuery = mongoMqlQuery;
        }

        public IAsyncEnumerator<BsonDocument> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            => new AsyncEnumerator(this, cancellationToken);

        private sealed class AsyncEnumerator : IAsyncEnumerator<BsonDocument>
        {
            private readonly MongoClientWrapper _mongoClientWrapper;
            private readonly string _collectionName;
            private readonly MongoMqlQuery _mongoMqlQuery;
            private readonly CancellationToken _cancellationToken;

            private BsonDocument? _current;
            private BsonDocument? _responseMessage;
            //private Stream? _responseStream;
            //private StreamReader? _reader;
            //private JsonTextReader? _jsonReader;

            private IAsyncCursor<BsonDocument>? _query;

            public BsonDocument Current
                => _current ?? throw new InvalidOperationException();

            public AsyncEnumerator(DocumentAsyncEnumerable documentEnumerable, CancellationToken cancellationToken)
            {
                _mongoClientWrapper = documentEnumerable._mongoClient;
                _collectionName = documentEnumerable._collectionName;
                _mongoMqlQuery = documentEnumerable._mongoMqlQuery;
                _cancellationToken = cancellationToken;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public async ValueTask<bool> MoveNextAsync()
            {
                _cancellationToken.ThrowIfCancellationRequested();

                //if (_jsonReader == null)
                {
                    _query ??= _mongoClientWrapper.CreateQuery(_collectionName, _mongoMqlQuery);

                    if (!await _query.MoveNextAsync(_cancellationToken).ConfigureAwait(false))
                    {
                        _current = null;
                        return false;
                    }

                    _responseMessage = _query.Current.First();

                    //_cosmosClientWrapper._commandLogger.ExecutedReadNext(
                    //    _responseMessage.Diagnostics.GetClientElapsedTime(),
                    //    _responseMessage.Headers.RequestCharge,
                    //    _responseMessage.Headers.ActivityId,
                    //    _containerId,
                    //    _partitionKey,
                    //    _cosmosSqlQuery);

                    //_responseMessage.EnsureSuccessStatusCode();

                    //_responseStream = _responseMessage.Content;
                    //_reader = new StreamReader(_responseStream);
                    //_jsonReader = CreateJsonReader(_reader);
                }

                //if (TryReadJObject(_jsonReader, out var jObject))
                //{
                //    _current = jObject;
                //    return true;
                //}

                await ResetReadAsync().ConfigureAwait(false);

                return await MoveNextAsync().ConfigureAwait(false);
            }

            private /*async*/ Task ResetReadAsync()
            {
                //_jsonReader?.Close();
                //_jsonReader = null;
                //await _reader.DisposeAsyncIfAvailable().ConfigureAwait(false);
                //_reader = null;
                //await _responseStream.DisposeAsyncIfAvailable().ConfigureAwait(false);
                //_responseStream = null;
                return Task.CompletedTask; // TODO
            }

            public async ValueTask DisposeAsync()
            {
                await ResetReadAsync().ConfigureAwait(false);

                //await _responseMessage.DisposeAsyncIfAvailable().ConfigureAwait(false);
                _responseMessage = null;
            }
        }
    }
}
