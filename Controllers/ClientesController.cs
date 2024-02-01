using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TestABan.Models;

namespace ToDoWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly ClienteContext _context;
        private static readonly string[] dateTimeFormats = ["dd/MM/yyyy", "dd-MM-yyyy", "dd.MM.yyyy", "d-M-yyyy", "d.M.yyyy", "d/M/yyyy"];
        private static readonly Dictionary<string, string> regExFormats = new()
        {
            { "DeserializeCUIT", @"[\.\-\/]+" },
            { "ValidateCUIT", @"\b(20|23|24|27|30|33|34)(\D)?[0-9]{8}(\D)?[0-9]"},
            { "ValidateEmail", "^[A-Za-z0-9._%+-]+@[A-Za-z0-9-]+[.][A-Za-z.]{2,}$" },
        };
        public ClientesController(ClienteContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        // GET: api/Clientes
        /// <summary>
        /// Obtiene todos los clientes habilitados.
        /// </summary>
        ///<returns>Lista de clientes habilitados</returns>
        /// <remarks>
        /// No hay consideraciones especiales
        /// </remarks>
        /// <param name="employee"></param>
        /// <response code="200">Devuelve todos los clientes</response>
        /// <response code="404">Si no existen registros de clientes habilitados</response>          


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetClientes()
        {
            var clientes =  await _context.Clientes
                .Where(x => !x.Deshabilitado)
                .Select(x => ClienteDTOMapper(x))
                .ToListAsync();
            if(clientes.Count == 0) return NotFound("No existen registros de clientes habilitados.");
            return Ok(clientes);
        }

        /// <summary>
        /// Obtiene un cliente por ID.
        /// </summary>
        /// <returns>Un cliente</returns>
        /// <remarks>
        /// Introducir el ID del cliente requerido.
        /// 
        /// Ejemplo:
        /// 
        ///     Request:
        ///         GET api/Clientes/23
        ///     
        ///     Response:
        ///     {        
        ///       "id": 23,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "01/01/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Calle Falsa 123",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     }
        /// </remarks>
        /// <param name="id"></param>
        /// <response code="200">Devuelve todos los clientes</response>
        /// <response code="404">Si no existen registros de clientes habilitados</response>          
        // GET: api/Clientes/{id}

        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDTO>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("El cliente no existe o no fue dado de alta.");
            return ClienteDTOMapper(cliente);
        }

        /// <summary>
        /// Obtiene una lista de clientes habilitados que contengan cierta palabra en su nombre.
        /// </summary>
        /// <returns>lista de clientes que contenga la palabra especificada</returns>
        /// <remarks>
        /// Ejemplo:
        ///     Request:
        ///     GET api/Clientes/FilterByName/mi
        ///     
        ///     Response:
        ///     {        
        ///       "id": 23,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "01/01/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Calle Falsa 123",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     },
        ///     {        
        ///       "id": 24,
        ///       "nombre": "Milagros",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "01/06/1982",
        ///       "cuit": "20-33333333-7",
        ///       "domicilio": "Calle Falsa 123",
        ///       "celular": "+5491123459876",
        ///       "email": milandres@proveedor.com
        ///     },
        /// </remarks>
        /// <param name="nombre"></param>
        /// <response code="200">Devuelve todos los clientes que contengan la cadena de caracteres especificada</response>
        /// <response code="404">Si no existen registros de clientes habilitados con el criterio de búsqueda</response>  
        /// GET: api/Clientes/SearchByName/{nombre}

        [HttpGet("SearchByName/{nombre}")]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetClientesByNombre(string nombre)
        {
            var clientes = await _context.Clientes
                .Where(x => x.Nombre.Contains(nombre) && !x.Deshabilitado)
                .Select(x => ClienteDTOMapper(x))
                .ToListAsync();
            if (clientes.Count == 0) 
                return NotFound($"No existen registros de clientes con nombre {nombre}.");
            return clientes;
        }

        /// <summary>
        /// Modifica al cliente con id definido
        /// </summary>
        /// <returns>El cliente modificado</returns>
        /// <remarks>
        /// Tener en cuenta el formato de la fecha, CUIT y email.
        /// 
        /// 
        /// Email: debe estar en el siguiente formato: 'nombrecuenta@dominio.tipodominio'
        /// 
        /// 
        /// Fecha: Se debe utilizar el formato dd/mm/yyyy o d/m/yyyy con '-', '.' o '/' como separadores.
        /// 
        /// 
        /// CUIT: Se debe tener en cuenta que debe empezar con 20, 23, 24, 27, 30, 33 o 34, luego contener 8 dígitos y un dígito final. Se pueden usar '-', '.' o '/' como separadores, o no utilizar separadores. Al obtener el CUIT, se devuelve con formato XX-XXXXXXXX-X.
        /// 
        /// 
        /// Ejemplo:      
        /// 
        ///     GET api/Clientes/{id}
        ///     Request:
        ///     {        
        ///       "id": 23,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "25/10/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Av. Siempre viva 742",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     }
        ///     Response:
        ///     {        
        ///       "id": 23,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "25/10/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Av. Siempre viva 742",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     }
        ///     
        ///     
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="clienteDTO"></param>
        /// <response code="200">El cliente actualizado</response>
        /// <response code="404">Si no existe el cliente con dicho id</response>  
        /// <response code="400">Error al ingresar algún dato</response>  
        // PUT: api/Clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCliente(int id, ClienteDTO clienteDTO)
        {
            try
            {
                if (id != clienteDTO.Id)
                    return BadRequest("Los IDs ingresados no son iguales.");
                StringBuilder sb = ValidateCliente(clienteDTO, false);
                if (sb.Length != 0)
                    return BadRequest(new { ErrorMessage = sb.ToString() });
                var cliente = await _context.Clientes.FindAsync(id);
                if (cliente == null)
                    return NotFound("El cliente no existe o no fue dado de alta.");
                ClientUpdater(ref cliente, clienteDTO);
                await _context.SaveChangesAsync();
                return Ok(cliente);
            }
            catch (FormatException fe)
            {
                return BadRequest(fe.Message);
            }
            catch (DbUpdateConcurrencyException) when (!ClienteExists(id))
            {
                return NotFound("El cliente no existe o no fue dado de alta.");
            }            
        }

        /// <summary>
        /// Crea un cliente nuevo si no hay otro dado de alta con el mismo CUIT o email
        /// </summary>
        /// <returns>El cliente creado</returns>
        /// <remarks>
        /// Tener en cuenta las mismas consideraciones que el método PUT, con adición que valida si ya existe un CUIT o email en otro registro, ya que se los consideran campos únicos.
        /// Ejemplo:
        /// 
        ///     POST api/Clientes
        ///     Request:
        ///     {        
        ///       "id": 0,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "01/01/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Calle Falsa 123",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     }
        ///     
        ///     Response:
        ///     {        
        ///       "id": 23,
        ///       "nombre": "Miguel",
        ///       "apellidos": "Andres",
        ///       "fechaNacimiento": "01/01/1980",
        ///       "cuit": "20-33333333-9",
        ///       "domicilio": "Calle Falsa 123",
        ///       "celular": "+5491123456789",
        ///       "email": mandres@proveedor.com
        ///     }
        /// </remarks>
        /// <param name="clienteDTO">Objeto que representa los datos del cliente</param>
        /// <response code="200">Devuelve el cliente creado</response>
        /// <response code="400">Si falló la validación (por cuestiones de formato o existencia de otro cliente con mismos campos)</response>          
        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDTO>> PostCliente(ClienteDTO clienteDTO)
        {
            try
            {
                var sb = ValidateCliente(clienteDTO, true);
                if (sb.Length != 0) return BadRequest(new { ErrorMessage = sb.ToString() });
                var cliente = ClienteMapper(clienteDTO);
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();
                return CreatedAtAction(
                    nameof(GetCliente),
                    new { id = cliente.Id },
                    ClienteDTOMapper(cliente));
            }
            catch(FormatException fe)
            {
                return BadRequest(fe.Message);
            }
        }

        /// <summary>
        /// Elimina un cliente por ID
        /// </summary>
        /// <returns>Código 200 con mensaje satisfactorio</returns>
        /// <remarks>
        /// Ingresar el ID del cliente a eliminar
        /// 
        /// Ejemplo:
        /// 
        ///     DELETE api/Clientes/{id}
        ///     Response:
        ///     {        
        ///       "Cliente eliminado satisfactoriamente";
        ///     }
        /// </remarks>
        /// <param name="id">ID del cliente</param>
        /// <response code="200">Mensaje informando la eliminación del cliente</response>
        /// <response code="404">Si el cliente no existe</response>          

        // DELETE: api/Clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound("El cliente no existe o no fue dado de alta.");
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return Ok("Cliente eliminado satisfactoriamente");
        }

        /// <summary>
        /// Deshabilita un cliente
        /// </summary>
        /// <returns>Código 200 con mensaje satisfactorio</returns>
        /// <remarks>
        /// Ingresar el ID del cliente a deshabilitar. 
        /// 
        /// Este método se utiliza como reemplazo a eliminar, ya que no es ideal borrar datos de la base de datos a menos que sea estríctamente necesario.
        /// 
        /// Ejemplo:
        /// 
        ///     PATCH api/Clientes/{id}
        ///     Response:
        ///     {        
        ///       "Al cliente se lo ha dado de baja."
        ///     }
        /// </remarks>
        /// <param name="id">ID del cliente</param>
        /// <response code="200">Mensaje informando la deshabilitación del cliente</response>
        /// <response code="404">Si el cliente no existe</response>     
        [HttpPatch("{id}")]
        public async Task<IActionResult> DisableCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente?.Deshabilitado == true)
                return NotFound("El cliente no existe o no fue dado de alta.");
            await _context.Clientes.Where(c => c.Id.Equals(id)).ExecuteUpdateAsync(c => c.SetProperty(b => b.Deshabilitado, true));
            return Ok("Al cliente se lo ha dado de baja.");
        }
        private bool ClienteExists(int id) => _context.Clientes.Any(e => e.Id == id);
        
        private bool CUITExists(string cuit) => _context.Clientes.Any(e => e.CUIT.Equals(DeserializeCUIT(cuit)));

        private bool EmailExists(string email) => _context.Clientes.Any(e => e.Email.Equals(email));
        
        private StringBuilder ValidateCliente(ClienteDTO cliente, bool isPost)
        {
            StringBuilder sb = new();
            if (CheckNullMainClienteValues(cliente))
            {
                sb.Append("Existen campos obligatorios sin cargar. Asegurese de cargar Nombre, Apellido, Fecha de nacimiento, CUIT y Email").Append("\n\n");
                return sb;
            }
            if (!CheckOnlyLettersAndSpaces(cliente.Nombre))
                sb.Append("El campo ").Append(nameof(cliente.Nombre)).Append(" posee caracteres inválidos. ").Append("\n\n");
            if (!CheckOnlyLettersAndSpaces(cliente.Apellidos))
                sb.Append("El campo ").Append(nameof(cliente.Apellidos)).Append(" posee caracteres inválidos. ").Append("\n\n");
            if (!CheckEmail(cliente.Email))
                sb.Append("El campo ").Append(nameof(cliente.Email)).Append(" posee caracteres inválidos. ").Append("\n\n");
            if (!cliente.FechaNacimiento.All(f => Char.IsDigit(f) || f == '/' || f == '-' || f == '.'))
                sb.Append("El campo ").Append(nameof(cliente.FechaNacimiento)).Append(" posee caracteres inválidos. ").Append("\n\n");
            if (!CheckCUIT(cliente.CUIT))
                sb.Append("El campo ").Append(nameof(cliente.CUIT)).Append(" posee caracteres inválidos. ").Append("\n\n");
            if (isPost && sb.Length == 0)
            {
                if (CUITExists(cliente.CUIT))
                    sb.Append("Ya existe un registro con el campo ").Append(nameof(cliente.CUIT)).Append(". ").Append("\n\n");
                if (EmailExists(cliente.Email))
                    sb.Append("Ya existe un registro con el campo ").Append(nameof(cliente.Email)).Append(". ").Append("\n\n");
            }
            return sb;
        }
        //Mapping methods
        private static ClienteDTO ClienteDTOMapper(Cliente cliente) => new()
       {
           Id = cliente.Id,
           Nombre = cliente.Nombre,
           Apellidos = cliente.Apellidos,
           FechaNacimiento = ProcessDateToString(cliente.FechaNacimiento),
           CUIT = SerializeCUIT(cliente.CUIT),
           Domicilio = cliente.Domicilio,
           Celular = cliente.Celular,
           Email = cliente.Email
       };

        private static Cliente ClienteMapper(ClienteDTO clienteDTO) => new()
        {
            Nombre = clienteDTO.Nombre,
            Apellidos = clienteDTO.Apellidos,
            FechaNacimiento = ProcessStringToDate(clienteDTO.FechaNacimiento),
            CUIT = DeserializeCUIT(clienteDTO.CUIT),
            Domicilio = clienteDTO.Domicilio,
            Celular = clienteDTO.Celular,
            Email = clienteDTO.Email
        };
        private static void ClientUpdater(ref Cliente cliente, ClienteDTO clienteDTO)
        {
            cliente.Apellidos = clienteDTO.Apellidos;
            cliente.Celular = clienteDTO.Celular;
            cliente.Nombre = clienteDTO.Nombre;
            cliente.Email = clienteDTO.Email;
            cliente.CUIT = DeserializeCUIT(clienteDTO.CUIT);
            cliente.Domicilio = clienteDTO.Domicilio;
            cliente.FechaNacimiento = ProcessStringToDate(clienteDTO.FechaNacimiento);
        }

        //Validation Methods
        private static bool CheckOnlyLettersAndSpaces(string palabra) => palabra.All(p => Char.IsLetter(p) || p == ' ');

        private static bool CheckEmail(string email) => Regex.IsMatch(email, regExFormats["ValidateEmail"]);

        private static bool CheckCUIT(string cuit) => Regex.IsMatch(cuit, regExFormats["ValidateCUIT"]);
        
        private static bool CheckNullMainClienteValues(ClienteDTO cliente) =>
            string.IsNullOrEmpty(cliente.Email)
            || string.IsNullOrEmpty(cliente.CUIT)
            || string.IsNullOrEmpty(cliente.Nombre)
            || string.IsNullOrEmpty(cliente.Apellidos)
            || string.IsNullOrEmpty(cliente.FechaNacimiento);

        //Utility methods
        private static string DeserializeCUIT(string cuit) => Regex.Replace(cuit, regExFormats["DeserializeCUIT"], "");

        private static string SerializeCUIT(string cuit) => $"{cuit[..2]}-{cuit[2..^(1)]}-{cuit[^1]}";
        private static string ProcessDateToString(DateOnly dateOnly) => dateOnly.ToString(dateTimeFormats[0]);

        private static DateOnly ProcessStringToDate(string fecha) =>
            DateOnly.TryParseExact(fecha, dateTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly fechaNacimiento)
                ? fechaNacimiento
                : throw new FormatException("La fecha de nacimiento no está en un formato permitido. De preferencia, utilice el formato dd/MM/yyyy");

    }

}
