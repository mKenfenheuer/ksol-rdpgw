using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KSol.RDPGateway.Data;
using KSol.RDPGateway.Models;

namespace KSol.RDPGateway.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RDPResourcesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RDPResourcesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: RDPResources
        public async Task<IActionResult> Index()
        {
            return View(await _context.RDPResources.ToListAsync());
        }

        // GET: RDPResources/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rDPResource = await _context.RDPResources
                .FirstOrDefaultAsync(m => m.ResourceIdentifier == id);
            if (rDPResource == null)
            {
                return NotFound();
            }

            return View(rDPResource);
        }

        // GET: RDPResources/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RDPResources/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ResourceIdentifier,Name,Description")] RDPResource rDPResource)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rDPResource);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rDPResource);
        }

        // GET: RDPResources/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rDPResource = await _context.RDPResources.FindAsync(id);
            if (rDPResource == null)
            {
                return NotFound();
            }
            return View(rDPResource);
        }

        // POST: RDPResources/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("ResourceIdentifier,Name,Description")] RDPResource rDPResource)
        {
            if (id != rDPResource.ResourceIdentifier)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rDPResource);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RDPResourceExists(rDPResource.ResourceIdentifier))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rDPResource);
        }

        // GET: RDPResources/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rDPResource = await _context.RDPResources
                .FirstOrDefaultAsync(m => m.ResourceIdentifier == id);
            if (rDPResource == null)
            {
                return NotFound();
            }

            return View(rDPResource);
        }

        // POST: RDPResources/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var rDPResource = await _context.RDPResources.FindAsync(id);
            if (rDPResource != null)
            {
                _context.RDPResources.Remove(rDPResource);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RDPResourceExists(string id)
        {
            return _context.RDPResources.Any(e => e.ResourceIdentifier == id);
        }
    }
}
