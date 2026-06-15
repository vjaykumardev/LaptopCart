using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopCart.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View(_context.Products.ToList());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                // Get the wwwroot path from the environment
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                // Clean and generate a unique filename
                string originalFileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName)
                                          .Replace(" ", "_"); // Remove spaces
                string extension = Path.GetExtension(product.ImageFile.FileName);
                string uniqueFileName = $"{originalFileName}_{Guid.NewGuid():N}{extension}";

                // Ensure the /images folder exists
                string imagesFolder = Path.Combine(wwwRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }

                // Path to save the image physically
                string filePath = Path.Combine(imagesFolder, uniqueFileName);

                // Save file to server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                // Save relative path (for Razor <img src=...>)
                product.ImagePath = "/images/" + uniqueFileName;

                // [Optional] Verify file was saved — useful for debugging
                string confirmPath = Path.Combine(wwwRootPath, product.ImagePath.TrimStart('/'));
                if (!System.IO.File.Exists(confirmPath))
                {
                    throw new FileNotFoundException("Image was not saved correctly", confirmPath);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Record Inserted Successfully";
                return RedirectToAction("Index");

            }
            else
            {
                return View(product);
            }

        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {

                // If a new image is uploaded, save it
                if (product.ImageFile != null && product.ImageFile.Length > 0)
                {
                    // Generate a unique file name
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                    var savePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", fileName);

                    // Ensure /images folder exists
                    var imagesDir = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);  //imagesDir is a variable name
                    }

                    // Save the new file
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await product.ImageFile.CopyToAsync(stream);
                    }

                    // Save relative path in database
                    product.ImagePath = "/images/" + fileName;
                }

                // Update product in DB
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["success"] = "Record Updated Successfully";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Optional: delete image file if needed
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }

                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            TempData["success"] = "Record Deleted Successfully";
            return RedirectToAction(nameof(Index));

        }
    }
}
