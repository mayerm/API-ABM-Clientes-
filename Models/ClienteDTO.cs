namespace TestABan.Models
{
    public class ClienteDTO
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellidos { get; set; }
        public required string FechaNacimiento { get; set; }
        public required string CUIT { get; set; }
        public string? Domicilio { get; set; }
        public string? Celular { get; set; }
        public required string Email { get; set; }
    }
}
