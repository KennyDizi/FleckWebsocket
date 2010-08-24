var PadSocket = function () {
    var _that = this;
    var _inner = undefined;

    this.onmessage = undefined;
    this.onclose = undefined;

    this.send = function (msg) {
        _inner.send(msg);
    }

    this.connect = function (callback) {
        new WebSocket('ws://localhost:8181/padsample');
        _inner.onmessage = function (evt) {
            _that.onmessage(evt);
        };

        _inner.onopen = function () {
            callback();
        };

        _inner.onclose = function () {
            _that.onclose();
        }
    }
}

var Cursor = function () {
    var that = this;
    var color = []; color[true] = '#ffffff'; color[false] = '#000000'
    var on = true;
    this.element = $('<span/>', { css: { backgroundColor: color[on], height: '1em', width: '2px'} });
    setInterval(function () {
        that.element.css('background-color', color[on]);
        on = !on;
    }, 1000);
}

var ChangeSet = function (item, before, after) {
    
    
    this.toString = function () {
        return JSON.stringify([item,before,after]);
    }
}

var Pad = function (parent) {
    var socket = new PadSocket();
    var textfield = $('<div/>', { css: { width: '100px', height: '100px'} });
    var cursor = new Cursor();
    var hasFocus = false; $(document).click(function () { hasFocus = false; });
    var changed = false;
    var change = [];
    var currentRevision = 1;

    textfield.click(function () {
        hasFocus = true;

        return false;
    });

    var sendChange = function (c, o) {
        var item = cursor.element.parent();
        var time = new Date().getTime().toString();

        var changeset = {
            'i': item.attr('id'),
            'o': o,
            'c': c
        }

        socket.send(JSON.stringify(changeset));
    }

    $(document).keydown(function (evt) {
        if (!hasFocus)
            return true;
        var key = evt.originalEvent.keyIdentifier;

        if (evt.keyCode == 38) {
            sendChange('<br/>', '+');
        }

        // donno how else to get the key pressed
        if (key.indexOf('+') > -1) { // if a letter is pressed
            var c = key.replace('U+', '\\u');
            eval("char = '" + c + "'");
            if (!evt.shiftKey) {
                c = c.toLowerCase();
            }
            sendChange(c, '+');
            return false;
        }

    });



    parent.append(textfield);
};

