var wsclient;
var connected = false;
var verifid = false;
var last_heartbeat_time = Date.now();
var server_status = false;

function ws_send(type, sub_type, data) {
    if (connected) {
        wsclient.send(JSON.stringify(
            {
                "type": type,
                "sub_type": sub_type,
                "data": data
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
    // console.log(e.data);
    connected = true;
    var json = JSON.parse(e.data);
    var type = json.type;
    var sub_type = json.sub_type;
    var data = json.data;
    switch (type) {
        case "event":
            switch (sub_type) {
                case "verify_request":
                    wsclient.send(JSON.stringify(
                        {
                            "type": "api",
                            "sub_type": "console_verify",
                            "data": md5(data + $("#login-main input.pwd").val()),
                            "custom_name": "webconsole"
                        }
                    ));
                    break;
                case "input":
                    for (var i = 0; i < data.length; i++) {
                        append_text(">"+html2Escape(data[i]));
                    }
                    break;
                case "output":
                    for (var i = 0; i < data.length; i++) {
                        append_text(color_escape(html2Escape(data[i])));
                    }
                    break;
                case "start":
                    append_text("#clear");
                    append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>启动中");
                    server_status = true;
                    break;
                case "stop":
                    append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>进程已退出（返回：" + html2Escape(data + "") + "）");
                    server_status = false;
                    update_info();
                    break;
                case "heartbeat":
                    update_info(data);
                    break;
            }
            break;
        case "response":
            switch (sub_type) {
                case "verify_success":
                    ws_send("api", "list");
                    verifid = true;
                    $("#login-container").hide();
                    $("footer").show();
                    $("body").css("overflow", "auto");
                    setInterval(() => { // 连接异常（心跳包超过10s未收到）
                        if (connected && last_heartbeat_time - Date.now() > 10) {
                            alert(2, "连接可能异常，请检查网络状态");
                        } else {
                            ws_send("api", "list");
                        }
                    }, 7500);
                    break;
                case "list":
                    update_panel_dic(data);
                    break;
            }
    }
}

