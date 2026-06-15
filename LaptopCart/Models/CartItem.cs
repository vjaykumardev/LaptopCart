using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LaptopCart.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }  //here id is added to the Product...which is naming convention for foreign key(compiler knows automatically)
        //[ForeignKey("ProductId")]          //if you didnot used naming convention, then specify as foreignkey
        public Product Product { get; set; }
        public string UserId { get; set; }
        public int Quantity { get; set; }
    }
}