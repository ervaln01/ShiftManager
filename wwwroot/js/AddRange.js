function FillNumbers(id, month, before, after, saturday, sunday) {
    var beforeDay = before === "all" ? 0 : Number(before.substring(8, 10));
    var afterDay = after === "all" ? 32 : Number(after.substring(8, 10));

    var days = document.getElementById(id).children[2].childNodes;
    for (var index = 0; index < days.length; index++) {
        days[index].classList.remove("with-shift", "without-shift");

        if (Number(days[index].innerText) < beforeDay) continue;
        if (Number(days[index].innerText) > afterDay) continue;

        if (days[index].classList.value.includes('sunday') && !sunday) continue;
        if (days[index].classList.value.includes('saturday') && !saturday) continue;
        var dayshift = month.filter(x => x.id == index + 1);
        if (dayshift.length == 0) {
            days[index].classList.add("without-shift");
            continue;
        }

        days[index].classList.add("with-shift");
        var node = document.createElement("div");
        node.classList.add("ShiftComment");
        node.innerText = dayshift[0].description;
        days[index].appendChild(node);
    }
}

function CreateCalendars(month1, month2, month3) {
    var date1 = new Date();
    var date2 = new Date();
    var date3 = new Date();

    date2.setDate(1);
    date3.setDate(1);
    date2.setMonth(date2.getMonth() + 1);
    date3.setMonth(date3.getMonth() + 2);

    VanillaCalendar({ selector: "#calendar1" }, date1, new Date());
    VanillaCalendar({ selector: "#calendar2" }, date2, null);
    VanillaCalendar({ selector: "#calendar3" }, date3, null);

    var currentDate = new Date();
    var startMonth = (new Date($('#rangeBefore').val())).getMonth() - currentDate.getMonth();
    var endMonth = (new Date($('#rangeAfter').val())).getMonth() - currentDate.getMonth();
    if (endMonth < 0) endMonth += 12;
    var before = $('#rangeBefore').val();
    var after = $('#rangeAfter').val();
    var saturday = $('#saturdayid')[0].checked;
    var sunday = $('#sundayid')[0].checked;

    $('#calendar1').hide();
    $('#calendar2').hide();
    $('#calendar3').hide();

    switch (startMonth) {
        case 0:
            switch (endMonth) {
                case 0:
                    FillNumbers('calendar1', month1, before, after, saturday, sunday);
                    $('#calendar1').show();
                    break;
                case 1:
                    FillNumbers('calendar1', month1, before, "all", saturday, sunday);
                    $('#calendar1').show();
                    FillNumbers('calendar2', month2, "all", after, saturday, sunday);
                    $('#calendar2').show();
                    break;
                case 2:
                    FillNumbers('calendar1', month1, before, "all", saturday, sunday);
                    $('#calendar1').show();
                    FillNumbers('calendar2', month2, "all", "all", saturday, sunday);
                    $('#calendar2').show();
                    FillNumbers('calendar3', month3, "all", after, saturday, sunday);
                    $('#calendar3').show();
                    break;
            }
            break;
        case 1:
            if (endMonth === 1) {
                FillNumbers('calendar2', month2, before, after, saturday, sunday);
                $('#calendar2').show();
            }
            if (endMonth === 2) {
                FillNumbers('calendar2', month2, before, "all", saturday, sunday);
                $('#calendar2').show();
                FillNumbers('calendar3', month3, "all", after, saturday, sunday);
                $('#calendar3').show();
            }
            break;
        case 2:
            FillNumbers('calendar3', month3, before, after, saturday, sunday);
            $('#calendar3').show();
            break;
    }
}

function VanillaCalendar(options, date) {

    var opts = {
        selector: null,
        date: date,
        month: null,
        month_label: null,
        months: ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
        shortWeekday: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun']
    }

    for (let k in options) {
        if (opts.hasOwnProperty(k)) {
            opts[k] = options[k];
        }
    }

    CreateCalendar(opts);
    opts.month = document.querySelector(opts.selector + ' [data-calendar-area=month]');
    opts.month_label = document.querySelector(opts.selector + ' [data-calendar-label=month]');

    opts.date.setDate(1);
    CreateMonth(opts);
    SetWeekDayHeader(opts);
}

function CreateCalendar(opts) {
    let options = '';
    options += '<div class="vanilla-calendar-header">';
    options += '<div class="vanilla-calendar-header__label" data-calendar-label="month"></div>';
    options += '</div>';
    options += '<div class="vanilla-calendar-week"></div>';
    options += '<div class="vanilla-calendar-body" data-calendar-area="month"></div>';

    document.querySelector(opts.selector).innerHTML = options;
}

function CreateMonth(opts) {
    opts.month.innerHTML = '';
    let currentMonth = opts.date.getMonth();
    while (opts.date.getMonth() === currentMonth) {
        let day = document.createElement('div');
        day.className = 'vanilla-calendar-date';

        if (opts.date.getDay() === 0)
            day.classList.add('sunday');
        if (opts.date.getDay() === 6)
            day.classList.add('saturday');

        if (opts.date.getDate() === 1)
            day.style.marginLeft = [6, 0, 1, 2, 3, 4, 5][opts.date.getDay()] * 14.28 + '%';

        day.innerHTML = opts.date.getDate();
        opts.month.appendChild(day);
        opts.date.setDate(opts.date.getDate() + 1);
    }

    opts.date.setDate(1);
    opts.date.setMonth(opts.date.getMonth() - 1);
    opts.month_label.innerHTML = opts.months[opts.date.getMonth()] + ' ' + opts.date.getFullYear();
}

function SetWeekDayHeader(opts) {
    let options = '';
    options += `<span>${opts.shortWeekday[0]}</span>`;
    options += `<span>${opts.shortWeekday[1]}</span>`;
    options += `<span>${opts.shortWeekday[2]}</span>`;
    options += `<span>${opts.shortWeekday[3]}</span>`;
    options += `<span>${opts.shortWeekday[4]}</span>`;
    options += `<span>${opts.shortWeekday[5]}</span>`;
    options += `<span>${opts.shortWeekday[6]}</span>`;

    document.querySelector(`${opts.selector} .vanilla-calendar-week`).innerHTML = options;
}