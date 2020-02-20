using Bogus;
using DataGeneration.Core;
using DataGeneration.Core.Api;
using DataGeneration.Core.Extensions;
using DataGeneration.Core.Settings;
using DataGeneration.GenerationInfo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static DataGeneration.Soap.SoapExtensions;

namespace DataGeneration.Scripts
{
    public class GenerateSalesDemoScript_Cases : GenerateSalesDemoScriptBase
    {
        public GeneratorConfig GetConfig_Stage1()
        {
            return GetConfig(
                GetSettings_Stage1()
                .ChangeSettings(s =>
                {
                    s.Count = 30;
                    s.ExecutionTypeSettings = ExecutionTypeSettings.Sequent();
                }));
        }
        public GeneratorConfig GetConfig_Stage2()
        {
            return GetConfig(
                GetSettings_Stage2()
                .ChangeSettings(s =>
                {
                    s.Count = 30;
                    s.ExecutionTypeSettings = ExecutionTypeSettings.Sequent();
                }));
        }

        protected IGenerationSettings GetSettings_Stage1()
        {
            IList<(Soap.BusinessAccount account, IList<Soap.Contact> contacts)> accounts = null;
            IList<(Soap.BusinessAccount account, IList<Soap.Contact> contacts)> customers = null;
            IList<Soap.StockItem> inventories = null;

            return DelegateGenerationSettings.Create<Soap.Case>(
                (@this, faker) =>
                {
                    var generator = @this
                        .GetRandomizer()
                        .GuaranteedRandomEnumerator(
                            ((classId: "BILLING", status: "Open", useCustomer: true, subjectId: 0), 5),
                            ((classId: "OTHER", status: "Open", useCustomer: false, subjectId: 1), 5),
                            ((classId: "PRODSUPINC", status: null, useCustomer: true, subjectId: 2), 0));

                    var currentDate = (date: GetDateTime(1), count: 0);

                    return faker.CustomInstantiator(f =>
                    {
                        generator.MoveNext();
                        var (classId, status, useCustomer, subjectId) = generator.Current;


                        var (baccount, contacts) = useCustomer switch
                        {
                            true => f.PickRandom(customers),
                            false => f.PickRandom(accounts),
                        };

                        var contact = useCustomer switch
                        {
                            true => f.PickRandom(contacts),
                            false => null
                        };

                        var subject = subjectId switch
                        {
                            0 => baccount.Name.Value,
                            1 => $"Question",
                            2 => f.PickRandom(inventories).Description.Value,
                            _ => null
                        };

                        var (date, count) = currentDate;
                        if (count == 0)
                        {
                            currentDate.count++;
                        }
                        else if (count >= 2 || f.Random.Bool())
                        {
                            currentDate = (GetDateTime(date.Day + 1), 0);
                        }

                        return new Soap.Case
                        {
                            ClassID = classId,
                            Subject = subject,
                            BusinessAccount = baccount.BusinessAccountID,
                            ContactID = contact?.ContactID,
                            Status = status,
                            DateReported = date,
                            LastModifiedDate = date + TimeSpan.FromMinutes(f.Random.Int(0, 120))
                        };
                    });
                },
                async (@this, client, entity, ct) =>
                {
                    await client.PutAsync(entity, ct);
                },
                beforeGenerateDelegate: async (@this, client, ct) =>
                {
                    var inventories_ = client.GetListAsync(new Soap.StockItem(), ct);
                    var customers_ = client.GetListAsync(new Soap.BusinessAccount { Type = new Soap.StringSearch("Customer") }, ct);
                    var prospects_ = client.GetListAsync(new Soap.BusinessAccount { Type = new Soap.StringSearch("Prospect") }, ct);
                    var contacts = await client.GetListAsync(
                        new Soap.Contact
                        {
                            BusinessAccount = new Soap.StringSearch { Condition = Soap.StringCondition.IsNotNull },
                            Active = new Soap.BooleanSearch { Value = true},
                        }, ct);
                    accounts = (await customers_)
                        .Union(await prospects_)
                        .Select(a => (a, contacts: contacts.Where(c => c.BusinessAccount == a.BusinessAccountID).ToList() as IList<Soap.Contact>))
                        .ToList();
                    customers = accounts.Where(b => b.account.Type == "Customer" && b.contacts.Count > 0).ToList();
                    inventories = await inventories_;
                },
                afterGenerateDelegate: null);
        }

        protected IGenerationSettings GetSettings_Stage2()
        {
            IProducerConsumerCollection<(Soap.Case @case, string email)> cases = null;

            return DelegateGenerationSettings.Create(
                new
                {
                    Case = default(Soap.Case),
                    Email = default(Soap.Email),
                    Activities = default(ICollection<Soap.Activity>),
                    Action = default(Soap.Action),
                },
                (@this, faker) =>
                {
                    IProducerConsumerCollection<bool> twoActivitiesList = new ConcurrentQueue<bool>(new[] { true, false, false, true, true });

                    return faker.CustomInstantiator(f =>
                    {
                        cases.TryTake(out var item);

                        var (email, activity, action) = item.@case.ClassID.Value switch
                        {
                            "PRODSUPINC" =>
                                (
                                    new Soap.Email
                                    {
                                        Subject = $"[Case #{item.@case.CaseID.Value}] {item.@case.Subject.Value}", //"Model and Serial number",
                                        To = item.email,
                                        MailStatus = "Processed",
                                        Body = "Model and Serial number",
                                        CreatedDate = item.@case.DateReported,
                                        LastModifiedDate = item.@case.LastModifiedDate,
                                    },
                                    default(ICollection<Soap.Activity>),
                                    (Soap.Action)new Soap.PendingCustomerCase { Reason = "More info Requested" }
                                ),
                            "BILLING" =>
                                (
                                    default(Soap.Email),
                                    twoActivitiesList.TryTake(out bool twoActivities)
                                        && twoActivities
                                        ? new[] {
                                            new Soap.Activity
                                            {
                                                Type = "P",
                                                Summary = "Called the customer",
                                                Status = "Completed",
                                                CreatedDate = item.@case.DateReported,
                                                LastModifiedDate = item.@case.LastModifiedDate,
                                            },
                                            new Soap.Activity
                                            {
                                                Type = "ES",
                                                Summary = "Escalated to AR team",
                                                CreatedDate = item.@case.DateReported,
                                                LastModifiedDate = item.@case.LastModifiedDate,
                                            },
                                        } : new[] {
                                            new Soap.Activity
                                            {
                                                Type = "P",
                                                Summary = "Called the customer",
                                                Status = "Completed",
                                                CreatedDate = item.@case.DateReported,
                                                LastModifiedDate = item.@case.LastModifiedDate,
                                            },
                                        },
                                    twoActivities
                                        ? null
                                        : new Soap.CloseCase { Reason = "Resolved" }
                                ),
                            _ => default
                        };

                        return new
                        {
                            Case = item.@case,
                            Email = email,
                            Activities = activity,
                            Action = action,
                        };
                    });
                },
                async (@this, client, entity, ct) =>
                {
                    if (entity.Email != null)
                    {
                        var email = await client.PutAsync(entity.Email, ct);
                        await client.InvokeAsync(
                            email,
                            new Soap.LinkEntityToEmail
                            {
                                Type = "PX.Objects.CR.CRCase",
                                RelatedEntity = entity.Case.CaseID.Value,
                            }, ct);
                    }
                    if (entity.Activities != null)
                    {
                        foreach (var activity in entity.Activities)
                        {
                            var activity__ = await client.PutAsync(activity, ct);
                            await client.InvokeAsync(
                                activity__,
                                new Soap.LinkEntityToActivity
                                {
                                    Type = "PX.Objects.CR.CRCase",
                                    RelatedEntity = entity.Case.CaseID.Value,
                                }, ct);
                        }
                    }
                    if (entity.Action != null)
                        await client.InvokeAsync(new Soap.Case { CaseID = entity.Case.CaseID.ToSearch() }, entity.Action, ct);
                },
                beforeGenerateDelegate: async (@this, client, ct) =>
                {
                    var cases__ = client.GetListAsync(
                            new Soap.Case
                            {
                                DateReported = new Soap.DateTimeSearch(
                                    DateTime.Parse("01/01/2020"),
                                    Soap.DateTimeCondition.IsGreaterThanOrEqualsTo)
                            }, ct);

                    var accounts = await client.GetListAsync(new Soap.BusinessAccount() /*{ MainContact = new Soap.BusinessAccountMainContact() }*/);
                    var cases____ = (await cases__)
                        .Where(c => c.ClassID.Value.IsIn("BILLING", "PRODSUPINC"))
                        .Select(c => (c, accounts.FirstOrDefault(a => a.BusinessAccountID == c.BusinessAccount).Email.Value));

                    cases = new ConcurrentQueue<(Soap.Case, string)>(cases____);
                    @this.ChangeGenerationCount(cases.Count, "надо");
                },
                afterGenerateDelegate: null);
        }



        private DateTime GetDateTime(int day)
        {
            var now = DateTime.Now;
            return new DateTime(2020, 1, day, now.Hour, now.Minute, now.Second, DateTimeKind.Local);
        }
    }
}
