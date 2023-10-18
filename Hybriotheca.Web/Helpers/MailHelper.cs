using Hybriotheca.Web.Data.Entities;
using Hybriotheca.Web.Helpers.Interfaces;
using Hybriotheca.Web.Repositories.Interfaces;
using MailKit.Net.Smtp;
using MimeKit;

namespace Hybriotheca.Web.Helpers
{
    public class MailHelper : IMailHelper
    {
        private readonly IConfiguration _configuration;
        private readonly ILoanRepository _loanRepository;

        public MailHelper(IConfiguration configuration, ILoanRepository loanRepository)
        {
            _configuration = configuration;
            _loanRepository = loanRepository;
        }

        public bool SendConfirmationEmail(AppUser user, string tokenUrl)
        {
            string emailBody = "<h2>Email confirmation</h2>" +
                $"<p>Click <a href=\"{tokenUrl}\"><u>here</u></a> to activate account.</p>";

            try
            {
                return SendEmail(user.Email, "Email confirmation", emailBody);
            }
            catch { return false; }
        }

        public bool SendEmail(string emailTo, string subject, string body)
        {
            var nameFrom = _configuration["Mail:NameFrom"];
            var emailFrom = _configuration["Mail:EmailFrom"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            var length = emailTo.IndexOf("@");
            var nameTo = emailTo[0..length];

            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(nameFrom, emailFrom));
            message.To.Add(new MailboxAddress(nameTo, emailTo));

            message.Subject = subject;

            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            try
            {
                using var client = new SmtpClient();

                client.Connect(smtp, int.Parse(port), false);
                client.Authenticate(emailFrom, password);
                client.Send(message);
                client.Disconnect(true);

                return true;
            }
            catch { return false; }
        }

        public async Task<bool> SendLoanCheckedOutEmail(Loan loan, string userEmail)
        {
            var model = await _loanRepository.SelectEmailModelAsync(loan.ID);
            if (model == null) return false;

            string emailBody = "<h2>Book has been checked out</h2>" +
                $"<p>Hi, {model.UserFirstName}.</p>" +
                $"<p>Your loan status is now {model.LoanStatus}." +
                $"<h4>Book: {model.BookTitle}</h4>" +
                $"<h4>Library: {model.LibraryName}, {model.LibraryLocation}</h4>" +
                $"<h4>Pickup date: {model.PickupDate}</h4>" +
                $"<h4>Term limit: {model.TermLimit}</h4>";

            try
            {
                return SendEmail(userEmail, "Book checked out", emailBody);
            }
            catch { return false; }
        }

        public async Task<bool> SendLoanCreatedEmail(Loan loan, string userEmail)
        {
            var model = await _loanRepository.SelectEmailModelAsync(loan.ID);
            if (model == null) return false;

            string emailBody = "<h2>Loan has been registered</h2>" +
                $"<p>Hi, {model.UserFirstName}.</p>" +
                $"<p>Your loan status is now {model.LoanStatus}." +
                $"<h4>Book: {model.BookTitle}</h4>" +
                $"<h4>Library: {model.LibraryName}, {model.LibraryLocation}</h4>" +
                $"<h4>Loan registered: {model.CreateDate}</h4>" +
                $"<h4>Pickup date: {model.PickupDate}</h4>" +
                $"<h4>Term limit: {model.TermLimit}</h4>";

            try
            {
                return SendEmail(userEmail, "Loan registered", emailBody);
            }
            catch { return false; }
        }

        public async Task<bool> SendLoanReturnedEmail(Loan loan, string userEmail)
        {
            var model = await _loanRepository.SelectEmailModelAsync(loan.ID);
            if (model == null) return false;

            string emailBody = "<h2>Book has been returned</h2>" +
                $"<p>Hi, {model.UserFirstName}.</p>" +
                $"<p>Your loan status is now {model.LoanStatus}." +
                $"<h4>Book: {model.BookTitle}</h4>" +
                $"<h4>Library: {model.LibraryName}, {model.LibraryLocation}</h4>" +
                $"<h4>Pickup date: {model.PickupDate}</h4>" +
                $"<h4>Return date: {model.ReturnDate}</h4>" +
                $"<h4>Term limit: {model.TermLimit}</h4>";

            try
            {
                return SendEmail(userEmail, "Book returned", emailBody);
            }
            catch { return false; }
        }

        public bool SendPasswordResetEmail(AppUser user, string tokenUrl)
        {
            string emailBody = "<h2>Password reset</h2>" +
                $"<p>Click <a href=\"{tokenUrl}\"><u>here</u></a> to reset password.</p>";

            try
            {
                return SendEmail(user.Email, "Password reset", emailBody);
            }
            catch { return false; }
        }
    }
}
