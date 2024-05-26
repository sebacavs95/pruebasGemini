using Microsoft.AspNetCore.Mvc;
using pruebasGemini.Web.Models;
using PruebasGemini.Logica.Servicios;

namespace pruebasGemini.Web.Controllers
{
    public class IaController : Controller
    {
        private readonly IaService _iaService;

        public IaController(IaService iaService)
        {
            _iaService = iaService;
        }

        public IActionResult VistaPreguntas()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GenerateQuestions(string resumen)
        {
            try
            {
                var preguntas = await _iaService.GenerateQuestions(resumen);

                // Verifica si se generaron preguntas
                if (preguntas != null && preguntas.Any())
                {
                    // Pasa las preguntas como modelo a la vista MostrarPreguntas
                    return View("MostrarPreguntas", preguntas);
                }
                else
                {
                    ViewBag.Error = "No se pudieron generar preguntas.";
                    return View("VistaPreguntas");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al generar preguntas: " + ex.Message;
                return View("VistaPreguntas");
            }
        }

        public IActionResult MostrarPreguntas(string[] preguntas)
        {
      
            return View(preguntas);
        }



        [HttpPost]
        public async Task<IActionResult> CorregirRespuestas(string[] preguntas, string[] respuestas)
        {
            try
            {
                var feedback = await _iaService.GetFeedback(preguntas, respuestas);

                if (!string.IsNullOrEmpty(feedback))
                {
                    ViewBag.Feedback = feedback;
                    return View("MostrarFeedback");
                }
                else
                {
                    ViewBag.Error = "No se pudo obtener el feedback.";
                    return View("MostrarPreguntas", preguntas);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al obtener el feedback: " + ex.Message;
                return View("MostrarPreguntas", preguntas);
            }
        }

        public IActionResult MostrarFeedback()
        {
            return View();
        }
    }

}
