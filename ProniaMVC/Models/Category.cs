using System.ComponentModel.DataAnnotations;

namespace ProniaMVC.Models
{
    public class Category:BaseEntity
    {
        [MaxLength(30, ErrorMessage ="aye duz emelli ad daxil ele dana")]
        public string Name { get; set; }
        //relational
        public List<Product>? Products { get; set; }
    }
}
