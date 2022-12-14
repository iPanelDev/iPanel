const VERSION = "1.3";
var checkedVersion = false;

$(document).ready(init);

/**
 * @description 初始化
 */
function init() {
    $("footer").hide();
    if (!getParameter("addr")) {
        notice(1, "如果是首次使用此控制台，你可以查看<a href='../'>食用方法</a>");
        $('#login-main>input.addr').focus();
        $("#login-main input.addr").val('ws://');
    } else {
        $("#login-main input.addr").val(getParameter("addr"));
        $('#login-main>input.pwd').focus();
    }
    $("#disconnect").click(function () { location.reload(); });
    $("#login-main>#state").hide();
    $("#login-main>#connect").click(tryConnect);
    $("#login-main>input.addr").bind("keypress", function (event) { if (event.keyCode == "13") { $("#login-main>input.pwd").focus(); } });
    $("#login-main>input.pwd").bind("keypress", function (event) { if (event.keyCode == "13") { tryConnect(); } });
    $("#login-main>input").bind("input propertychange", function () { $("#login-main>#state").hide(); });
    $("#input-area>input").bind("keypress", function (event) { if (event.keyCode == "13") { sendCommand(); } });
    $("#input-area>button").click(sendCommand);
    $("#button_start").click(startServer);
    $("#button_stop").click(stopServer);
    $("#button_kill").click(killServer);
    $("#button_restart").click(restartServer);
    $(".section#console").height(($(".child-container").height() - 90) + "px");
    $("header select").change(changeInstance);
    $("header select").change(() => updateInfo());
    $(window).resize(function () { $(".section#console").height(($(".child-container").height() - 90) + "px"); });
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}

/**
 * @description 弹窗计数器
 */
var noticeCounter = 0;

/**
 * @description 显示弹窗
 * @param {number} level 等级(1=info)
 * @param {string} text 显示内容
 */
function notice(level, text) {
    setTimeout(() => { $(".alert div").css("transform", "none"); }, 200);
    let id = noticeCounter;
    noticeCounter++;
    if (level == 1 || level == 2) {
        $(".alert").append("<div class='" + (level == 1 ? "info" : "warn") + "' id='alert" + id + "'><span>!</span>" + text + "</div>");
        setTimeout(() => $(".alert div#alert" + id).remove(), 5500);
        setTimeout(() => $(".alert div#alert" + id).css("transform", ""), (level == 1 ? 5000 : 2500));
    }
    else {
        $(".alert").append("<div id='alert" + id + "' class='error'><span>!</span>" + text + "<span class='close' onclick='$(\".alert div.warn#alert" + id + ",.alert div.error#alert" + id + "\").remove()'>×</span></div>");
    }
}


/**
 * @description html转义
 * @param {string} text 文本
 * @returns 转义后的文本
 */
function html2Escape(text) {
    return text.replace(/[<>&" ]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;', ' ': '&ensp;' }[c];
    });
}

/**
 * @description 获取当前URL参数
 * @param {string|null} name 参数名
 * @returns 参数
 */
function getParameter(name) {
    let reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    let r = window.location.search.substring(1).match(reg);
    if (r != null) return decodeURIComponent(r[2]); return null;
}

/**
 * @description 计算字典长度
 * @param {Array} dic  字典
 * @returns 字典长度
 */
function count(dic) {
    let n = 0;
    for (var _key in dic) {
        n++;
    }
    return n;
}
