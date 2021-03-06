﻿using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Common;
using DataGeneration.Core.Settings;
using DataGeneration.Soap;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static DataGeneration.Entities.CrossEntityGenerationHelper;

namespace DataGeneration.Entities.Opportunities
{
    public class OpportunityRandomizerSettings : RandomizerSettings<Opportunity>
    {
        public ProbabilityCollection<string> OpportunityClasses { get; set; }
        public ProbabilityCollection<OpportunityAccountType> OpportunityAccountTypes { get; set; }
        public ProbabilityCollection<OpportunityStatusType> OpportunityStatusTypes { get; set; }
        public ProbabilityCollection<OpportunityProductsSettings> OpportunityProductsSettings { get; set; }
        public bool UseExistingOpportunities { get; set; }

        // assigned in RunBeforeGeneration
        [JsonIgnore]
        public IDictionary<OpportunityAccountType, BusinessAccountWrapper[]> BusinessAccounts { get; set; }
        [JsonIgnore]
        public IList<string> InventoryIds { get; set; }
        [JsonIgnore]
        public IProducerConsumerCollection<Opportunity> ExistingOpportunities { get; set; }

        // little optimization if no need to fetch anything
        [JsonIgnore]
        public bool FetchBusinessAccounts => OpportunityAccountTypes?.AsList.Any(i => i != OpportunityAccountType.WithoutAccount) ?? false;
        [JsonIgnore]
        public bool FetchInventoryIds => OpportunityProductsSettings?.AsList.Any(i => i?.ProductsCounts != null) ?? false;

        protected override Faker<Opportunity> GetFaker()
        {
            var faker = base
                .GetFaker()
                .Rules((f, o) =>
                {
                    o.ReturnBehavior = ReturnBehavior.None;

                    o.ClassID = f.Random.ProbabilityRandomIfAny(OpportunityClasses);

                    #region Add Products

                    var productsSettings = f.Random.ProbabilityRandomIfAny(OpportunityProductsSettings);
                    if (productsSettings != null)
                    {
                        if (productsSettings.ManualAmount)
                        {
                            o.ManualAmount = true;
                            if (productsSettings.Amounts.TryGetValues(out var amin, out var amax))
                            {
                                o.Amount = decimal.Round(f.Random.Decimal(amin, amax), 2);
                            }
                        }
                        else if (productsSettings.ProductsCounts.TryGetValues(out var pmin, out var pmax)
                            && !InventoryIds.IsNullOrEmpty())
                        {
                            var count = f.Random.Int(pmin, pmax);

                            Func<int> GetQty;
                            if (productsSettings.Quantities.TryGetValues(out var qmin, out var qmax))
                                GetQty = () => f.Random.Int(qmin, qmax);
                            else
                                GetQty = () => 1;

                            o.Products = new OpportunityProduct[count];
                            for (int i = 0; i < count; i++)
                            {
                                var id = f.PickRandom(InventoryIds);
                                o.Products[i] = new OpportunityProduct
                                {
                                    InventoryID = id,
                                    Qty = GetQty(),
                                    ReturnBehavior = ReturnBehavior.None,
                                };
                            }
                        }
                    }

                    #endregion

                    #region Add Statuses

                    var status = f.Random.ProbabilityRandomIfAny(OpportunityStatusTypes);
                    switch (status)
                    {
                        case OpportunityStatusType.Prospect:
                            o.Status = "Open";
                            o.Stage = "Prospect";
                            if (o.ManualAmount == true)
                                o.Discount = 20;
                            break;

                        case OpportunityStatusType.Nurture:
                            o.Status = "Open";
                            o.Stage = "Nurture";
                            if (o.ManualAmount == true)
                                o.Discount = 10;
                            break;

                        case OpportunityStatusType.Development:
                            o.Status = "Open";
                            o.Stage = "Development";
                            if (o.ManualAmount == true)
                                o.Discount = 5;
                            break;

                        case OpportunityStatusType.Negotiation:
                            o.Status = "Lost";
                            o.Stage = "Negotiation";
                            break;

                        case OpportunityStatusType.Won:
                            o.Status = "Won";
                            o.Stage = "Won";
                            break;

                        case OpportunityStatusType.New:
                        default:
                            break;
                    }

                    #endregion
                });

            if(UseExistingOpportunities)
            {
                if (ExistingOpportunities == null)
                    throw new InvalidOperationException($"Property {nameof(UseExistingOpportunities)} is not specified.");

                return GetFaker<Opportunity>()
                    .CustomInstantiator(f =>
                    {
                        // todo: rewrite all this try takes to use ConsumerCollectionDataGenerator -> 
                        // in overridden GetDataGenerator method
                        if (!ExistingOpportunities.TryTake(out var opportunity))
                        {
                            throw new GenerationException("Cannot fill existing entity. No opportunities remain.");
                        }

                        faker.Populate(opportunity);
                        return opportunity;
                    });
            }
            else
            {
                return faker.Rules((f, o) =>
                {
                    o.Subject = f.Lorem.Sentence(2, 10);

                    #region Link BAccount and Contact

                    var accountType = f.Random.ProbabilityRandomIfAny(OpportunityAccountTypes);
                    if (BusinessAccounts != null 
                        && BusinessAccounts.TryGetValue(accountType, out var accounts)
                        && accounts != null)
                    {
                        var account = f.PickRandom(accounts);
                        o.BusinessAccount = account.AccountId;
                        if (!account.Contacts.IsNullOrEmpty())
                        {
                            o.ContactID = f.PickRandom(account.Contacts).ContactId;
                        }
                    }
                    else
                    {
                        o.ContactInformation = new OpportunityContact
                        {
                            Email = f.Internet.Email(),
                            ReturnBehavior = ReturnBehavior.None
                        };
                    }

                    #endregion
                });
            }
        }
    }
    public class OpportunityProductsSettings : IProbabilityObject
    {
        public decimal? Probability { get; set; }
        public bool ManualAmount { get; set; }
        public (decimal min, decimal max)? Amounts { get; set; }
        public (int min, int max)? ProductsCounts { get; set; }
        public (int min, int max)? Quantities { get; set; }
    }
}