using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
        OfferItems = new List<OfferItem>();
    }

    [BindProperty]
    public Offer Offer { get; set; }

    [BindProperty]
    public OfferItem NewItem { get; set; }

    [TempData]
    public string StatusMessage { get; set; }

    public List<OfferItem> OfferItems { get; set; }
    public SelectList EquipmentList { get; set; }
    public SelectList CompanyList { get; set; }

    private async Task LoadDropDownLists()
    {
        EquipmentList = new SelectList(await _context.Equipment.ToListAsync(), "Id", "Name");
        CompanyList = new SelectList(await _context.Companies.ToListAsync(), "Id", "Name");
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDropDownLists();
        // Initialize session for offer items
        HttpContext.Session.Set("OfferItems", OfferItems);
        return Page();
    }

    public async Task<IActionResult> OnPostAddItemAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropDownLists();
            return Page();
        }

        // Load existing items from session
        OfferItems = HttpContext.Session.Get<List<OfferItem>>("OfferItems") ?? new List<OfferItem>();

        // Load related entities for display
        NewItem.Equipment = await _context.Equipment.FindAsync(NewItem.EquipmentId);
        NewItem.Company = await _context.Companies.FindAsync(NewItem.CompanyId);

        OfferItems.Add(NewItem);

        // Save back to session
        HttpContext.Session.Set("OfferItems", OfferItems);

        await LoadDropDownLists();
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveItemAsync(int index)
    {
        OfferItems = HttpContext.Session.Get<List<OfferItem>>("OfferItems") ?? new List<OfferItem>();
        
        if (index >= 0 && index < OfferItems.Count)
        {
            OfferItems.RemoveAt(index);
            HttpContext.Session.Set("OfferItems", OfferItems);
        }

        await LoadDropDownLists();
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        OfferItems = HttpContext.Session.Get<List<OfferItem>>("OfferItems") ?? new List<OfferItem>();

        if (!ModelState.IsValid || !OfferItems.Any())
        {
            if (!OfferItems.Any())
            {
                ModelState.AddModelError("", "At least one offer item is required.");
            }
            await LoadDropDownLists();
            return Page();
        }

        // Calculate total price
        Offer.TotalPrice = CalculateTotalPrice();
        Offer.CreatedDate = DateTime.UtcNow;

        // Add offer
        _context.Offers.Add(Offer);
        await _context.SaveChangesAsync();

        // Add offer items
        foreach (var item in OfferItems)
        {
            item.OfferId = Offer.Id;
            _context.OfferItems.Add(item);
        }
        await _context.SaveChangesAsync();

        // Clear session
        HttpContext.Session.Remove("OfferItems");

        return RedirectToPage("./Index");
    }

    public decimal CalculateTotalPrice()
    {
        return OfferItems.Sum(item => item.Price * item.Quantity);
    }
} 