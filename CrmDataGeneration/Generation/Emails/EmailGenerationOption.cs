using CrmDataGeneration.Common;
using CrmDataGeneration.OpenApi.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrmDataGeneration.Generation.Emails
{
    public class EmailGenerationOption : GenerationOption<Email>
    {
        public ProbabilityCollection<int> EmailsCount { get; set; }

        public async Task<IEnumerable<Email>> GenerateEmails(GeneratorClient client, IEnumerable<Email> emails, CancellationToken cancellationToken = default)
        {
            var apiWrappedClient = client.GetApiWrappedClient<Email>();
            if (GenerateInParallel)
                return await apiWrappedClient.CreateAllInParallel(emails, MaxExecutionThreadsParallel, cancellationToken);
            else
                return await apiWrappedClient.CreateAllSequentially(emails, SkipErrorsSequent, cancellationToken);
        }
    }
}
