namespace fitnessCenter.web.Models
{
    public partial class Service
    {
        public int Id { get; set; }

        public string Ad { get; set; } = null!;

        public int SureDakika { get; set; }

        public decimal Ucret { get; set; }
        public int DurationMinutes { get; set; } = 60;


        public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<TrainerService> TrainerServices { get; set; }
        = new List<TrainerService>();
    }
}
