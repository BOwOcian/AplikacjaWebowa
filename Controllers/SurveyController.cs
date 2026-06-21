using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SurveyApp.Data;
using SurveyApp.Models;
using SurveyApp.ViewModels;

namespace SurveyApp.Controllers
{
    [Authorize]
    public class SurveyController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SurveyController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var surveys = await _context.Surveys.Include(s => s.Options).ToListAsync();
            return View(surveys);
        }

		[Authorize(Roles = "Ankieter")]
		public IActionResult Create()
		{
			var model = new CreateSurveyViewModel();
			return View(model);
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Ankieter")]
        public async Task<IActionResult> Create(CreateSurveyViewModel model)
        {
            model.Options = model.Options.Where(o => !string.IsNullOrWhiteSpace(o)).ToList();

            if (ModelState.IsValid && model.Options.Count >= 2)
            {
                var user = await _userManager.GetUserAsync(User);
                var survey = new Survey
                {
                    Title = model.Title,
                    Description = model.Description,
                    CreatorId = user.Id,
                    CreatedAt = DateTime.Now
                };

                foreach (var optText in model.Options)
                {
                    survey.Options.Add(new Option { Text = optText });
                }

                _context.Surveys.Add(survey);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", "Ankieta musi mieć tytuł i co najmniej dwie opcje.");
            return View(model);
        }

        public async Task<IActionResult> Vote(int id)
        {
            var survey = await _context.Surveys.Include(s => s.Options).FirstOrDefaultAsync(s => s.Id == id);
            if (survey == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool hasVoted = await _context.Votes.AnyAsync(v => v.Option.SurveyId == id && v.VoterId == user.Id);
            
            if (hasVoted)
            {
                TempData["Message"] = "Już wypełniłeś tę ankietę.";
                return RedirectToAction(nameof(Results), new { id = id });
            }

            return View(survey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int surveyId, int optionId)
        {
            var user = await _userManager.GetUserAsync(User);
            bool hasVoted = await _context.Votes.AnyAsync(v => v.Option.SurveyId == surveyId && v.VoterId == user.Id);

            if (hasVoted)
            {
                return RedirectToAction(nameof(Results), new { id = surveyId });
            }

            var vote = new Vote
            {
                OptionId = optionId,
                VoterId = user.Id,
                VotedAt = DateTime.Now
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Results), new { id = surveyId });
        }

        public async Task<IActionResult> Results(int id)
        {
            var survey = await _context.Surveys
                .Include(s => s.Options)
                    .ThenInclude(o => o.Votes)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (survey == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            bool hasVoted = survey.Options.SelectMany(o => o.Votes).Any(v => v.VoterId == user.Id);

            var model = new ResultsViewModel
            {
                Survey = survey,
                HasVoted = hasVoted,
                OptionVotes = new Dictionary<string, int>(),
                TotalVotes = 0
            };

            foreach (var option in survey.Options)
            {
                int count = option.Votes.Count;
                model.OptionVotes.Add(option.Text, count);
                model.TotalVotes += count;
            }

            return View(model);
        }
    }
}