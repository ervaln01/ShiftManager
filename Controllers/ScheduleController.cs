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
		private string CurrentUser 
		{ 
			get 
			{
				var user = HttpContext.User.Identity.Name;
				return string.IsNullOrEmpty(user) ? "unknown" : user.Split('\\').Last().ToUpper();
			}
		}

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
			var timelines = Sql.GetTimelines(date);

			if (string.IsNullOrEmpty(shifts.dataAction)) 
				return View(timelines.GetTargetDay(date));

			if (shifts.dataAction.Equals("Save"))
				timelines.Save(CurrentUser, date, shifts, false);

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
				timelines.Save(CurrentUser, range.before, shifts, true, range.after, !string.IsNullOrEmpty(saturday), !string.IsNullOrEmpty(sunday));
			}

			return RedirectToAction("Index");
			static List<DayShift> GetMonth(IEnumerable<ShiftTimeline> active, DateTime date) => active
				.Where(x => x.TargetDate >= date && x.TargetDate <= date.AddMonths(1))
				.GroupBy(x => x.TargetDate)
				.Select(x => new DayShift { day = x.Key.Day, desc = x.Where(c=>c.TargetDate == x.Key).GetShiftDescription()})
				.OrderBy(x=>x.day).ToList();
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
		public IEnumerable<TableRow> GetTimelines(DateRange range) => range.GetTable();
		public IEnumerable<string> GetAllShortTemplates() => Sql.GetAllShortTemplates();

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
		/// Получение шаблонов смен.
		/// </summary>
		/// <returns>Набор всех шаблонов.</returns>
		public IEnumerable<Template> GetAllTemplates() => Sql.GetAllTemplates();

		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public dynamic VerifySchedules(VerifyModel model) => Sql.VerifySchedules(model);
		#endregion
	}
}