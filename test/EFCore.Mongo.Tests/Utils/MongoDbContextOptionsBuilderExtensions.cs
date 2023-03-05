// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Net.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class MongoDbContextOptionsBuilderExtensions
{
    public static MongoDbContextOptionsBuilder ApplyConfiguration(this MongoDbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .ExecutionStrategy(d => new TestMongoExecutionStrategy(d))
            .RequestTimeout(TimeSpan.FromMinutes(20))
            .HttpClientFactory(
                () => new HttpClient(
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    }))
            //.ConnectionMode(ConnectionMode.Gateway)
            ;

        return optionsBuilder;
    }
}
