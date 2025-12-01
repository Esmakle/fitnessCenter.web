using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public partial class Trainer
    {
        public int Id { get; set; }

        [Display(Name = "Fitness Center")]
        public int FitnessCenterId { get; set; }

        // Formdan bind / validate edilmesini istemiyoruz
        [ValidateNever]
        public virtual FitnessCenter FitnessCenter { get; set; } = null!;

        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = null!;

        [Display(Name = "Uzmanlık Notu")]
        public string? UzmanlikNotu { get; set; }

        [ValidateNever]
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();

        [ValidateNever]
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
