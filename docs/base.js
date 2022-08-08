var wsclient;
var connected = false;
var verifid = false;
var last_input = null;
var server_list = [];

$(document).ready(init);

function debugMode() {
    $("#login-main input.pwd").val("pwd");
    wsclient = new WebSocket("ws://127.0.0.1:30000");
    wsclient.onmessage = receive;
    wsclient.onopen = open;
    wsclient.onclose = close;
}

function init() {
    // debugMode();
    $("footer").hide();
    $("#disconnect").click(function () {
        location.reload()
    })
    $("#login-main>#state").hide();
    $("#login-main>#connect").click(tryConnect);
    $("#input-area>button").click(cmdInput);
    $("#login-main>input.addr").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            $("#login-main>input.pwd").focus();
        }
    });
    $("#login-main>input.pwd").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            tryConnect();
        }
    });
    $("#login-main>input").bind("input propertychange", function () {
        $("#login-main>#state").hide();
    });
    $("#input-area>input").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            cmdInput();
        }
    });
}

function cmdInput() {
    var cmd = $("#input-area>input").val();
    last_input = cmd;
    AppendText(">" + html2Escape(cmd));
    if (connected) {
        wsclient.send(JSON.stringify(
            {
                "type": "input",
                "sub_type": "console",
                "data": cmd,
                "target": "all"
            }
        ));
    }
    $("#input-area>input").val("");
}

function tryConnect() {
    connected = false;
    verifid = false;
    $("#login-main>#state").show();
    $("#login-main>#state").text("正在连接");
    try {
        wsclient = new WebSocket($("#login-main>input.addr").val());
        wsclient.onmessage = receive;
        wsclient.onopen = open;
        wsclient.onclose = close;
    } catch {
        $("#login-main>#state").text("地址不合法");
    }
}

function open() {
    connected = true;
    $("#login-main>#state").text("连接成功，正在进行验证");
}

function close() {
    connected = false;
    if (connected && verifid) {
        alert("连接断开");
    } else if (connected) {
        $("#login-main>#state").text("密码验证失败");
    } else {
        $("#login-main>#state").text("连接超时");
    }
}

function receive(e) {
    console.log(e.data)
    connected = true;
    var json = JSON.parse(e.data);
    if (json.type == "verify") {
        if (json.sub_type == "request") {
            wsclient.send(JSON.stringify(
                {
                    "type": "verify",
                    "sub_type": "console_reply",
                    "data": md5(json.data + $("#login-main input.pwd").val()),
                    "from": "WebConsole"
                }
            ))
        }
    }
    if (json.type == "notice" && json.sub_type == "verify_success") {
        verifid = true;
        $("#login-container").hide();
        $("footer").show();
        $("body").css("overflow", "auto");
        updateServers();
        setInterval(() => {
            if (connected) {
                updateServers();
            }
        }, 10000);
    }
    if (json.type == "output") {
        if (json.sub_type == "colored") {
            AppendText(json.data);
        }
    }
    if (json.type == "input" && json.data != last_input) {
        AppendText("<span style=\"color:#666\">" + ">" + html2Escape(json.data) + "</span><span style=\"color:#ccc\">(来自[" + json.sub_type + "]" + json.from + ")</span>");
        last_input = null;
    }
    if (json.type == "response" && json.sub_type == "server_info") {
        server_list.push({
            "guid": json.from,
            "status": json.data.split("\t")[0],
            "start_time": json.data.split("\t")[1],
            "cpu": json.data.split("\t")[2],
            "file": json.data.split("\t")[3],
        });
    }
}

function updateServers() {
    $("header select").html("");
    for (var i = 0; i < server_list.length; i++) {
        $("header select").append('<option value="">' + server_list[i].file + '#' + server_list[i].guid.substring(0, 6) + '</option>')
    }
    server_list = [];
    wsclient.send(JSON.stringify(
        {
            "type": "request",
            "sub_type": "server_info",
            "data": "",
            "from": "WebConsole",
            "target": "all"
        }
    ));
}

function html2Escape(sHtml) {
    return sHtml.replace(/[<>&"]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;' }[c];
    });
}

