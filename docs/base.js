$(document).ready(init);

function alert(text){
    $(".alert").html("<div onclick=\"$('.alert').html('')\"><span>!</span>"+text+"</div>");
    setTimeout(() => {
    $(".alert>div").css("transform","none");
        
    }, 200);
}

function debugMode() {
    $("#login-main input.pwd").val("pwd");
    wsclient = new WebSocket("ws://127.0.0.1:30000");
    wsclient.onmessage = ws_receive;
    wsclient.onopen = ws_open;
    wsclient.onclose = ws_close;
}

function init() {
    debugMode();
    $("footer").hide();
    $("#disconnect").click(function () {
        location.reload()
    })
    $("#login-main>#state").hide();
    $("#login-main>#connect").click(try_connect);
    $("#input-area>button").click(send_command);
    $("#login-main>input.addr").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            $("#login-main>input.pwd").focus();
        }
    });
    $("#login-main>input.pwd").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            try_connect();
        }
    });
    $("#login-main>input").bind("input propertychange", function () {
        $("#login-main>#state").hide();
    });
    $("#input-area>input").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            send_command();
        }
    });
}

function html2Escape(sHtml) {
    return sHtml.replace(/[<>&"]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;' }[c];
    });
}
