using System.Text.Json;

namespace Maggsoft.Core.Model
{
    public sealed record Error(string Code, string Description)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
    }
}
