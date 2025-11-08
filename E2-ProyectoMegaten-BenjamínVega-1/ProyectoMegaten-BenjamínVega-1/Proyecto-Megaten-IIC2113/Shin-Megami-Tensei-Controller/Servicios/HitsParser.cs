using System.Text.RegularExpressions;

namespace Shin_Megami_Tensei.Manejo.Services
{
    public static class HitsParser
    {
        public static (int min, int max) Parse(string? textoCrudo)
        {
            if (string.IsNullOrWhiteSpace(textoCrudo)) return (1, 1);
            var texto = textoCrudo.Trim();
            if (texto.StartsWith("[") && texto.EndsWith("]"))
                texto = texto.Substring(1, texto.Length - 2).Trim();
            texto = texto.Replace('–', '-').Replace('—', '-');

            var coincidencia = Regex.Match(texto, @"\b(\d+)\s*-\s*(\d+)\b");
            if (coincidencia.Success)
            {
                int minimo = int.Parse(coincidencia.Groups[1].Value);
                int maximo = int.Parse(coincidencia.Groups[2].Value);
                if (minimo < 1) minimo = 1;
                if (maximo < minimo) maximo = minimo;
                return (minimo, maximo);
            }

            coincidencia = Regex.Match(texto, @"\b(\d+)\s*,\s*(\d+)\b");
            if (coincidencia.Success)
            {
                int minimo = int.Parse(coincidencia.Groups[1].Value);
                int maximo = int.Parse(coincidencia.Groups[2].Value);
                if (minimo < 1) minimo = 1;
                if (maximo < minimo) maximo = minimo;
                return (minimo, maximo);
            }

            coincidencia = Regex.Match(texto, @"\b(\d+)\b");
            if (coincidencia.Success)
            {
                int unico = int.Parse(coincidencia.Groups[1].Value);
                if (unico < 1) unico = 1;
                return (unico, unico);
            }

            return (1, 1);
        }
    }
}