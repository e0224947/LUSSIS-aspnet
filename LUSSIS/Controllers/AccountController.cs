﻿using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using LUSSIS.Models;
using System.Collections.Generic;
using LUSSIS.Constants;
using LUSSIS.Repositories;
using LUSSIS.Models.WebDTO;
using LUSSIS.Emails;
using LUSSIS.Models.Account;

namespace LUSSIS.Controllers
{
    //Authors: Ton That Minh Nhat
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly EmployeeRepository _employeeRepo = new EmployeeRepository();
        private readonly DelegateRepository _delegateRepo = new DelegateRepository();

        public AccountController()
        {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            private set => _signInManager = value;
        }

        public ApplicationUserManager UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            private set => _userManager = value;
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe,
                shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    var employee = _employeeRepo.GetEmployeeByEmail(model.Email);
                    var cookie = new HttpCookie("Employee");
                    if (employee != null)
                    {
                        cookie["Name"] = employee.FullName;
                        cookie["DeptCode"] = employee.DeptCode;
                        cookie["EmpNum"] = employee.EmpNum.ToString();
                    }

                    Response.Cookies.Add(cookie);

                    //return RedirectToLocal(returnUrl);
                    return RedirectByRole(employee?.JobTitle);
                //return RedirectToAction("Index","Home");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                default:
                    ModelState.AddModelError("", @"Wrong email or password. Please try again.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (!IsEmailExist(model.Email))
                {
                    ModelState.AddModelError("", @"Email not exist in database");
                    return View(model);
                }

                var user = new ApplicationUser {UserName = model.Email, Email = model.Email};
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    var employee = _employeeRepo.GetEmployeeByEmail(model.Email);

                    UserManager.AddToRole(user.Id, employee.JobTitle);

                    var cookie = new HttpCookie("Employee")
                    {
                        ["Name"] = employee.FullName,
                        ["DeptCode"] = employee.DeptCode,
                        ["EmpNum"] = employee.EmpNum.ToString()
                    };

                    Response.Cookies.Add(cookie);

                    return RedirectByRole(employee.JobTitle);
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        private bool IsEmailExist(string email)
        {
            var emp = _employeeRepo.GetEmployeeByEmail(email);
            return emp != null;
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // Send an email with reset link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new {userId = user.Id, code},
                    protocol: Request.Url?.Scheme);
                var fullName = _employeeRepo.GetEmployeeByEmail(model.Email).FullName;
                var email = new LUSSISEmail.Builder().From("sa45team7@gmail.com").To(model.Email)
                    .ForResetPassword(fullName, callbackUrl).Build();
                new System.Threading.Thread(delegate() { EmailHelper.SendEmail(email); }).Start();
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session["Name"] = "";
            Session["Roles"] = new List<string>();
            Session["MyCart"] = new ShoppingCartDTO();
            return RedirectToAction("Index", "Home");
        }

        [HandleError]
        public ActionResult NotAuthorized()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                _employeeRepo.Dispose();
                _delegateRepo.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectByRole(string role)
        {
            switch (role)
            {
                case Role.Staff:
                    var empNum = Convert.ToInt32(Response.Cookies["Employee"]?["EmpNum"]);
                    var isDelegate = _delegateRepo.FindCurrentByEmpNum(empNum) != null;
                    return RedirectToAction("Index",
                        isDelegate ? "RepAndDelegate" : "Requisitions");
                case Role.Representative:
                    return RedirectToAction("Index", "Collection");
                case Role.DepartmentHead:
                    return RedirectToAction("Index", "RepAndDelegate");
                case Role.Clerk:
                    return RedirectToAction("Consolidated", "Requisitions");
                case Role.Supervisor:
                    return RedirectToAction("Index", "SupervisorDashboard");
                case Role.Manager:
                    return RedirectToAction("Index", "SupervisorDashboard");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties {RedirectUri = RedirectUri};
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion
    }
}