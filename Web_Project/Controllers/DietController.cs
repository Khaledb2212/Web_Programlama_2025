using Microsoft.AspNetCore.Mvc;
using Web_Project.Models;
using Web_Project.Services;

namespace Web_Project.Controllers
{
    public class DietController : Controller
    {
        private readonly GeminiService _geminiService;
        private readonly IWebHostEnvironment _env;

        public DietController(GeminiService geminiService, IWebHostEnvironment env)
        {
            _geminiService = geminiService;
            _env = env;
        }

        [HttpGet]
        public IActionResult Index() => View(new DietAnalysisVm());

        [HttpPost]
        public async Task<IActionResult> Analyze(DietAnalysisVm model)
        {
            // Ensure folders exist
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            if (model.Photo != null && model.Photo.Length > 0)
            {
                // Read bytes once
                using var ms = new MemoryStream();
                await model.Photo.CopyToAsync(ms);
                var imageBytes = ms.ToArray();

                // Save original upload
                var originalName = $"{Guid.NewGuid()}_{model.Photo.FileName}";
                var originalPath = Path.Combine(uploadsFolder, originalName);
                await System.IO.File.WriteAllBytesAsync(originalPath, imageBytes);
                model.ImageUrl = "/uploads/" + originalName;

                // TEXT plan
                model.AnalysisResult = await _geminiService.AnalyzeImage(imageBytes, model.Photo.ContentType);

                // Optional IMAGE preview
                if (model.GeneratePreviewImage)
                {
                    var futureBytes = await _geminiService.GenerateFutureSelfPreviewFromPhoto(imageBytes, model.Photo.ContentType);

                    if (futureBytes != null && futureBytes.Length > 0)
                    {
                        var futureName = $"{Guid.NewGuid()}_future.png";
                        var futurePath = Path.Combine(uploadsFolder, futureName);
                        await System.IO.File.WriteAllBytesAsync(futurePath, futureBytes);
                        model.FuturePreviewImageUrl = "/uploads/" + futureName;
                    }
                }

                return View("Index", model);
            }

            // Manual stats
            if (model.Height == null || model.Weight == null || model.Age == null)
            {
                ModelState.AddModelError("", "Please enter Height, Weight, and Age, or upload a photo.");
                return View("Index", model);
            }

            model.AnalysisResult = await _geminiService.AnalyzeText(model.Height.Value, model.Weight.Value, model.Age.Value);

            if (model.GeneratePreviewImage)
            {
                var futureBytes = await _geminiService.GenerateFutureSelfPreviewFromStats(model.Height.Value, model.Weight.Value, model.Age.Value);
                if (futureBytes != null && futureBytes.Length > 0)
                {
                    var futureName = $"{Guid.NewGuid()}_future.png";
                    var futurePath = Path.Combine(uploadsFolder, futureName);
                    await System.IO.File.WriteAllBytesAsync(futurePath, futureBytes);
                    model.FuturePreviewImageUrl = "/uploads/" + futureName;
                }
            }

            return View("Index", model);
        }
    }
}
