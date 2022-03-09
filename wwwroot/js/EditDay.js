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