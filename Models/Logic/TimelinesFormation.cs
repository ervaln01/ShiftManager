namespace ShiftManager.Models.Logic
{
	using ShiftManager.Models.Entity;

	using System;
	using System.Collections.Generic;
	using System.Linq;

	/// <summary>
	/// Класс, осуществляющий формирование смен.
	/// </summary>
	public class TimelinesFormation
	{
		/// <summary>
		/// Контекст базы данных.
		/// </summary>
		private readonly ApplicationContext _context;

		/// <summary>
		/// Конструктор класса <see cref="TimelinesFormation"/>.
		/// </summary>
		/// <param name="context">Контекст базы данных.</param>
		public TimelinesFormation(ApplicationContext context) => _context = context;

		/// <summary>
		/// Создание набора смен на заданный диапазон.
		/// </summary>
		/// <param name="datetime">Временной диапазон.</param>
		/// <param name="saturday">Заполнять субботу?</param>
		/// <param name="sunday">Заполнять воскресенье?</param>
		/// <param name="rf">Набор шаблонов смен на линии холодильников.</param>
		/// <param name="wm">Набор шаблонов смен на линии стиральных машин.</param>
		/// <param name="now">Время добавления записи в БД.</param>
		/// <param name="user">Пользователь, добавивший запись.</param>
		/// <returns>Список всех смен на заданный период.</returns>
		public List<ShiftTimeline> CreateTimelines(DateTime[] datetime, bool saturday, bool sunday, string[] rf, string[] wm, DateTime now, string user)
		{
			var timeline = new List<ShiftTimeline>();
			for (var day = datetime.First(); day <= datetime.Last(); day = day.AddDays(1))
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
		public List<ShiftTimeline> CreateTimelines(DateTime datetime, string[] rf, string[] wm, DateTime now, string user)
		{
			var list = new List<ShiftTimeline>();

			for (var index = 0; index < 3; index++)
			{
				if (rf[index] != null && !rf[index].Equals("Select"))
					list.Add(FillTimeline(int.Parse(rf[index]), index, 1));

				if (wm[index] != null && !wm[index].Equals("Select"))
					list.Add(FillTimeline(int.Parse(wm[index]), index, 2));
			}

			return list;

			ShiftTimeline FillTimeline(int id, int index, int line)
			{
				var template = _context.Templates.FirstOrDefault(x => x.Id == id);

				var shiftTimeline = new ShiftTimeline
				{
					TargetDate = datetime,
					InsertTime = now,
					InsertUser = user,
					IsActive = true,
					Line = line,
					ShiftNumber = index + 1,
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