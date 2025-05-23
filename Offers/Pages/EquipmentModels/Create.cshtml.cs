using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Pages.EquipmentModelPage
{
    [Authorize(Policy = "CanAddEquipmentModel")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EquipmentModel EquipmentModel { get; set; }

        [BindProperty]
        public List<EquipmentModelFeature> Features { get; set; } = new List<EquipmentModelFeature>();

        public SelectList EquipmentList { get; set; }

        public List<Unit> Units { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadEquipmentList();
            Units = await _context.Units.OrderBy(u => u.Name).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetEquipmentFeaturesAsync(int equipmentId)
        {
            var features = await _context.EquipmentFeatures
                .Where(ef => ef.EquipmentId == equipmentId)
                .Select(ef => new
                {
                    featureKey = ef.FeatureKey,
                    featureValue = ef.FeatureValue,
                    unitId = ef.UnitId
                })
                .ToListAsync();

            return new JsonResult(features);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (EquipmentModel.EquipmentId == null || string.IsNullOrEmpty(EquipmentModel.Brand) || string.IsNullOrEmpty(EquipmentModel.Model))
            {
                await LoadEquipmentList();
                Units = await _context.Units.OrderBy(u => u.Name).ToListAsync();
                return Page();
            }

            _context.EquipmentModels.Add(EquipmentModel);
            await _context.SaveChangesAsync();

            // Add features
            foreach (var feature in Features)
            {
                feature.EquipmentModelId = EquipmentModel.Id;
                _context.EquipmentModelFeatures.Add(feature);
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Equipment model created successfully.";

            return RedirectToPage("./Index");
        }

        private async Task LoadEquipmentList()
        {
            EquipmentList = new SelectList(
                await _context.Equipment.OrderBy(e => e.Name).ToListAsync(),
                "Id",
                "Name"
            );
        }
    }
}