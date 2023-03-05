// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using MongoDB.EntityFramework.Provider.Tests.Utils;

namespace EFCore.Mongo.Tests
{
    public class MongoTestStore : TestStore
    {
        private readonly TestStoreContext _storeContext;
        private readonly Action<MongoDbContextOptionsBuilder> _configureMongo;
        private bool _initialized;

        private static readonly Guid _runId = Guid.NewGuid();

        public static MongoTestStore Create(string name, Action<MongoDbContextOptionsBuilder>? extensionConfiguration = null)
            => new MongoTestStore(name, shared: false, extensionConfiguration: extensionConfiguration);

        public static MongoTestStore CreateInitialized(string name, Action<MongoDbContextOptionsBuilder>? extensionConfiguration = null)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            => (MongoTestStore)Create(name, extensionConfiguration).Initialize(null, (Func<DbContext>)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        public static MongoTestStore GetOrCreate(string name) => new MongoTestStore(name);

        public static MongoTestStore GetOrCreate(string name, string dataFilePath) => new MongoTestStore(name, dataFilePath: dataFilePath);
        private MongoTestStore(
            string name,
            bool shared = true,
            string? dataFilePath = null, // TODO: remove
            Action<MongoDbContextOptionsBuilder>? extensionConfiguration = null)
            : base(CreateName(name), shared)
        {
            ConnectionUri = TestEnvironment.DefaultConnection;
            AuthToken = TestEnvironment.AuthToken;
            ConnectionString = TestEnvironment.ConnectionString;
            _configureMongo = extensionConfiguration == null
                ? (Action<MongoDbContextOptionsBuilder>)(b => b.ApplyConfiguration())
                : (b =>
                {
                    b.ApplyConfiguration();
                    extensionConfiguration(b);
                });

            _storeContext = new TestStoreContext(this);
        }

        private static string CreateName(string name)
            => TestEnvironment.IsEmulator || name == "Northwind"
                ? name
                : name + _runId;

        public string ConnectionUri { get; }
        public string AuthToken { get; }
        public string ConnectionString { get; }

        protected override DbContext CreateDefaultContext()
            => new TestStoreContext(this);

        public override DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder)
            => builder.UseMongo(
                ConnectionUri,
                //AuthToken,
                //Name,
                CreateName("Temp"), // TODO
                _configureMongo);

        protected override void Initialize(Func<DbContext> createContext, Action<DbContext> seed, Action<DbContext> clean)
        {
            _initialized = true;
            base.Initialize(createContext ?? (() => _storeContext), seed, clean);
        }
        public override void Clean(DbContext context)
            => CleanAsync(context).GetAwaiter().GetResult();

        public override Task CleanAsync(DbContext context)
        {
            return Task.CompletedTask;
        }

        public override void Dispose() => throw new InvalidOperationException("Calling Dispose can cause deadlocks. Use DisposeAsync instead.");

        public override async Task DisposeAsync()
        {
            if (_initialized)
            {
                await _storeContext.Database.EnsureDeletedAsync();
            }

            _storeContext.Dispose();
        }

        private class TestStoreContext : DbContext
        {
            private readonly MongoTestStore _testStore;

            public TestStoreContext(MongoTestStore testStore)
            {
                _testStore = testStore;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseMongo(_testStore.ConnectionUri, nameof(TestStoreContext), /* _testStore.AuthToken, _testStore.Name,*/ _testStore._configureMongo);
            }
        }
    }

}
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
