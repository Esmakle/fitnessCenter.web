using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace fitnessCenter.web.Models
{
    public class TrainerAvailability
    {
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }

        public DayOfWeek DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }

        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }

        [ValidateNever]
        public Trainer Trainer { get; set; } = null!;

        // CHECKBOX gün seçimi için
        [NotMapped]
        public List<DayOfWeek> SelectedDays { get; set; } = new();
    }
}
