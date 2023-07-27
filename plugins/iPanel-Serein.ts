/// <reference path="SereinJSPluginHelper/index.d.ts"/>

declare interface Packet {
    type: string,
    sub_type: string,
    data?: any,
    sender?: any
}

declare interface IConfig {
    customName?: string,
    websocket: {
        addr: string,
        password: string
    }
    reconnect: {
        enable: boolean,
        interval: number,
        maxTimes: number
    }
}

declare interface IInfo {
    verified: boolean,
    disconnectInfo?: string,
    trialTimes: number
}

const VERSION = '2.1.7.26';
serein.registerPlugin('iPanel-Serein', VERSION, 'Zaitonn', '网页版控制台-Serein插件');
serein.setListener('onPluginsLoaded', connect);
serein.setListener('onServerStart', onServerStart);
serein.setListener('onServerStop', onServerStop);
serein.setListener('onServerSendCommand', onServerSendCommand);
serein.setListener('onServerOriginalOutput', onServerOriginalOutput);

const {
    Security: {
        Cryptography: {
            MD5
        }
    },
    Text: {
        Encoding
    }
} = System;

const logger = new Logger('iPanel');
const paths = {
    config: 'plugins/iPanel-Serein/config.json',
    dir: 'plugins/iPanel-Serein'
};

const inputCache = [];
const outputCache = [];

const baseConfig: IConfig = {
    websocket: {
        addr: 'ws://127.0.0.1:30000',
        password: ''
    },
    customName: '',
    reconnect: {
        enable: true,
        interval: 1000 * 7.5,
        maxTimes: 10
    }
};

import stdio = require('./modules/stdio.js');

const {
    existFile,
    createDirectory,
    writeAllTextToFile,
    readAllTextFromFile
} = stdio;

const config = loadConfig();

let ws: WSClient = null;
const info: IInfo = {
    verified: false,
    trialTimes: 0
};

function send(packet: Packet) {
    if (ws?.state === 1) {
        ws.send(JSON.stringify(packet));
    }
}

function fastSend(type: string, sub_type: string, data?: any) {
    send({ type, sub_type, data });
}

function msgHandler(msg: string) {
    const packet = JSON.parse(msg) as Packet;

    switch (packet.type) {
        case 'action':
            handleAction(packet);
            break;

        case 'event':
            handleEvent(packet);
            break;
    }
}

function handleEvent({ sub_type, data }: Packet) {
    switch (sub_type) {
        case 'verify_result':
            if (data.success) {
                logger.info('[Host] 验证通过');
                info.verified = true;
            }
            else
                logger.info(`[Host] 验证失败: ${data.reason}`);
            break;

        case 'disconnection':
            info.disconnectInfo = data.reason;
            break;
    }
}

function handleAction({ sub_type, data, sender }: Packet) {
    switch (sub_type) {
        case 'verify_request':
            send({
                type: 'action',
                sub_type: 'verify',
                data: {
                    token: getMD5(data.random_key + config.websocket.password),
                    custom_name: config.customName || null,
                    client_type: 'instance'
                },
            });
            break;

        case 'heartbeat':
            const {
                name: os,
                hardware: {
                    CPUs: [
                        { name: cpu_name }
                    ],
                    RAM: {
                        free: free_ram,
                        total: total_ram
                    }

                } } = serein.getSysInfo();
            send({
                type: 'action',
                sub_type: 'heartbeat',
                data: {
                    sys: {
                        os,
                        cpu_name,
                        total_ram,
                        free_ram,
                        cpu_usage: serein.getCPUUsage()
                    },
                    server: {
                        filename: serein.getServerStatus() && serein.getServerFile() || null,
                        status: serein.getServerStatus(),
                        run_time: serein.getServerTime() || null,
                        usage: serein.getServerCPUUsage()
                    }
                }
            });
            break;

        case 'server_start':
            logger.info(`[${sender.address}] 启动服务器`);
            serein.startServer();
            break;

        case 'server_stop':
            logger.info(`[${sender.address}] 关闭服务器`);
            serein.stopServer();
            break;

        case 'server_kill':
            logger.warn(`[${sender.address}] 强制结束服务器`);
            serein.killServer();
            break;

        case 'server_input':
            logger.info(`[${sender.address}] 服务器输入`);
            Array.from(data).forEach((line: string) => serein.sendCmd(line));
            break;
    }
}

function getMD5(text: string) {
    let result = '';
    MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(String(text))).forEach((byte: Number) => result += Number(byte).toString(16).padStart(2, '0'));
    return result;
}

function connect() {
    ws = new WSClient(config.websocket.addr, serein.namespace);
    ws.onopen = openHandler;
    ws.onclose = closeHandler;
    ws.onmessage = msgHandler;
    ws.open();
}

function openHandler() {
    info.disconnectInfo = undefined;
    info.trialTimes = 0;
    logger.info(`已连接到“${config.websocket.addr}”`);
}

function closeHandler() {
    logger.warn(`连接已断开${info.disconnectInfo ? ': ' + info.disconnectInfo : ''}`);
    if (!info.verified)
        logger.warn('貌似没有成功连接过，自动重连已关闭。请检查地址是否配置正确');
    else {
        // @ts-ignore
        setTimeout(reconnect, config.reconnect.interval);
    }
}

function reconnect() {
    info.trialTimes += 1;
    if (info.trialTimes < config.reconnect.maxTimes) {
        logger.info(`尝试重连中...${info.trialTimes}/${config.reconnect.maxTimes}`);
        connect();
    } else {
        logger.warn('重连次数已达上限');
    }
}

function sendInputCache() {
    if (inputCache.length > 0) {
        fastSend('event', 'server_input', inputCache);
        inputCache.splice(0, inputCache.length);
    }
}

function sendOutputCache() {
    if (outputCache.length > 0) {
        fastSend('event', 'server_output', outputCache);
        outputCache.splice(0, outputCache.length);
    }
}

function onServerStart() {
    fastSend('event', 'server_start');
}

function onServerStop(code: number) {
    fastSend('event', 'server_stop', code);
}

function onServerSendCommand(line: string) {
    inputCache.push(line);
}

function onServerOriginalOutput(line: string) {
    outputCache.push(line);
}

function loadConfig(): IConfig {
    if (existFile(paths.config)) {
        const config = JSON.parse(readAllTextFromFile(paths.config));
        checkConfig(config);
        return config;
    } else {
        createConfigFile();
        throw new Error('配置文件已创建，请修改后重新加载此插件');
    }
}

function createConfigFile() {
    createDirectory(paths.dir);
    writeAllTextToFile(paths.config, JSON.stringify(baseConfig, null, 4));
}

function checkConfig(config: IConfig) {
    if (!config.customName)
        logger.warn('配置文件中自定义名称为空');

    if (!config.websocket)
        throw new Error('配置文件中`websocket`项丢失，请删除配置文件后重新加载以创建');

    if (!config.reconnect)
        throw new Error('配置文件中`reconnect`项丢失，请删除配置文件后重新加载以创建');

    if (!config.websocket.addr)
        throw new Error('配置文件中`websocket.addr`项为空');

    if (!config.websocket.password)
        throw new Error('配置文件中`websocket.password`项为空');

    if (typeof (config.websocket.addr) != 'string')
        throw new Error('配置文件中`websocket.addr`类型不正确');

    if (typeof (config.websocket.password) != 'string')
        throw new Error('配置文件中`websocket.password`类型不正确');

    if (!/^wss?:\/\/.+/.test(config.websocket.addr))
        throw new Error('配置文件中`websocket.addr`项格式不正确');

    if (typeof (config.reconnect.enable) != 'boolean')
        throw new Error('配置文件中`reconnect.enable`类型不正确');

    if (typeof (config.reconnect.interval) != 'number')
        throw new Error('配置文件中`reconnect.interval`类型不正确');

    if (typeof (config.reconnect.maxTimes) != 'number')
        throw new Error('配置文件中`reconnect.maxTimes`类型不正确');

    if (config.reconnect.interval <= 500)
        throw new Error('配置文件中`reconnect.interval`数值超出范围');

    if (config.reconnect.maxTimes < 0)
        throw new Error('配置文件中`reconnect.maxTimes`数值超出范围');
}


// @ts-ignore
setInterval(() => { sendInputCache(); sendOutputCache(); }, 250);