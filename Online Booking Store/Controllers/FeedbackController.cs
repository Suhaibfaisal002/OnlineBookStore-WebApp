using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Models;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class FeedbackController(ShradhaBookStoreContext context) : AppController
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new FeedbackPageViewModel
        {
            IsSignedIn = CurrentUserId.HasValue
        };

        if (CurrentUserId.HasValue)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId.Value);
            if (user is not null)
            {
                model.Form.FullName = user.FullName;
                model.Form.Email = user.Email;
            }

            model.Entries = await context.FeedbackEntries
                .Where(x => x.UserId == CurrentUserId.Value)
                .OrderByDescending(x => x.SubmittedOn)
                .Select(x => new FeedbackEntryViewModel
                {
                    Subject = x.Subject,
                    Message = x.Message,
                    SubmittedOn = x.SubmittedOn,
                    IsResolved = x.IsResolved,
                    AdminReply = x.AdminReply,
                    RepliedOn = x.RepliedOn
                })
                .ToListAsync();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit([Bind(Prefix = "Form")] FeedbackViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var pageModel = new FeedbackPageViewModel
            {
                Form = model,
                IsSignedIn = CurrentUserId.HasValue
            };

            if (CurrentUserId.HasValue)
            {
                pageModel.Entries = await context.FeedbackEntries
                    .Where(x => x.UserId == CurrentUserId.Value)
                    .OrderByDescending(x => x.SubmittedOn)
                    .Select(x => new FeedbackEntryViewModel
                    {
                        Subject = x.Subject,
                        Message = x.Message,
                        SubmittedOn = x.SubmittedOn,
                        IsResolved = x.IsResolved,
                        AdminReply = x.AdminReply,
                        RepliedOn = x.RepliedOn
                    })
                    .ToListAsync();
            }

            return View("Index", pageModel);
        }

        string fullName = model.FullName;
        string email = model.Email;

        if (CurrentUserId.HasValue)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.UserId == CurrentUserId.Value);
            if (user is not null)
            {
                fullName = user.FullName;
                email = user.Email;
            }
        }

        context.FeedbackEntries.Add(new Feedback
        {
            UserId = CurrentUserId,
            FullName = fullName,
            Email = email,
            Subject = model.Subject,
            Message = model.Message,
            SubmittedOn = DateTime.UtcNow,
            IsResolved = false
        });

        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Your feedback has been submitted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
