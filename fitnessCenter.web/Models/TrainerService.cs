using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public class TrainerService
    {
        public int Id { get; set; }

        [Display(Name = "Eğitmen")]
        [Required]
        public int TrainerId { get; set; }

  
        [ValidateNever]
        public Trainer? Trainer { get; set; }

        [Display(Name = "Hizmet")]
        [Required]
        public int ServiceId { get; set; }

       
        [ValidateNever]
        public Service? Service { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
