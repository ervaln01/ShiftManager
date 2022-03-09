function ChangeDate(url) {
    $('#after')[0].min = $('#before').val();
    $('#before')[0].max = $('#after').val();
    RangeTimelines($('#before'), $('#after'), url, '#table');
}

function RangeTimelines(dateBefore, dateAfter, url, id) {
    if (dateBefore[0].value === "" || dateAfter[0].value === "") return;
    $.ajax({
        url: url,
        data: { "before": dateBefore[0].value, "after": dateAfter[0].value },
        success: (response) => CreateTable(response, id)
    });
}

function CreateTable(response, id) {
    var options = '';
    $(id).text(options);
    options += '<tr><th style="width:15%">Target date</th><th style="width:35%">RF</th><th style="width:35%">WM</th><th style="width:15%"></th></tr>';

    for (var index = 0; index < response.length; index++) {
        options += '<tr style="border-top:1px solid; height:35px">';
        options += `<td>${response[index].date.slice(0, 10)}</td>`
        options += colorTable(response[index].rf);
        options += colorTable(response[index].wm);
        options += `<td><button style="width:80%;height:35px;border-width:1px" onclick="{$('#editDate').val('${response[index].date.slice(0,10)}'); $('#edit').click();}">Edit</button></td>`;
        options += '</tr>'
    }
    $(id).append(options);
}

let colorTable = (shifts) => {
    var table = '';
    table += '<td>';
    table += '<table style="margin-left:auto;margin-right:auto;width:80%;height:35px">';
    table += '<tr hidden><th style="width:33%"></th><th style="width:33%"></th><th style="width:33%"></th></tr>';
    table += `<tr>${colorCell(shifts[0])}${colorCell(shifts[1])}${colorCell(shifts[2])}</tr>`
    table += '</table>';
    table += '</td>';
    return table;
}
let colorCell = (template) => {
    var cell = '';
    cell += `<td class="CellWithComment" style="border-left:1px solid;border-right:1px solid;background:${ShiftColor(template)}"><span class="CellComment">`;
    cell += template.length == 0 ? 'Not set' : template;
    cell += '</span></td>';
    return cell;
}

function SetColorLegend(id, url) {
    var options = '';
    $(id).text(options);

    $.ajax({
        url: url,
        success: (response) => {
            for (var index = 0; index < response.length; index++) {
                options += '<tr>';
                options += `<td style="background:${ShiftColor(response[index])}; height: 15px; width: 30px"></td>`;
                options += `<td style="width: 100px">${response[index]}</td>`;
                options += '</tr>';
            }
            options += '<tr>';
            options += `<td style="background:${ShiftColor(null)}; height: 15px; width: 30px"></td>`;
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