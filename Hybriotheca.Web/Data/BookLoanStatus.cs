namespace Hybriotheca.Web.Data
{
    public static class BookLoanStatus
    {
        public static string Reserved => "Reserved";
        public static string Active => "Active";
        public static string Returned => "Returned";

        public static string[] All => new[] { Reserved, Active, Returned };
    }
}
