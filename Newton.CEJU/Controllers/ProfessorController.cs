﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Newton.CJU.Models;
using Newton.CJU.ViewModel;
using System.Net;
using Microsoft.AspNet.Identity.EntityFramework;
using Newton.CJU.DAL;

namespace Newton.CJU.Controllers
{
    [Authorize]
    public class ProfessorController : Controller
    {
        private CJUContext db = new CJUContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public ProfessorController()
        {

        }

        public ProfessorController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [Authorize(Roles = "Professor")]
        public ActionResult Index()
        {
            var role = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db)).FindByName("Professor");
            return View(UserManager.Users.Where(u => u.Roles.Select(r => r.RoleId).Contains(role.Id)).ToList());
        }

        [Authorize(Roles = "Professor")]
        public ActionResult Details(string id)
        {
            return View(GetViewModel(id));
        }

        [Authorize(Roles = "Professor")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Professor")]
        public ActionResult Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Usuario { Nome = model.Nome, UserName = model.Email, Email = model.Email };
                var result = UserManager.Create(user, model.Senha);
                if (result.Succeeded)
                {
                    UserManager.AddToRole(user.Id, "Professor");

                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [Authorize(Roles = "Professor")]
        public ActionResult Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = UserManager.FindById(id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(GetViewModel(usuario));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Professor")]
        public ActionResult Edit([Bind(Include = "Id,Nome")] ProfessorViewModel professor)
        {
            if (ModelState.IsValid)
            {
                Usuario usuario = UserManager.FindById(professor.Id);
                usuario.Nome = professor.Nome;
                UserManager.Update(usuario);
                return RedirectToAction("Index");
            }

            return View(professor);
        }

        [Authorize(Roles = "Professor")]
        public ActionResult Delete(string Id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Usuario usuario = UserManager.FindById(Id);
            if (usuario == null)
            {
                return HttpNotFound();
            }
            return View(GetViewModel(usuario));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Professor")]
        public ActionResult DeleteConfirmed(string id)
        {
            Usuario usuario = UserManager.FindById(id);
            if (UserManager.FindById(User.Identity.GetUserId()) != usuario)
                UserManager.Delete(usuario);
            return RedirectToAction("Index");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ProfessorViewModel GetViewModel(string id)
        {
            Usuario usuario = UserManager.FindById(id);
            return GetViewModel(usuario);
        }

        private static ProfessorViewModel GetViewModel(Usuario usuario)
        {
            return new ProfessorViewModel()
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                UserName = usuario.UserName
            };
        }
    }
}
