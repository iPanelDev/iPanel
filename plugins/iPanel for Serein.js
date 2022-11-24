/* 设置 */
var config = {
    addr: "ws://127.0.0.1:30000",  // ws地址
    pwd: "123",         // 密码
    name: "",           // 自定义面板名称，为空则为"Serein (版本号)"
    coolingTime: 200,   // 发送冷却，设置为0则立即发送
    debug: true,        // 调试模式
};


var ws = new WebSocket(config.addr);
var output_lines = [];
var input_lines = [];
var logger = new Logger("iPanel");
var reconnectTimer;

serein.registerPlugin("iPanel", "v1.1", "Zaitonn", "提供网页版控制台交互");

function init() {
    if (!config.pwd)
        logger.error("密码为空");
    else {
        if (!config.name) {
            config.name = "Serein " + serein.version;
        }
        ws.open();
        reconnectTimer = setInterval(function () {
            if (ws.state != 1) {
                ws.open();
                logger.info("尝试重连中...")
            }
        }, 5000);
    }
}

// 数据包接收处理
function recieve(text) {
    if (config.debug)
        logger.info(text);
    var json = JSON.parse(text);
    var type = json.type;
    var sub_type = json.sub_type;
    var data = json.data;
    switch (type) {
        case "event":
            switch (sub_type) {
                case "verify_request":
                    ws.send(JSON.stringify({
                        "type": "api",
                        "sub_type": "instance_verify",
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
        case "response":
            if (sub_type == "verify_failed") {
                // 验证失败，自动停止重连
                logger.error(data);
                clearInterval(reconnectTimer);
            }
            break;
    }
}

ws.onopen = function () {
    logger.info("成功连接至" + config.addr)
};

ws.onclose = function () {
    logger.warn("连接已断开");
};

ws.onmessage = recieve;

// 设置服务器启动监听
serein.setListener("onServerStart", function () {
    ws.send(JSON.stringify({
        "type": "event",
        "sub_type": "start"
    }));
});

// 设置服务器关闭监听
serein.setListener("onServerStop", function (exitcode) {
    ws.send(JSON.stringify({
        "type": "event",
        "sub_type": "stop",
        "data": exitcode
    }));
});

// 设置服务器输出监听
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
        }, config.coolingTime + 20);
    }
});

// 设置服务器命令输入监听
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
        }, config.coolingTime);
    }
});

init(); // 初始化
