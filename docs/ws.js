var wsclient;
var connected = false;
var verifid = false;
var last_input = null;
var server_list = [];
var last_heartbeat_time = Date.now();

function ws_send(type, sub_type, data, target = "", from = "") {
    if (connected) {
        wsclient.send(JSON.stringify(
            {
                "type": type,
                "sub_type": sub_type,
                "data": data,
                "target": target,
                "from": from
            }
        ));
    }
}

function try_connect() {
    connected = false;
    verifid = false;
    $("#login-main>#state").show();
    $("#login-main>#state").text("正在连接");
    try {
        wsclient = new WebSocket($("#login-main>input.addr").val());
        wsclient.onmessage = ws_receive;
        wsclient.onopen = ws_open;
        wsclient.onclose = ws_close;
    } catch {
        $("#login-main>#state").text("地址不合法");
    }
}

function ws_open() {
    connected = true;
    setInterval(() => { // 连接异常（心跳包超过10s未收到）
        if (connected && last_heartbeat_time - Date.now() > 10) {
            alert("连接可能异常，请检查网络状态");
        }
    }, 5000);
    $("#login-main>#state").text("连接成功，正在进行验证");
}

function ws_close() {
    if (connected && verifid) {
        alert("连接断开，请刷新页面");
    } else if (connected) {
        $("#login-main>#state").text("密码验证失败");
    } else {
        $("#login-main>#state").text("连接超时");
    }
    connected = false;
}

function ws_receive(e) {
    // 控制台输出
    console.log(e.data);
    connected = true;
    var json = JSON.parse(e.data);
    switch (json.type) {
        case "verify":
            // 验证请求
            if (json.sub_type == "request") {
                ws_send("verify", "console_reply", md5(json.data + $("#login-main input.pwd").val()), "", "webconsole")
            }
            break;
        case "notice":
            // 验证通过
            if (json.sub_type == "verify_success") {
                verifid = true;
                $("#login-container").hide();
                $("footer").show();
                $("body").css("overflow", "auto");
            }
            break;
        case "output":
            // 输出
            if (json.sub_type == "colored") {
                append_text(json.data);
            }
            break;
        case "input":
            if (json.data != last_input) {
                append_text("<span style=\"color:#666\">" + ">" + html2Escape(json.data) + "</span><span style=\"color:#ccc\">(来自[" + json.sub_type + "]" + json.from.substring(0, 6) + ")</span>");
                last_input = null;
            }
            break;
        case "heartbeat":
            last_heartbeat_time = Date.now();
            break;
    }
}

