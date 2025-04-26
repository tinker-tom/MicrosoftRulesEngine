// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace DemoApp.Context
{
    public class RulesEngineMySSqlContext : DbContext
    {
        public DbSet<Workflow> Workflows { get; set; }
        public DbSet<Rule> Rules { get; set; }
        public DbSet<ScopedParam> ScopedParam { get; set; }

        public string DbPath { get; private set; } = "RulesEngineDemo.db";
        public string ConnectionString { get; private set; } = "server=127.0.0.1;database=RulesDb;user=root;password=2du77Ppa";

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        //=> options.UseSqlite($"Data Source={DbPath}");
        => options.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ScopedParam>()
              .HasKey(k => k.Name);

            modelBuilder.Entity<Workflow>(entity => {
                entity.HasKey(k => k.WorkflowName);
                entity.Ignore(b => b.WorkflowsToInject);
            });

            modelBuilder.Entity<Rule>().HasOne<Rule>().WithMany(r => r.Rules).HasForeignKey("RuleNameFK");

            var serializationOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

            modelBuilder.Entity<Rule>(entity => {
                entity.HasKey(k => k.RuleName);

                var valueComparer = new ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c);

                entity.Property(b => b.Properties)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializationOptions),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, serializationOptions))
                    .Metadata
                    .SetValueComparer(valueComparer);

                entity.Property(p => p.Actions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, serializationOptions),
                   v => JsonSerializer.Deserialize<RuleActions>(v, serializationOptions));

                entity.Ignore(b => b.WorkflowsToInject);
            });

        }

    }
}