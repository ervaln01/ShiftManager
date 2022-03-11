namespace ShiftManager.Controllers
{
	using Microsoft.AspNetCore.Mvc;

	using ShiftManager.Models;
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;
	using ShiftManager.Models.Logic;

	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	/// <summary>
	/// Контроллер добавления смен.
	/// </summary>
	public class ScheduleController : Controller
	{
		/// <summary>
		/// Пользователь, работающий в данный момент.
		/// </summary>
		private string CurrentUser => (HttpContext.User.Identity.Name ?? "unknown").Split('\\').Last().ToUpper();

		/// <summary>
		/// Начало текущего месяца.
		/// </summary>
		private static DateTime StartMonth => new(DateTime.Now.Year, DateTime.Now.Month, 1);

		/// <summary>
		/// Метод обработки страницы отображения смен.
		/// </summary>
		/// <returns>Страница отображения смен.</returns>
		public IActionResult Index()
		{
			ViewBag.Before = StartMonth;
			ViewBag.After = StartMonth.AddMonths(1).AddDays(-1);
			return View();
		}

		/// <summary>
		/// Метод обработки страницы редактирования дня.
		/// </summary>
		/// <returns>Страница редактирования дня.</returns>
		public IActionResult EditDay(DailyShifts shifts, DateTime date)
		{
			if (string.IsNullOrEmpty(shifts.dataAction)) 
				return View(Helper.GetTargetDay(date));

			if (shifts.dataAction.Equals("Save"))
				Sql.GetTimelines(date).Save(CurrentUser, date, shifts, false);

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
				var active = Sql.GetTimelines(new DateRange() { Before = StartMonth, After = StartMonth.AddMonths(3) });
				ViewBag.Month1 = Helper.GetMonth(active, StartMonth, StartMonth.AddMonths(1));
				ViewBag.Month2 = Helper.GetMonth(active, StartMonth.AddMonths(1), StartMonth.AddMonths(2));
				ViewBag.Month3 = Helper.GetMonth(active, StartMonth.AddMonths(2), StartMonth.AddMonths(3));
				return View();
			}

			if (shifts.dataAction.Equals("Save"))
				Sql.GetTimelines(range).Save(CurrentUser, range.Before, shifts, true, range.After, !string.IsNullOrEmpty(saturday), !string.IsNullOrEmpty(sunday));

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Метод загрузки данных в таблицу Excel.
		/// </summary>
		public FileResult ExportShifts(DateRange range) => File(range.GetExcelBytes(), "application/octet-stream", $"EXPORT SHIFTS {DateTime.Now:dd.MM.yyyy (HH.mm.ss)}.xlsx");

		/// <summary>
		/// Метод обработки страницы ошибок.
		/// </summary>
		/// <returns>Страница ошибок.</returns>
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

		#region jQueryHelper
		public IEnumerable<TableRow> GetTimelines(DateRange range) => Helper.GetTable(range);
		public IEnumerable<string> GetAllShortTemplates() => Helper.GetAllShortTemplates();
		public IEnumerable<Template> GetAllTemplates() => Helper.GetAllTemplates();

		public string GetDetails(int option) => Helper.GetDetails(option);

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public IEnumerable<Info> GetTemplates(int line, int shiftNumber) => Helper.GetTemplates(line, shiftNumber);

		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public dynamic VerifySchedules(VerifyModel model) => Sql.VerifySchedules(model);
		#endregion
	}
}