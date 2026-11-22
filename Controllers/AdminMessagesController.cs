using EcommerceStore.Data;
using EcommerceStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceStore.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMessagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var messages = await _context.AppDynamicMessages
                .OrderBy(m => m.Tag)
                .ToListAsync();
            return View(messages);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AppDynamicMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppDynamicMessage model)
        {
            if (ModelState.IsValid)
            {
                // Check if Tag already exists
                var exists = await _context.AppDynamicMessages.AnyAsync(m => m.Tag == model.Tag);
                if (exists)
                {
                    ModelState.AddModelError("Tag", "A message with this tag name already exists.");
                    return View(model);
                }

                model.LastUpdated = DateTime.UtcNow;
                _context.AppDynamicMessages.Add(model);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Message '{model.Tag}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var message = await _context.AppDynamicMessages.FindAsync(id);
            if (message == null) return NotFound();
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AppDynamicMessage model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.AppDynamicMessages.FindAsync(id);
                if (existing == null) return NotFound();

                // Check if new Tag name is already taken by a different ID
                var duplicateTag = await _context.AppDynamicMessages
                    .AnyAsync(m => m.Tag == model.Tag && m.Id != id);
                if (duplicateTag)
                {
                    ModelState.AddModelError("Tag", "Another message with this tag name already exists.");
                    return View(model);
                }

                existing.Tag = model.Tag;
                existing.Content = model.Content;
                existing.LastUpdated = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Message '{model.Tag}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var message = await _context.AppDynamicMessages.FindAsync(id);
            if (message != null)
            {
                _context.AppDynamicMessages.Remove(message);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Message '{message.Tag}' deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
