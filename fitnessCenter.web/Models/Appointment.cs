using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace fitnessCenter.web.Models
{
    public partial class Appointment
    {
        public int Id { get; set; }

        [Display(Name = "Üye")]
        public int MemberId { get; set; }

        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }

        [Display(Name = "Hizmet")]
        public int ServiceId { get; set; }

        [Display(Name = "Başlangıç Zamanı")]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [Display(Name = "Bitiş Zamanı")]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = "Pending";

        [ValidateNever]
        public virtual Member Member { get; set; } = null!;

        [ValidateNever]
        public virtual Trainer Trainer { get; set; } = null!;

        [ValidateNever]
        public virtual Service Service { get; set; } = null!;
    }
}
