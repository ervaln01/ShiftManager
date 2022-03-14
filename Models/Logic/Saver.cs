namespace ShiftManager.Models.Logic
{
	using ShiftManager.Models.Data;
	using ShiftManager.Models.Entity;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class Saver
	{
		/// <summary>
		/// Получение корректной даты.
		/// </summary>
		/// <param name="datetime">Дата.</param>
		/// <param name="templateDate">Дата из шаблона.</param>
		/// <returns>Корректная дата.</returns>
		private static DateTime? GetCorrectDate(this DateTime datetime, DateTime? templateDate) => templateDate.HasValue ? GetCorrectDate(datetime, templateDate.Value) : null;

		/// <summary>
		/// Получение корректной даты.
		/// </summary>
		/// <param name="datetime">Дата.</param>
		/// <param name="templateDate">Дата из шаблона.</param>
		/// <returns>Корректная дата.</returns>
		private static DateTime GetCorrectDate(this DateTime datetime, DateTime templateDate) =>
			new DateTime(datetime.Year, datetime.Month, datetime.Day, templateDate.Hour, templateDate.Minute, 0).AddDays(templateDate.Day == 2 ? 1 : 0);

		/// <summary>
		/// Запись данных в БД.
		/// </summary>
		/// <param name="timelines">Последовательность смен.</param>
		/// <param name="user">Пользователь, добавивший данные.</param>
		/// <param name="currentDate">Текущая дата.</param>
		/// <param name="range">Диапазон дней?</param>
		/// <param name="lastDate">Последняя дата диапазона.</param>
		/// <param name="saturday">Задавать смены по субботам?</param>
		/// <param name="sunday">Задавать смены по воскресеньям?</param>
		public static void Save(this IEnumerable<ShiftTimeline> timelines, string user, DateTime currentDate,
			DailyShifts shifts, bool range,
			DateTime? lastDate = null, bool saturday = false, bool sunday = false)
		{
			using var context = new ApplicationContext();
			var RFShift = shifts.RFShift;
			var WMShift = shifts.WMShift;
			for (var index = 0; index < 3; index++)
			{
				if (shifts.RFActive[index] == null) RFShift[index] = null;
				if (shifts.WMActive[index] == null) WMShift[index] = null;
			}

			foreach (var timeline in timelines)
			{
				timeline.IsActive = false;
				context.Timelines.Update(timeline);
			}

			var newTimelines = !range ?
				CreateTimelines(currentDate, RFShift, WMShift, DateTime.Now, user) :
				CreateTimelines(new () { Before = currentDate, After = lastDate.Value }, saturday, sunday, RFShift, WMShift, DateTime.Now, user);

			newTimelines.ForEach(item => context.Timelines.Add(item));
			context.SaveChanges();
		}

		/// <summary>
		/// Создание набора смен на заданный диапазон.
		/// </summary>
		/// <param name="range">Временной диапазон.</param>
		/// <param name="saturday">Заполнять субботу?</param>
		/// <param name="sunday">Заполнять воскресенье?</param>
		/// <param name="rf">Набор шаблонов смен на линии холодильников.</param>
		/// <param name="wm">Набор шаблонов смен на линии стиральных машин.</param>
		/// <param name="now">Время добавления записи в БД.</param>
		/// <param name="user">Пользователь, добавивший запись.</param>
		/// <returns>Список всех смен на заданный период.</returns>
		private static List<ShiftTimeline> CreateTimelines(DateRange range, bool saturday, bool sunday, string[] rf, string[] wm, DateTime now, string user)
		{
			var timeline = new List<ShiftTimeline>();
			for (var day = range.Before; day <= range.After; day = day.AddDays(1))
			{
				timeline.AddRange(CreateTimelines(day, rf, wm, now, user));
			}

			if (!saturday)
			{
				var saturdays = timeline.Where(x => x.TargetDate.DayOfWeek == DayOfWeek.Saturday);
				timeline = timeline.Except(saturdays).ToList();
			}

			if (!sunday)
			{
				var sundays = timeline.Where(x => x.TargetDate.DayOfWeek == DayOfWeek.Sunday);
				timeline = timeline.Except(sundays).ToList();
			}

			return timeline;
		}

		/// <summary>
		/// Создание набора смен на заданную дату.
		/// </summary>
		/// <param name="datetime">Целевая дата.</param>
		/// <param name="rf">Набор шаблонов смен на линии холодильников.</param>
		/// <param name="wm">Набор шаблонов смен на линии стиральных машин.</param>
		/// <param name="now">Время добавления записи в БД.</param>
		/// <param name="user">Пользователь, добавивший запись.</param>
		/// <returns>Список всех смен на заданную дату.</returns>
		private static List<ShiftTimeline> CreateTimelines(DateTime datetime, string[] rf, string[] wm, DateTime now, string user)
		{
			return GetShifts(rf, 1).Union(GetShifts(wm, 2)).ToList();
			List<ShiftTimeline> GetShifts(string[] arr, int index) => arr.Where(x => x != null && !x.Equals("Select")).Select((x, i) => FillTimeline(int.Parse(x), i + 1, index)).ToList();
			ShiftTimeline FillTimeline(int id, int shift, int line)
			{
				using var context = new ApplicationContext();

				var template = context.Templates.FirstOrDefault(x => x.Id == id);

				var shiftTimeline = new ShiftTimeline
				{
					TargetDate = datetime,
					InsertTime = now,
					InsertUser = user,
					IsActive = true,
					Line = line,
					ShiftNumber = shift,
					ShiftId = id,
					ShiftBegin = datetime.GetCorrectDate(template.ShiftBegin),
					ShiftEnd = datetime.GetCorrectDate(template.ShiftEnd),
					LunchBegin = datetime.GetCorrectDate(template.LunchBegin),
					LunchEnd = datetime.GetCorrectDate(template.LunchEnd),
					Break1Begin = datetime.GetCorrectDate(template.Break1Begin),
					Break1End = datetime.GetCorrectDate(template.Break1End),
					Break2Begin = datetime.GetCorrectDate(template.Break2Begin),
					Break2End = datetime.GetCorrectDate(template.Break2End),
					Break3Begin = datetime.GetCorrectDate(template.Break3Begin),
					Break3End = datetime.GetCorrectDate(template.Break3End)
				};

				return SetCorrectTimes(shiftTimeline);
			}
		}

		/// <summary>
		/// Корректировка дат для смен.
		/// </summary>
		/// <param name="shiftTimeline">Смены на день.</param>
		/// <returns>Смены на день с корректными датами.</returns>
		private static ShiftTimeline SetCorrectTimes(ShiftTimeline shiftTimeline)
		{
			if (shiftTimeline.ShiftBegin > shiftTimeline.ShiftEnd)
				shiftTimeline.ShiftEnd = shiftTimeline.ShiftEnd.AddDays(1);

			if (shiftTimeline.LunchBegin.HasValue && shiftTimeline.LunchEnd.HasValue)
			{
				if (shiftTimeline.LunchBegin.Value < shiftTimeline.ShiftBegin)
				{
					shiftTimeline.LunchBegin = shiftTimeline.LunchBegin.Value.AddDays(1);
					shiftTimeline.LunchEnd = shiftTimeline.LunchEnd.Value.AddDays(1);
				}
			}

			if (shiftTimeline.Break1Begin.HasValue && shiftTimeline.Break1End.HasValue)
			{
				if (shiftTimeline.Break1Begin.Value < shiftTimeline.ShiftBegin)
				{
					shiftTimeline.Break1Begin = shiftTimeline.Break1Begin.Value.AddDays(1);
					shiftTimeline.Break1End = shiftTimeline.Break1End.Value.AddDays(1);
				}
			}

			if (shiftTimeline.Break2Begin.HasValue && shiftTimeline.Break2End.HasValue)
			{
				if (shiftTimeline.Break2Begin.Value < shiftTimeline.ShiftBegin)
				{
					shiftTimeline.Break2Begin = shiftTimeline.Break2Begin.Value.AddDays(1);
					shiftTimeline.Break2End = shiftTimeline.Break2End.Value.AddDays(1);
				}
			}

			if (shiftTimeline.Break3Begin.HasValue && shiftTimeline.Break3End.HasValue)
			{
				if (shiftTimeline.Break3Begin.Value < shiftTimeline.ShiftBegin)
				{
					shiftTimeline.Break3Begin = shiftTimeline.Break3Begin.Value.AddDays(1);
					shiftTimeline.Break3End = shiftTimeline.Break3End.Value.AddDays(1);
				}
			}

			return shiftTimeline;
		}
	}
}