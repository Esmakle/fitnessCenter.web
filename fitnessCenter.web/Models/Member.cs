using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public partial class Member
    {
        public int Id { get; set; }

        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = null!;

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Fitness Center")]
        public int FitnessCenterId { get; set; }

        [ValidateNever]
        public virtual FitnessCenter FitnessCenter { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
