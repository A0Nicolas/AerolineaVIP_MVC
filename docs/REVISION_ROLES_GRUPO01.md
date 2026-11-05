# AeroVIP — Revisión de roles y preparación de defensa | Grupo 01

**Integrantes:** Melany Torres y Nicolás García.  
**Versión de partida:** `Primera_Entrega.zip` (la versión funcional indicada por el grupo).  
**Referencia comparativa:** `Segunda_Entrega_Proyecto_Grupo04.zip` y `Segunda_Entrega_Presentacion_Grupo04.pptx`.  
**Fuentes normativas:** `Instrucciones_Segunda_Entrega.pdf`, `Enunciado_Segunda_Entrega.pdf`, `ENUNCIADO PROYECTO.pdf`, `Wireframes_21_Interfaces_Aerolinea_VIP.pdf`.

## 1. Resultado de la comparación, sin copiar el Grupo 04

El Grupo 04 organiza el sistema en cuatro roles Identity (`SuperUsuario`, `Administrador`, `Counter`, `Cliente`) y un módulo de counter, junto a su navegación y datos de demostración. **Su proyecto no es intercambiable con el del Grupo 01**: tiene namespaces, vistas, estructura de solución y módulos diferentes. Lo utilizamos solo como referencia arquitectónica y de presentación. No se incorporó código suyo.

El enunciado original, secciones 9.8 y 16, propone perfiles de cliente, pasajero, empleado de counter, administrador y responsables operativos/comerciales. **La matriz del capítulo 16 es una propuesta inicial, no exige sembrar seis cuentas Identity con esos nombres**. `Pasajero` existe como entidad del dominio, asociada a viajes; no es automáticamente un usuario con permiso de administración. Las instrucciones de segunda entrega exigen diez módulos y 21 interfaces. Por ello debe distinguirse el alcance *implementado y verificado* del alcance *pendiente*.

## 2. Matriz funcional del Grupo 01 — ajuste propuesto y aplicado

| Rol Identity | Puede hacer | No debe poder hacer | Comprobación |
|---|---|---|---|
| `SuperUsuario` | Gestión administrativa, crear cuentas, asignar roles, consultas de counter y gerencia; soporte al flujo del cliente | Cambiar su propio rol; alterar la cuenta semilla `SuperAdmin` | `/Admin/Gestion`, `/Admin/Roles`, `/Counter`, `/Gerencia` |
| `Administrador` | Panel, gestionar cuentas de menor privilegio, registrar/editar vuelos y aeronaves; consultar counter | Asignar roles o eliminar vuelos/aeronaves (operaciones reservadas a SuperUsuario) | `/Admin`, `/Aeronaves/Create`, `/Vueloes/Create`, `/Counter` |
| `Counter` | Consultar una reserva por su código completo, con su estado y pasajeros | Asignar roles; crear compras en nombre de otros mediante el flujo del cliente; CRUD administrativo | `/Counter` |
| `Cliente` | Buscar vuelos, reservar, pagar de manera simulada, consultar sus reservas, check-in y comprobantes vinculados a su cuenta | Consultar reservas ajenas, administración, counter o gerencia | `/Reservas/MisReservas`, `/CheckIn`, `/Comprobantes` |
| `Gerente` | Ver totales agregados y no personales de vuelos, aeronaves, reservas y compras | CRUD, datos personales de pasajeros, cambios de roles, cobros | `/Gerencia` |

**Importante:** siguen presentes los roles auxiliares `view`, `create`, `edit`, `delete` y `Usuario` en el sembrado original del taller de Identity. No deben confundirse con cinco nuevos perfiles funcionales; la autorización de las acciones se comprueba por rol en los controladores. No se modificó `Global.asax.cs`.

## 3. Cambios aplicados al código propio

- `Controllers/ReservasController.cs`, `ComprasController.cs`, `CheckInController.cs`, `ComprobantesController.cs`: se sustituyó la autorización genérica `[Authorize]` por `Cliente,SuperUsuario`. **No se cambiaron sus operaciones**, ni la lógica de transacción ni la consulta por titularidad.
- `Controllers/DiagnosticoBDController.cs`: disponible solo para `SuperUsuario` y `Administrador`.
- `Controllers/AdminController.cs`: el SuperUsuario no puede cambiar su propio rol; agregó ruta informativa `/Admin/Roles`.
- `Controllers/AeronavesController.cs`: el Administrador puede crear aeronaves; **eliminar** permanece solo para SuperUsuario. Vuelos mantiene alta/edición de Administrador y eliminación restringida.
- `Controllers/CounterController.cs`, `Views/Counter/Index.cshtml`: **nuevo módulo de consulta por código exacto**; no modifica reservas, pagos ni check-in. Su alcance es deliberadamente limitado, por lo que **no debe presentarse como todo el MOD-09**.
- `Controllers/GerenciaController.cs`, `Views/Gerencia/Index.cshtml`: **nuevo resumen de solo lectura**, sin datos personales.
- `Models/RolesAerolinea.cs` y `Models/ConsultasPersonalViewModels.cs`: constantes para roles y modelos de presentación; **sin regenerar el EDMX**.
- `Views/Admin/Roles.cshtml`: matriz de responsabilidades visibles.
- `Views/Shared/_Layout.cshtml` y `_LayoutMela.cshtml`: navegación por función y rol; los controles de servidor siguen siendo los que determinan el permiso.
- `Views/Aeronaves/Index.cshtml` y `Views/Vueloes/Index.cshtml`: ocultar **Eliminar** a Administrador, respetando los permisos del controlador.
- `Views/Vueloes/Index.cshtml`: corrección de la ruta `Vuloes` → `Vueloes` para el botón Crear.
- `AerolineaVIP_MVC.csproj`: inclusión explícita de los nuevos archivos en el proyecto MVC 5 clásico.

## 4. Funciones ya validadas por el Grupo 01 que **no se tocaron**

Inicio de sesión con SuperAdmin, registro inicial de cuentas/roles en `Global.asax.cs`, acceso a `AerolineaVIP` y `AerolineaVIP_Identity`, Entity Framework Database First, búsqueda de vuelos, reservas, pago simulado, comprobantes y check-in; todas se conservaron tal como aparecían en el ZIP. La nueva versión necesita ser recompilada y probada, **no afirmamos que las modificaciones ya hayan sido validadas en Visual Studio**.

## 5. Pruebas recomendadas en Visual Studio, por ambos integrantes

1. **Antes de tocar el repositorio**, respaldar la solución funcional y descomprimir el proyecto de prueba en otra carpeta. Abrir `AerolineaVIP_MVC.sln`, restaurar NuGet y usar `Compilar → Recompilar solución`.
2. **SuperAdmin** (credencial de demostración ya existente): `/Admin/Roles` muestra matriz, `/Admin/Gestion` permite asignar un rol a otra cuenta, `/Counter` y `/Gerencia` accesibles. Verificar que intentar cambiar su propio rol se deniega.
3. **Administrador:** `/Aeronaves/Create` y `/Vueloes/Create` accesibles. `/Aeronaves/Delete/1` y `/Vueloes/Delete/1` deben denegarse (no confirmar eliminación). `/Admin/Gestion` permite bloquear/desbloquear solo usuarios no privilegiados. No debe asignar roles.
4. **CounterDemo:** iniciar sesión; navegar a `/Counter`, consultar un **código de reserva REAL** existente en SQL Server y confirmar pasajero, vuelo, asiento y estado. `/Admin` y `/Compras/Pagar/1` no deben autorizarse.
5. **Gerente:** `/Gerencia` devuelve cuatro totales; `/Admin`, `/Counter` y `/Reservas/MisReservas` denegados.
6. **Cliente** (cuenta normal o `UsuarioDemo` que recibió rol Cliente): buscar un vuelo y recorrer **el mismo** flujo de reserva, pago simulado, check-in y comprobantes. No debe acceder a `/Admin`, `/Counter` ni `/Gerencia`.
7. Abrir dos navegadores o ventanas privadas para comprobar que los roles cambian con la cuenta y no con el menú. Un error de acceso debe proceder del servidor, no solo de un enlace oculto.
8. Comparar consultas de SQL antes/después: las tablas y relaciones del script original permanecen intactas; solo se guardan datos operativos cuando el usuario realiza la operación.

### Credenciales de laboratorio disponibles en el `Global.asax.cs` original

Son **solo cuentas ficticias para la demostración**, no contraseñas recomendadas para producción: `SuperAdmin`, `Administrador`, `Gerente`, `CounterDemo` y `UsuarioDemo` se inicializan con la contraseña de laboratorio `123`. No copiar credenciales reales ni cadenas locales a un repositorio público.

## 6. Repositorio y defensa — 4 de noviembre de 2026

- El PDF `Instrucciones_Segunda_Entrega.pdf` exige un **ZIP sin `bin/`, `obj/` ni `packages/`**, y un PowerPoint con GitHub público, credenciales **ficticias** de prueba y recorrido buscar → reservar → comprar → check-in. Se recomienda tag `v2.0-segunda-entrega` y commits de ambos integrantes.
- El script ya incluido en `Database/Script_BD_AerolineaVIP.sql` crea **dos bases comunes a los grupos**: `AerolineaVIP` y `AerolineaVIP_Identity`; **no** crear bases nuevas por grupo ni alterar el esquema.
- La versión de laboratorio conserva las cadenas de conexión con autenticación SQL que ya le funcionaban al grupo. **No subirla literalmente al GitHub público**, porque `Web.config` contiene credenciales locales.
- El ZIP de publicación reemplaza esos datos por `Integrated Security=True` y exige que Windows/SQL Server del laboratorio autorice la conexión. Si su entorno no acepta autenticación Windows, configurar credenciales seguras **solo localmente**, nunca en GitHub.
- El módulo de Counter añadido aquí **consulta reservas**. Aún queda pendiente, si el profesor lo solicita como alcance completo, diseñar el flujo autorizado para cambios de asiento, cobro y asistencia a check-in en nombre de un cliente; no se copió sin adaptar la implementación del Grupo 04.
- Se observó una diferencia entre la tabla de grupos del PDF de instrucciones y los nombres que ustedes identifican como Grupo 01. Confirmen que **Melany Torres y Nicolás García** aparecen en la presentación y en el enlace de su grupo antes de publicar; no sustituir la información por la de otro equipo.

## 7. Orden para la presentación (se elaborará tras las capturas)

1. Portada del Grupo 01: **Melany Torres y Nicolás García**.
2. Enlace público y versión exacta de GitHub, disponible en laboratorio.
3. Tabla de usuarios de **su** solución, credenciales ficticias y funciones demostrables.
4. Instalación: las dos bases y los dos contextos SQL/Identity; apertura de solución, NuGet, F5.
5. Navegación del cliente con capturas reales: búsqueda → asiento → pasajero → reserva → compra → check-in → comprobante.
6. Ejemplos de SuperUsuario, Administrador, Counter y Gerente con pruebas de autorización.
7. Tecnologías enseñadas por el docente: MVC 5, Entity Framework Database First, scaffolding, LINQ, Identity, `Global.asax`, Razor, Bootstrap, validaciones, transacciones SQL.

**No preparar PPT final con capturas inventadas**: se generará cuando el grupo pruebe y comparta sus imágenes reales.
