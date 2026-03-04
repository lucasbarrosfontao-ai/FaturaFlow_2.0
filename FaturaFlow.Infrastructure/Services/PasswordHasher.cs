using FaturaFlow.Domain.Interfaces;
using BCrypt.Net; // Você precisaria instalar o pacote: BCrypt.Net-Next

namespace FaturaFlow.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            // Gera um Hash seguro com um "Salt" automático
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            bool isValid = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            Console.WriteLine($"Tentando verificar a senha...");
            if (isValid == true)
            {
                Console.WriteLine("Senha verificada com sucesso.");
            }
            else 
            {
                Console.WriteLine("Erro ao verificar a senha. Tente recuperar a senha");
            }
            return isValid;
        }
    }
}