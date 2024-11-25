using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Base
{
    public class BasePaginatedList<T>
    {
        public IReadOnlyCollection<T> Items { get; private set; }

        // Total number of items
        public int TotalItems { get; private set; }

        // Current page number
        public int CurrentPage { get; private set; }

        // Total number of pages
        public int TotalPages { get; private set; }

        // Number of items per page
        public int PageSize { get; private set; }

        // Constructor to initialize the paginated list
        public BasePaginatedList(IReadOnlyCollection<T> items, int count, int pageNumber, int pageSize)
        {
            TotalItems = count;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            Items = items;
        }

        // Check if there is a previous page
        public bool HasPreviousPage => CurrentPage > 1;

        // Check if there is a next page
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
