namespace ShiftManager.Models.Data
{
	public class DailyShifts
	{
		public string dataAction { get; set; }
		public string rfshift1 { get; set; }
		public string rfactive1 { get; set; }
		public string rfshift2 { get; set; }
		public string rfactive2 { get; set; }
		public string rfshift3 { get; set; }
		public string rfactive3 { get; set; }
		public string wmshift1 { get; set; }
		public string wmactive1 { get; set; }
		public string wmshift2 { get; set; }
		public string wmactive2 { get; set; }
		public string wmshift3 { get; set; }
		public string wmactive3 { get; set; }

		public string[] RFShift { get => new string[3] { rfshift1, rfshift2, rfshift3 }; }
		public string[] WMShift { get => new string[3] { wmshift1, wmshift2, wmshift3 }; }
		public string[] RFActive { get => new string[3] { rfactive1, rfactive2, rfactive3 }; }
		public string[] WMActive { get => new string[3] { wmactive1, wmactive2, wmactive3 }; }

	}
}