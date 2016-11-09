using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OData.Models
{
    [Serializable]
    public class DateRange
    {
        private DateTime? _startDate;

        public DateTime StartDate
        {
            get
            {
                return SeededStartDate ?? _startDate ?? DateTime.Parse("1/1/1900 12:00:00 AM");
            }

            set
            {
                _startDate = value;
            }
        }

        private DateTime? SeededStartDate
        {
            get
            {
                if (DateRangeSeed == DateRangeSeed.None)
                {
                    return null;
                }

                var today = DateTime.Today;

                switch (DateRangeSeed)
                {
                    case DateRangeSeed.NinetyDays:
                        return today.AddDays(-90);
                    case DateRangeSeed.ThisMonth:
                        return new DateTime(today.Year, today.Month, 1);
                    case DateRangeSeed.ThisWeek:
                        return today.StartOfWeek(DayOfWeek.Sunday);
                    case DateRangeSeed.Today:
                        return today;
                    default:
                        return null;
                }
            }
        }

        private DateTime? _endDate;

        public DateTime EndDate
        {
            get
            {
                return _endDate ?? DateTime.Now.AddDays(5);
            }

            set
            {
                _endDate = value;
            }
        }

        public DateRangeSeed DateRangeSeed { get; set; }

        public DateRange()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DateRange"/> class. 
        ///     Creates an instance of a DateRange using the passed in string that should represent the parts of the date range.
        /// </summary>
        /// <param name="dateRange">A string representing the date range.  Ex.) 10/1/2011 or 10/1/2011-10/2/2011</param>
        public DateRange(string dateRange)
        {
            if (string.IsNullOrWhiteSpace(dateRange))
            {
                throw new ArgumentNullException(
                    "dateRange", "The string passed into DateRange must be a valid date range. Ex.) 10/1/2011 or 10/1/2011-10/2/2011");
            }

            var dateParts = dateRange.Split('-');

            if (dateParts.Length >= 1)
            {
                StartDate = DateTime.Parse(dateParts[0]);
            }

            if (dateParts.Length >= 2)
            {
                EndDate = DateTime.Parse(dateParts[1]);
            }
        }

        public override string ToString()
        {
            if (_startDate == null && _endDate == null && DateRangeSeed == DateRangeSeed.None)
            {
                return string.Empty;
            }

            var dateRangeString = StartDate.ToShortDateString() + "-" + EndDate.ToShortDateString();

            return dateRangeString;
        }
    }

    public enum DateRangeSeed
    {
        None = 0,

        NinetyDays = 1,

        ThisMonth = 2,

        ThisWeek = 3,

        Today = 4
    }

    public static class DateTimeExtensions
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            var diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }
}