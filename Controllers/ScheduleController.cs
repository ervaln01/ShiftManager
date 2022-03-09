namespace ShiftManager.Controllers
{
	using ExportToExcel;

	using Microsoft.AspNetCore.Mvc;

	using ShiftManager.Models;
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;
	using ShiftManager.Models.Logic;

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Контроллер добавления смен.
	/// </summary>
	public class ScheduleController : Controller
	{
		/// <summary>
		/// Пользователь, работающий в данный момент.
		/// </summary>
		private string CurrentUser => HttpContext.User.Identity.Name.Split('\\').Last().ToUpper();

		/// <summary>
		/// Начало текущего месяца.
		/// </summary>
		private static DateTime StartMonth => new(DateTime.Now.Year, DateTime.Now.Month, 1);

		/// <summary>
		/// Метод обработки страницы отображения смен.
		/// </summary>
		/// <param name="dataAction">Действие, полученное со страницы.</param>
		/// <returns>Страница отображения смен.</returns>
		public IActionResult Index(string dataAction)
		{
			ViewBag.Before = StartMonth;
			ViewBag.After = StartMonth.AddMonths(1).AddDays(-1);

			return string.IsNullOrEmpty(dataAction) ? View() : RedirectToAction("AddRange");
		}

		/// <summary>
		/// Метод обработки страницы редактирования дня.
		/// </summary>
		/// <returns>Страница редактирования дня.</returns>
		public IActionResult EditDay(DailyShifts shifts, DateTime currentDate)
		{
			var timelines = Sql.GetTimelines(currentDate);

			if (string.IsNullOrEmpty(shifts.dataAction)) 
				return View(timelines.GetTargetDay(currentDate));

			if (shifts.dataAction.Equals("Save"))
				timelines.Process(CurrentUser, currentDate, shifts, false);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Метод обработки страницы добавления смен.
		/// </summary>
		/// <returns>Страница добаления смен.</returns>
		public IActionResult AddRange(DailyShifts shifts, DateRange range, string saturday, string sunday)
		{
			if (string.IsNullOrEmpty(shifts.dataAction))
			{
				ViewBag.Before = DateTime.Today;
				ViewBag.After = StartMonth.AddMonths(1).AddDays(-1);
				var active = Sql.GetTimelines(new DateRange() { before = StartMonth, after = StartMonth.AddMonths(2) });
				ViewBag.Month1 = GetMonth(active, StartMonth);
				ViewBag.Month2 = GetMonth(active, StartMonth.AddMonths(1));
				ViewBag.Month3 = GetMonth(active, StartMonth.AddMonths(2));
				return View();
			}

			if (shifts.dataAction.Equals("Save"))
			{
				var timelines = Sql.GetTimelines(range);
				timelines.Process(CurrentUser, range.before, shifts, true, range.after, !string.IsNullOrEmpty(saturday), !string.IsNullOrEmpty(sunday));
			}

			return RedirectToAction("Index");
			static dynamic GetMonth(IEnumerable<ShiftTimeline> active, DateTime date) => new
			{
				shifts = active.GetShifts(date),
				descriptions = active.GetShiftDescriptions(date).ToArray()
			};
		}

		/// <summary>
		/// Метод загрузки данных в таблицу Excel.
		/// </summary>
		/// <param name="before">Дата начала периода.</param>
		/// <param name="after">Дата конца периода.</param>
		/// <returns>Загружаемый файл.</returns>
		public FileResult ExportShifts(DateRange range)
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
			var bytes = System.IO.File.ReadAllBytes(file);
			System.IO.File.Delete(file);
			return File(bytes, "application/octet-stream", $"EXPORT SHIFTS {DateTime.Now:dd.MM.yyyy (HH.mm.ss)}.xlsx");
		}

		/// <summary>
		/// Метод обработки страницы ошибок.
		/// </summary>
		/// <returns>Страница ошибок.</returns>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		#region jQueryHelper
		/// <summary>
		/// Получение деталей определенного шаблона.
		/// </summary>
		/// <param name="option">Выбранная опция.</param>
		/// <returns>Детали шаблона.</returns>
		public string GetDetails(int option) => Sql.GetDetails(option);

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public List<Descriptions> GetTemplates(int line, int shiftNumber) => Sql.GetTemplates(line, shiftNumber);

		/// <summary>
		/// Получение сокращенных шаблонов смен.
		/// </summary>
		/// <returns>Набор всех сокращенных шаблонов.</returns>
		public IEnumerable<string> GetAllShortTemplates() => Sql.GetAllShortTemplates();

		/// <summary>
		/// Получение шаблонов смен.
		/// </summary>
		/// <returns>Набор всех шаблонов.</returns>
		public IEnumerable<dynamic> GetAllTemplates() => Sql.GetAllTemplates();

		/// <summary>
		/// Получение списка для формирования таблицы смен.
		/// </summary>
		/// <param name="before">Дата начала периода.</param>
		/// <param name="after">Дата конца периода.</param>
		/// <returns>Список смен за заданный период.</returns>
		public IEnumerable<dynamic> GetTimelines(DateRange range) => Sql.GetTimelines(range).GetTable(range);

		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public dynamic VerifySchedules(VerifyModel model) => Sql.VerifySchedules(model);
		#endregion
	}
}