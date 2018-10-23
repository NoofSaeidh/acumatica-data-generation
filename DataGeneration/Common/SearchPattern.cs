using DataGeneration.Soap;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataGeneration.Common
{
    public class SearchPattern
    {
        [Required]
        public string EntityType { get; set; }

        public DateTimeSearch CreatedDate { get; set; }
        // public DateTimeSearch ModifiedDate { get; set; }
        // may add many props, but need to map it in GenerationRunner
        public LinqPattern LinqPattern { get; set; }

        public void AdjustSearcher(EntitySearcher entitySearcher)
        {
            if (entitySearcher == null) throw new System.ArgumentNullException(nameof(entitySearcher));

            entitySearcher
                .AdjustInput(adj =>
                    adj.AdjustIf(!(CreatedDate is null), adj_ =>
                        adj_.AdjustIfIsOrThrow<ICreatedDateEntity>(e =>
                            e.CreatedDate = CreatedDate)));

            if(LinqPattern != null)
                entitySearcher
                    .AdjustOutput(adj =>
                        adj.AdjustIf(LinqPattern.Reverse, adj_ =>
                                adj_.Adjust(e => e.Reverse()))
                            .AdjustIf(LinqPattern.Skip != null, adj_ =>
                                adj_.Adjust(e => e.Skip(LinqPattern.Skip.Value)))
                            .AdjustIf(LinqPattern.Take != null, adj_ =>
                                adj_.Adjust(e => e.Take(LinqPattern.Take.Value))));
        }
    }

    public class LinqPattern
    {
        // Reverse executed first, than Skip, than Take
        public bool Reverse { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}