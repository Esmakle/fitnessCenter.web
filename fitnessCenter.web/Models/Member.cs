using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public partial class Member
    {
        public int Id { get; set; }

        [Display(Name = "Ad Soyad")]
        [Required(ErrorMessage = "Ad soyad zorunludur.")]
        public string AdSoyad { get; set; } = null!;

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        public string? Telefon { get; set; }

        [Display(Name = "Doğum Tarihi")]
        public DateOnly? DogumTarihi { get; set; }   // opsiyonel

        [Display(Name = "Cinsiyet")]
        public string? Cinsiyet { get; set; }        // "Kadın", "Erkek", "Belirtmek istemiyorum"

        [Display(Name = "Kayıt Tarihi")]
        public DateOnly KayitTarihi { get; set; } =
            DateOnly.FromDateTime(DateTime.Now);

        [Display(Name = "Fitness Center")]
        [Required(ErrorMessage = "Bir salon seçilmelidir.")]
        public int FitnessCenterId { get; set; }

        // --- Navigation Properties ---
        [ValidateNever]
        public virtual FitnessCenter FitnessCenter { get; set; } = null!;

        [ValidateNever]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
