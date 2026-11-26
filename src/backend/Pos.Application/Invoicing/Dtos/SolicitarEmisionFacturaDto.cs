namespace Pos.Application.Invoicing.Dtos;

public class SolicitarEmisionFacturaDto
{
    // Datos fiscales del receptor confirmados por el usuario
    public string RfcReceptor { get; set; } = string.Empty;
    public string RazonSocialReceptor { get; set; } = string.Empty;
    public string CodigoPostalReceptor { get; set; } = string.Empty;
    public string RegimenFiscalReceptor { get; set; } = string.Empty;
    public string UsoCfdi { get; set; } = string.Empty;
    public string EmailReceptor { get; set; } = string.Empty;

    // Opción para guardar los datos fiscales en el cliente para futuras ventas
    public bool GuardarDatosEnCliente { get; set; } = true;

    // Forma y método de pago (opcionalmente sobreescribible si el usuario lo ajusta)
    public string? FormaPago { get; set; }
    public string? MetodoPago { get; set; }

    // En caso de reintento manual tras error
    public string? IdempotencyKey { get; set; }
}

public class ResultadoEmisionFacturaDto
{
    public bool Exitoso { get; set; }
    public Guid? FacturaId { get; set; }
    public string? Uuid { get; set; }
    public string? Serie { get; set; }
    public string? Folio { get; set; }
    public string? Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? DetalleTecnico { get; set; }
    public string? TraceIdentifier { get; set; }
    public bool RequiereConfirmacionManual { get; set; }
}

public class SolicitarCancelacionFacturaDto
{
    public string MotivoCancelacion { get; set; } = "02"; // 01, 02, 03, 04
    public string? UuidSustitucion { get; set; }
    public string? MotivoDescripcion { get; set; }
}

public class ResultadoCancelacionFacturaDto
{
    public bool Exitoso { get; set; }
    public string? EstadoSat { get; set; }
    public string? EstadoCancelacionSat { get; set; }
    public string? EstadoLocal { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool TieneAcuseXml { get; set; }
}

public class EnviarFacturaCorreoDto
{
    public string EmailDestino { get; set; } = string.Empty;
    public string? MensajePersonalizado { get; set; }
}

public class ResultadoEstadoSatDto
{
    public string CodigoEstatus { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty; // Vigente, Cancelado, No Encontrado
    public string EsCancelable { get; set; } = string.Empty;
    public string EstatusCancelacion { get; set; } = string.Empty;
    public string ValidacionEfos { get; set; } = string.Empty;
    public string EstadoLocalActualizado { get; set; } = string.Empty;
    public DateTime FechaConsultaUtc { get; set; } = DateTime.UtcNow;
}

public class CatalogoSatItemDto
{
    public string Clave { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

