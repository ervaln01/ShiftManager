function RangeTimelines(dateBefore, dateAfter, url, id, urlEdit) {
    if (dateBefore[0].value === "" || dateAfter[0].value === "") return;
    $.ajax({ url: url, data: { "before": dateBefore[0].value, "after": dateAfter[0].value }, success: (response) => CreateTable(response, id, urlEdit) });
}

function CreateTable(response, id, url) {
    var options = '';
    $(id).text(options);
    options += '<tr><th style="width:15%">Target date</th><th style="width:35%">RF</th><th style="width:35%">WM</th><th style="width:15%"></th></tr>';

    for (var index = 0; index < response.length; index++) {
        options += '<tr style="border-top:1px solid; height:35px">';
        options += `<td>${response[index].date}</td>`;

        options += '<td>';
        options += '<table style="margin-left:auto;margin-right:auto;width:80%;height:35px">';
        options += '<tr hidden><th style="width:33%"></th><th style="width:33%"></th><th style="width:33%"></th></tr>';

        options += '<tr>';
        options += `<td class="CellWithComment" style="border-left:1px solid;background:${ShiftColor(response[index].rf1)}"><span class="CellComment">`;
        if (response[index].rf1.length == 0) options += 'Not set';
        else options += response[index].rf1;
        options += '</span></td>';

        options += `<td class="CellWithComment" style="border-left:1px solid;border-right:1px solid;background:${ShiftColor(response[index].rf2)}"><span class="CellComment">`;
        if (response[index].rf2.length == 0) options += 'Not set';
        else options += response[index].rf2;
        options += '</span></td>';

        options += `<td class="CellWithComment" style="border-right:1px solid;background:${ShiftColor(response[index].rf3)}"><span class="CellComment">`;
        if (response[index].rf3.length == 0) options += 'Not set';
        else options += response[index].rf3;
        options += '</span></td>';
        options += '</tr>'

        options += '</table>';
        options += '</td>';

        options += '<td>';
        options += '<table style="margin-left:auto;margin-right:auto;width:80%;height:35px">';
        options += '<tr hidden><th style="width:33%"></th><th style="width:33%"></th><th style="width:33%"></th></tr>';

        options += '<tr>';
        options += `<td class="CellWithComment" style="border-left:1px solid;background:${ShiftColor(response[index].wm1)}"><span class="CellComment">`;
        if (response[index].wm1.length == 0) options += 'Not set';
        else options += response[index].wm1;
        options += '</span></td>';

        options += `<td class="CellWithComment" style="border-left:1px solid;border-right:1px solid;background:${ShiftColor(response[index].wm2)}"><span class="CellComment">`;
        if (response[index].wm2.length == 0) options += 'Not set';
        else options += response[index].wm2;
        options += '</span></td>';

        options += `<td class="CellWithComment" style="border-right:1px solid;background:${ShiftColor(response[index].wm3)}"><span class="CellComment">`;
        if (response[index].wm3.length == 0) options += 'Not set';
        else options += response[index].wm3;
        options += '</span></td>';
        options += '</tr>'
        options += '</table>';
        options += '</td>';

        options += '<td>';

        options += `<form action="${url}?currentDate=${response[index].date}" method="post">`;
        options += `<input type="date" id="before${index}" name="dateBefore" hidden />`;
        options += `<input type="date" id="after${index}" name="dateAfter" hidden />`;
        options += `<input type="submit" style="width:80%;height:35px" value="Edit" onclick="SendDate('before${index}', 'after${index}')"/>`;
        options += '</form>';

        options += '</td>';
        options += '</tr>'
    }
    $(id).append(options);
}