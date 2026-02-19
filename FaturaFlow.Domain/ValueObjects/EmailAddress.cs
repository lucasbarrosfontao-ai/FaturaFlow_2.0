using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
	public class EmailAddress
	{
		public string? Value { get; init; }

		public EmailAddress(string? value)
		{
			// 1. Opcional: Se for nulo ou vazio, aceitamos
			if (string.IsNullOrWhiteSpace(value))
			{
				Value = null;
				return;
			}

			value = value.Trim().ToLower(); // Emails são normalmente minúsculos

			// 2. Validação com Regex (mais independente que o DataAnnotations)
			if (!Validate(value))
				throw new Exception("O formato do e-mail é inválido.");

			Value = value;
		}

		private bool Validate(string email)
		{
			// Este Regex é um padrão seguro para validar a estrutura básica: texto@texto.texto
			var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			return Regex.IsMatch(email, pattern);
		}

		public override string ToString() => Value ?? "N/A";
	}
}