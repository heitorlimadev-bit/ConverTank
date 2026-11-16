using System.Globalization;

namespace ConverTank.Services
{
    /// <summary>
    /// Formatação no padrão brasileiro (R$ 1.234,56). Usa uma cultura pt-BR "limpa",
    /// independente da cultura customizada configurada no Program.cs.
    /// </summary>
    public static class FormatoBR
    {
        public static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("pt-BR");

        public static string Moeda(decimal? valor) =>
            valor.HasValue ? valor.Value.ToString("C2", Cultura) : "—";

        public static string Ppl(decimal? valor) =>
            valor.HasValue ? "R$ " + valor.Value.ToString("0.000#", Cultura) : "—";

        public static string Numero(double? valor, int casas = 0) =>
            valor.HasValue ? valor.Value.ToString("N" + casas, Cultura) : "—";

        /// <summary>Valor para preencher um input de texto (ex.: "5,899").</summary>
        public static string Entrada(decimal? valor) =>
            valor.HasValue ? valor.Value.ToString("0.000#", Cultura) : string.Empty;
    }
}
