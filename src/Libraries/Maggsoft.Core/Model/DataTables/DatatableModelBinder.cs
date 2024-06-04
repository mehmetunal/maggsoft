using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Maggsoft.Core.Model.DataTables
{
    public class DatatableModelBinder
    {
        static string COLUMN_DATA_FORMATTING = "columns[{0}][data]";
        static string COLUMN_NAME_FORMATTING = "columns[{0}][name]";
        static string COLUMN_SEARCHABLE_FORMATTING = "columns[{0}][searchable]";
        static string COLUMN_ORDERABLE_FORMATTING = "columns[{0}][orderable]";
        static string COLUMN_SEARCH_VALUE_FORMATTING = "columns[{0}][search][value]";
        static string COLUMN_SEARCH_REGEX_FORMATTING = "columns[{0}][search][regex]";
        static string ORDER_COLUMN_FORMATTING = "order[{0}][column]";
        static string ORDER_DIRECTION_FORMATTING = "order[{0}][dir]";

        public static Task<DataTablesRequest> BindModelAsync(IQueryCollection query)
        {
            DataTablesRequest dataTablesRequest = new();

            if (!string.IsNullOrEmpty(query["draw"].ToString()))
                dataTablesRequest.Draw = int.Parse(query["draw"].ToString());

            if (!string.IsNullOrEmpty(query["start"].ToString()))
                dataTablesRequest.Start = int.Parse(query["start"].ToString());

            if (!string.IsNullOrEmpty(query["length"].ToString()))
                dataTablesRequest.Length = int.Parse(query["length"].ToString());

            string value = query["search[value]"].ToString();

            if (!string.IsNullOrEmpty(value))
            {
                bool isRegexValue = bool.Parse(query["search[regex]"].ToString());
                dataTablesRequest.Search = new Search(value, isRegexValue);
            }

            List<Column> columns = GetColumns(query);
            ParseColumnOrdering(query, columns);

            dataTablesRequest.Columns = new ColumnCollection(columns);
            MapAditionalProperties(dataTablesRequest, query);


            return Task.FromResult(dataTablesRequest);
        }

        private static List<Column> GetColumns(IQueryCollection queryCollection)
        {
            try
            {
                List<Column> list = new List<Column>();
                for (int i = 0; i < queryCollection.Count; i++)
                {
                    string text = queryCollection[string.Format(COLUMN_DATA_FORMATTING, i)].ToString();
                    string text2 = queryCollection[string.Format(COLUMN_NAME_FORMATTING, i)].ToString();
                    if (!string.IsNullOrEmpty(text) && text2 != null)
                    {
                        bool searchable = bool.Parse(queryCollection[string.Format(COLUMN_SEARCHABLE_FORMATTING, i)].ToString());
                        bool orderable = bool.Parse(queryCollection[string.Format(COLUMN_ORDERABLE_FORMATTING, i)].ToString());
                        string searchValue = queryCollection[string.Format(COLUMN_SEARCH_VALUE_FORMATTING, i)].ToString();
                        bool isRegexValue = bool.Parse(queryCollection[string.Format(COLUMN_SEARCH_REGEX_FORMATTING, i)].ToString());
                        list.Add(new Column(text, text2, searchable, orderable, searchValue, isRegexValue));
                        continue;
                    }
                }

                return list;
            }
            catch
            {
                return new List<Column>();
            }
        }
        private static void ParseColumnOrdering(IQueryCollection queryCollection, IEnumerable<Column> columns)
        {
            for (int i = 0; i < queryCollection.Count; i++)
            {

                var txtNum = queryCollection[string.Format(ORDER_COLUMN_FORMATTING, i)];
                if (string.IsNullOrEmpty(txtNum))
                    continue;

                int num = int.Parse(txtNum);
                string text = queryCollection[string.Format(ORDER_DIRECTION_FORMATTING, i)];
                if (num > -1 && text != null)
                {
                    columns.ElementAt(num).SetColumnOrdering(i, text);
                }
            }
        }
        private static void MapAditionalProperties(DataTablesRequest requestModel, IQueryCollection requestParameters)
        {
        }
    }
}


/*
    columns[0][data]
    columns[0][name]
    columns[0][searchable]
    columns[0][orderable]
    columns[0][search][value]
    columns[0][search][regex]
    columns[1][data]
    columns[1][name]
    columns[1][searchable]
    columns[1][orderable]
    columns[1][search][value]
    columns[1][search][regex]
    order[0][column]
    order[0][dir]
    start
    length
    search[value]
    search[regex]         
 */
