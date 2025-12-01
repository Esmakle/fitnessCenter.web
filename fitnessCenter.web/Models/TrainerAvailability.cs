using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }
         
        [Display(Name = "Gün")]
        public DayOfWeek DayOfWeek { get; set; }   // Pazartesi, Salı vs.

        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }


        [ValidateNever]
        public Trainer Trainer { get; set; } = null!;
    }
}
