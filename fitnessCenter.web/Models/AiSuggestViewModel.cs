using System.ComponentModel.DataAnnotations;

namespace fitnessCenter.web.Models.ViewModels
{
    public class AiSuggestViewModel
    {
        [Required(ErrorMessage = "Lütfen hedefinizi yazın.")]
        [StringLength(300, ErrorMessage = "En fazla 300 karakter.")]
        [Display(Name = "Hedef / İstek")]
        public string Prompt { get; set; } = "";

        public string? Result { get; set; }
    }
}
