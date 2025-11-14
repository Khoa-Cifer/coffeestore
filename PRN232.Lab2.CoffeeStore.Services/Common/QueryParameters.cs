using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab2.CoffeeStore.Services.Common
{
    public class QueryParameters
    {
        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string SortOrder { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Select { get; set; }
    }
}
