using System.ComponentModel.DataAnnotations;

namespace ProyectoFinal.Models
{
    public class Images
    {
        [Key]
        [Display(Name = "Id de la Imagen")]
        public int Id { get; set; }

        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Display(Name = "Ruta")]
        public string Route { get; set; }
    }
}
