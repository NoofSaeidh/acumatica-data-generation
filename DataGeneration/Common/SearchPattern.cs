using DataGeneration.Soap;

namespace DataGeneration.Common
{
    public class SearchPattern
    {
        public string EntityType { get; set; }

        public DateTimeSearch CreatedDate { get; set; }
        // public DateTimeSearch ModifiedDate { get; set; }
        // may add many props, but need to map it in GenerationRunner
    }
}