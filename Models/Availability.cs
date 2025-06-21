using System;
using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Availability
    {
        [Key]
        [Display(Name = "Id del Usuario")]
        public int Id { get; set; }

        [Display(Name = "Fecha Disponible")]
        [DataType(DataType.Date)]
        public DateTime AvailableDate { get; set; }

        [Display(Name = "Hora Disponible")]
        [DataType(DataType.Time)]
        public TimeSpan Hour { get; set; }

        [Display(Name = "¿Disponible?")]
        public bool Available { get; set; }
    }
}

