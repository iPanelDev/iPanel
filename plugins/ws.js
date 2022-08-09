var config = {
    addr: "ws://127.0.0.1:30000",
    pwd: "pwd",
    name: "Serein "+serein.version
};

var ws = new WebSocket(config.addr);

ws.onopen = function () {
    serein.log("成功连接至" + config.addr)
};

ws.onclose = function () {
    serein.log("连接已断开");
};

ws.onmessage = function (msg) {
    serein.log(msg);
    switch (JSON.parse(msg).type) {
        case "verify":
            if (JSON.parse(msg).sub_type == "request") {
                ws.send(JSON.stringify({
                    "type": "verify",
                    "sub_type": "panel_reply",
                    "data": getMD5(JSON.parse(msg).data + config.pwd),
                    "custom_name": config.name
                }))
            }
            break;
        case "input":
            if (JSON.parse(msg).sub_type == "console_input") {
                serein.sendCmd(JSON.parse(msg).data);
            }
            break;
        case "heartbeat":
            ws.send(JSON.stringify(
                {
                    "type": "heartbeat",
                    "sub_type": "response",
                    "data": {
                        "server_status": serein.getServerStatus(),
                        "server_file": serein.getServerFile(),
                        "server_cpuperc": serein.getServerCPUPersent(),
                        "server_time": serein.getServerTime(),
                        "os": serein.getSysInfo("os"),
                        "cpu": serein.getSysInfo("CPUName"),
                        "ram_total": serein.getSysInfo("TotalRAM"),
                        "ram_used": serein.getSysInfo("UsedRAM"),
                        "ram_perc": serein.getSysInfo("RAMPercentage"),
                        "cpu_perc": serein.getSysInfo("CPUPercentage")
                    },
                }
            ));
            break;
    }
};

serein.setListener("onServerOriginalOutput", function (line) {
    ws.send(JSON.stringify({
        "type": "output",
        "sub_type": "output",
        "data": line,
    }))
});

serein.setListener("onServerSendCommand", function (line) {
    ws.send(JSON.stringify({
        "type": "input",
        "sub_type": "server",
        "data": line,
    }))
});

// 自动关闭
serein.setListener("onPluginsReload", function () {
    ws.close()
});
serein.setListener("onSereinClose", function () {
    ws.close()
});
