namespace DataGeneration.Core.Queueing
{
    public class LinqPattern
    {
        // Reverse executed first, than Skip, than Take
        public bool Reverse { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
    }
}