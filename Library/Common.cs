namespace jep_construction_api.Library
{
    public static class Common
    {

        public static string GetFormattedExceptionMessage(Exception ex)
        {
            string exm = "";
            if (ex.Message != null && ex.Message.Trim().Length > 0)
                exm += string.Format("Message: {0}", ex.Message);

            if (ex.InnerException != null && ex.InnerException.Message != null && ex.InnerException.Message.Trim().Length > 0)
            {
                exm += string.Format(";Inner Exception Message: {0}", ex.InnerException.Message);
            }

            if (ex.InnerException != null && ex.InnerException.Message != null && ex.InnerException.Message.Trim().Length > 0 &&
                ex.InnerException.InnerException != null && ex.InnerException.InnerException.Message != null && ex.InnerException.InnerException.Message.Trim().Length > 0)
            {
                exm += string.Format(";Inner Exception Message: {0}", ex.InnerException.InnerException.Message);
            }

            return exm;
        }

        public static DateTime DateTimeNow(string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        /// <summary>
        /// Gets the current DateTimeOffset for a specific timezone (includes UTC offset).
        /// </summary>
        public static DateTimeOffset DateTimeOffsetNow(string timeZoneId)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
        }

        public static string GenEmployeeNumber(string number)
        {
            if (!int.TryParse(number, out int num))
                throw new ArgumentException("Input must be a valid number.", nameof(number));

            return $"e-{num:D6}";
        }

    }
}
