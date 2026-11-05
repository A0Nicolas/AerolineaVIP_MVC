// CÓDIGO PROPIO - NO GENERADO POR EL FRAMEWORK.
// Roles funcionales de AeroVIP: la identidad se crea en Global.asax.cs (sin cambios).
// Los perfiles Pasajero / Operaciones / Comercial del análisis son propuestas; no son
// cuentas Identity adicionales mientras no exista una función y permiso comprobable.
namespace AerolineaVIP_MVC.Models
{
    public static class RolesAerolinea
    {
        public const string SuperUsuario = "SuperUsuario";
        public const string Administrador = "Administrador";
        public const string Counter = "Counter";
        public const string Cliente = "Cliente";
        public const string Gerente = "Gerente";
        public const string PersonalCounter = SuperUsuario + "," + Administrador + "," + Counter;
        public const string ConsultaGerencia = SuperUsuario + "," + Gerente;
    }
}
