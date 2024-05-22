namespace Maggsoft.Core.Model.DataTables;

public class Search
{
    public string Value { get; private set; }

    public bool IsRegexValue { get; private set; }

    public Search(string value, bool isRegexValue)
    {
        Value = value;
        IsRegexValue = isRegexValue;
    }
}
