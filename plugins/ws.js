/* 设置 */
var config = {
    addr: "ws://127.0.0.1:30000",  // ws地址
    pwd: "", // 密码
    name: "Serein " + serein.version,  // 自定义面板名称
    cd_time: 200 // 发送冷却，设置为0则立即发送
};


var ws = new WebSocket(config.addr);
var output_lines = [];
var input_lines = [];

ws.onopen = function () {
    serein.log("成功连接至" + config.addr)
};

ws.onclose = function () {
    serein.log("连接已断开");
};

ws.onmessage = function (msg) {
    serein.log(msg);
    var json = JSON.parse(msg);
    var type = json.type;
    var sub_type = json.sub_type;
    var data = json.data;
    switch (type) {
        case "event":
            switch (sub_type) {
                case "verify_request":
                    ws.send(JSON.stringify({
                        "type": "api",
                        "sub_type": "panel_verify",
                        "data": getMD5(data + config.pwd),
                        "custom_name": config.name
                    }));
                    break;
            }
            break;
        case "execute":
            switch (sub_type) {
                case "input":
                    if (typeof (data) != "string") {
                        for (var i = 0; i < data.length; i++) {
                            serein.sendCmd(data[i]);
                        }
                    } else {
                        serein.sendCmd(data);
                    }
                    break;
                case "start":
                    serein.startServer();
                    break;
                case "stop":
                    serein.stopServer();
                    break;
                case "kill":
                    serein.killServer();
                    break;
            }
            break;
        case "heartbeat":
            ws.send(JSON.stringify(
                {
                    "type": "event",
                    "sub_type": "heartbeat",
                    "data": {
                        "server_status": serein.getServerStatus(),
                        "server_file": serein.getServerFile(),
                        "server_cpuperc": serein.getServerCPUPersent(),
                        "server_time": serein.getServerTime(),
                        "os": serein.getSysInfo("os"),
                        "cpu": serein.getSysInfo("CPUName"),
                        "cpu_perc": serein.getSysInfo("CPUPercentage"),
                        "ram_total": serein.getSysInfo("TotalRAM"),
                        "ram_used": serein.getSysInfo("UsedRAM"),
                        "ram_perc": serein.getSysInfo("RAMPercentage")
                    },
                }
            ));
            break;
    }
};

serein.setListener("onServerStart", function () {
    ws.send(JSON.stringify({
        "type": "event",
        "sub_type": "start"
    }));
});

serein.setListener("onServerStop", function (code) {
    ws.send(JSON.stringify({
        "type": "event",
        "sub_type": "stop",
        "data": code
    }));
});

serein.setListener("onServerOriginalOutput", function (line) {
    if (line != null) {
        output_lines.push(line);
        var tmp = output_lines;
        setTimeout(function () { // 发送冷却，防止发送数据包过快引起卡顿
            if (tmp == output_lines) {
                ws.send(JSON.stringify({
                    "type": "event",
                    "sub_type": "output",
                    "data": output_lines,
                }));
                output_lines.splice(0, output_lines.length);
            }
        }, config.cd_time + 20);
    }
});

serein.setListener("onServerSendCommand", function (line) {
    if (line != null) {
        input_lines.push(line);
        var tmp = input_lines;
        setTimeout(function () { // 发送冷却，防止发送数据包过快引起卡顿
            if (tmp == input_lines) {
                ws.send(JSON.stringify({
                    "type": "event",
                    "sub_type": "input",
                    "data": input_lines,
                }));
                input_lines.splice(0, input_lines.length);
            }
        }, config.cd_time);
    }
});

// 自动关闭
serein.setListener("onPluginsReload", function () {
    ws.close();
});
serein.setListener("onSereinClose", function () {
    ws.close();
});

