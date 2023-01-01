ll.registerPlugin('iPanel for LiteLoaderBDS', '提供网页版控制台交互', [1, 3], {
    "Author": "Zaitonn"
});
let config = new JsonConfigFile('plugins/iPanel/config.json');
config.init('addr', "ws://127.0.0.1:30000");
config.init('name', '');
config.init('pwd', '');
config.init('reconnectCount', 10);

let outputLines = [];
let inputLines = [];
let reconnectTimer = -1;
let reconnectCount = 0;
const startTime = new Date().getTime();

let wsc = new WSClient();
wsc.listen('onTextReceived', onRecieve);
wsc.listen('onError', onError);
wsc.listen('onLostConnection', onLostConnection);
logger.setTitle('iPanel');

setInterval(() => {
    var _outputLines = outputLines;
    var _inputLines = inputLines;
    if (_inputLines.length != 0)
        wsc.send(JSON.stringify({
            "type": "event",
            "sub_type": "input",
            "data": _inputLines,
        }));
    if (_outputLines.length != 0)
        wsc.send(JSON.stringify({
            "type": "event",
            "sub_type": "output",
            "data": _outputLines,
        }));
    inputLines.splice(0, inputLines.length);
    outputLines.splice(0, outputLines.length);
}, 200);

if (config.get('pwd')) {
    wsc.connect(config.get('addr'));
    mc.listen("onConsoleOutput", onConsoleOutput);
    mc.listen("onConsoleCmd", onConsoleCmd);
} else {
    logger.error('ws地址或密码为空，请重新修改配置文件');
}


function onConsoleCmd(cmd) {
    inputLines.push(cmd);
}

function onConsoleOutput(line) {
    let time = new Date().toLocaleTimeString().split(' ')[0];
    line = line.trimEnd();
    if (line.indexOf('\n') > 0) {
        line.split('\n').forEach((value) => outputLines.push("\u001b[38;2;173;216;230m" + time + "\u001b[0m " + value));
    } else {
        outputLines.push("\u001b[38;2;173;216;230m" + time + "\u001b[0m " + line);
    }
}

function onLostConnection(code) {
    logger.warn('连接已断开，错误码：' + code);
    clearInterval(reconnectTimer);
    reconnectTimer = setInterval(() => {
        if (wsc.status != wsc.Open) {
            if (config.get('reconnectCount') != undefined && config.get('reconnectCount') > reconnectCount) {
                reconnectCount++;
                logger.info("尝试重连中...（第" + reconnectCount + "次）");
                wsc.connect(config.get('addr'));
            } else {
                logger.info("重连次数已达上限");
                clearInterval(reconnectTimer);
            }
        }
    }, 5000);
}

function onError(e) {
    logger.error('ws客户端出现问题' + e);
}

function onRecieve(line) {
    clearInterval(reconnectTimer);
    let json = JSON.parse(line);
    let type = json.type;
    let sub_type = json.sub_type;
    let jsonData = json.data;
    if (sub_type != "heartbeat" && json.sender != undefined) {
        logger.info("来自" + json.sender.type + ": " + type + "-" + sub_type);
    }
    switch (type) {
        case "execute":
            switch (sub_type) {
                case "input":
                    if (typeof (jsonData) != "string") {
                        for (let i = 0; i < jsonData.length; i++) {
                            mc.runcmd(jsonData[i]);
                        }
                    } else {
                        mc.runcmd(jsonData);
                    }
                    break;
                case "stop":
                    wsc.send(JSON.stringify({
                        "type": "event",
                        "sub_type": "stop",
                        "data": -1,
                    }));
                    mc.runcmd('stop');
                    wsc.close();
                    break;
                default:
                    wsc.send(JSON.stringify({
                        "type": "event",
                        "sub_type": "not_support",
                        "data": "当前实例不支持此功能",
                    }));
            }
            break;
        case "event":
            if (sub_type == "heartbeat")
                wsc.send(JSON.stringify(
                    {
                        "type": "event",
                        "sub_type": "heartbeat",
                        "data": {
                            "server_status": true,
                            "server_file": 'bedrock_server_mod.exe ' + mc.getBDSVersion(),
                            "server_time": getSpan(),
                            "server_cpuusage": '',
                            "os": '',
                            "cpu": '',
                            "cpu_usage": '',
                            "ram_total": '',
                            "ram_used": '',
                            "ram_usage": ''
                        },
                    }
                ));
            break;
        case "response":
            switch (sub_type) {
                case "verify_success":
                    reconnectCount = 0;
                    break;
                case "verify_request":
                    wsc.send(JSON.stringify({
                        "type": "api",
                        "sub_type": "instance_verify",
                        "data": data.toMD5(jsonData + config.get('pwd')),
                        "custom_name": config.get('name') ? config.get('name') : `BDS ${mc.getBDSVersion()}`
                    }));
                    break;
                case "verify_failed":
                    logger.error(jsonData);
                    clearInterval(reconnectTimer);
                    break;
            }
            break;
    }
}

/**
 * @returns 时间差
 */
function getSpan() {
    let span = (new Date().getTime() - startTime) / 1000;
    if (span < 60)
        return `${span.toFixed(1)}s`;
    else if (span < 60 * 60)
        return `${(span / 60).toFixed(1)}m`;
    else if (span < 60 * 60 * 24)
        return `${(span / 60 / 60).toFixed(1)}h`;
    else
        return `${(span / 60 / 60 / 24).toFixed(1)}d`;
}