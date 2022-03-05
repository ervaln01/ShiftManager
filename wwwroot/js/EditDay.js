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
            if (response.templateIds.length > 0) {
                let options = '';
                options += '<option value="Select">Select</option>'
                for (let i = 0; i < response.templateIds.length; i++) {
                    options += '<option value="' + response.templateIds[i] + '">' + response.templateDesc[i] + '</option>';
                }
                $(id).append(options);
            }
        }
    });
}

function ShowDetails(e, url, context) {
    if (e.value === 'Select') context.text('');
    else $.ajax({ url: url, data: { "option": e.value }, success: (response) => context.text(response) });
}

function SetDay(rf, wm) {
    if (rf[0] != "") {
        $('#rfactive1').click();
        setTimeout(() => {
            var context = $(`#rfschedules1 option[value=${rf[0]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
    if (rf[1] != "") {
        $('#rfactive2').click();
        setTimeout(() => {
            var context = $(`#rfschedules2 option[value=${rf[1]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
    if (rf[2] != "") {
        $('#rfactive3').click();
        setTimeout(() => {
            var context = $(`#rfschedules3 option[value=${rf[2]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
    if (wm[0] != "") {
        $('#wmactive1').click();
        setTimeout(() => {
            var context = $(`#wmschedules1 option[value=${wm[0]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
    if (wm[1] != "") {
        $('#wmactive2').click();
        setTimeout(() => {
            var context = $(`#wmschedules2 option[value=${wm[1]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
    if (wm[2] != "") {
        $('#wmactive3').click();
        setTimeout(() => {
            var context = $(`#wmschedules3 option[value=${wm[2]}]`);
            context.attr('selected', 'selected');
            context.change();
        }, 1000);
    }
}

function AllTemplatesTable(url, id) {
    $.ajax({
        url: url,
        success: (response) => {
            var options = '';
            $(id).text(options);
            options += '<p />';
            options += '<table style="width:100%">';
            options += '<tr style="border-top: 1px solid;">';
            options += '<th style="width:14.28%">LINE</th>';
            options += '<th style="width:14.28%">SHIFT NUMBER</th>';
            options += '<th style="width:14.28%">SHIFT</th>';
            options += '<th style="width:14.28%">LUNCH</th>';
            options += '<th style="width:14.28%">BREAK1</th>';
            options += '<th style="width:14.28%">BREAK2</th>';
            options += '<th style="width:14.28%">BREAK3</th>';
            options += '</tr>';
            for (var index = 0; index < response.length; index++) {
                options += '<tr style="border-top: 1px solid;">';
                options += '<td>' + response[index].line + '</td>';
                options += '<td>' + response[index].number + '</td>';
                options += '<td>' + response[index].shift + '</td>';
                options += '<td>' + response[index].lunch + '</td>';
                options += '<td>' + response[index].break1 + '</td>';
                options += '<td>' + response[index].break2 + '</td>';
                options += '<td>' + response[index].break3 + '</td>';
                options += '</tr>';
            }
            options += '</table>';
            $(id).append(options);
        }
    });
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