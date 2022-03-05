function BlockChangeDate(before) {
    if (before) $('#dateAfter')[0].min = $('#dateBefore').val();
    else $('#dateBefore')[0].max = $('#dateAfter').val();
}

function SendDate(before, after) {
    $('#' + before).val($('#dateBefore').val());
    $('#' + after).val($('#dateAfter').val());
}

function ExportToExcel(url) {
    $.ajax(
        {
            url: url,
            data: { "before": $('#dateBefore').val(), "after": $('#dateAfter').val() }
        });
}

function SetColorLegend(id, url) {
    var options = '';
    $(id).text(options);

    $.ajax({
        url: url,
        success: (response) => {
            for (var index = 0; index < response.length; index++) {
                options += '<tr>';
                options += '<td style="background:' + ShiftColor(response[index]) + '; height: 15px; width: 30px"></td>';
                options += '<td style="width: 100px">' + response[index] + '</td>';
                options += '</tr>';
            }
            options += '<tr>';
            options += '<td style="background:' + ShiftColor(null) + '; height: 15px; width: 30px"></td>';
            options += '<td style="width: 100px">Not set</td>';
            options += '</tr>';

            $(id).append(options);
        }
    });
}

function ShiftColor(shift) {
    switch (shift) {
        case "08:00-20:00":
            return "#ffc060";
        case "20:00-08:00":
            return "#80ff60";
        case "08:00-16:00":
            return "#50d0ff";
        case "16:00-00:00":
            return "#f050ff";
        case "00:00-08:00":
            return "#ffb0b0";
        default:
            return "#d0d0d0";
    }
}