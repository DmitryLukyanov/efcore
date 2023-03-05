// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Mongo.Infrastructure.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class MongoSingletonOptions : IMongoSingletonOptions
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountEndpoint { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? AccountKey { get; private set; }

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //public virtual TokenCredential? TokenCredential { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? ConnectionString { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual string? Region { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? LimitToEndpoint { get; private set; }

    ///// <summary>
    /////     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    /////     the same compatibility standards as public APIs. It may be changed or removed without notice in
    /////     any release. You should only use it directly in your code with extreme caution and knowing that
    /////     doing so can result in application failures when updating to a new Entity Framework Core release.
    ///// </summary>
    //public virtual ConnectionMode? ConnectionMode { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IWebProxy? WebProxy { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? RequestTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? OpenTcpConnectionTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual TimeSpan? IdleTcpConnectionTimeout { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? GatewayModeMaxConnectionLimit { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxTcpConnectionsPerEndpoint { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual int? MaxRequestsPerTcpConnection { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool? EnableContentResponseOnWrite { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Func<HttpClient>? HttpClientFactory { get; private set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Initialize(IDbContextOptions options)
    {
        var mongoOptions = options.FindExtension<MongoOptionsExtension>();
        if (mongoOptions != null)
        {
            AccountEndpoint = mongoOptions.AccountEndpoint;
            AccountKey = mongoOptions.AccountKey;
            //TokenCredential = mongoOptions.TokenCredential;
            ConnectionString = mongoOptions.ConnectionString;
            Region = mongoOptions.Region;
            LimitToEndpoint = mongoOptions.LimitToEndpoint;
            EnableContentResponseOnWrite = mongoOptions.EnableContentResponseOnWrite;
            //ConnectionMode = mongoOptions.ConnectionMode;
            WebProxy = mongoOptions.WebProxy;
            RequestTimeout = mongoOptions.RequestTimeout;
            OpenTcpConnectionTimeout = mongoOptions.OpenTcpConnectionTimeout;
            IdleTcpConnectionTimeout = mongoOptions.IdleTcpConnectionTimeout;
            GatewayModeMaxConnectionLimit = mongoOptions.GatewayModeMaxConnectionLimit;
            MaxTcpConnectionsPerEndpoint = mongoOptions.MaxTcpConnectionsPerEndpoint;
            MaxRequestsPerTcpConnection = mongoOptions.MaxRequestsPerTcpConnection;
            HttpClientFactory = mongoOptions.HttpClientFactory;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Validate(IDbContextOptions options)
    {
        var mongoOptions = options.FindExtension<MongoOptionsExtension>();

        if (mongoOptions != null
            && (AccountEndpoint != mongoOptions.AccountEndpoint
                || AccountKey != mongoOptions.AccountKey
                //|| TokenCredential != mongoOptions.TokenCredential
                || ConnectionString != mongoOptions.ConnectionString
                || Region != mongoOptions.Region
                || LimitToEndpoint != mongoOptions.LimitToEndpoint
                //|| ConnectionMode != cosmosOptions.ConnectionMode
                || WebProxy != mongoOptions.WebProxy
                || RequestTimeout != mongoOptions.RequestTimeout
                || OpenTcpConnectionTimeout != mongoOptions.OpenTcpConnectionTimeout
                || IdleTcpConnectionTimeout != mongoOptions.IdleTcpConnectionTimeout
                || GatewayModeMaxConnectionLimit != mongoOptions.GatewayModeMaxConnectionLimit
                || MaxTcpConnectionsPerEndpoint != mongoOptions.MaxTcpConnectionsPerEndpoint
                || MaxRequestsPerTcpConnection != mongoOptions.MaxRequestsPerTcpConnection
                || EnableContentResponseOnWrite != mongoOptions.EnableContentResponseOnWrite
                || HttpClientFactory != mongoOptions.HttpClientFactory
            ))
        {
            throw new InvalidOperationException(
                //CoreStrings.SingletonOptionChanged(
                //    nameof(CosmosDbContextOptionsExtensions.UseCosmos),
                //    nameof(DbContextOptionsBuilder.UseInternalServiceProvider))
                );
        }
    }
}
