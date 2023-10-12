using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Controllers
{
    [Authorize(Roles = "Admin")]
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
                try
                {
                    await _subscriptionRepository.CreateAsync(subscription);

                    // Success.
                    return RedirectToAction(nameof(Index));
                }
                catch { }
            }

            AddModelError($"Could not create {nameof(Subscription)}.");
            return View(subscription);
        }


        // GET: Subscriptions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return SubscriptionNotFound();

            var subscription = await _subscriptionRepository.GetByIdAsync(id.Value);
            if (subscription == null) return SubscriptionNotFound();

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
                        return SubscriptionNotFound();
                    }
                }
                catch { }
            }

            AddModelError($"Could not update {nameof(Subscription)}.");
            return View(subscription);
        }


        // GET: Subscriptions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return SubscriptionNotFound();

            var subscription = await _subscriptionRepository.GetByIdAsync(id.Value);
            if (subscription == null) return SubscriptionNotFound();

            ViewBag.IsDeletable = ! await _subscriptionRepository.IsConstrainedAsync(subscription.ID);

            return PartialView("_ModalDelete", subscription);
        }

        // POST: Subscriptions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subscription = await _subscriptionRepository.GetByIdAsync(id);
            if (subscription == null) return SubscriptionNotFound();

            try
            {
                await _subscriptionRepository.DeleteAsync(subscription);

                // Success.
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException innerEx)
                {
                    if (innerEx.Message.Contains("FK_AspNetUsers_Subscriptions_SubscriptionID"))
                    {
                        ViewBag.ErrorTitle = "Cannot delete this Subscription.";
                        ViewBag.ErrorMessage =
                            "You can't delete this Subscription" +
                            " because it is assigned to at least one User.";
                    }
                }
            }
            catch { }

            return View("Error");
        }


        private void AddModelError(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
        }

        private ViewResult SubscriptionNotFound()
        {
            ViewBag.Title = "Subscription not found";
            ViewBag.ItemNotFound = "Subscription";

            Response.StatusCode = StatusCodes.Status404NotFound;
            return View("NotFound");
        }
    }
}
