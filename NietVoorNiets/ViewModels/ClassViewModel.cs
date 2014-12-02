using System.ComponentModel.DataAnnotations;

namespace NietVoorNiets
{
    public class ClassViewModel
    {
        [Required]
        public string Name { get; set; }
    }
}