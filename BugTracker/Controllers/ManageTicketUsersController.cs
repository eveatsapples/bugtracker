﻿using BugTracker.Models;
using BugTracker.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    public class ManageTicketUsersController : Controller
    {
        private ApplicationDbContext DbContext;

        public ManageTicketUsersController()
        {
            DbContext = new ApplicationDbContext();
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult EditTicketUsers(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(MyProjectsController.Index));
            }

            var ticket = DbContext.Tickets
              .FirstOrDefault(p => p.ID == id);

            var model = new EditTicketUsersViewModel
            {
                ID = ticket.ID,
                Title = ticket.Title
            };

            DbContext.SaveChanges();
            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult CurrentTicketUsers(int id)
        {
            var ticket = DbContext.Tickets
                .FirstOrDefault(t => t.ID == id);

            var assignedUser = DbContext.Users
                .FirstOrDefault(p => p.Id == ticket.AssignedToUserID);

            var model = new TicketUserViewModel();

            if (assignedUser != null)
            {
                model = new TicketUserViewModel
                {
                    ID = id,
                    UserID = assignedUser.Id,
                    UserName = assignedUser.UserName,
                    FirstName = assignedUser.FirstName,
                    LastName = assignedUser.LastName
                };

                DbContext.SaveChanges();
                return PartialView(model);
            }
            else
            {
                return PartialView(model);
            }

        }

        [HttpGet]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddTicketUsers(int id)
        {
            var ticket = DbContext.Tickets
                .FirstOrDefault(t => t.ID == id);

            var developerRole = DbContext.Roles.FirstOrDefault(r => r.Name == "Developer").Id;

            var unassignedDeveloperUsers = DbContext.Users.Where(u => u.Roles.Any(p => p.RoleId == developerRole))
                .Where(p => p.Id != ticket.AssignedToUserID)
                .Select(p => new TicketUserViewModel
                {
                    ID = id,
                    UserID = p.Id,
                    UserName = p.UserName,
                    FirstName = p.FirstName,
                    LastName = p.LastName
                }).ToList();

            DbContext.SaveChanges();
            return PartialView(unassignedDeveloperUsers);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult AddTicketUsers(TicketUserViewModel ticketUser)
        {
            var developerRole = DbContext.Roles.FirstOrDefault(r => r.Name == "Developer").Id;
            var unassignedDeveloperUsers = DbContext.Users;
            var user = DbContext.Users.Where(u => u.Roles.Any(p => p.RoleId == developerRole))
                .FirstOrDefault(
                p => p.Id == ticketUser.UserID);

            var ticket = DbContext.Tickets.FirstOrDefault(
                p => p.ID == ticketUser.ID);

            if (user == null)
            {
                return RedirectToAction(nameof(ManageProjectUsersController.EditProjectUsers));
            }

            ticket.AssignedToUserID = user.Id;

            DbContext.SaveChanges();
            return RedirectToAction("EditTicketUsers", "ManageTicketUsers", new { id = ticketUser.ID });
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult RemoveTicketUsers(TicketUserViewModel ticketUser)
        {
            var user = DbContext.Users.FirstOrDefault(
                p => p.Id == ticketUser.UserID);

            var ticket = DbContext.Tickets.FirstOrDefault(
                p => p.ID == ticketUser.ID);

            if (user == null)
            {
                return RedirectToAction(nameof(ManageProjectUsersController.EditProjectUsers));
            }

            ticket.AssignedToUserID = null;

            DbContext.SaveChanges();
            return RedirectToAction("EditTicketUsers", "ManageTicketUsers", new { id = ticketUser.ID });
        }
    }
}
