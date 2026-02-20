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
            Console.WriteLine(HashPassword(password));
            Console.WriteLine($"Tentando entrar na Base de Dados...");
            if (isValid == true)
            {
                Console.WriteLine("Conectado");
            }
            else 
            {
                Console.WriteLine("Erro ao conectar com a base de dados, Tente admin/admin se for a primeira secção ou tente recuperar a senha");
            }
            return isValid;
        }
    }
}