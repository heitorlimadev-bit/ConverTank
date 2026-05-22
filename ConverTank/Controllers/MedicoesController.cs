using Microsoft.AspNetCore.Mvc;
using ConverTank.Data;
using ConverTank.Models;

namespace ConverTank.Controllers
{
    public class MedicoesController : Controller
    {

        private ConteudoBanco context;

        public MedicoesController(ConteudoBanco context)
        {
            this.context = context;
        }

        public IActionResult Index(int Id)
        {

            var medicoes = context.Medicoes.Where(t => t.TanqueId == Id).ToList();
            

            ViewBag.TanqueId = Id;

            return View(medicoes);
        }

        public IActionResult Adicionar(int TanqueId)
        {

            ViewBag.TanqueId = TanqueId;

            return View();

        }
        [HttpPost]
        public IActionResult Adicionar(Medicao medicao)
        {

            var tanque = context.Tanques.Find(medicao.TanqueId);

            double raio = tanque.Raio;
            double comprimento = tanque.Comprimento;
            double altura = medicao.Altura;

            

            double litro = CalcularVolume(raio, comprimento, altura);



            medicao.DataMedicao = DateTime.Now;

            context.Medicoes.Add(medicao);


            context.SaveChanges();

            return RedirectToAction("index", new { id = medicao.TanqueId });

        }
    }
}
