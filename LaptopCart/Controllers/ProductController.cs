using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaptopCart.Controllers
{

    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userdId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userdId != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _context.CartItems.Count(x => x.UserId == userdId));
            }

            var products = _context.Products.ToList();
            return View(products);
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            return View(product);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(CartItem cartItem)
        {
            var userdId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartproduct = _context.CartItems.FirstOrDefault(x => x.ProductId == cartItem.ProductId && x.UserId == userdId);
            if (cartproduct != null)
            {
                cartproduct.Quantity += cartItem.Quantity;
                _context.CartItems.Update(cartproduct);
                await _context.SaveChangesAsync();
            }
            else
            {
                cartItem.Id = 0;
                cartItem.UserId = userdId;
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }



            return RedirectToAction("Index");
        }


    }
}