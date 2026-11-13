namespace Pos.Domain.Common;

public static class FacturaEstados
{
    public const string Borrador = "Borrador";
    public const string EnProceso = "EnProceso";
    public const string Timbrada = "Timbrada";
    public const string Rechazada = "Rechazada";
    public const string ResultadoPorConfirmar = "ResultadoPorConfirmar";
    public const string CancelacionSolicitada = "CancelacionSolicitada";
    public const string Cancelada = "Cancelada";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Borrador,
        EnProceso,
        Timbrada,
        Rechazada,
        ResultadoPorConfirmar,
        CancelacionSolicitada,
        Cancelada
    };
}

public static class FacturacionAmbientes
{
    public const string QA = "QA";
    public const string PR = "PR";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        QA,
        PR
    };
}

public static class SatCatalogosConstantes
{
    // Regímenes Fiscales SAT más comunes
    public static class RegimenesFiscales
    {
        public const string GeneralLeyPersonasMorales = "601";
        public const string PersonasMoralesFinesNoLucrativos = "603";
        public const string SueldosSalarios = "605";
        public const string Arrendamiento = "606";
        public const string DemasIngresos = "608";
        public const string ResidentesExtranjero = "610";
        public const string ActividadesEmpresarialesProfesionales = "612";
        public const string IngresosDividendos = "614";
        public const string SinObligacionesFiscales = "616";
        public const string IncorporacionFiscal = "621";
        public const string ActividadesEmpresarialesPlataformas = "625";
        public const string RegimenSimplificadoDeConfianza = "626";
    }

    // Usos de CFDI SAT v4.0
    public static class UsosCfdi
    {
        public const string AdquisicionMercancias = "G01";
        public const string DevolucionesDescuentos = "G02";
        public const string GastosEnGeneral = "G03";
        public const string Construcciones = "I01";
        public const string MobiliarioEquipoOficina = "I02";
        public const string EquipoComputo = "I04";
        public const string SinEfectosFiscales = "S01";
        public const string Pagos = "CP01";
        public const string Nomina = "CN01";
    }

    // Formas de Pago SAT
    public static class FormasPago
    {
        public const string Efectivo = "01";
        public const string ChequeNominativo = "02";
        public const string TransferenciaElectronica = "03";
        public const string TarjetaCredito = "04";
        public const string MonederoElectronico = "05";
        public const string DineroElectronico = "06";
        public const string ValesDespensa = "08";
        public const string TarjetaDebito = "28";
        public const string TarjetaServicios = "29";
        public const string PorDefinir = "99";
    }

    // Métodos de Pago SAT
    public static class MetodosPago
    {
        public const string PagoEnUnaSolaExhibicion = "PUE";
        public const string PagoEnParcialidadesODiferido = "PPD";
    }

    // Motivos de Cancelación SAT
    public static class MotivosCancelacion
    {
        public const string ComprobanteConErroresConRelacion = "01";
        public const string ComprobanteConErroresSinRelacion = "02";
        public const string NoSeLlevoACaboOperacion = "03";
        public const string OperacionNominativaFacturaGlobal = "04";
    }

    // RFCs genéricos del SAT
    public const string RfcGenericoNacional = "XAXX010101000";
    public const string RfcGenericoExtranjero = "XEXX010101000";

    // Claves SAT por defecto para Lambrín / WPC
    public const string ClaveProdServLambrinDefault = "30161801"; // Paneles de pared
    public const string ClaveUnidadPiezaDefault = "H87"; // Pieza
    public const string ClaveUnidadCajaDefault = "XBX"; // Caja
    public const string ClaveUnidadMetroCuadradoDefault = "MTK"; // Metro cuadrado
    public const string ObjetoImpuestoSiAplica = "02"; // Sí objeto de impuesto
}

