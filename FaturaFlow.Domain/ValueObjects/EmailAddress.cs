using System.Text.RegularExpressions;

namespace FaturaFlow.Domain.ValueObjects
{
	public class EmailAddress
	{
		public string? Value { get; init; }

		public EmailAddress(string? value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				Value = null;
				return;
			}

			value = value.Trim().ToLower(); 

			if (!Validate(value))
				throw new Exception("O formato do e-mail é inválido.");

			Value = value;
		}

		private bool Validate(string email)
		{
			var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
			return Regex.IsMatch(email, pattern);
		}

		public override string ToString() => Value ?? "N/A";
	}
}