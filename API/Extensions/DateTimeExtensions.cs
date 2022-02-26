namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        public static int GetAge(this DateTime dateOfBirth)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;

            if (dateOfBirth.Date > today.AddYears(-age))
            // Birthday hasn't been reached this year yet so we substract 1 year
                age--;
                
            return age;
        }
    }
}