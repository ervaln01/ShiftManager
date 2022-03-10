function ShowTemplates(e, id, url, line) {
    document.getElementById(id).style.display = e.checked ? 'inline-block' : 'none';

    if (e.checked) DrawTemplates(line, id.slice(-1), "#" + id, url);
    else $('#' + (line == 1 ? "rf" : "wm") + 'description' + id.slice(-1)).text('');
}

function DrawTemplates(line, shiftNumber, id, url) {
    $.ajax({
        url: url,
        data: { "line": line, "shiftNumber": shiftNumber },
        success: (response) => {
            $(id).html('');
            if (response.length > 0) {
                let options = '<option value="Select">Select</option>'
                for (let i = 0; i < response.length; i++) {
                    options += `<option value="${response[i].id}">${response[i].description}</option>`;
                }
                $(id).append(options);
            }
        }
    });
}

function ShowDetails(e, url, context) {
    if (e.value === 'Select') context.text('');
    else $.ajax({
        url: url,
        data: { "option": e.value },
        success: (response) => context.text(response)
    });
}

function DrawTableShifts(id, line, nline) {
    var options = '';
    $(id).text(options);

    options += `<tr><th style="width:10%">Shift</th><th style="width:10%">Active</th><th style="width:10%">Template</th><th style="width:70%">Details</th></tr>`;
    for (var i = 1; i <= 3; i++) {
        options += `<tr style="border-top: 1px solid">`
        options += `<td>${i}</td>`
        options += `<td>`
        options += `<input type="checkbox" onclick="Templates(this, ${i}, ${nline})" name="${line}active${i}" id="${line}active${i}" />`
        options += `</td>`
        options += `<td>`
        options += `<select id="${line}schedules${i}" onchange="Details(this, ${i}, ${nline})" style="display: none"> </select>`
        options += `</td>`
        options += `<td id="${line}description${i}"></td>`
        options += `</tr >`
    }
    $(id).append(options);
}

function Save(url) {
    $.ajax({
        url: url,
        data: {
            "rf1": $('#rfschedules1').val(),
            "rf2": $('#rfschedules2').val(),
            "rf3": $('#rfschedules3').val(),
            "wm1": $('#wmschedules1').val(),
            "wm2": $('#wmschedules2').val(),
            "wm3": $('#wmschedules3').val(),
        },
        success: (response) => {
            if (response.rf === 0 && response.wm === 0) {
                $('#save').click();
                return;
            }

            if (response.rf === 2 || response.wm === 2) {
                alert("Unexpected error! No database connection. Сheck the data, reload the page and try again");
                return;
            }

            if (response.rf === 1) alert("RF data is not correct");
            if (response.wm === 1) alert("WM data is not correct");
        }
    })
}