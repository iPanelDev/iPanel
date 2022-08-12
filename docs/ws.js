var wsclient;
var connected = false;
var verifid = false;
var last_input = null;
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
    history.pushState({ 'page_id': 1, 'user_id': 5 }, "", "?addr=" + encodeURIComponent($("#login-main input.addr").val()));
    $("#login-main>#state").show();
    $("#login-main>#state").text("正在连接");
    if ($("#login-main>input.addr").val() == "debug") {
        $("#login-container").hide();
        $("footer").show();
        $("body").css("overflow", "auto");
    } else {
        try {
            wsclient = new WebSocket($("#login-main>input.addr").val());
            wsclient.onmessage = ws_receive;
            wsclient.onopen = ws_open;
            wsclient.onclose = ws_close;
        } catch (e) {
            alert(3, e);
            $("#login-main>#state").text("连接失败");
        }
    }

}

function ws_open() {
    connected = true;
    setInterval(() => { // 连接异常（心跳包超过10s未收到）
        if (connected && last_heartbeat_time - Date.now() > 10) {
            alert(2, "连接可能异常，请检查网络状态");
        }
    }, 5000);
    $("#login-main>#state").text("连接成功，正在进行验证");
}

function ws_close() {
    if (connected && verifid) {
        alert(2, "连接断开，请刷新页面重试");
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
    var type = json.type;
    var sub_type = json.sub_type;
    var data = json.data;
    switch (type) {
        case "verify":
            // 验证请求
            if (sub_type == "request") {
                ws_send("verify", "console_reply", md5(data + $("#login-main input.pwd").val()), "", "webconsole");
                $("#login-main input.pwd").val('');
            }
            break;
        case "notice":
            // 验证通过
            if (sub_type == "verify_success") {
                verifid = true;
                $("#login-container").hide();
                $("footer").show();
                $("body").css("overflow", "auto");
            }
            break;
        case "output":
            // 输出
            if (sub_type == "colored" && json.from == $("header select").find("option:selected").val()) {
                append_text(data);
            }
            break;
        case "input":
            if (data != last_input) {
                append_text("<span style=\"color:#666\">" + ">" + html2Escape(data) + "</span><span style=\"color:#ccc\">(来自[" + sub_type + "]" + json.from.substring(0, 6) + ")</span>");
                last_input = null;
            }
            break;
        case "heartbeat":
            last_heartbeat_time = Date.now();
            if (sub_type == "info") {
                update_server(data);
            }
            break;
        case "event":
            if (!json.from == $("header select").find("option:selected").val()) {
                break;
            }
            switch (sub_type) {
                case "server_start":
                    append_text("#clear");
                    append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>启动中");
                    break;
                case "server_stop":
                    append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>进程已退出（返回：" + html2Escape(data + "") + "）");
            }
            break;
    }
}

