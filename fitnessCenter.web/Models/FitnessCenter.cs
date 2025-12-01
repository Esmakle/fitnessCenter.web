namespace fitnessCenter.web.Models
{
    public partial class FitnessCenter
    {
        public int Id { get; set; }

        public string Ad { get; set; } = null!;

        public string? Adres { get; set; }

        public TimeOnly? CalismaBaslangic { get; set; }

        public TimeOnly? CalismaBitis { get; set; }

        public string? Aciklama { get; set; }

        // İlişkiler – şimdilik sadece koleksiyonlar tanımlanıyor
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();

        public virtual ICollection<Trainer> Trainers { get; set; } = new List<Trainer>();

        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}
