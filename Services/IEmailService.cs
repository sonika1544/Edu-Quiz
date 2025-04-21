namespace EduQuiz_.Services
{
    public interface IEmailService
    {
        Task SendPasswordSetupEmailAsync(string email, string firstName, string token);
    }
} 