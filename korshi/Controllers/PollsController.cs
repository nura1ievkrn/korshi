using korshi.Models;
using korshi.Services;
using korshi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace korshi.Controllers
{
    [Authorize]
    public class PollsController : Controller
    {
        private readonly PollService _pollService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PollsController(
            PollService pollService,
            UserManager<ApplicationUser> userManager)
        {
            _pollService = pollService;
            _userManager = userManager;
        }

        // ── GET /Polls ────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var polls = await _pollService.GetActivePollsAsync();

            var vm = new PollsIndexViewModel();

            foreach (var poll in polls)
            {
                var voteId = user != null
                    ? await _pollService.GetUserVoteAsync(poll.Id, user.Id)
                    : null;

                vm.Polls.Add(new PollCardViewModel
                {
                    Poll = poll,
                    UserVoteId = voteId
                });
            }

            ViewData["ActiveNav"] = "polls";
            return View(vm);
        }

        // ── GET /Polls/Create ─────────────────────────────
        public IActionResult Create()
        {
            ViewData["ActiveNav"] = "polls";
            return View(new CreatePollViewModel());
        }

        // ── POST /Polls/Create ────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePollViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ActiveNav"] = "polls";
                return View(model);
            }

            if (model.EndDate <= DateTime.Now)
            {
                ModelState.AddModelError("EndDate", "Дата окончания должна быть в будущем.");
                ViewData["ActiveNav"] = "polls";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var options = model.GetOptions();
            if (options.Count < 2)
            {
                ModelState.AddModelError("", "Нужно минимум 2 варианта ответа.");
                ViewData["ActiveNav"] = "polls";
                return View(model);
            }

            await _pollService.CreatePollAsync(
                model.Question, options, model.EndDate.ToUniversalTime(), user.Id);

            TempData["Success"] = "Голосование создано!";
            return RedirectToAction(nameof(Index));
        }

        // ── POST /Polls/Vote ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int pollId, int optionId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var (success, message) = await _pollService.VoteAsync(pollId, optionId, user.Id);

            if (success)
                TempData["Success"] = message;
            else
                TempData["Error"] = message;

            return RedirectToAction(nameof(Index));
        }

        // ── POST /Polls/Delete/{id} ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var isAdmin = await _userManager.IsInRoleAsync(user, "AdminOSI");
            await _pollService.DeletePollAsync(id, user.Id, isAdmin);

            TempData["Success"] = "Голосование удалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}