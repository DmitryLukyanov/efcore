// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Mongo.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.EntityFramework.Provider.Tests.Utils;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestMongoExecutionStrategy : MongoExecutionStrategy
{
    protected static new readonly int DefaultMaxRetryCount = 10;

    protected static new readonly TimeSpan DefaultMaxDelay = TimeSpan.FromSeconds(60);

    public TestMongoExecutionStrategy()
        : base(
            new DbContext(
                new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseMongo(
                        TestEnvironment.DefaultConnection,
                        TestEnvironment.AuthToken,
                        "NonExistent").Options),
            DefaultMaxRetryCount, DefaultMaxDelay)
    {
    }

    public TestMongoExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay)
    {
    }
}
