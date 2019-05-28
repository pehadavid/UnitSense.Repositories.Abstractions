using System.Collections.Generic;

namespace UnitSense.Repositories.Abstractions
{
    public class FilteredDataSetResult<T>
    {
        public int CurrentPage { get; set; }
        public int MaxPage { get; set; }
        public int NbPerPage { get; set; }
        public int TotalItems { get; set; }
        public IEnumerable<T> Results { get; set; }

        public FilteredDataSetResult()
        {
            
        }

        public FilteredDataSetResult(FilteredDataSetResult<T> source)
        {
            
        }
    }
}
