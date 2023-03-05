// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using EFCore.Mongo.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MongoDB.EntityFramework.Provider.Tests
{
    public class EndToEndTests
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();
        private readonly IServiceProvider _serviceProvider;
        private readonly TestStore _testStore = MongoTestStore.Create("db1");

        public EndToEndTests()
        {
            _serviceProvider = Initialize();
        }

        [Fact]
        public void Can_add_update_delete_end_to_end()
        {
            var options = CreateOptions(_serviceProvider);

            var customer = new Customer { Id = 44, Name = "Theon" };

            using (var context = new MongoCustomerContext(options))
            {
                context.Database.EnsureCreated();

                context.Add(customer);

                context.SaveChanges();
            }

            using (var context = new MongoCustomerContext(options))
            {
                var customerFromStore = context.Set<Customer>().Single();

                Assert.Equal(44, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);

                customerFromStore.Name = "Theon Greyjoy";

                context.SaveChanges();
            }

            using (var context = new MongoCustomerContext(options))
            {
                //var customerFromStore = context.Set<Customer>().Single();

                //Assert.Equal(44, customerFromStore.Id);
                //Assert.Equal("Theon", customerFromStore.Name);

                context.Remove(customer/*customerFromStore*/);

                context.SaveChanges();
            }
        }

        private DbContextOptions CreateOptions(IServiceProvider serviceProvider)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            _testStore.Initialize(null, (Func<DbContext>)null);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            var providerOptionsBuilder = _testStore.AddProviderOptions(new DbContextOptionsBuilder());
            return AddOptions(providerOptionsBuilder)
                .EnableDetailedErrors()
                .UseInternalServiceProvider(serviceProvider)
                .EnableServiceProviderCaching(false)
                .Options;

            DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
               => builder
                   .EnableSensitiveDataLogging()
                   ;
        }

        private IServiceProvider Initialize()
        {
            _serviceCollection
                .AddEntityFrameworkMongo()
                .AddSingleton<ILoggerFactory>(new NullLoggerFactory())
                ;
            return _serviceCollection.BuildServiceProvider(validateScopes: true);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int PartitionKey { get; set; }
        }


        private class MongoCustomerContext : DbContext
        {
            public MongoCustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }
    }
}
