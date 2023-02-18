namespace Maggsoft.Core.Model
{
    public class Filter : IFilter
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
    }

    public interface IFilter
    {
        string Field { get; set; }
        string Operator { get; set; }
        object Value { get; set; }
    }
}
