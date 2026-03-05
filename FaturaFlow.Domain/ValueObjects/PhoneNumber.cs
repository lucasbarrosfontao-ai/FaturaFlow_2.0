using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
    public class PhoneNumber
    {
        public string? Value { get; init; }

        public PhoneNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                Value = null;
                return;
            }

            string cleanedValue = Regex.Replace(value, @"[^\d+]", "");

            if (!Validate(cleanedValue))
                throw new Exception("Número de telefone inválido (deve ter entre 7 a 15 dígitos).");

            Value = cleanedValue;
        }

        private bool Validate(string phone)
        {
            return Regex.IsMatch(phone, @"^[\+]?\d{7,15}$");
        }

        public override string ToString() => Value ?? "N/A";
    }
}