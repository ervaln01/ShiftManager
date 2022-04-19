namespace ShiftManager.Models.Logic
{
	using ExportToExcel;
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Класс расширений обработки данных для облегчения контроллеров.
	/// </summary>
	public static class Helper
	{
		public static List<Info> GetMonth(IEnumerable<ShiftTimeline> active, DateTime dt1, DateTime dt2) => active
			.Where(x => x.TargetDate >= dt1 && x.TargetDate < dt2)
			.GroupBy(x => x.TargetDate)
			.Select(x => new Info { Id = x.Key.Day, Description = x.Where(c => c.TargetDate == x.Key).GetShiftDescription() })
			.OrderBy(x => x.Id).ToList();

		/// <summary>
		/// Формирование таблицы смен на заданный период.
		/// </summary>
		/// <param name="timelines">Последовательность смен.</param>
		/// <param name="range">Период дат.</param>
		/// <returns>Последовательность динамически задаваемых строк таблицы.</returns>
		public static IEnumerable<TableRow> GetTable(DateRange range)
		{
			var timelines = Sql.GetTimelines(range);
			for (var date = range.Before; date <= range.After; date = date.AddDays(1))
			{
				var response = new TableRow(date);
				var dayShifts = timelines.Where(x => x.TargetDate == date);

				foreach (var shift in dayShifts.Where(x => x.Line == 1).OrderBy(shift => shift.ShiftNumber))
				{
					response.RF[shift.ShiftNumber - 1] = $"{shift.ShiftBegin.GetTime()}-{shift.ShiftEnd.GetTime()}";
				}

				foreach (var shift in dayShifts.Where(x => x.Line == 2).OrderBy(shift => shift.ShiftNumber))
				{
					response.WM[shift.ShiftNumber - 1] = $"{shift.ShiftBegin.GetTime()}-{shift.ShiftEnd.GetTime()}";
				}

				yield return response;
			}
		}

		public static List<ShiftTimeline> GetTimelines(DateRange range) => Sql.GetTimelines(range);

		public static byte[] GetExcelBytes(this DateRange range)
		{
			var path = Path.GetTempPath();
			var tableName = $"LastShiftsExport.xlsx";
			Sql.GetTimelines(range)
				.OrderBy(x => x.TargetDate)
				.ThenBy(x => x.Line)
				.ThenBy(x => x.ShiftNumber)
				.ToTable(true, "Shift timelines")
				.AddColumn(x => x.TargetDate, "Target date", typeof(DateTime), "dd.MM.yyyy")
				.AddColumn(x => x.Line == 1 ? "RF" : "WM", "Line", typeof(string))
				.AddColumn(x => x.ShiftNumber, "Shift number", typeof(int))
				.AddColumn(x => x.ShiftBegin, "Shift begin", typeof(DateTime), "dd.MM.yyyy HH:mm:ss")
				.AddColumn(x => x.ShiftEnd, "Shift end", typeof(DateTime), "dd.MM.yyyy HH:mm:ss")
				.AddColumn(x => x.LunchBegin, "Lunch begin", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.LunchEnd, "Lunch end", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break1Begin, "Break 1 begin", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break1End, "Break 1 end", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break2Begin, "Break 2 begin", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break2End, "Break 2 end", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break3Begin, "Break 3 begin", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.Break3End, "Break 3 end", typeof(DateTime), "HH:mm:ss")
				.AddColumn(x => x.InsertUser, "Insert user", typeof(string))
				.AddColumn(x => x.InsertTime, "Insert time", typeof(DateTime), "dd.MM.yyyy HH:mm:ss")
				.GenerateTable(path, tableName);

			var file = $"{path}\\{tableName}";
			var bytes = File.ReadAllBytes(file);
			File.Delete(file);
			return bytes;
		}

		/// <summary>
		/// Получение строки таблицы отображения смен.
		/// </summary>
		/// <param name="currentDate">Текущая дата.</param>
		/// <returns>Строка таблицы отображения смен.</returns>
		public static TableRow GetTargetDay(DateTime currentDate)
		{
			var shiftTimelines = Sql.GetTimelines(new(currentDate));
			if (!shiftTimelines.Any()) return new(currentDate);

			var response = new TableRow(currentDate);

			for (var shiftNumber = 1; shiftNumber <= 3; shiftNumber++)
			{
				var rf = shiftTimelines.FirstOrDefault(x => x.ShiftNumber == shiftNumber && x.Line == 1);
				if (rf != null) response.RF[shiftNumber - 1] = $"{rf.ShiftId}";

				var wm = shiftTimelines.FirstOrDefault(x => x.ShiftNumber == shiftNumber && x.Line == 2);
				if (wm != null) response.WM[shiftNumber - 1] = $"{wm.ShiftId}";
			}
			return response;
		}

		/// <summary>
		/// Получение деталей шаблона.
		/// </summary>
		/// <param name="template">Шаблон.</param>
		/// <returns>Развернутая информация текущего шаблона.</returns>
		public static string GetDetails(int option)
		{
			var template = Sql.GetTemplate(option);
			if (template == null) return string.Empty;
			return $"Shift {template.ShiftBegin.GetTime()}-{template.ShiftEnd.GetTime()}; " +
			$"Lunch {template.LunchBegin.GetTime()}-{template.LunchEnd.GetTime()}; " +
			$"Breaks {template.Break1Begin.GetTime()}-{template.Break1End.GetTime()}; " +
			$"{template.Break2Begin.GetTime()}-{template.Break2End.GetTime()}; " +
			$"{template.Break3Begin.GetTime()}-{template.Break3End.GetTime()}";
		}

		public static IEnumerable<Template> GetAllTemplates()
		{
			var shiftTemplates = Sql.GetTemplates();
			if (!shiftTemplates.Any()) return new List<Template>();
			return shiftTemplates.Select(template => new Template()
			{
				Line = template.Line == 1 ? "RF" : "WM",
				Number = template.ShiftNumber,
				Shift = $"{template.ShiftBegin.GetTime()}-{template.ShiftEnd.GetTime()}",
				Lunch = $"{template.LunchBegin.GetTime()}-{template.LunchEnd.GetTime()}",
				Break1 = $"{template.Break1Begin.GetTime()}-{template.Break1End.GetTime()}",
				Break2 = $"{template.Break2Begin.GetTime()}-{template.Break2End.GetTime()}",
				Break3 = $"{template.Break3Begin.GetTime()}-{template.Break3End.GetTime()}",
			});
		}

		public static IEnumerable<string> GetAllShortTemplates()
		{
			var shiftTemplates = Sql.GetTemplates();
			if (!shiftTemplates.Any()) return new List<string>();
			return shiftTemplates.Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}").Distinct();
		}

		/// <summary>
		/// Получения информации о шаблонах смен.
		/// </summary>
		/// <param name="templates">Последовательность шаблонов смен.</param>
		/// <returns>Информация о шаблонах.</returns>
		public static IEnumerable<Info> GetTemplates(int line, int shiftNumber)
		{
			var shiftTemplate = Sql.GetTemplates(line, shiftNumber);
			if (!shiftTemplate.Any()) return new List<Info>();
			return shiftTemplate.Select(x => new Info() { Id = x.Id, Description = $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}" });
		}


		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public static VerifyModel VerifySchedules(VerifyModel model)
		{
			model.rf = Sql.Verify(model.rf1, model.rf2, model.rf3);
			model.wm = Sql.Verify(model.wm1, model.wm2, model.wm3);
			return model;
		}

		/// <summary>
		/// Получение форматированного времени.
		/// </summary>
		/// <param name="dateTime">Дата.</param>
		/// <returns>Форматированная строка времени.</returns>
		private static string GetTime(this DateTime dateTime) => $"{dateTime.Hour:D2}:{dateTime.Minute:D2}";

		/// <summary>
		/// Получение форматированного времени.
		/// </summary>
		/// <param name="dateTime">Дата.</param>
		/// <returns>Форматированная строка времени.</returns>
		private static string GetTime(this DateTime? dateTime) => dateTime.HasValue ? GetTime(dateTime.Value) : string.Empty;

		/// <summary>
		/// Получение описаний смен на дни, где они заданы.
		/// </summary>
		/// <param name="active">Дни с активными сменами.</param>
		/// <param name="date">Первый день месяца.</param>
		/// <returns>Набор описаний заданных смен.</returns>
		private static string GetShiftDescription(this IEnumerable<ShiftTimeline> active)
		{
			var rf = $"{string.Join(" ", active.Where(x => x.Line == 1).Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}"))}";
			var wm = $"{string.Join(" ", active.Where(x => x.Line == 2).Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}"))}";
			return (!string.IsNullOrEmpty(rf) && !string.IsNullOrEmpty(wm)) ? $"RF {rf}\nWM {wm}" : !string.IsNullOrEmpty(rf) ? $"RF {rf}" : $"WM {wm}";
		}
	}
}