$(document).ready(init);

function alert(level, text) {
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

function debugMode() {
    $("#login-main input.pwd").val("pwd");
    $("#login-main>#connect").click();
}

function init() {
    $("footer").hide();
    if (getQueryString("addr") == null) {
        alert(1, "如果是首次使用此控制台，你可以查看<a href='#'>食用方法</a>");
    } else {
        $("#login-main input.addr").val(getQueryString("addr"));
    }
    $("#disconnect").click(function () { location.reload() });
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
    $("#login-main>input").bind("input propertychange", function () { $("#login-main>#state").hide(); });
    $("#input-area>input").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            send_command();
        }
    });
    // debugMode();
}

function html2Escape(sHtml) {
    return sHtml.replace(/[<>&"]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;' }[c];
    });
}

function getQueryString(name) {
    var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    var r = window.location.search.substring(1).match(reg);
    if (r != null) return decodeURIComponent(r[2]); return null;
}