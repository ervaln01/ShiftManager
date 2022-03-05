namespace ShiftManager.Models.Logic
{
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

		/// <summary>
		/// Получение информации о шаблонах.
		/// </summary>
		/// <param name="line">Линия.</param>
		/// <param name="shiftNumber">Номер смены.</param>
		/// <returns>Набор шаблонов по заданным параметрам.</returns>
		public static dynamic GetTemplates(int line, int shiftNumber)
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
		public static IEnumerable<dynamic> GetAllTemplates()
		{
			using var context = new ApplicationContext();
			return context.Templates.OrderBy(x => x.Line).ThenBy(x => x.ShiftBegin).GetAllTemplates().ToList();
		}

		/// <summary>
		/// Получение списка для формирования таблицы смен.
		/// </summary>
		/// <param name="before">Дата начала периода.</param>
		/// <param name="after">Дата конца периода.</param>
		/// <returns>Список смен за заданный период.</returns>
		public static IEnumerable<dynamic> GetTimelines(string before, string after)
		{
			using var context = new ApplicationContext();
			return context.Timelines.Where(x => x.TargetDate >= DateTime.Parse(before) && x.TargetDate <= DateTime.Parse(after) && x.IsActive).GetTable(DateTime.Parse(before), DateTime.Parse(after)).ToList();
		}

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
		public static dynamic VerifySchedules(int rf1, int rf2, int rf3, int wm1, int wm2, int wm3)
		{
			using var context = new ApplicationContext();
			return new
			{
				rf = context.Verify(rf1, rf2, rf3),
				wm = context.Verify(wm1, wm2, wm3)
			};
		}
	}
}
