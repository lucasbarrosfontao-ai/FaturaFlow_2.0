using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
    public class PostalCode
    {
        public string? Value { get; init; }

        public PostalCode(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Value = null;
                return;
            }

            value = value.Trim();

            if (!Validate(value))
                throw new Exception("Formato de Código Postal inválido (esperado: XXXX-XXX).");

            Value = value;
        }

        private bool Validate(string code)
        {
            return Regex.IsMatch(code, @"^\d{4}-\d{3}$");
        }

        public override string ToString() => Value ?? "N/A";
    }
}