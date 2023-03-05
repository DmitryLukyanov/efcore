// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;

namespace MongoDB.EntityFramework.Provider.Tests.Utils
{
    public static class TestEnvironment
    {
        public static IConfiguration Config { get; } = new ConfigurationBuilder().Build()
            //.SetBasePath(Directory.GetCurrentDirectory())
            //.AddJsonFile("config.json", optional: true)
            //.AddJsonFile("config.test.json", optional: true)
            //.AddEnvironmentVariables()
            //.Build()
            //.GetSection("Test:Cosmos")
            ;

        public static string DefaultConnection { get; } =
            //string.IsNullOrEmpty(Config["DefaultConnection"])
            //    ? 
            //"https://localhost:8081"
            "mongodb://localhost:27017"
            //    : Config["DefaultConnection"]
            ;

        public static string AuthToken { get; } = ""; // TODO: remove
                                                      //string.IsNullOrEmpty(Config["AuthToken"])
                                                      //? "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                                                      //: Config["AuthToken"];

        public static string ConnectionString { get; } = DefaultConnection; //$"AccountEndpoint={DefaultConnection};AccountKey={AuthToken}";

        public static bool IsEmulator { get; } = true;//DefaultConnection.StartsWith("https://localhost:8081", StringComparison.Ordinal);
    }
}
