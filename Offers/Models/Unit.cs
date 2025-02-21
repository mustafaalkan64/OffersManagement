using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Unit
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Birim Ad�")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "K�sa Kod")]
        [StringLength(10)]
        public string ShortCode { get; set; }
    }
}