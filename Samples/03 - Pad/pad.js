var Pad = function (id, parent) {
    var element = $('<div id="' + id + '"><div><br></div></div>').attr('contenteditable', 'true').addClass('pad').addClass('connecting');

    var ws = new WebSocket('ws://localhost:8181/padsample');

    parent.append(element);

    ws.onopen = function () {
        element.removeClass('connecting');
    }

    ws.onmessage = function (evt) {
        var diff = evt.data;
        applyDiff(diff);
    }

    ws.onclose = function () {
        element.attr('disabled', 'disabled');
        element.removeClass('connecting');
        element.addClass('not_connected');
    }

    var before = "";
    var after = "";

    var isDown = false;

    element.keydown(function (evt) {
        if (!isDown) {
            before = element.html();
            isDown = true;
        }
    });

    element.keyup(function (evt) {
        if (element.html() == '') {
            element.html('<div><br></div>');
        }
        after = element.html();
        var diff = getDiff();
        ws.send(JSON.stringify(diff));
        isDown = false;
    });

    var getDiff = function () {
        var diff = [];
        var before_lines = $(before);
        var after_lines = $(after);
        var max = (before_lines.length > after_lines.length) ? before_lines.length : after_lines.length

        for (var i = 0; i < max; i++) {

            if (after_lines.eq(i).html() != before_lines.eq(i).html()) {
                diff.push({
                    'l': i,
                    'p': after_lines.eq(i).html(),
                    'm': before_lines.eq(i).html()
                });
            }
        }

        return diff;
    }

    var applyDiff = function (diff) {
        console.log(diff);
        diff = JSON.parse(diff);
        
        for (var i in diff) {

            var plus = diff[i].p;
            var minus = diff[i].m;

            if (minus == null) {
                element.append('<div><br></div>');
            }

            var line = $('#' + id + ' div').eq(diff[i].l);

            if (line.html() == minus) {
                line.html(plus);
            }
        }

    }
}