import json
import os
import platform
import time
from hashlib import md5
from pathlib import Path
from threading import Thread

import psutil
from psutil import Process
from websocket import WebSocketApp

from mcdreforged.api.all import *

PLUGIN_METADATA = {
    'id': 'ipanel',
    'version': '2.1.7.29',
    'name': 'iPanel-MCDReforged',
    'description': '适用于MCDReforged的iPanel实例插件',
    'author': 'Zaitonn',
    'link': 'https://github.com/Zaitonn/iPanel',
}

DEFAULT_CONFIG = {
    'websocket': {
        'addr': 'ws://127.0.0.1:30000',
        'password': ''
    },
    'customName': '',
    'reconnect': {
        'enable': True,
        'interval': 10000,
        'maxTimes': 10
    }
}

CONFIG_PATH = 'plugins/iPanel-MCDReforged/config.json'
PATH = 'plugins/iPanel-MCDReforged'

logger = MCDReforgedLogger(PLUGIN_METADATA['id'])

inputs = []
outputs = []
unload = False


class Config():
    '''配置'''
    class WebSocket():
        '''ws配置'''

        def __init__(self, websocketConfig: dict):
            self.addr: str = websocketConfig.get('addr')
            self.password: str = websocketConfig.get('password')

            if not isinstance(self.addr, str):
                raise TypeError('`config.websocket.addr`类型不正确')

            if not isinstance(self.password, str):
                raise TypeError('`config.websocket.password`类型不正确')

    class Reconnect():
        '''重连配置'''

        def __init__(self, reconnectConfig: dict):
            self.enable: bool = reconnectConfig.get('enable')
            self.interval: int = reconnectConfig.get('interval')
            self.maxTimes: int = reconnectConfig.get('maxTimes')

            if not isinstance(self.enable, bool):
                raise TypeError('`config.reconnect.enable`类型不正确')

            if not isinstance(self.enable, int):
                raise TypeError('`config.reconnect.interval`类型不正确')

            if not isinstance(self.maxTimes, int):
                raise TypeError('`config.reconnect.maxTimes`类型不正确')

            if self.maxTimes < 0:
                raise ValueError('`config.reconnect.maxTimes`值过小')

    def __init__(self):
        self.__config__ = self.load()

    def write(self):
        '''写入文件'''
        Path(PATH).mkdir(parents=True, exist_ok=True)
        with open(CONFIG_PATH, 'w', encoding='utf8') as file:
            file.write(json.dumps(DEFAULT_CONFIG,
                       ensure_ascii=False, indent=4))

    def load(self):
        '''读取'''
        if not os.path.exists(CONFIG_PATH):
            self.write()
            raise FileNotFoundError(
                '配置文件"{}"未找到，已重新创建。请打开修改相应配置后使用【!!MCDR plg ra】重新加载此插件'.format(CONFIG_PATH))

        with open(CONFIG_PATH, 'r', encoding='utf8') as file:
            text = file.read()

        config: dict = json.loads(text)
        self.custom_name = config.get('custonName')
        if not self.custom_name or len(self.custom_name) == 0:
            self.custom_name = 'MCDReforged#{}'.format(
                PLUGIN_METADATA['version'])
            logger.warn(
                '"config.customName"为空，将使用默认名称"{}"'.format(self.custom_name))

        self.reconnect = self.Reconnect(config.get('reconnect'))
        self.websocket = self.WebSocket(config.get('websocket'))


CONFIG = Config()


class Packet:
    '''数据包'''

    class Sender:
        '''发送者'''

        def __init__(self, obj: dict) -> None:
            self.type: str = obj.get('type')
            self.address: str = obj.get('address')
            self.name: str = obj.get('name')

    def __init__(self, text: str) -> None:
        self.__obj___: dict = json.loads(text)
        self.type: str = self.__obj___.get('type')
        self.sub_type: str = self.__obj___.get('sub_type')
        self.data: dict | str = self.__obj___.get('data')
        self.sender = self.Sender(self.__obj___.get('sender'))
        self.time: int = self.__obj___.get('time')


class iPanelWsConnnection():
    '''ws连接对象'''

    def __init__(self, config: Config) -> None:
        self.config = config
        self.config.websocket.password = self.config.websocket.password
        self.__check__()
        self.verified = False
        self.reason = None
        self.restart_times = 0

    def __check__(self):
        '''检查输入值'''
        if not isinstance(self.config.websocket.addr, str):
            raise TypeError('`address`类型不正确')

        if not isinstance(self.config.websocket.password, str):
            raise TypeError('`password`类型不正确')

        if len(self.config.websocket.addr) == 0:
            raise ValueError('`address`为空')

        if len(self.config.websocket.password) == 0:
            raise ValueError('`password`为空')

        if not self.config.websocket.addr.startswith(('ws://', 'wss://')):
            raise ValueError('`address`格式不正确')

    def wait_for_reconnect(self):
        '''等待重连'''
        time.sleep(self.config.reconnect.interval/1000)
        logger.info('尝试重连中...{}/{}'.format(self.restart_times,
                    self.config.reconnect.maxTimes))
        self.connect()

    def connect(self):
        '''连接'''
        self._ws = WebSocketApp(
            self.config.websocket.addr,
            on_open=self._on_open,
            on_close=self._on_close,
            on_error=self._on_error,
            on_message=self._on_message
        )
        self._ws.run_forever()

    def disconnect(self):
        '''断开连接'''
        self._ws.close()

    def _on_open(self, this: WebSocketApp):
        '''开启事件'''
        logger.info('已连接到"{}"'.format(self.config.websocket.addr))

    def _on_close(self, this: WebSocketApp, code: int = 0, _: str = None):
        '''关闭事件'''
        if code is not None:
            logger.warn('连接已断开(Code: {})'.format(code))
        else:
            logger.warn('连接已断开')

        if self.reason:
            logger.warn('原因：{}'.format(self.reason))

        if not self.verified:
            logger.warn('貌似没有成功连接过，自动重连已关闭。请检查地址是否配置正确')
        elif 0 < self.restart_times < self.config.reconnect.maxTimes and self.config.reconnect.enable:
            self.restart_times += 1
            self.wait_for_reconnect()

    def _on_error(self, this: WebSocketApp, e: Exception):
        '''错误事件'''
        logger.error(e)

    def _on_message(self, this: WebSocketApp, msg: str):
        '''消息事件'''
        packet = Packet(msg)
        match packet.type:
            case 'event':
                self.events_handle(packet)

            case 'action':
                self.actions_handle(packet)

            case _:
                logger.warn('接收到不支持的数据包类型：{}'.format(packet.type))

    def actions_handle(self, packet: Packet):
        '''处理action数据包'''
        match packet.sub_type:
            case 'verify_request':
                # 请求验证
                self.send('action',
                          'verify',
                          {
                              'token': md5((packet.data['random_key']+self.config.websocket.password).encode(encoding='UTF-8')).hexdigest(),
                              'client_type': 'instance',
                              'custom_name': self.config.custom_name
                          })

            case 'heartbeat':
                # 心跳
                self.send(
                    'action',
                    'heartbeat',
                    {
                        'sys': get_sys_info(),
                        'server': get_server_info()
                    }
                )

            case 'server_start':
                PluginServerInterface.get_instance().start()

            case 'server_stop':
                PluginServerInterface.get_instance().stop()

            case 'server_kill':
                PluginServerInterface.get_instance().kill()

            case 'server_input':
                instance = PluginServerInterface.get_instance()
                if not instance.is_server_running:
                    return

                list = []
                for line in packet.data:
                    if not isinstance(line, str):
                        continue

                    instance.execute(line)
                    list.append(line)

                self.send('event', 'server_input', list)

    def events_handle(self, packet: Packet):
        '''处理event数据包'''
        match packet.sub_type:
            case 'verify_result':
                if packet.data.get('success'):
                    logger.info('[Host] 验证通过')
                    self.verified = True
                    self.restart_times = 0
                else:
                    logger.info('[Host] 验证失败: {}'.format(
                        packet.data.get('reason')))

            case 'disconnection':
                self.reason = packet.data.get('reason')

    def send(self, type: str, sub_type: str, data: dict | str = None):
        '''发送数据包'''
        try:
            self._ws.send(json.dumps({
                'type': type,
                'sub_type': sub_type,
                'data': data
            }, ensure_ascii=False))
        except Exception as e:
            logger.debug(e)

    def sync_cache(self):
        '''同步缓存'''
        while not unload:
            if len(inputs) > 0:
                connection.send('event', 'server_input', inputs)
                inputs.clear()

            if len(outputs) > 0:
                connection.send('event', 'server_output', outputs)
                outputs.clear()

            time.sleep(0.25)


connection = iPanelWsConnnection(CONFIG)


# MCDR插件事件

def on_load(server: PluginServerInterface, prev_module):
    '''插件被加载'''
    logger.info(
        'iPanel-MCDReforged@{}加载成功.'.format(PLUGIN_METADATA.get('version')))
    Thread(
        target=connection.connect,
        name='WSConnection',
        daemon=True).start()
    Thread(
        target=connection.sync_cache,
        daemon=True,
        name='SyncCache').start()


def on_unload(server: PluginServerInterface):
    '''插件被卸载'''
    global unload
    unload = True
    connection.config.reconnect.enable = False
    connection.disconnect()


# 服务器事件

def on_server_start(server: PluginServerInterface):
    '''服务器启动'''
    connection.send('event', 'server_start')
    server.set_exit_after_stop_flag(False)  # 关闭服务器停止后退出


def on_server_stop(server: PluginServerInterface, server_return_code: int):
    '''服务器停止'''
    connection.send('event', 'server_stop', server_return_code)


def on_info(server: PluginServerInterface, info: Info):
    '''服务器输出'''

    if not server.is_server_running() or (info.content.startswith('!!MCDR') and info.source):
        # 屏蔽未启动和“!!MCDR”之类的内置指令
        return

    if info.source:
        inputs.append(info.raw_content)
        return

    outputs.append(info.raw_content)


# 工具方法

def get_sys_info() -> dict:
    '''获取系统信息'''
    mem = psutil.virtual_memory()
    return {
        'os': platform.platform(),
        'cpu_name': platform.processor(),
        'cpu_usage': psutil.cpu_percent(),
        'total_ram': mem.total,
        'free_ram': mem.free,
    }


def get_server_info() -> dict:
    mcdr = PluginServerInterface.get_instance()
    if mcdr.is_server_running():
        pids = mcdr.get_server_pid_all()
        cpu_usage = 0
        file_name = None
        time_span = 0

        for i in pids:
            try:
                cpu_usage = cpu_usage + Process(i).cpu_percent()
            except:
                continue

        try:
            current_process = Process(mcdr.get_server_pid())
            time_span = time.time() - current_process.create_time()
            file_name = os.path.basename(current_process.exe())
        except:
            pass

        return {
            'status': True,
            'cpu_usage': cpu_usage,
            'filename': file_name,
            'run_time': get_time_str(time_span)
        }
    return {
        'status': False
    }


def get_time_str(time_span: float | int):
    '''获取时间文本'''
    if time_span < 60*60:
        return '{}m'.format(round(time_span/60, 1))

    if time_span < 60*60*24:
        return '{}h'.format(round(time_span/60/60, 1))

    return '{}d'.format(round(time_span/60/60/24, 1))
