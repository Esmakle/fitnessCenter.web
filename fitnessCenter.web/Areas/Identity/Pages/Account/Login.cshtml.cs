
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace fitnessCenter.web.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        // Sadece bu email admin formundan giriş yapabilsin
        private const string AllowedAdminEmail = "kolesmanurr@gmail.com";

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            // Login.cshtml’den gelen hidden input: loginType=admin / member
            var loginType = Request.Form["loginType"].ToString();

            if (!ModelState.IsValid)
                return Page();

            // Kullanıcıyı bul (rol kontrolü için)
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // ----- ADMIN FORM KURALI -----
            if (string.Equals(loginType, "admin", StringComparison.OrdinalIgnoreCase))
            {
                // 1) Email sabit olmalı
                if (!string.Equals(Input.Email, AllowedAdminEmail, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(string.Empty, "Bu alandan sadece admin hesabı giriş yapabilir.");
                    return Page();
                }

                // 2) Rol Admin olmalı
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    ModelState.AddModelError(string.Empty, "Admin yetkiniz yok.");
                    return Page();
                }
            }
            // ----- MEMBER FORM KURALI -----
            else if (string.Equals(loginType, "member", StringComparison.OrdinalIgnoreCase))
            {
                // Member rolü yoksa üye girişini engelle
                if (!await _userManager.IsInRoleAsync(user, "Member"))
                {
                    ModelState.AddModelError(string.Empty, "Üye girişi için Member rolü gerekli.");
                    return Page();
                }
            }
            else
            {
                // loginType gelmezse / bozuksa
                ModelState.AddModelError(string.Empty, "Geçersiz giriş tipi.");
                return Page();
            }

            // Şartlar sağlandıysa normal sign-in
            var result = await _signInManager.PasswordSignInAsync(
                Input.Email,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }

            if (result.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

            if (result.IsLockedOut)
                return RedirectToPage("./Lockout");

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
