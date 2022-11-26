$(document).ready(init);

function notice(level, text) {
    setTimeout(() => { $(".alert div:last").css("transform", "none"); }, 200);
    if (level == 1) {
        $(".alert").append("<div class='info'><span>!</span>" + text + "</div>");
        setTimeout(() => { $(".alert div:last").remove(); }, 5500);
        setTimeout(() => { $(".alert div:last").css("transform", ""); }, 5000);
    }
    else {
        $(".alert").append("<div class='error'><span>!</span>" + text + "<span class='close' onclick='$(\".alert div.warn,.alert div.error\").remove()'>×</span></div>");
    }
}

function init() {
    $("footer").hide();
    if (!getQueryString("addr")) {
        notice(1, "如果是首次使用此控制台，你可以查看<a href='./document/'>食用方法</a>");
        $('#login-main>input.addr').focus();
        $("#login-main input.addr").val('ws://');
    } else {
        $("#login-main input.addr").val(getQueryString("addr"));
        $('#login-main>input.pwd').focus();
    }
    $("#disconnect").click(function () { location.reload(); });
    $("#login-main>#state").hide();
    $("#login-main>#connect").click(try_connect);
    $("#login-main>input.addr").bind("keypress", function (event) { if (event.keyCode == "13") { $("#login-main>input.pwd").focus(); } });
    $("#login-main>input.pwd").bind("keypress", function (event) { if (event.keyCode == "13") { try_connect(); } });
    $("#login-main>input").bind("input propertychange", function () { $("#login-main>#state").hide(); });
    $("#input-area>input").bind("keypress", function (event) { if (event.keyCode == "13") { send_command(); } });
    $("#input-area>button").click(send_command);
    $("#button_start").click(start_server);
    $("#button_stop").click(stop_server);
    $("#button_kill").click(kill_server);
    $("#button_restart").click(restart_server);
    $(".section#console").height(($(".child-container").height() - 90) + "px");
    $("header select").change(change_panel);
    $(window).resize(function () { $(".section#console").height(($(".child-container").height() - 90) + "px"); });
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

function html2Escape(sHtml) {
    return sHtml.replace(/[<>&" ]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;', ' ': '&ensp;' }[c];
    });
}

function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substring(1).match(reg);
    if (r != null) return decodeURIComponent(r[2]); return null;
}

function dic_count(dic) {
    var n = 0;
    for (var _key in dic) {
        n++;
    }
    return n;
}
