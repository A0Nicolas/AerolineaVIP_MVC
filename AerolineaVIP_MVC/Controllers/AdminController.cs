using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
    // El administrador consulta y bloquea cuentas; solo SuperUsuario asigna roles.
    [Authorize(Roles = "SuperUsuario,Administrador")]
    public class AdminController : Controller
    {
        private static readonly string[] RolesPrincipales =
            { "SuperUsuario", "Administrador", "Counter", "Cliente", "Gerente" };

        public ActionResult Index() { return View(); }
        public ActionResult Vuelos() { return RedirectToAction("Index", "Vueloes"); }
        // Compatibilidad con el enlace /Admin/Vueloes usado en la interfaz.
        public ActionResult Vueloes() { return RedirectToAction("Index", "Vueloes"); }
        public ActionResult Aeronaves() { return RedirectToAction("Index", "Aeronaves"); }
        public ActionResult Reportes() { return View(); }

        public ActionResult Gestion()
        {
            using (var db = new ApplicationDbContext())
            using (var users = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
            {
                var datos = db.Users.OrderBy(u => u.UserName).ToList();
                var resultado = new List<UsuarioGestionViewModel>();
                foreach (var u in datos)
                {
                    var roles = users.GetRoles(u.Id);
                    resultado.Add(new UsuarioGestionViewModel
                    {
                        Id = u.Id, UserName = u.UserName, Email = u.Email,
                        Rol = RolesPrincipales.FirstOrDefault(x => roles.Contains(x)) ?? "Sin asignar",
                        Bloqueado = u.LockoutEnabled && u.LockoutEndDateUtc.HasValue &&
                            u.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow
                    });
                }
                return View(resultado);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult AsignarRol(string id, string rol)
        {
            if (string.IsNullOrWhiteSpace(id) || !RolesPrincipales.Contains(rol))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            using (var db = new ApplicationDbContext())
            using (var users = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
            {
                var cuenta = users.FindById(id);
                if (cuenta == null) return HttpNotFound();
                // Proteger al superusuario inicial para no perder el acceso de administración.
                if (cuenta.UserName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                foreach (var previo in users.GetRoles(cuenta.Id).ToList())
                {
                    if (RolesPrincipales.Contains(previo) ||
                        new[] { "view", "create", "edit", "delete", "Usuario" }.Contains(previo))
                    {
                        var r = users.RemoveFromRole(cuenta.Id, previo);
                        if (!r.Succeeded) { TempData["Error"] = string.Join("; ", r.Errors); return RedirectToAction("Gestion"); }
                    }
                }
                var resultado = users.AddToRole(cuenta.Id, rol);
                if (resultado.Succeeded)
                {
                    foreach (var permiso in Permisos(rol))
                    {
                        if (!users.IsInRole(cuenta.Id, permiso))
                        {
                            resultado = users.AddToRole(cuenta.Id, permiso);
                            if (!resultado.Succeeded) break;
                        }
                    }
                }
                if (resultado.Succeeded)
                {
                    users.UpdateSecurityStamp(cuenta.Id);
                    TempData["Exito"] = "Rol actualizado correctamente.";
                }
                else TempData["Error"] = string.Join("; ", resultado.Errors);
            }
            return RedirectToAction("Gestion");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CambiarEstado(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (id == User.Identity.GetUserId())
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            using (var db = new ApplicationDbContext())
            using (var users = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
            {
                var cuenta = users.FindById(id);
                if (cuenta == null) return HttpNotFound();
                if (users.IsInRole(id, "SuperUsuario") || users.IsInRole(id, "Administrador"))
                {
                    // Un Administrador no puede bloquear cuentas privilegiadas.
                    if (!User.IsInRole("SuperUsuario") ||
                        cuenta.UserName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                        return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                bool bloqueado = cuenta.LockoutEnabled && cuenta.LockoutEndDateUtc.HasValue &&
                    cuenta.LockoutEndDateUtc.Value > DateTimeOffset.UtcNow;
                cuenta.LockoutEnabled = true;
                cuenta.LockoutEndDateUtc = bloqueado
                    ? (DateTime?)null
                    : DateTime.UtcNow.AddYears(5); var r = users.Update(cuenta);
                if (r.Succeeded)
                {
                    users.UpdateSecurityStamp(id);
                    TempData["Exito"] = bloqueado ? "Cuenta activada." : "Cuenta bloqueada.";
                }
                else TempData["Error"] = string.Join("; ", r.Errors);
            }
            return RedirectToAction("Gestion");
        }

        private static string[] Permisos(string rol)
        {
            if (rol == "SuperUsuario" || rol == "Administrador")
                return new[] { "view", "create", "edit", "delete" };
            if (rol == "Counter" || rol == "Gerente") return new[] { "view" };
            return new string[0];
        }
    }
}
