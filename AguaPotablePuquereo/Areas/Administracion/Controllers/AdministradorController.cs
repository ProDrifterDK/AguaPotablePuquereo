﻿using AguaPotablePuquereo.Areas.Administracion.Models;
using AguaPotablePuquereo.Models.SQL;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AguaPotablePuquereo.Areas.Administracion.Controllers
{
    public class AdministradorController : Base.BaseAdministrar
    {
        // GET: Administracion/Administrador
        #region Vistas

        public ActionResult Index()
        {
            ViewBag.SelectMes = SelectMes(BDD);
            return View();
        }

        #region Clientes
        public ActionResult MantencionCliente(int id)
        {
            var modelo = BDD.TBL_CLIENTE.Where(o => o.CLI_ID == id).ToList().Select(o => new ModelCliente
            {
                Id = o.CLI_ID,
                Nombre = o.CLI_NOMBRE,
                ApellidoM = o.CLI_APELLIDO_MATERNO,
                ApellidoP = o.CLI_APELLIDO_PATERNO,
                Rut = o.CLI_RUT,
                Cuenta = o.CLI_CUENTA,
            }).FirstOrDefault() ?? new ModelCliente();

            return View(modelo);
        }

        [HttpPost]
        public ActionResult MantencionCliente(ModelCliente model)
        {
            var cliente = BDD.TBL_CLIENTE.FirstOrDefault(o => o.CLI_ID == model.Id) ?? new TBL_CLIENTE();

            cliente.CLI_NOMBRE = model.Nombre;
            cliente.CLI_APELLIDO_MATERNO = model.ApellidoM;
            cliente.CLI_APELLIDO_PATERNO = model.ApellidoP;
            cliente.CLI_RUT = model.Rut;
            cliente.CLI_CUENTA = model.Cuenta;
            cliente.CLI_COMPLETO = model.Nombre + " " + model.ApellidoP + " " + model.ApellidoM;

            if (cliente.CLI_ID != 0)
            {
                BDD.TBL_CLIENTE.Attach(cliente);
                BDD.Entry(cliente).State = System.Data.Entity.EntityState.Modified;
            }
            else
            {
                cliente.CLI_CREADO = DateTime.Now;

                BDD.TBL_CLIENTE.Add(cliente);
                BDD.Entry(cliente).State = System.Data.Entity.EntityState.Added;
            }

            BDD.SaveChanges();

            return RedirectToAction("Index");
        }
        #endregion

        #endregion
        //Json

        //Cliente
        public JsonResult GetListaCliente()
        {
            try
            {
                var data = BDD.TBL_CLIENTE.ToList().Select(o => new
                {
                    Id = o.CLI_ID,
                    Nombre = o.CLI_COMPLETO,
                    Rut = o.CLI_RUT,
                    Cuenta = o.CLI_CUENTA,
                    TotalDeuda = o.TBL_DEUDA.Where(p => p.PAG_ID == null).Select(p => p.DEU_DEUDA).Sum(),
                }).ToList();

                return JsonExito("", data);
            }
            catch (Exception ex)
            {
                Logger(ex);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetListaDeudasPagadas()
        {
            try
            {
                var data = BDD.TBL_DEUDA.Where(o => o.PAG_ID != null && !o.DEU_CHECK).ToList().Select(o => new
                {
                    Id = o.DEU_ID,
                    Nombre = o.TBL_CLIENTE.CLI_COMPLETO,
                    Rut = o.TBL_CLIENTE.CLI_RUT,
                    Cuenta = o.TBL_CLIENTE.CLI_CUENTA,
                    Periodo = o.TBL_MES.MES_NOMBRE + "/" + o.DEU_PERIODO_ANO,
                    Fecha = o.TBL_PAGOS.PAG_FECHA,
                    Deuda = o.DEU_DEUDA,
                });

                return JsonExito("", data);
            }
            catch (Exception ex)
            {
                Logger(ex);
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult AsignarVisto(int id)
        {
            try
            {
                var deuda = BDD.TBL_DEUDA.FirstOrDefault(o => o.DEU_ID == id);

                deuda.DEU_CHECK = true;

                BDD.TBL_DEUDA.Attach(deuda);
                BDD.Entry(deuda).State = System.Data.Entity.EntityState.Modified;

                BDD.SaveChanges();

                return JsonExito();
            }
            catch (Exception ex)
            {
                Logger(ex);
                return JsonError("Hubo un error al realizar esta acción, consulte con el administrador del sistema.");
            }
        }

        public JsonResult JsonGetListaDeudas(int Id)
        {
            try
            {
                var data = BDD.TBL_DEUDA.Where(o => o.CLI_ID == Id).ToList().Select(o => new
                {
                    Periodo = o.TBL_MES.MES_NOMBRE + " " + o.DEU_PERIODO_ANO,
                    Monto = o.DEU_DEUDA,
                    Vence = o.DEU_PERIODO_VENCE.ToString("dd/MM/yyyy"),
                    CLiId = o.CLI_ID,
                    Id = o.DEU_ID,
                }).ToList();

                return JsonExito("", data);
            }
            catch (Exception ex)
            {
                Logger(ex);
                return JsonError("Ha habido un problema al cargar las deudas.");
            }
        }

        public JsonResult EliminarCliente(int id)
        {
            try
            {
                var cliente = BDD.TBL_CLIENTE.FirstOrDefault(o => o.CLI_ID == id);

                foreach (var item in cliente.TBL_DEUDA)
                {
                    if (item.PAG_ID != null)
                    {
                        var pago = BDD.TBL_PAGOS.FirstOrDefault(o => o.PAG_ID == item.PAG_ID);
                        pago.PAG_VIGENCIA = false;

                        BDD.TBL_PAGOS.Attach(pago);
                        BDD.Entry(pago).State = System.Data.Entity.EntityState.Modified;

                        BDD.SaveChanges();
                    }
                }

                BDD.Database.ExecuteSqlCommand("DELETE FROM TBL_DEUDA WHERE CLI_ID = " + id);

                BDD.TBL_CLIENTE.Remove(cliente);
                BDD.Entry(cliente).State = System.Data.Entity.EntityState.Deleted;

                BDD.SaveChanges();

                return JsonExito("Cliente eliminado con éxito");
            }
            catch (Exception ex)
            {
                Logger(ex);
                return JsonError("No se ha podido eliminar al cliente");
            }
        }

        public JsonResult AgregarDeuda(int monto, int mes, int ano, int cliente)
        {
            try
            {
                var periodo = new DateTime();
                periodo = periodo.AddYears(ano - periodo.Year);
                periodo = periodo.AddMonths((mes + 1) - periodo.Month);
                var deuda = new TBL_DEUDA
                {
                    DEU_DEUDA = monto,
                    DEU_PERIODO_ANO = ano,
                    MES_ID = mes,
                    DEU_PERIODO_VENCE = periodo,
                    CLI_ID = cliente,
                };

                BDD.TBL_DEUDA.Add(deuda);
                BDD.Entry(deuda).State = System.Data.Entity.EntityState.Added;

                BDD.SaveChanges();

                return JsonExito("Deuda asignada correctamente");
            }
            catch (Exception ex)
            {
                Logger(ex);
                return JsonError("No se pudo agregar la deuda, inténtelo mas tarde.");
            }
        }
    }
}