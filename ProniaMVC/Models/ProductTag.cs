namespace ProniaMVC.Models
{
    public class ProductTag
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int TagId { get; set; }
        public int ColorId { get; set; }
        public int SizeId { get; set; }
        public Product Product { get; set; }
        public Tag Tag { get; set; }
        public Color Color { get; set; }
        public Size Size { get; set; }
    }
}
