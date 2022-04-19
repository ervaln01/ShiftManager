namespace ShiftManager.Models.Data
{
	using System;

	public class DateRange
	{
		public DateTime Before { get; set; }
		public DateTime After { get; set; }
		public DateRange(DateTime dateTime) : this(dateTime, dateTime) { }
		public DateRange(DateTime before, DateTime after)
		{
			Before = before;
			After = after;
		}
	}
}