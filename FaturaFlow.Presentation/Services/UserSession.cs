namespace FaturaFlow.Presentation.Services
{
    public class UserSession
    {
        public bool IsLoggedIn { get; set; }
        public string? UserName { get; set; }
        public Guid? UserId { get; set; }

        public void Logout()
        {
            IsLoggedIn = false;
            UserName = null;
            UserId = null;
        }
    }
}
