namespace dlog.server.Infrasructure.Models.Helpers
{
    public class FilteredList<T> where T : class
    {
        public IEnumerable<T>? data { get; set; }

        public int totalItems { get; set; }

        public Filter? filter { get; set; }

        public T? filterModel { get; set; }

    }
    public class Filter
    {
        public Filter()
        {
            Keyword = "";
            CategoryID = 0;
            CategoryName = "";
            pageSize = 10;
            page = 1;
            SortBy = "desc";
        }

        public string? Keyword { get; set; }

        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }

        public string? SortBy { get; set; }

        public int page { get; set; }

        public int pageSize { get; set; }

        public Page? pager { get; set; }
    }
    public class Page
    {
        public Page(int totalItems, int pageSize, int currentPage = 1, int maxPages = 10)
        {
            var totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize);
            if (currentPage < 1)
            {
                currentPage = 1;
            }
            else if (currentPage > totalPages)
            {
                currentPage = totalPages;
            }

            int startPage, endPage;
            if (totalPages <= maxPages)
            {
                startPage = 1;
                endPage = totalPages;
            }
            else
            {
                var maxPagesBeforeCurrentPage = (int)Math.Floor((decimal)maxPages / (decimal)2);
                var maxPagesAfterCurrentPage = (int)Math.Ceiling((decimal)maxPages / (decimal)2) - 1;
                if (currentPage <= maxPagesBeforeCurrentPage)
                {
                    startPage = 1;
                    endPage = maxPages;
                }
                else if (currentPage + maxPagesAfterCurrentPage >= totalPages)
                {
                    startPage = totalPages - maxPages + 1;
                    endPage = totalPages;
                }
                else
                {
                    startPage = currentPage - maxPagesBeforeCurrentPage;
                    endPage = currentPage + maxPagesAfterCurrentPage;
                }
            }

            var startIndex = (currentPage - 1) * pageSize;
            var endIndex = Math.Min(startIndex + pageSize - 1, totalItems - 1);

            if (startIndex < 0 || endIndex < 0)
            {
                startIndex = 0;
                endIndex = 0;
            }

            var pages = Enumerable.Range(startPage, (endPage + 1) - startPage).ToList();

            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;
            StartPage = startPage;
            EndPage = endPage;
            StartIndex = startIndex;
            EndIndex = endIndex;
            Pages = pages;
        }

        public int TotalItems { get; private set; }
        public int CurrentPage { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages { get; private set; }
        public int StartPage { get; private set; }
        public int EndPage { get; private set; }
        public int StartIndex { get; private set; }
        public int EndIndex { get; private set; }
        public List<int> Pages { get; private set; }

    }
}
