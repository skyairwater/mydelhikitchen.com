using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
        await Task.Yield(); // Simulate async work
        return View();
    }

    public async Task<IActionResult> Groceries()
    {
        var categories = await _context.Categories
            .Include(c => c.Products.Where(p => p.IsActive))
            .ToListAsync();
        return View(categories);
    }

    public async Task<IActionResult> Food()
    {
        await Task.Yield(); // Simulate async work
        return View();
    }

    public async Task<IActionResult> Catering()
    {
        await Task.Yield(); // Simulate async work
        return View();
    }

    public async Task<IActionResult> Contact()
    {
        await Task.Yield(); // Simulate async work
        return View();
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
