using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Utilities
{
    public static class DateTimeExtensions
    {
        public static long ToUnixTimestamp(this DateTime d)
        {
            var epoch = d - new DateTime(1970, 1, 1, 0, 0, 0);

            return (long)epoch.TotalSeconds;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        public static List<DateTime> GetDatesBetween(DateTime startDate, DateTime endDate)
        {
            List<DateTime> dates = new List<DateTime>();
            DateTime currentDate = startDate;
            while (currentDate <= endDate)
            {
                dates.Add(currentDate);
                currentDate = currentDate.AddDays(1);
            }
            return dates;
        }

        public static Dictionary<string, List<DateTime>> GroupDatesByMonthYear(List<DateTime> dates)
        {
            Dictionary<string, List<DateTime>> groupedDates = new Dictionary<string, List<DateTime>>();
            foreach (DateTime date in dates)
            {
                string monthYearKey = (date.Month + 1) + "/" + date.Year;
                if (!groupedDates.ContainsKey(monthYearKey))
                {
                    groupedDates[monthYearKey] = new List<DateTime>();
                }
                groupedDates[monthYearKey].Add(date);
            }
            return groupedDates;
        }

        public static DateTime? SetToEndOfDay(DateTime? date = null)
        {
            try
            {
                if (date == null)
                    return date;

                return date.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(999);
            }
            catch(Exception ex)
            {
                return date;
            }
        }
    }
}
