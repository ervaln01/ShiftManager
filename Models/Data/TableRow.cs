namespace ShiftManager.Models.Data
{
	using System;

	/// <summary>
	/// Модель строки таблицы отображения смен.
	/// </summary>
	public class TableRow
	{
		/// <summary>
		/// Дата.
		/// </summary>
		public DateTime Date;

		/// <summary>
		/// Краткие описания шаблонов смен на линии холодильников.
		/// </summary>
		public string[] RF;

		/// <summary>
		/// Краткие описания шаблонов смен на линии стиральных машин.
		/// </summary>
		public string[] WM;

		/// <summary>
		/// Конструктор класса <see cref="TableRow"/>.
		/// </summary>
		/// <param name="date">Дата.</param>
		public TableRow(DateTime date)
		{
			Date = date;
			RF = new string[3] { string.Empty, string.Empty, string.Empty };
			WM = new string[3] { string.Empty, string.Empty, string.Empty };
		}
	}
}