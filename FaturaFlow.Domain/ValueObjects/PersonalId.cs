using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaturaFlow.Domain.ValueObjects
{
    public class PersonalId
    {
        public string Value { get; init; }

        public PersonalId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("O Número de Identificação não pode estar vazio.");

            value = value.Trim().Replace(".", "").Replace("-", "").Replace(" ", "");

            if (!Validate(value))
                throw new Exception("Número de Identificação inválido (Erro no algoritmo de controlo).");

            Value = value;
        }

        private bool Validate(string taxId)
        {
            if (taxId.Length != 9 || !taxId.All(char.IsDigit))
                return false;

            int[] weights = { 9, 8, 7, 6, 5, 4, 3, 2 };
            int sum = 0;

            for (int i = 0; i < 8; i++)
            {
                sum += (taxId[i] - '0') * weights[i];
            }

            int remainder = sum % 11;
            int checkDigit = remainder < 2 ? 0 : 11 - remainder;

            int originalCheckDigit = taxId[8] - '0';

            return checkDigit == originalCheckDigit;
        }

        public override string ToString() => Value;
    }
}