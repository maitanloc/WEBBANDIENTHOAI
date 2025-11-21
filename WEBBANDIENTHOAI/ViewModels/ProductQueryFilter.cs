namespace WEBBANDIENTHOAI.ViewModels
{
    public class ProductQueryFilter
    {
        public int? CategoryId { get; set; }
        public string? Brand { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public byte? StatusId { get; set; }
        public string? Search { get; set; }
        /// <summary>Examples: "price_asc", "price_desc", "name_asc", "newest"</summary>
        public string? SortBy { get; set; }
    }
}
