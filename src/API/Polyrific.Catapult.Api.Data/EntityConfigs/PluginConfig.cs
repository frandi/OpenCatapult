﻿// Copyright (c) Polyrific, Inc 2018. All rights reserved.

using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polyrific.Catapult.Api.Core.Entities;

namespace Polyrific.Catapult.Api.Data.EntityConfigs
{
    public class PluginConfig : BaseEntityConfig<Plugin>
    {
        public override void Configure(EntityTypeBuilder<Plugin> builder)
        {
            base.Configure(builder);

            builder.HasData(
                new Plugin { Id = 1, Name = "Polyrific.Catapult.Plugins.AspNetCoreMvc", Type = "GeneratorProvider", Author = "Polyrific", Version = "1.0.0-beta2", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a1" },
                new Plugin { Id = 2, Name = "Polyrific.Catapult.Plugins.GitHub", Type = "RepositoryProvider", Author = "Polyrific", Version = "1.0.0-beta2", RequiredServicesString = "GitHub", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a2" },
                new Plugin { Id = 3, Name = "Polyrific.Catapult.Plugins.DotNetCore", Type = "BuildProvider", Author = "Polyrific", Version = "1.0.0-beta2", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a3" },
                new Plugin { Id = 4, Name = "Polyrific.Catapult.Plugins.DotNetCoreTest", Type = "TestProvider", Author = "Polyrific", Version = "1.0.0-beta2", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a4" },
                new Plugin { Id = 5, Name = "Polyrific.Catapult.Plugins.EntityFrameworkCore", Type = "DatabaseProvider", Author = "Polyrific", Version = "1.0.0-beta2", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a5" },
                new Plugin { Id = 6, Name = "Polyrific.Catapult.Plugins.AzureAppService", Type = "HostingProvider", Author = "Polyrific", RequiredServicesString = "Azure", Version = "1.0.0-beta2", Created = new DateTime(2018, 9, 28, 7, 23, 37, 58, DateTimeKind.Utc), ConcurrencyStamp = "976e0533-360a-4e46-8220-7c1cfdf0e0a6" }
            );
        }
    }
}
