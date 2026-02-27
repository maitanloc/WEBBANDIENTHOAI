namespace WEBBANDIENTHOAI.ViewModels
{
    public class ProductOptionVm
    {
        public int ProductOptionId { get; set; }
        public string GroupName { get; set; } = "";
        public string OptionName { get; set; } = "";
        public decimal AdditionalPrice { get; set; }
        public int DisplayOrder { get; set; }
    }
}
