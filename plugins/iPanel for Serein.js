var config = {
    addr: "ws://127.0.0.1:30000",
    debug: false,
    reportInterval: 200,
    name: "",
    pwd: "123",
    reconnectCount: 10,
};

var namespace = serein.registerPlugin("iPanel", "v1.3", "Zaitonn", "提供网页版控制台交互");
serein.log(namespace)
var IO = importNamespace('System.IO');
var File = IO.File;
var Directory = IO.Directory;

var ws = new WebSocket(config.addr, namespace);
var logger = new Logger("iPanel");

var outputLines = [];
var inputLines = [];
var reconnectTimer;
var reconnectCount = 0;


/**
 * @description 发送数据包
 * @param {string} type 
 * @param {string} sub_type 
 * @param {*} data 
 */
function send(type, sub_type, data = null) {
    if (data || typeof (data) == 'number')
        ws.send(JSON.stringify({
            "type": type,
            "sub_type": sub_type,
            "data": data
        }));
    else
        ws.send(JSON.stringify({
            "type": type,
            "sub_type": sub_type
        }));
}

/**
 * @description 上报缓存内容
 */
function report() {
    var _inputLines = inputLines;
    if (_inputLines.length != 0)
        send("event", "input", _inputLines);
    inputLines.splice(0, inputLines.length);
    var _outputLines = outputLines;
    if (_outputLines.length != 0)
        send("event", "output", _outputLines);
    outputLines.splice(0, outputLines.length);
}

serein.setListener("onServerStart", () => send("event", "start"));
serein.setListener("onServerStop", (exitcode) => send("event", "stop", exitcode));

serein.setListener("onServerOriginalOutput", (line) => {
    if (line)
        outputLines.push(line);
});

serein.setListener("onServerSendCommand", (line) => {
    if (line)
        inputLines.push(line);
});

if (Directory.Exists('plugins/iPanel') && File.Exists('plugins/iPanel/config.json')) {
    try {
        config = JSON.parse(File.ReadAllText('plugins/iPanel/config.json'));
    } catch (e) {
        throw new Error(`读取配置文件时出现问题：${e}`);
    }
    if (!config.pwd) {
        logger.error("密码为空");
        logger.error("请使用文本编辑器打开 plugins/iPanel/config.json 修改相应配置");
    } else {
        if (!config.name)
            config.name = "Serein " + serein.version;
        ws.open();
        reconnectTimer = setInterval( ()=> {
            if (ws.state != 0 && ws.state != 1) {
                if (config.reconnectCount != undefined && config.reconnectCount > reconnectCount) {
                    reconnectCount++;
                    ws.open();
                    logger.info(`尝试重连中...（第${reconnectCount}次）`);
                } else {
                    logger.info("重连次数已达上限");
                    clearInterval(reconnectTimer);
                }
            }
        }, 5000);
        setInterval(
            report,
            config.reportInterval == undefined || config.reportInterval < 200 ? 200 : config.reportInterval
        );
    }
}
else {
    Directory.CreateDirectory('plugins/iPanel');
    File.WriteAllText('plugins/iPanel/config.json', JSON.stringify(config, null, 4));
    logger.error('配置文件不存在，已重新创建')
    logger.info("请使用文本编辑器打开 plugins/iPanel/config.json 修改相应配置");
}

ws.onopen = () => {
    logger.info("成功连接至 " + config.addr);
    reconnectCount = 0;
};

ws.onclose = () => {
    logger.warn("连接已断开");
};

ws.onmessage = (text) => {
    if (config.debug)
        logger.info(text);
    var json = JSON.parse(text);
    var type = json.type;
    var sub_type = json.sub_type;
    var data = json.data;
    if (sub_type != "heartbeat" && json.sender != undefined) {
        logger.info("来自" + json.sender.type + ": " + type + "-" + sub_type);
    }
    switch (type) {
        case "execute":
            switch (sub_type) {
                case "input":
                    if (!serein.getServerStatus())
                        send("response", "execute_failed", "服务器不在运行中");
                    else if (typeof (data) != "string") {
                        for (let i = 0; i < data.length; i++) {
                            serein.sendCmd(data[i]);
                        }
                    } else {
                        serein.sendCmd(data);
                    }
                    break;
                case "start":
                    if (!serein.getServerStatus())
                        serein.startServer();
                    else
                        send("event", "execute_failed", "服务器正在运行中");
                    break;
                case "stop":
                    if (serein.getServerStatus())
                        serein.stopServer();
                    else
                        send("event", "execute_failed", "服务器不在运行中");
                    break;
                case "kill":
                    if (serein.getServerStatus())
                        serein.killServer();
                    else
                        send("event", "execute_failed", "服务器不在运行中");
                    break;
            }
            break;
        case "event":
            if (sub_type == "heartbeat")
                send("event", "heartbeat", {
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
                });
            break;
        case "response":
            switch (sub_type) {
                case "verify_request":
                    ws.send(JSON.stringify({
                        "type": "api",
                        "sub_type": "instance_verify",
                        "data": getMD5(data + config.pwd),
                        "custom_name": config.name
                    }));
                    break;
                case "verify_failed":
                    // 验证失败，自动停止重连
                    logger.error(data);
                    clearInterval(reconnectTimer);
                    break;
            }
            break;
    }
};
