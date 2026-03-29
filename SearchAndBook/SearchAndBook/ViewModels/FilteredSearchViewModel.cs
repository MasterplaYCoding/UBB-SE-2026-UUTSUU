using SearchAndBook.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndBook.ViewModels
{
    internal class FilteredSearchViewModel
    {
        public ISearchAndFilterService SearchService { get; set; }

    }
}
