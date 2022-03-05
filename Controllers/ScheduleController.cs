namespace ShiftManager.Controllers
{
	using ExportToExcel;

	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Options;

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
		public IActionResult EditDay(
			string dataAction,
			string rfshift1,
			string rfactive1,
			string rfshift2,
			string rfactive2,
			string rfshift3,
			string rfactive3,
			string wmshift1,
			string wmactive1,
			string wmshift2,
			string wmactive2,
			string wmshift3,
			string wmactive3,
			DateTime currentDate)
		{
			var timelines = new ApplicationContext().Timelines.Where(x => x.IsActive && x.TargetDate == currentDate);

			if (string.IsNullOrEmpty(dataAction)) return View(timelines.GetTargetDay(currentDate));

			if (dataAction.Equals("Save"))
			{
				var RFShift = new string[3] { rfshift1, rfshift2, rfshift3 };
				var WMShift = new string[3] { wmshift1, wmshift2, wmshift3 };
				var RFActive = new string[3] { rfactive1, rfactive2, rfactive3 };
				var WMActive = new string[3] { wmactive1, wmactive2, wmactive3 };
				timelines.Process(new ApplicationContext(), CurrentUser, currentDate, RFShift, RFActive, WMShift, WMActive, false);
			}

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Метод обработки страницы добавления смен.
		/// </summary>
		/// <returns>Страница добаления смен.</returns>
		public IActionResult AddRange(
			string dataAction,
			string rfshift1,
			string rfactive1,
			string rfshift2,
			string rfactive2,
			string rfshift3,
			string rfactive3,
			string wmshift1,
			string wmactive1,
			string wmshift2,
			string wmactive2,
			string wmshift3,
			string wmactive3,
			DateTime rangeBefore,
			DateTime rangeAfter,
			string saturday,
			string sunday)
		{
			if (string.IsNullOrEmpty(dataAction))
			{
				ViewBag.Before = DateTime.Today;
				ViewBag.After = StartMonth.AddMonths(1).AddDays(-1);
				var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
				var active = new ApplicationContext().Timelines.Where(x => x.IsActive);
				ViewBag.Month1 = GetMonth(active, date);
				ViewBag.Month2 = GetMonth(active, date.AddMonths(1));
				ViewBag.Month3 = GetMonth(active, date.AddMonths(2));
				return View();
			}

			if (dataAction.Equals("Save"))
			{
				var timelines = new ApplicationContext().Timelines.Where(x => x.IsActive && x.TargetDate >= rangeBefore && x.TargetDate <= rangeAfter);
				var RFShift = new string[3] { rfshift1, rfshift2, rfshift3 };
				var WMShift = new string[3] { wmshift1, wmshift2, wmshift3 };
				var RFActive = new string[3] { rfactive1, rfactive2, rfactive3 };
				var WMActive = new string[3] { wmactive1, wmactive2, wmactive3 };
				timelines.Process(new ApplicationContext(), CurrentUser, rangeBefore, RFShift, RFActive, WMShift, WMActive, true, rangeAfter, !string.IsNullOrEmpty(saturday), !string.IsNullOrEmpty(sunday));
			}

			return RedirectToAction("Index");
			static dynamic GetMonth(IQueryable<ShiftTimeline> active, DateTime date) => new
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
		public FileResult ExportShifts(DateTime before, DateTime after)
		{
			var path = Path.GetTempPath();
			var tableName = $"LastShiftsExport.xlsx";
			new ApplicationContext().Timelines
				.Where(x => x.TargetDate >= before && x.TargetDate <= after && x.IsActive)
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
		public dynamic GetDetails(int option) => Sql.GetDetails(option);

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public dynamic GetTemplates(int line, int shiftNumber) => Sql.GetTemplates(line, shiftNumber);

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
		public IEnumerable<dynamic> GetTimelines(string before, string after) => Sql.GetTimelines(before, after);

		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <param name="rf1">Шаблон 1 смены линии холодильников.</param>
		/// <param name="rf2">Шаблон 2 смены линии холодильников.</param>
		/// <param name="rf3">Шаблон 3 смены линии холодильников.</param>
		/// <param name="wm1">Шаблон 1 смены линии стиральных машин.</param>
		/// <param name="wm2">Шаблон 2 смены линии стиральных машин.</param>
		/// <param name="wm3">Шаблон 3 смены линии стиральных машин.</param>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public dynamic VerifySchedules(int rf1, int rf2, int rf3, int wm1, int wm2, int wm3) => Sql.VerifySchedules(rf1, rf2, rf3, wm1, wm2, wm3);
		#endregion
	}
}