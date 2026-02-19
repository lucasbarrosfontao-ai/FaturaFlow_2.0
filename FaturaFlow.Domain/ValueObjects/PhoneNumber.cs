using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
    public class PhoneNumber
    {
        public string? Value { get; init; }

        public PhoneNumber(string? value)
        {
            // 1. Se for nulo ou vazio, aceitamos (opcional)
            if (string.IsNullOrWhiteSpace(value))
            {
                Value = null;
                return;
            }

            // 2. Limpeza: Manter apenas dígitos e o sinal +
            // Isso permite que o utilizador escreva "(+351) 912 345 678" 
            // e o sistema guarde "+351912345678"
            string cleanedValue = Regex.Replace(value, @"[^\d+]", "");

            // 3. Validação com o teu Regex
            if (!Validate(cleanedValue))
                throw new Exception("Número de telefone inválido (deve ter entre 7 a 15 dígitos).");

            Value = cleanedValue;
        }

        private bool Validate(string phone)
        {
            // O padrão que forneceste: Opcionalmente começa com +, seguido de 7 a 15 dígitos
            return Regex.IsMatch(phone, @"^[\+]?\d{7,15}$");
        }

        public override string ToString() => Value ?? "N/A";
    }
}