﻿// Copyright (c) Microsoft Corporation.
//  Licensed under the MIT License.

using Microsoft.EntityFrameworkCore;
using RulesEngine.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using static RulesEngine.Extensions.ListofRuleResultTreeExtension;

namespace DemoApp
{
    using DemoApp.Context;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public class EFDemo
    {
        public void Run()
        {
            // write the rules to the database
            RulesEngineMySSqlContext db = new RulesEngineMySSqlContext();
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "Discount.json", SearchOption.AllDirectories);
            if (files == null || files.Length == 0) throw new Exception("Rules not found.");
            var fileData = File.ReadAllText(files[0]);
            var workflow = JsonConvert.DeserializeObject<List<Workflow>>(fileData);
            if (db.Database.EnsureCreated()) { db.Workflows.AddRange(workflow); db.SaveChanges(); }

            // Sample Data
            Console.WriteLine($"Running {nameof(EFDemo)}....");
            var basicInfo = "{\"name\": \"hello\",\"email\": \"abcy@xyz.com\",\"creditHistory\": \"good\",\"country\": \"canada\",\"loyaltyFactor\": 3,\"totalPurchasesToDate\": 10000}";
            var orderInfo = "{\"totalOrders\": 5,\"recurringItems\": 2}";
            var telemetryInfo = "{\"noOfVisitsPerMonth\": 10,\"percentageOfBuyingToVisit\": 15}";
            var converter = new ExpandoObjectConverter();
            dynamic? input1 = JsonConvert.DeserializeObject<ExpandoObject>(basicInfo, converter);
            dynamic? input2 = JsonConvert.DeserializeObject<ExpandoObject>(orderInfo, converter);
            dynamic? input3 = JsonConvert.DeserializeObject<ExpandoObject>(telemetryInfo, converter);
            var inputs = new dynamic[] { input1!, input2!, input3! };

            // Load the rules from the database
            var wfr = db.Workflows.Include(i => i.Rules).ThenInclude(i => i.Rules).ToArray();
            var bre = new RulesEngine.RulesEngine(wfr, null);

            // Execute the rules against the input
            List<RuleResultTree> resultList = bre.ExecuteAllRulesAsync("Discount", inputs).Result;

            // Perform actions based on the results
            string discountOffered = "No discount offered.";
            resultList.OnSuccess((eventName) => { discountOffered = $"Discount offered is {eventName} % over MRP."; });
            resultList.OnFail(() => { discountOffered = "The user is not eligible for any discount."; });
            Console.WriteLine(discountOffered);
        }
    }
}
