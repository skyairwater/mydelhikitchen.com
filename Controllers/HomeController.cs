using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using EcommerceStore.Models;
using EcommerceStore.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // Added for Include and ToListAsync
using System.Threading.Tasks; // Added for async/await
using Microsoft.Extensions.Logging; // Added for ILogger

namespace EcommerceStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _context.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .ToListAsync();
        return View(categories);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
