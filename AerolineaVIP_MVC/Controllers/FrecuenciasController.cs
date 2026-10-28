using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AerolineaVIP_MVC.Models;

namespace AerolineaVIP_MVC.Controllers
{
    // CÓDIGO PROPIO ADAPTADO - scaffold de Mela ajustado al único EDMX de Nico.
    // CRUD restringido a los roles administrativos.
    [Authorize(Roles = "SuperUsuario,Administrador")]
    public class FrecuenciasController : Controller
    {
        private AerolineaVIPEntities db = new AerolineaVIPEntities();

        // GET: Frecuencias
        public ActionResult Index()
        {
            var frecuencia = db.Frecuencias.Include(f => f.Ruta);
            return View(frecuencia.ToList());
        }

        // GET: Frecuencias/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            return View(frecuencia);
        }

        // GET: Frecuencias/Create
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult Create()
        {
            ViewBag.RutaId = new SelectList(db.Rutas, "RutaId", "RutaId");
            return View();
        }

        // POST: Frecuencias/Create
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult Create([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                db.Frecuencias.Add(frecuencia);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RutaId = new SelectList(db.Rutas, "RutaId", "RutaId", frecuencia.RutaId);
            return View(frecuencia);
        }

        // GET: Frecuencias/Edit/5
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            ViewBag.RutaId = new SelectList(db.Rutas, "RutaId", "RutaId", frecuencia.RutaId);
            return View(frecuencia);
        }

        // POST: Frecuencias/Edit/5
        // Para protegerse de ataques de publicación excesiva, habilite las propiedades específicas a las que quiere enlazarse. Para obtener 
        // más detalles, vea https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult Edit([Bind(Include = "FrecuenciaId,RutaId,DiaSemana,HoraSalida,HoraLlegada,Activa")] Frecuencia frecuencia)
        {
            if (ModelState.IsValid)
            {
                db.Entry(frecuencia).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.RutaId = new SelectList(db.Rutas, "RutaId", "RutaId", frecuencia.RutaId);
            return View(frecuencia);
        }

        // GET: Frecuencias/Delete/5
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            if (frecuencia == null)
            {
                return HttpNotFound();
            }
            return View(frecuencia);
        }

        // POST: Frecuencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperUsuario")]
        public ActionResult DeleteConfirmed(int id)
        {
            Frecuencia frecuencia = db.Frecuencias.Find(id);
            db.Frecuencias.Remove(frecuencia);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
