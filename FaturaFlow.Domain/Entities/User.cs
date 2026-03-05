using FaturaFlow.Domain.ValueObjects;

namespace FaturaFlow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; } 
        public EmailAddress? Email { get; private set; } 
        public string? RecoveryCode { get; private set; }
        #pragma warning disable CS8618 
        private User () {}
        #pragma warning restore CS8618
        public User(string username, string password, EmailAddress? email = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("O nome de utilizador é obrigatório.");
            
            if (string.IsNullOrWhiteSpace(password))
                throw new Exception("A palavra-passe é obrigatória.");

            Id = Guid.NewGuid();
            Username = username;
            Password = password;
            Email = email;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new Exception("A nova palavra-passe não pode estar vazia.");
                
            Password = newPasswordHash;
        }

        public void SetRecoveryCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) throw new Exception("Código inválido");
            RecoveryCode = code;
        }
        
        public void ClearRecoveryCode()
        {
            RecoveryCode = null;
        }
        public void UpdateDetails(string username, EmailAddress? email)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new Exception("O nome de utilizador não pode ser vazio.");

            Username = username;
            Email = email;
        }
    }
}