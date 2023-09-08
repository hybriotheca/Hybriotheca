using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    public class SubscriptionsController : Controller
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionsController(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }


        // GET: Subscriptions
        public IActionResult Index()
        {
            return View(_subscriptionRepository.GetAll());
            //return _context.Subscriptions != null ?
            //            View(await _context.Subscriptions.ToListAsync()) :
            //            Problem("Entity set 'DataContext.Subscriptions'  is null.");
        }


        // GET: Subscriptions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _subscriptionRepository.GetByIdAsync(id.Value);
            if (subscription == null) return NotFound();

            // Success.
            return View(subscription);
        }


        // GET: Subscriptions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subscriptions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                await _subscriptionRepository.CreateAsync(subscription);

                // Success.
                return RedirectToAction(nameof(Index));
            }

            AddModelError($"Could not create {nameof(Subscription)}.");
            return View(subscription);
        }


        // GET: Subscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _subscriptionRepository.GetByIdAsync(id.Value);
            if (subscription == null) return NotFound();

            // Success.
            return View(subscription);
        }

        // POST: Subscriptions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subscription subscription)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _subscriptionRepository.UpdateAsync(subscription);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _subscriptionRepository.ExistsAsync(subscription.ID))
                    {
                        return NotFound();
                    }
                    else throw;
                }
            }

            AddModelError($"Could not update {nameof(Subscription)}.");
            return View(subscription);
        }


        // GET: Subscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var subscription = await _subscriptionRepository.GetByIdWithUsers(id.Value);

            if (subscription == null) return NotFound();

            if (subscription.Users == null || !subscription.Users.Any())
            {
                // Success.
                return PartialView("_ModalDelete", subscription);
            }

            return PartialView("_ModalDeleteNotPossible", subscription);
        }

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription != null)
            {
                await _subscriptionRepository.DeleteAsync(subscription);
            }

            return RedirectToAction(nameof(Index));
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }
    }
}
