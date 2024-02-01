using System.ComponentModel.DataAnnotations;

namespace TestABan.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string CUIT {  get; set; }
        public string? Domicilio { get; set; }
        public string? Celular {  get; set; }
        public string Email {  get; set; }
        public bool Deshabilitado { get; set; }

    }
}
