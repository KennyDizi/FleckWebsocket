﻿<html> 
 
<head> 
    <title>Web Socket Chat</title>

    <script type="text/javascript" src="jquery-1.4.min.js"></script> 	
<script type="text/javascript">
    var ws;
    $(document).ready(function () {

        // test if the browser supports web sockets
        if ("WebSocket" in window) {
            debug("Browser supports web sockets!", 'success');
            //connect($('#host').val());
            $('#console_send').removeAttr('disabled');
        } else {
            debug("Browser does not support web sockets", 'error');
        };

        // function to send data on the web socket
        function ws_send(str) {
            try {
                ws.send(str);
            } catch (err) {
                debug(err, 'error');
            }
        }

        // connect to the specified host
        function connect(host) {

            debug("Connecting to " + host + " ...");
            try {
                ws = new WebSocket(host, "msg"); // create the web socket
            } catch (err) {
                debug(err, 'error');
            }
            $('#host_connect').attr('disabled', true); // disable the 'reconnect' button

            ws.onopen = function () {
                debug("connected... ", 'success'); // we are in! :D
            };

            ws.onmessage = function (evt) {
                debug(evt.data, 'response'); // we got some data - show it omg!!
            };

            ws.onclose = function () {
                debug("Socket closed!", 'error'); // the socket was closed (this could be an error or simply that there is no server)
                $('#host_connect').attr('disabled', false); // re-enable the 'reconnect button
            };
        };

        // function to display stuff, the second parameter is the class of the <p> (used for styling)
        function debug(msg, type) {
            $("#console").append('<p class="' + (type || '') + '">' + msg + '</p>');
        };

        // the user clicked to 'reconnect' button
        $('#host_connect').click(function () {
            debug("\n");
            connect($('#host').val());
        });

        // the user clicked the send button
        $('#console_send').click(function () {
            ws_send($('#console_input').val());
        });

        $('#console_input').keyup(function (e) {
            if(e.keyCode == 13) // enter is pressed
                ws_send($('#console_input').val());
        });

    });
</script> 
 
<style type="text/css"> 
	.error {color: red;}
	.success {color: green;}
	#console_wrapper {background-color: black; color:white;padding:5px;}
	p {padding:0;margin:0;}
</style> 
</head> 
 
<body> 
 
<h1>Web Socket Chat</h1> 
 
<div id="server_wrapper"> 
	<p>Connect to /chat or /admin</p>
    <p>Server
		<input type="text" name="host" id="host" value="ws://localhost:8181/chat" /> 
		<input type="submit" name="host_connect" id="host_connect" value="Connect!" /> 
	</p>
    <p>Type /nick &lt;name&gt; to change your nick</p>
    <p>Type /kick &lt;name&gt; to kick a user (if you are connected to /admin)<p>
</div> 
 
<div id="console_wrapper"> 
	<pre id="console"></pre> 
	<input type="text" name="console_input" id="console_input" value="" /> 
	<input type="submit" name="console_send" id="console_send" value="Send" /> 
</div> 
 
</body> 
 
</html>

