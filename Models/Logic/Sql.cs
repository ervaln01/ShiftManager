namespace ShiftManager.Models.Logic
{
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Sql
	{
		/// <summary>
		/// Получение деталей определенного шаблона.
		/// </summary>
		/// <param name="option">Выбранная опция.</param>
		/// <returns>Детали шаблона.</returns>
		public static string GetDetails(int option)
		{
			using var context = new ApplicationContext();
			return context.Templates.FirstOrDefault(x => x.Id == option).GetDetails();
		}

		public static List<ShiftTimeline> GetTimelines(DateTime currentDate)
		{
			using var context = new ApplicationContext();
			return context.Timelines.Where(x => x.IsActive && x.TargetDate == currentDate).ToList();
		}

		public static List<ShiftTimeline> GetTimelines(DateRange range)
		{
			using var context = new ApplicationContext();
			return context.Timelines.Where(x => x.IsActive && x.TargetDate >= range.before && x.TargetDate <= range.after).ToList();
		}

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public static List<Descriptions> GetTemplates(int line, int shiftNumber)
		{
			using var context = new ApplicationContext();
			return context.Templates.Where(x => x.Line == line && x.ShiftNumber == shiftNumber).GetTemplates();
		}

		/// <summary>
		/// Получение сокращенных шаблонов смен.
		/// </summary>
		/// <returns>Набор всех сокращенных шаблонов.</returns>
		public static IEnumerable<string> GetAllShortTemplates()
		{
			using var context = new ApplicationContext();
			return context.Templates.OrderBy(x => x.ShiftBegin).Select(x => $"{x.ShiftBegin.GetTime()}-{x.ShiftEnd.GetTime()}").Distinct().ToList();
		}

		/// <summary>
		/// Получение шаблонов смен.
		/// </summary>
		/// <returns>Набор всех шаблонов.</returns>
		public static IEnumerable<Template> GetAllTemplates()
		{
			using var context = new ApplicationContext();
			return context.Templates.OrderBy(x => x.Line).ThenBy(x => x.ShiftNumber).Select(template => new Template()
			{
				Line = template.Line == 1 ? "RF" : "WM",
				Number = template.ShiftNumber,
				Shift = $"{template.ShiftBegin.GetTime()}-{template.ShiftEnd.GetTime()}",
				Lunch = $"{template.LunchBegin.GetTime()}-{template.LunchEnd.GetTime()}",
				Break1 = $"{template.Break1Begin.GetTime()}-{template.Break1End.GetTime()}",
				Break2 = $"{template.Break2Begin.GetTime()}-{template.Break2End.GetTime()}",
				Break3 = $"{template.Break3Begin.GetTime()}-{template.Break3End.GetTime()}",
			}).ToList();
		}

		/// <summary>
		/// Проверка корректности не пересечения смен.
		/// </summary>
		/// <returns>Код корректности заданных шаблонов смен.</returns>
		public static dynamic VerifySchedules(VerifyModel model)
		{
			using var context = new ApplicationContext();
			return new
			{
				rf = context.Verify(model.rf1, model.rf2, model.rf3),
				wm = context.Verify(model.wm1, model.wm2, model.wm3)
			};
		}
	}
}
