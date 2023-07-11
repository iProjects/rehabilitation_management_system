
$(document).ready(function () {

    var now = new Date();
    var day = ("0" + now.getDate()).slice(-2);
    var month = ("0" + (now.getMonth() + 1)).slice(-2);
    var year = now.getFullYear();

    var today = (year) + "-" + (month) + "-" + (day);

    setInterval(function () {
        showcurrenttime();
    }, 1000);

    $("#progress_bar").hide();

    log_info_messages("finished load...");
});

function showcurrenttime() {
    try {
        var d = new Date();

        g_arrMonth = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");

        g_arrDay = new Array("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday");

        var strmonthname = g_arrMonth[d.getMonth()];
        var strdayname = g_arrDay[d.getDay()];

        var datestring = ("0" + d.getDate()).slice(-2) + "-" + strdayname + "-" + ("0" + (d.getMonth() + 1)).slice(-2) + "-" + strmonthname + "-" + d.getFullYear();

        var ampm = d.getHours() >= 12 ? 'pm' : 'am';

        var timestring = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2) + " " + ampm;

        $("#lblfooterdate").html(datestring);
        $("#lblfooterdate").attr("title", datestring);
        $("#lblfootertime").html(timestring);
        $("#lblfootertime").attr("title", timestring);

        var strcopyright = "Copyright (c) Software Providers. " + d.getFullYear() + " All rights reserved"
        $("#lblcopyright").html(strcopyright);
        $("#lblcopyright").attr("title", strcopyright);

        //$('#lblfooterelapsedtime').text(today);

    }
    catch (err) {
        log_error_messages(err);
        console.log(err);
    }
}

function getCookie(name) {
    var cookieValue = null;
    if (document.cookie && document.cookie !== "") {
        var cookies = document.cookie.split(";");
        for (var i = 0; i < cookies.length; i++) {
            var cookie = cookies[i].trim();
            // Does this cookie string begin with the name we want?
            if (cookie.substring(0, name.length + 1) === (name + "=")) {
                cookieValue = decodeURIComponent(cookie.substring(name.length + 1));
                break;
            }
        }
    }
    return cookieValue;
}
function log_info_messages(message) {
    $("#div_messages")
        .fadeIn(2000, function () {

            var d = new Date();
            var datestring = ("0" + d.getDate()).slice(-2) + "-" + ("0" + (d.getMonth() + 1)).slice(-2) + "-" + d.getFullYear();
            var timestring = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2);
            var today = datestring + " " + timestring;

            var msg = today + " : " + message;

            $("#div_messages")
                .prepend('<div class="div_info_message_item">' + msg + '</div>')
        });
}

function log_error_messages(message) {
    $("#div_messages")
        .fadeIn(2000, function () {

            var d = new Date();
            var datestring = ("0" + d.getDate()).slice(-2) + "-" + ("0" + (d.getMonth() + 1)).slice(-2) + "-" + d.getFullYear();
            var timestring = ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2) + ":" + ("0" + d.getSeconds()).slice(-2);
            var today = datestring + " " + timestring;

            var msg = today + " : " + message;

            $("#div_messages")
                .prepend('<div class="div_error_message_item">' + msg + '</div>')
        });
}

function generate_random_integer() {
    var d = new Date();
    var milliseconds = d.getMilliseconds().toString();
    var virtual_weight = milliseconds.substring(0, 2);
    return virtual_weight;
}

myInterval = setInterval(elapsed_time, 1000);

function myStopFunction() {
    clearInterval(myInterval);
}

var _initial_date = new Date();

function elapsed_time() {

    var _today = _initial_date;
    var _current = new Date();
    var _days = parseInt((_current - _today) / (1000 * 60 * 60 * 24));
    var _hours = parseInt(Math.abs(_current - _today) / (1000 * 60 * 60) % 24);
    var _minutes = parseInt(Math.abs(_current.getTime() - _today.getTime()) / (1000 * 60) % 60);
    var _seconds = parseInt(Math.abs(_current.getTime() - _today.getTime()) / (1000) % 60);

    var _elapsed_time = _days + ':' + _hours + ':' + _minutes + ':' + _seconds;

    $('#lblfooterelapsedtime').text(_elapsed_time);


}