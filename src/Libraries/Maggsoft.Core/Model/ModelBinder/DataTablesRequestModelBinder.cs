using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maggsoft.Core.Model.DataTables;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Maggsoft.Core.Model.ModelBinder;
    /// <summary>
    /// DataTables jQuery eklentisinden gelen query string parametrelerini
    /// DataTablesRequest modeline otomatik bind eder
    /// 
    /// Query String Format:
    /// draw=1
    /// start=0
    /// length=10
    /// search[value]=
    /// search[regex]=false
    /// columns[0][data]=id
    /// columns[0][name]=Id
    /// columns[0][searchable]=true
    /// columns[0][orderable]=true
    /// columns[0][search][value]=
    /// columns[0][search][regex]=false
    /// order[0][column]=0
    /// order[0][dir]=asc
    /// </summary>
    public class DataTablesRequestModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var query = bindingContext.HttpContext.Request.Query;

            // Draw
            var draw = 0;
            if (query.ContainsKey("draw") && int.TryParse(query["draw"], out var drawValue))
            {
                draw = drawValue;
            }

            // Start (pagination)
            var start = 0;
            if (query.ContainsKey("start") && int.TryParse(query["start"], out var startValue))
            {
                start = startValue;
            }

            // Length (page size)
            var length = 10; // default
            if (query.ContainsKey("length") && int.TryParse(query["length"], out var lengthValue))
            {
                length = lengthValue;
            }

            // Search
            var searchValue = query.ContainsKey("search[value]") 
                ? query["search[value]"].ToString() ?? string.Empty 
                : string.Empty;
            
            var searchRegex = false;
            if (query.ContainsKey("search[regex]") && bool.TryParse(query["search[regex]"], out var regexValue))
            {
                searchRegex = regexValue;
            }

            var search = new Search(searchValue, searchRegex);

            // Columns - DataTables'dan gelen kolon bilgileri
            var columns = new List<Column>();
            var columnIndex = 0;
            while (query.ContainsKey($"columns[{columnIndex}][data]"))
            {
                var columnData = query[$"columns[{columnIndex}][data]"].ToString() ?? string.Empty;
                var columnName = query.ContainsKey($"columns[{columnIndex}][name]") 
                    ? query[$"columns[{columnIndex}][name]"].ToString() ?? string.Empty 
                    : string.Empty;
                
                var columnSearchable = true;
                if (query.ContainsKey($"columns[{columnIndex}][searchable]") && 
                    bool.TryParse(query[$"columns[{columnIndex}][searchable]"], out var searchableValue))
                {
                    columnSearchable = searchableValue;
                }

                var columnOrderable = true;
                if (query.ContainsKey($"columns[{columnIndex}][orderable]") && 
                    bool.TryParse(query[$"columns[{columnIndex}][orderable]"], out var orderableValue))
                {
                    columnOrderable = orderableValue;
                }

                var columnSearchValue = query.ContainsKey($"columns[{columnIndex}][search][value]") 
                    ? query[$"columns[{columnIndex}][search][value]"].ToString() ?? string.Empty 
                    : string.Empty;

                var columnSearchRegex = false;
                if (query.ContainsKey($"columns[{columnIndex}][search][regex]") && 
                    bool.TryParse(query[$"columns[{columnIndex}][search][regex]"], out var columnRegexValue))
                {
                    columnSearchRegex = columnRegexValue;
                }

                // Column constructor: data, name, searchable, orderable, searchValue, searchRegex
                var column = new Column(
                    columnData, 
                    columnName, 
                    columnSearchable, 
                    columnOrderable, 
                    columnSearchValue, 
                    columnSearchRegex);

                columns.Add(column);
                columnIndex++;
            }

            // Order - Sıralama bilgilerini log'la (şimdilik kullanmıyoruz, servis içinde handle edilecek)
            var orderIndex = 0;
            var orderInfo = new List<string>();
            while (query.ContainsKey($"order[{orderIndex}][column]"))
            {
                if (int.TryParse(query[$"order[{orderIndex}][column]"], out var columnIdx))
                {
                    var dir = query.ContainsKey($"order[{orderIndex}][dir]") 
                        ? query[$"order[{orderIndex}][dir]"].ToString() ?? "asc" 
                        : "asc";
                    
                    if (columnIdx < columns.Count)
                    {
                        orderInfo.Add($"{columns[columnIdx].Data} {dir}");
                    }
                }
                orderIndex++;
            }
            
            if (orderInfo.Count > 0)
            {
                Console.WriteLine($"  Order: {string.Join(", ", orderInfo)}");
            }

            // ColumnCollection oluştur
            var columnCollection = new ColumnCollection(columns);

            // DataTablesRequest oluştur
            var request = new DataTablesRequest
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Columns = columnCollection
            };

            Console.WriteLine($"[ModelBinder] Parsed DataTablesRequest:");
            Console.WriteLine($"  Draw: {draw}, Start: {start}, Length: {length}");
            Console.WriteLine($"  Search: '{searchValue}'");
            Console.WriteLine($"  Columns: {columns.Count}");

            bindingContext.Result = ModelBindingResult.Success(request);
            return Task.CompletedTask;
        }
    }