// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Text;
using Microsoft.EntityFrameworkCore.Mongo.Storage.Internal;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

/// <summary>
/// TODO
/// </summary>
public abstract class MongoSerializer
{
    /// <summary>
    /// Convert a Stream of JSON to an object. 
    /// The implementation is responsible for Disposing of the stream,
    /// including when an exception is thrown, to avoid memory leaks.
    /// </summary>
    /// <typeparam name="T">Any type passed to <see cref="Container"/>.</typeparam>
    /// <param name="stream">The Stream response containing JSON from Cosmos DB.</param>
    /// <returns>The object deserialized from the stream.</returns>
    public abstract T FromStream<T>(Stream stream);

    /// <summary>
    /// Convert the object to a Stream. 
    /// The caller will take ownership of the stream and ensure it is correctly disposed of.
    /// <see href="https://docs.microsoft.com/dotnet/api/system.io.stream.canread">Stream.CanRead</see> must be true.
    /// </summary>
    /// <param name="input">Any type passed to <see cref="Container"/>.</param>
    /// <returns>A readable Stream containing JSON of the serialized object.</returns>
    public abstract Stream ToStream<T>(T input);
}

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class JsonMongoSerializer : MongoSerializer
{
    private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

    /// <inheritdoc />
    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            using var jsonTextReader = new BsonBinaryReader(stream);
            return BsonSerializer.Deserialize<T>(jsonTextReader);
        }
    }

    /// <inheritdoc />
    public override Stream ToStream<T>(T input)
    {
        //var streamPayload = new MemoryStream();
        //using (var streamWriter = new StreamWriter(streamPayload, encoding: DefaultEncoding, bufferSize: 1024, leaveOpen: true))
        //{
        //    using var jsonTextWriter = new BsonBinaryWriter(streamWriter);
        //    jsonTextWriter.Formatting = Formatting.None;
        //    GetSerializer().Serialize(jsonTextWriter, input);
        //    jsonTextWriter.Flush();
        //    streamWriter.Flush();
        //}

        //streamPayload.Position = 0;
        //return streamPayload;
        throw new NotImplementedException();
    }

    //private static JsonSerializer GetSerializer() // TODO:
    //    => MongoClientWrapper.Serializer;
}
