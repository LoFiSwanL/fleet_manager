using FleetManager.Domain.Models;
using FleetManager.WebMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

namespace FleetManager.WebMVC.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAdminOneTimePassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ErrorMessage"] = "Будь ласка, введіть email.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByNameAsync(email) ?? await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Користувача з таким email не знайдено.";
                return RedirectToAction(nameof(Login));
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            if (!isAdmin)
            {
                TempData["ErrorMessage"] = "Для цього облікового запису не доступна опція одноразового пароля.";
                return RedirectToAction(nameof(Login));
            }

            var oneTime = Guid.NewGuid().ToString("N").Substring(0, 8);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, oneTime);
            if (!reset.Succeeded)
            {
                TempData["ErrorMessage"] = "Не вдалося згенерувати тимчасовий пароль. Спробуйте пізніше.";
                return RedirectToAction(nameof(Login));
            }

            var emailSender = HttpContext.RequestServices.GetService(typeof(FleetManager.WebMVC.Services.IEmailSender)) as FleetManager.WebMVC.Services.IEmailSender;
            if (emailSender != null && !string.IsNullOrEmpty(user.Email))
            {
                try
                {
                    await emailSender.SendEmailAsync(user.Email, "Ваш тимчасовий пароль", $"Ваш тимчасовий пароль: {oneTime}");
                    TempData["InfoMessage"] = "Тимчасовий пароль надіслано на вказаний email.";
                }
                catch (Exception)
                {
                    TempData["InfoMessage"] = "Тимчасовий пароль згенеровано. Але не вдалося надіслати email (перевірте налаштування SMTP).";
                }
            }
            else
            {
                TempData["InfoMessage"] = "Тимчасовий пароль згенеровано. (Email не налаштовано або відсутній)";
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl; 
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    user = await _userManager.FindByEmailAsync(model.UserName);
                }
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Користувача не знайдено.");
                    return View(model);
                }

                var checkPassword = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!checkPassword)
                {
                    ModelState.AddModelError(string.Empty, "Невірний пароль.");
                    return View(model);
                }

                var signInName = user.UserName;
                if (user != null && !user.IsActive.GetValueOrDefault(true))
                {
                    ModelState.AddModelError(string.Empty, "Ваш обліковий запис деактивовано. Зверніться до адміністратора.");
                    return View(model);
                }
                var result = await _signInManager.PasswordSignInAsync(signInName, model.Password, isPersistent: false, lockoutOnFailure: false);

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(string.Empty, "Вхід заборонено налаштуваннями безпеки.");
                    return View(model);
                }

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                        return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Невірний логін або пароль.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}