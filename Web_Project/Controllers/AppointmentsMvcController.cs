using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

[Authorize]
public class AppointmentsMvcController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    public AppointmentsMvcController(IHttpClientFactory httpClientFactory)
    { _httpClientFactory = httpClientFactory; }

    [HttpGet]
    public async Task<IActionResult> Book(DateTime? date, int? serviceId)
    {
        var client = _httpClientFactory.CreateClient("WebApi");

        var vm = new BookAppointmentVm
        {
            Date = date ?? DateTime.Today,
            ServiceId = serviceId ?? 0
        };

        // load services for dropdown
        var servicesResp = await client.GetAsync("api/Services/GetServices");
        servicesResp.EnsureSuccessStatusCode();
        var services = await servicesResp.Content.ReadFromJsonAsync<List<dynamic>>() ?? new();

        vm.Services = services.Select(s => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = (string)s.serviceID.ToString(),
            Text = (string)s.serviceName
        }).ToList();

        // if user selected date+service, load available trainers
        if (vm.ServiceId != 0)
        {
            var availResp = await client.GetAsync($"api/Trainers/Available?date={vm.Date:yyyy-MM-dd}&serviceId={vm.ServiceId}");
            if (availResp.IsSuccessStatusCode)
            {
                var avail = await availResp.Content.ReadFromJsonAsync<List<dynamic>>() ?? new();
                vm.Trainers = avail.Select(t => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = (string)t.trainerID.ToString(),
                    Text = (string)t.trainerName + $" ({t.startTime}-{t.endTime})"
                }).ToList();
            }
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookAppointmentVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var client = _httpClientFactory.CreateClient("WebApi");

        var startAt = vm.Date.Date.Add(vm.StartTime);
        var endAt = vm.Date.Date.Add(vm.EndTime);

        var resp = await client.PostAsJsonAsync("api/Appointments/Book", new
        {
            TrainerId = vm.TrainerId,
            ServiceId = vm.ServiceId,
            StartAt = startAt,
            EndAt = endAt
        });

        if (resp.IsSuccessStatusCode)
            return RedirectToAction(nameof(MyAppointments));

        var body = await resp.Content.ReadAsStringAsync();
        ModelState.AddModelError("", $"Booking failed: {(int)resp.StatusCode} - {body}");
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> MyAppointments()
    {
        var client = _httpClientFactory.CreateClient("WebApi");
        var resp = await client.GetAsync("api/Appointments/MyAppointments");

        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            return Redirect("/Identity/Account/Login");

        var body = await resp.Content.ReadAsStringAsync();

        if (!resp.IsSuccessStatusCode)
            return Content($"API returned {(int)resp.StatusCode}: {body}");


        var json = await resp.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }
}
