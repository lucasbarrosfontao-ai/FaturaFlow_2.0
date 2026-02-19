using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
    public class PostalCode
    {
        public string? Value { get; init; }

        public PostalCode(string? value)
        {
            // Se for nulo ou vazio, aceitamos (pois é opcional)
            if (string.IsNullOrWhiteSpace(value))
            {
                Value = null;
                return;
            }

            // Se não for nulo, limpamos espaços extras
            value = value.Trim();

            // Validamos o padrão de Portugal: 4 dígitos - 3 dígitos
            if (!Validate(value))
                throw new Exception("Formato de Código Postal inválido (esperado: XXXX-XXX).");

            Value = value;
        }

        private bool Validate(string code)
        {
            // Regex: 4 números, um hífen opcional, 3 números
            // O padrão oficial é 0000-000
            return Regex.IsMatch(code, @"^\d{4}-\d{3}$");
        }

        public override string ToString() => Value ?? "N/A";
    }
}