using Hybriotheca.Web.Data.Entities;

namespace Hybriotheca.Web.Helpers.Interfaces
{
    public interface IMailHelper
    {
        bool SendConfirmationEmail(AppUser user, string tokenUrl);
        bool SendEmail(string to, string subject, string body);
        Task<bool> SendLoanCheckedOutEmail(Loan loan, string userEmail);
        Task<bool> SendLoanCreatedEmail(Loan loan, string userEmail);
        Task<bool> SendLoanReturnedEmail(Loan loan, string userEmail);
        bool SendPasswordResetEmail(AppUser user, string tokenUrl);
    }
}