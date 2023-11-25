namespace dlog.server.Infrasructure.Models.Returns
{
    public class PostReturn
    {
        public int? ID { get; set; }
        public string? Title { get; set; }
        public string? Body { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }
        public DateTime Date { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
