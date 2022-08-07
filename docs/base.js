var wsclient = new WebSocket("ws://0.0.0.0");
var connected = false;
var verifid = false;
var last_input =null;

$(document).ready(init);

function debugMode() {
    $("#login input.pwd").val("pwd");
    wsclient = new WebSocket("ws://127.0.0.1:30000");
    wsclient.onmessage = receive;
    wsclient.onopen = open;
    wsclient.onclose = close;
}

function init() {
    debugMode();
    $("#login>#state").hide();
    $("#connect").click(tryConnect);
    $("#input-area>button").click(cmdInput);
    $("#login>input.addr").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            $("#login>input.pwd").focus();
        }
    });
    $("#login>input.pwd").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            tryConnect();
        }
    });
    $("#login>input").bind("input propertychange", function () {
        $("#login>#state").hide();
    });
    $("#input-area>input").bind("keypress", function (event) {
        if (event.keyCode == "13") {
            cmdInput();
        }
    });
}

function cmdInput() {
    var cmd = $("#input-area>input").val();
    last_input=cmd;
    AppendText(">" + html2Escape(cmd));
    if (connected) {
        wsclient.send(JSON.stringify(
            {
                "type": "input",
                "sub_type": "console",
                "data": cmd
            }
        ));
    }
    $("#input-area>input").val("");
}

function tryConnect() {
    connected = false;
    verifid = false;
    $("#login>#state").show();
    $("#login>#state").text("正在连接");
    try {
        wsclient = new WebSocket($("#login input.addr").val());
        wsclient.onmessage = receive;
        wsclient.onopen = open;
        wsclient.onclose = close;
    } catch {
        $("#login>#state").text("地址不合法");
    }
}

function open() {
    connected = true;
    $("#login>#state").text("连接成功，正在进行验证");
}

function close() {
    connected = false;
    if (connected && verifid) {
        $("#login>#state").text("连接断开");
    } else if (connected) {
        $("#login>#state").text("密码验证失败");
    } else {
        $("#login>#state").text("连接超时");
    }
    $("#mask").show();
    $("#main").css("filter", "blur(2px)");
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
                    "data": md5(json.data + $("#login input.pwd").val()),
                    "from": "WebConsole"
                }
            ))
        }
    }
    if (json.type == "notice" && json.sub_type == "verify_success") {
        verifid = true;
        $("#mask").hide();
        $("#main").css("filter", "none");
        $("#login>#state").text("验证成功");
    }
    if (json.type == "output") {
        if (json.sub_type == "colored") {
            AppendText(json.data);
        }
    }
    if (json.type == "input"&&json.data!=last_input) {
        AppendText("<span style=\"color:#666\">" + ">" + html2Escape(json.data) + "</span><span style=\"color:#ccc\">(来自["+json.sub_type+"]"+json.from+")</span>");
        last_input=null;
    }
}

function html2Escape(sHtml) {
    return sHtml.replace(/[<>&"]/g, function (c) {
        return { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;' }[c];
    });
}