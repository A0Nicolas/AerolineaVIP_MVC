using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using AerolineaVIP_MVC.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace AerolineaVIP_MVC
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
            // Datos de DEMOSTRACIÓN de la universidad: no utilizar este sembrado en producción.
            InicializarCuentasYPermisos();
        }

        private static void InicializarCuentasYPermisos()
        {
            using (var db = new ApplicationDbContext())
            using (var roles = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db)))
            using (var usuarios = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db)))
            {
                // Rol y permisos según el ejercicio de Identity del ingeniero.
                foreach (var nombre in new[]
                {
                    "SuperUsuario", "Administrador", "Gerente", "Usuario", "Cliente", "Counter",
                    "view", "create", "edit", "delete"
                })
                {
                    if (!roles.RoleExists(nombre))
                    {
                        var resultado = roles.Create(new IdentityRole(nombre));
                        Comprobar(resultado, "crear el rol " + nombre);
                    }
                }

                // SOLO PARA LABORATORIO: el docente utilizará la clave 123.
                // Identity guarda el HASH de la clave, no texto plano en la base.
                usuarios.PasswordValidator = new PasswordValidator
                {
                    RequiredLength = 3,
                    RequireDigit = true,
                    RequireLowercase = false,
                    RequireUppercase = false,
                    RequireNonLetterOrDigit = false
                };

                CrearCuentaSiFalta(usuarios, "SuperAdmin", "superadmin@aerovip.test", "123", "SuperUsuario",
                    new[] { "view", "create", "edit", "delete" });
                CrearCuentaSiFalta(usuarios, "Administrador", "administrador@aerovip.test", "123", "Administrador",
                    new[] { "view", "create", "edit", "delete" });
                CrearCuentaSiFalta(usuarios, "Gerente", "gerente@aerovip.test", "123", "Gerente",
                    new[] { "view" });
                CrearCuentaSiFalta(usuarios, "UsuarioDemo", "usuariodemo@aerovip.test", "123", "Usuario",
                    new string[0]);
                CrearCuentaSiFalta(usuarios, "CounterDemo", "counter@aerovip.test", "123", "Counter", new[] { "view" });

                // Compatibilidad: los usuarios normales existentes reciben el rol Cliente.
                foreach (var existente in db.Users.ToList())
                {
                    if (!usuarios.IsInRole(existente.Id, "SuperUsuario") &&
                        !usuarios.IsInRole(existente.Id, "Administrador") &&
                        !usuarios.IsInRole(existente.Id, "Gerente") &&
                        !usuarios.IsInRole(existente.Id, "Counter") &&
                        !usuarios.IsInRole(existente.Id, "Cliente"))
                        ConcederSiFalta(usuarios, existente.Id, "Cliente");
                }
            }
        }

        private static void CrearCuentaSiFalta(UserManager<ApplicationUser> usuarios,
            string nombre, string correo, string clave, string rol, string[] permisos)
        {
            var cuenta = usuarios.FindByName(nombre);
            if (cuenta == null)
            {
                cuenta = new ApplicationUser { UserName = nombre, Email = correo };
                Comprobar(usuarios.Create(cuenta, clave), "crear la cuenta " + nombre);
            }

            // Si la cuenta ya existe, se conservan su contraseña y sus datos.
            // Se completan solamente los roles que le falten.
            ConcederSiFalta(usuarios, cuenta.Id, rol);
            foreach (var permiso in permisos)
                ConcederSiFalta(usuarios, cuenta.Id, permiso);
        }

        private static void ConcederSiFalta(UserManager<ApplicationUser> usuarios,
            string usuarioId, string rol)
        {
            if (!usuarios.IsInRole(usuarioId, rol))
                Comprobar(usuarios.AddToRole(usuarioId, rol), "asignar " + rol);
        }

        private static void Comprobar(IdentityResult resultado, string operacion)
        {
            if (!resultado.Succeeded)
                throw new InvalidOperationException("No se pudo " + operacion + ": " +
                    string.Join("; ", resultado.Errors));
        }
    }
}
