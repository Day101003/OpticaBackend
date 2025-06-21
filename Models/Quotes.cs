using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoFinal.Models
{
    public class Quotes
    {
        [Key]
        [Display(Name = "Id de la Cita")]
        public int Id { get; set; }

        [Display(Name = "Notas")]
        public string Notes { get; set; }

        [Display(Name = "¿Activa?")]
        public bool IsActive { get; set; } = true;

        [Required]
        [ForeignKey("Client")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        public Clients Client { get; set; }

        [Required]
        [ForeignKey("Availability")]
        [Display(Name = "Availability")]
        public int AvailabilityID { get; set; }

        public Availability Availability { get; set; }
    }
}
