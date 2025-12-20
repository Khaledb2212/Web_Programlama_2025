using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Json;
using System.Text.Json;
using Web_Project.Models;


namespace Web_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<UserDetails> _userManager;
        private readonly IHttpClientFactory _httpClientFactory;

        public AdminController(UserManager<UserDetails> userManager, IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
        }

        // 1. DASHBOARD
        [HttpGet]
        public IActionResult Index()
        { 
            return View();
        }


        // 2. LIST TRAINERS
        [HttpGet]
        public async Task<IActionResult> Trainers()
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var response = await client.GetAsync("api/Trainers/GetTrainers");

            if (response.IsSuccessStatusCode)
            {
                var rawData = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
                var trainers = new List<TrainerDTO>();

                foreach (var item in rawData)
                {
                    // HELPER: Try to get property with Upper OR Lower case
                    int GetInt(string key)
                    {
                        if (item.TryGetProperty(key, out var val)) return val.GetInt32();
                        if (item.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out var val2)) return val2.GetInt32();
                        return 0;
                    }

                    string GetStr(string key)
                    {
                        if (item.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String) return val.GetString();
                        if (item.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out var val2) && val2.ValueKind == JsonValueKind.String) return val2.GetString();
                        return "";
                    }

                    var dto = new TrainerDTO
                    {
                        // Now handles both "trainerID" and "TrainerID"
                        TrainerID = GetInt("trainerID"),
                        ExpertiseAreas = GetStr("expertiseAreas"),
                        Description = GetStr("description")
                    };

                    // Handle Nested Person Object safely
                    JsonElement person;
                    if (item.TryGetProperty("person", out person) || item.TryGetProperty("Person", out person))
                    {
                        // Helper for nested object properties
                        int GetPersonInt(string key) =>
                            (person.TryGetProperty(key, out var v) || person.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out v)) ? v.GetInt32() : 0;

                        string GetPersonStr(string key) =>
                            (person.TryGetProperty(key, out var v) || person.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out v)) && v.ValueKind == JsonValueKind.String ? v.GetString() : "";

                        dto.PersonID = GetPersonInt("personID");
                        dto.FirstName = GetPersonStr("firstname");
                        dto.LastName = GetPersonStr("lastname");
                        dto.Phone = GetPersonStr("phone");

                        // Safe Email Check
                        var emailJson = person.TryGetProperty("email", out var e) ? e : (person.TryGetProperty("Email", out var e2) ? e2 : default);
                        dto.Email = (emailJson.ValueKind == JsonValueKind.String) ? emailJson.GetString() : "Registered User (Identity)";
                    }

                    trainers.Add(dto);
                }

                return View(trainers);
            }

            return View(new List<TrainerDTO>());
        }

        //// 3. CREATE TRAINER (GET)
        //[HttpGet]
        //public IActionResult CreateTrainer()
        //{ 
        //    return View(); 
        //}

        // 3. CREATE TRAINER (GET)
        [HttpGet]
        public async Task<IActionResult> CreateTrainer()
        {
            var model = new TrainerDTO();
            var client = _httpClientFactory.CreateClient("WebApi");

            // Fetch Services to populate the checkboxes
            var servicesResp = await client.GetAsync("api/Services/GetServices");
            if (servicesResp.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var allServices = await servicesResp.Content.ReadFromJsonAsync<List<ServiceDTO>>(options);

                model.AvailableServices = allServices?.Select(s => new SelectListItem
                {
                    Value = s.ServiceID.ToString(),
                    Text = $"{s.ServiceName} (${s.FeesPerHour}/hr)"
                }).ToList();
            }

            return View(model);
        }

        //// 3. CREATE TRAINER (POST)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> CreateTrainer(TrainerDTO dto)
        //{
        //    if (!ModelState.IsValid) return View(dto);
        //
        //    // A. Create Identity User
        //    var user = new UserDetails { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        //    var result = await _userManager.CreateAsync(user, dto.Password);
        //
        //    if (!result.Succeeded)
        //    {
        //        foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
        //        return View(dto);
        //    }
        //
        //    await _userManager.AddToRoleAsync(user, "Trainer");
        //
        //    // B. Create Person via API
        //    var client = _httpClientFactory.CreateClient("WebApi");
        //    var personData = new { UserId = user.Id, Firstname = dto.FirstName, Lastname = dto.LastName, Phone = dto.Phone, Email = dto.Email };
        //
        //    var personResp = await client.PostAsJsonAsync("api/People/PostPerson", personData);
        //    if (!personResp.IsSuccessStatusCode)
        //    {
        //        await _userManager.DeleteAsync(user); // Cleanup
        //        ModelState.AddModelError("", "Failed to save Person details.");
        //        return View(dto);
        //    }
        //
        //    var createdPerson = await personResp.Content.ReadFromJsonAsync<PersonIdHelper>();
        //
        //    // C. Create Trainer via API
        //    var trainerData = new { PersonID = createdPerson.PersonID, ExpertiseAreas = dto.ExpertiseAreas, Description = dto.Description };
        //    var trainerResp = await client.PostAsJsonAsync("api/Trainers/AddTrainer", trainerData);
        //
        //    if (trainerResp.IsSuccessStatusCode)
        //    {
        //        TempData["Success"] = $"Trainer {dto.FirstName} created!";
        //        return RedirectToAction(nameof(Trainers));
        //    }
        //
        //    ModelState.AddModelError("", "Failed to save Trainer details.");
        //    return View(dto);
        //}

        // 3. CREATE TRAINER (POST)


        // 3. CREATE TRAINER (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTrainer(TrainerDTO dto)
        {
            // Reload services if validation fails
            if (!ModelState.IsValid)
            {
                return await CreateTrainer();
            }

            // A. Create Identity User
            var user = new UserDetails { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
                return await CreateTrainer();
            }

            await _userManager.AddToRoleAsync(user, "Trainer");


            var client = _httpClientFactory.CreateClient("WebApi");
            var personDto = new
            {

                // Donot use User.FindFirstValue -> That is YOU (The Admin)
                // Use 'user.Id' -> That is the NEW TRAINER
                UserId = user.Id,
                Firstname = dto.FirstName,
                Lastname = dto.LastName,
                Phone = dto.Phone,
                Email = dto.Email
            };


            var personResp = await client.PostAsJsonAsync("api/People/PostPerson", personDto);

           

            if (!personResp.IsSuccessStatusCode)
            {
                //Delete the user we just created so we don't have "ghost" logins
                await _userManager.DeleteAsync(user);

                //Read the REAL error message from the API
                var errorContent = await personResp.Content.ReadAsStringAsync();

                //Show it to you
                ModelState.AddModelError("", $"API Error: {personResp.StatusCode} - {errorContent}");
                return await CreateTrainer();
            }

            var createdPerson = await personResp.Content.ReadFromJsonAsync<PersonIdHelper>();

            //Create Trainer via API
            var trainerData = new { PersonID = createdPerson.PersonID, ExpertiseAreas = dto.ExpertiseAreas, Description = dto.Description };
            var trainerResp = await client.PostAsJsonAsync("api/Trainers/AddTrainer", trainerData);

            if (trainerResp.IsSuccessStatusCode)
            {
                //Skills (Checkboxes)
                if (dto.SelectedServiceIds != null && dto.SelectedServiceIds.Any())
                {
                    var createdTrainerJson = await trainerResp.Content.ReadFromJsonAsync<JsonElement>();

                    int newTrainerId = createdTrainerJson.TryGetProperty("trainerID", out var idVal) ? idVal.GetInt32() :
                                      (createdTrainerJson.TryGetProperty("TrainerID", out var idVal2) ? idVal2.GetInt32() : 0);

                    if (newTrainerId > 0)
                    {
                        foreach (var serviceId in dto.SelectedServiceIds)
                        {
                            var skillPayload = new { TrainerId = newTrainerId, ServiceId = serviceId };
                            await client.PostAsJsonAsync("api/TrainerSkills/PostTrainerSkill", skillPayload);
                        }
                    }
                }

                TempData["Success"] = $"Trainer {dto.FirstName} created with skills!";
                return RedirectToAction(nameof(Trainers));
            }

            // If Trainer creation fails, log that too
            var trainerError = await trainerResp.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Failed to save Trainer details. API says: {trainerError}");
            return await CreateTrainer();
        }

        //// 4. EDIT TRAINER (GET)
        //[HttpGet]
        //public async Task<IActionResult> EditTrainer(int id)
        //{
        //    var client = _httpClientFactory.CreateClient("WebApi");
        //    // Correct URL with query string
        //    var response = await client.GetAsync($"api/Trainers/GetTrainer?id={id}");
        //
        //    if (!response.IsSuccessStatusCode) return NotFound();
        //
        //    var item = await response.Content.ReadFromJsonAsync<JsonElement>();
        //
        //    // HELPER: Try to get property with Upper OR Lower case to avoid crashes
        //    string GetStr(JsonElement el, string key)
        //    {
        //        if (el.TryGetProperty(key, out var val) && val.ValueKind == JsonValueKind.String) return val.GetString();
        //        if (el.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out var val2) && val2.ValueKind == JsonValueKind.String) return val2.GetString();
        //        return "";
        //    }
        //
        //    int GetInt(JsonElement el, string key)
        //    {
        //        if (el.TryGetProperty(key, out var val)) return val.GetInt32();
        //        if (el.TryGetProperty(char.ToUpper(key[0]) + key.Substring(1), out var val2)) return val2.GetInt32();
        //        return 0;
        //    }
        //
        //    // Manual Map: API JSON -> TrainerDTO
        //    var dto = new TrainerDTO
        //    {
        //        TrainerID = GetInt(item, "trainerID"),
        //        ExpertiseAreas = GetStr(item, "expertiseAreas"),
        //        Description = GetStr(item, "description")
        //    };
        //
        //    // Safely handle nested Person data
        //    JsonElement person;
        //    if (item.TryGetProperty("person", out person) || item.TryGetProperty("Person", out person))
        //    {
        //        dto.PersonID = GetInt(person, "personID");
        //        dto.FirstName = GetStr(person, "firstname");
        //        dto.LastName = GetStr(person, "lastname");
        //        dto.Phone = GetStr(person, "phone");
        //
        //        // CRITICAL FIX: Don't crash if Email is missing
        //        var email = GetStr(person, "email");
        //        dto.Email = string.IsNullOrEmpty(email) ? "Registered User (Identity)" : email;
        //    }
        //
        //    return View(dto);
        //}
        //
        //// 5. EDIT TRAINER (POST)
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditTrainer(TrainerDTO dto)
        //{
        //    // IMPORTANT: Remove Password validation because we don't require it for updates
        //    ModelState.Remove("Password");
        //
        //    if (!ModelState.IsValid) return View(dto);
        //
        //    var client = _httpClientFactory.CreateClient("WebApi");
        //
        //    var updateDto = new
        //    {
        //        TrainerID = dto.TrainerID,
        //        PersonID = dto.PersonID,
        //        ExpertiseAreas = dto.ExpertiseAreas,
        //        Description = dto.Description,
        //        FirstName = dto.FirstName,
        //        LastName = dto.LastName,
        //        Phone = dto.Phone
        //    };
        //
        //    var response = await client.PutAsJsonAsync($"api/Trainers/UpdateTrainer?id={dto.TrainerID}", updateDto);
        //
        //    if (response.IsSuccessStatusCode)
        //    {
        //        TempData["Success"] = "Trainer updated successfully!";
        //        return RedirectToAction(nameof(Trainers));
        //    }
        //
        //    ModelState.AddModelError("", "Failed to update trainer.");
        //    return View(dto);
        //}

        // 6. DELETE TRAINER

        // 4. EDIT TRAINER (GET) - NOW INCLUDES SKILLS

        [HttpGet]
        public async Task<IActionResult> EditTrainer(int id)
        {
            var client = _httpClientFactory.CreateClient("WebApi");

            // ---------------------------------------------------------
            // 1. Fetch the Trainer Data
            // ---------------------------------------------------------
            var response = await client.GetAsync($"api/Trainers/GetTrainer?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound($"Trainer not found (API Error: {response.StatusCode})");
            }

            // USE THE NEW HELPER CLASS HERE:
            // We read into 'ApiTrainer' because it matches the nested JSON structure.
            var apiTrainer = await response.Content.ReadFromJsonAsync<Web_Project.Models.ApiTrainer>();

            if (apiTrainer == null) return NotFound();

            // ---------------------------------------------------------
            // 2. Fetch All Services (for the "Add Skill" dropdown)
            // ---------------------------------------------------------
            var services = new List<ServiceDTO>();
            var servicesResp = await client.GetAsync("api/Services/GetServices"); 
            if (servicesResp.IsSuccessStatusCode)
            {
                services = await servicesResp.Content.ReadFromJsonAsync<List<ServiceDTO>>() ?? new List<ServiceDTO>();
            }

            // ---------------------------------------------------------
            // 3. MAP Nested Data -> Flat DTO
            // ---------------------------------------------------------
            var dto = new TrainerDTO
            {
                TrainerID = apiTrainer.TrainerID,
                PersonID = apiTrainer.PersonID,

                // Map Person info (Flattening the structure)
                FirstName = apiTrainer.Person?.FirstName,
                LastName = apiTrainer.Person?.LastName,
                Phone = apiTrainer.Person?.Phone,
                Email = apiTrainer.Person?.Email,

                // Map Trainer info
                ExpertiseAreas = apiTrainer.ExpertiseAreas,
                Description = apiTrainer.Description,

                // Map Skills (This fixes the "Invisible Skills" issue)
                AssignedSkills = apiTrainer.Skills?.Select(s => new TrainerSkillItem
                {
                    Id = s.TrainerSkillID,       // The Link ID (needed for delete)
                    ServiceId = s.ServiceID,     // The Service ID
                    ServiceName = s.Service?.ServiceName ?? "Unknown" // The Name
                }).ToList() ?? new List<TrainerSkillItem>(),

                // Populate the dropdown list
                AvailableServices = services.Select(s => new SelectListItem
                {
                    Value = s.ServiceID.ToString(),
                    Text = $"{s.ServiceName} (${s.FeesPerHour}/hr)"
                })
            };

            return View(dto);
        }

        // 5. EDIT TRAINER (POST) - Updates Profile Info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTrainer(TrainerDTO dto)
        {
            ModelState.Remove("Password");
            ModelState.Remove("AssignedSkills"); // Don't validate these on profile save

            if (!ModelState.IsValid) return await EditTrainer(dto.TrainerID); // Reload page with data

            var client = _httpClientFactory.CreateClient("WebApi");
            var updateDto = new
            {
                TrainerID = dto.TrainerID,
                PersonID = dto.PersonID,
                ExpertiseAreas = dto.ExpertiseAreas,
                Description = dto.Description,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Phone = dto.Phone
            };

            var response = await client.PutAsJsonAsync($"api/Trainers/UpdateTrainer?id={dto.TrainerID}", updateDto);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Profile updated.";
            else
                TempData["Error"] = "Update failed.";

            // Redirect back to the SAME Edit page to see changes
            return RedirectToAction(nameof(EditTrainer), new { id = dto.TrainerID });
        }

        // --- NEW HELPER ACTIONS FOR SKILL CRUD ---

        [HttpPost]
        public async Task<IActionResult> AddSkillToTrainer(int trainerId, int serviceId)
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var payload = new { TrainerId = trainerId, ServiceId = serviceId };

            var response = await client.PostAsJsonAsync("api/TrainerSkills/PostTrainerSkill", payload);

            if (response.IsSuccessStatusCode) TempData["Success"] = "Skill added!";
            else TempData["Error"] = "Could not add skill.";

            return RedirectToAction(nameof(EditTrainer), new { id = trainerId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSkillFromTrainer(int skillId, int trainerId)
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var response = await client.DeleteAsync($"api/TrainerSkills/DeleteTrainerSkill?id={skillId}");

            if (response.IsSuccessStatusCode) TempData["Success"] = "Skill removed.";
            else TempData["Error"] = "Could not remove skill.";

            return RedirectToAction(nameof(EditTrainer), new { id = trainerId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTrainer(int id)
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var response = await client.DeleteAsync($"api/Trainers/DeleteTrainer?id={id}");

            if (response.IsSuccessStatusCode) TempData["Success"] = "Deleted successfully.";
            else TempData["Error"] = "Delete failed.";

            return RedirectToAction(nameof(Trainers));
        }

        // 7. TRAINER DETAILS (GET)
        [HttpGet]
        public async Task<IActionResult> TrainerDetails(int id)
        {

            var client = _httpClientFactory.CreateClient("WebApi");

            // ---------------------------------------------------------
            // 1. Fetch the Trainer Data
            // ---------------------------------------------------------
            var response = await client.GetAsync($"api/Trainers/GetTrainer?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound($"Trainer not found (API Error: {response.StatusCode})");
            }

            // USE THE NEW HELPER CLASS HERE:
            // We read into 'ApiTrainer' because it matches the nested JSON structure.
            var apiTrainer = await response.Content.ReadFromJsonAsync<Web_Project.Models.ApiTrainer>();

            if (apiTrainer == null) return NotFound();

            // ---------------------------------------------------------
            // 2. Fetch All Services (for the "Add Skill" dropdown)
            // ---------------------------------------------------------
            var services = new List<ServiceDTO>();
            var servicesResp = await client.GetAsync("api/Services/GetServices");
            if (servicesResp.IsSuccessStatusCode)
            {
                services = await servicesResp.Content.ReadFromJsonAsync<List<ServiceDTO>>() ?? new List<ServiceDTO>();
            }

            // ---------------------------------------------------------
            // 3. MAP Nested Data -> Flat DTO
            // ---------------------------------------------------------
            var dto = new TrainerDTO
            {
                TrainerID = apiTrainer.TrainerID,
                PersonID = apiTrainer.PersonID,

                // Map Person info (Flattening the structure)
                FirstName = apiTrainer.Person?.FirstName,
                LastName = apiTrainer.Person?.LastName,
                Phone = apiTrainer.Person?.Phone,
                Email = apiTrainer.Person?.Email,

                // Map Trainer info
                ExpertiseAreas = apiTrainer.ExpertiseAreas,
                Description = apiTrainer.Description,

                // Map Skills (This fixes the "Invisible Skills" issue)
                AssignedSkills = apiTrainer.Skills?.Select(s => new TrainerSkillItem
                {
                    Id = s.TrainerSkillID,       // The Link ID (needed for delete)
                    ServiceId = s.ServiceID,     // The Service ID
                    ServiceName = s.Service?.ServiceName ?? "Unknown" // The Name
                }).ToList() ?? new List<TrainerSkillItem>(),

                // Populate the dropdown list
                AvailableServices = services.Select(s => new SelectListItem
                {
                    Value = s.ServiceID.ToString(),
                    Text = $"{s.ServiceName} (${s.FeesPerHour}/hr)"
                })
            };

            return View(dto);
        }
        
        
        // ==========================================
        // SERVICES MANAGEMENT
        // ==========================================

        // 1. LIST SERVICES
        [HttpGet]
        public async Task<IActionResult> Services()
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var response = await client.GetAsync("api/Services/GetServices");

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var services = await response.Content.ReadFromJsonAsync<List<ServiceDTO>>(options) ?? new();
                return View(services);
            }

            return View(new List<ServiceDTO>());
        }

        // 2. CREATE SERVICE (GET)
        [HttpGet]
        public IActionResult CreateService()
        {
            return View();
        }

        // 2. CREATE SERVICE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateService(ServiceDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var client = _httpClientFactory.CreateClient("WebApi");

            // API expects POST to "api/Services/PostService"
            var response = await client.PostAsJsonAsync("api/Services/PostService", dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Service created successfully!";
                return RedirectToAction(nameof(Services));
            }

            ModelState.AddModelError("", "Failed to create service.");
            return View(dto);
        }

        // 3. EDIT SERVICE (GET)
        [HttpGet]
        public async Task<IActionResult> EditService(int id)
        {
            var client = _httpClientFactory.CreateClient("WebApi");

            // API uses Query String: ?id=
            var response = await client.GetAsync($"api/Services/GetService?id={id}");

            if (!response.IsSuccessStatusCode) return NotFound();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var service = await response.Content.ReadFromJsonAsync<ServiceDTO>(options);

            return View(service);
        }

        // 3. EDIT SERVICE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditService(ServiceDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            var client = _httpClientFactory.CreateClient("WebApi");

            // API uses PUT "api/Services/PutService?id="
            var response = await client.PutAsJsonAsync($"api/Services/PutService?id={dto.ServiceID}", dto);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Service updated successfully!";
                return RedirectToAction(nameof(Services));
            }

            ModelState.AddModelError("", "Failed to update service.");
            return View(dto);
        }

        // 4. DELETE SERVICE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteService(int id)
        {
            var client = _httpClientFactory.CreateClient("WebApi");

            // API uses DELETE "api/Services/DeleteService?id="
            var response = await client.DeleteAsync($"api/Services/DeleteService?id={id}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Service deleted.";
            else
                TempData["Error"] = "Failed to delete service.";

            return RedirectToAction(nameof(Services));
        }

        // 5. SERVICE DETAILS
        [HttpGet]
        public async Task<IActionResult> ServiceDetails(int id)
        {
            var client = _httpClientFactory.CreateClient("WebApi");
            var response = await client.GetAsync($"api/Services/GetService?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Service not found.";
                return RedirectToAction(nameof(Services));
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var service = await response.Content.ReadFromJsonAsync<ServiceDTO>(options);

            return View(service);
        }

        // Helper class for Person ID response
        public class PersonIdHelper { public int PersonID { get; set; } }
    }
}