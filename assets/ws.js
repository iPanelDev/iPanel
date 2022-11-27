let wsclient;
let connected = false;
let verifid = false;
let lastHeartbeatTime = Date.now();
let server_status = false;

/**
 * @description 发送数据包
 * @param {string} type 
 * @param {string} sub_type 
 * @param {*} data 
 */
function send(type, sub_type, data) {
    if (connected) {
        wsclient.send(JSON.stringify(
            {
                "type": type,
                "sub_type": sub_type,
                "data": data
            }
        ));
        console.log(
            "[↑]\n" + JSON.stringify(
                {
                    "type": type,
                    "sub_type": sub_type,
                    "data": data
                }, null, 2
            ));
    }
}

/**
 * @description 尝试连接
 */
function tryConnect() {
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
            wsclient.onmessage = receive;
            wsclient.onopen = function () {
                connected = true;
                $("#login-main>#state").text("连接成功，正在进行验证");
            };
            wsclient.onclose = function () {
                if (connected && verifid) {
                    notice(2, "连接断开，请刷新页面重试");
                } else if (connected) {
                    $("#login-main>#state").text("密码验证失败");
                } else {
                    $("#login-main>#state").text("连接超时");
                }
                connected = false;
            };
        } catch (e) {
            notice(3, e);
            $("#login-main>#state").text("连接失败");
        }
    }

}

/**
 * @description 接收数据包处理
 * @param {MessageEvent<*>} e 
 */
function receive(e) {
    connected = true;
    let json = JSON.parse(e.data);
    let error = false;
    let type = json.type;
    let sub_type = json.sub_type;
    let data = json.data;
    switch (type) {
        case "event":
            switch (sub_type) {
                case "input":
                    for (var i = 0; i < data.length; i++) {
                        appendText(">" + html2Escape(data[i]));
                    }
                    break;
                case "output":
                    for (var i = 0; i < data.length; i++) {
                        appendText(colorEscape(html2Escape(data[i])));
                    }
                    break;
                case "start":
                    appendText("#clear");
                    appendText("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>启动中");
                    server_status = true;
                    break;
                case "stop":
                    appendText("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>进程已退出（返回：" + html2Escape(data + "") + "）");
                    server_status = false;
                    updateInfo();
                    changeInstance();
                    if (waitingToRestart) {
                        appendText("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>你可以按下停止按钮来取消这次重启")
                        setTimeout(() => {
                            if (waitingToRestart)
                                startServer();
                            waitingToRestart = false;
                        }, 5000);
                    }
                    break;
                case "heartbeat":
                    updateInfo(data);
                    changeInstance();
                    break;
            }
            break;
        case "response":
            switch (sub_type) {
                case "verify_request":
                    wsclient.send(JSON.stringify(
                        {
                            "type": "api",
                            "sub_type": "console_verify",
                            "data": md5(data + $("#login-main input.pwd").val()),
                            "custom_name": "iPanel Web Console"
                        }
                    ));
                    break;
                case "verify_success":
                    verifid = true;
                    send("api", "list");
                    $("#login-container").hide();
                    $("footer").show();
                    $("body").css("overflow", "auto");
                    setInterval(() => { // 连接异常（心跳包超过10s未收到）
                        if (connected && lastHeartbeatTime - Date.now() > 10) {
                            notice(2, "连接可能异常，请检查网络状态");
                        } else {
                            send("api", "list");
                        }
                    }, 7500);
                    break;
                case "list":
                    updateInstanceDic(data);
                    break;
                case "invalid":
                case "verify_failed":
                    error = true;
                    break;
            }
    }
    if (!error)
        console.log("[↓]\n" + JSON.stringify(json, null, 4));
    else
        console.error("[↓]\n" + JSON.stringify(json, null, 4));
    if (!checkedVersion && json.sender.type == "iPanel") {
        if (json.sender.version != VERSION)
            notice(2, "控制台和iPanel版本不符，可能导致部分功能无法使用，请即时更新")
        checkedVersion = true;
    }
}

