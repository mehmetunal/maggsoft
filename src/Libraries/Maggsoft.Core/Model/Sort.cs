namespace Maggsoft.Core.Model
{
    public class Sort : ISort
    {
        public string Field { get; set; }
        public bool Asc { get; set; }
    }

    public interface ISort
    {
        string Field { get; set; }
        bool Asc { get; set; }
    }
}
