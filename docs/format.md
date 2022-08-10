# 数据包格式

- [数据包格式](#数据包格式)
  - [格式一览](#格式一览)
  - [验证](#验证)
    - [服务器请求](#服务器请求)
    - [客户端答复](#客户端答复)
    - [服务器答复](#服务器答复)
      - [验证通过](#验证通过)
      - [密码错误](#密码错误)
      - [客户端类型错误](#客户端类型错误)
      - [验证超时](#验证超时)
  - [心跳](#心跳)
    - [心跳数据包（请求）](#心跳数据包请求)
    - [心跳数据包（答复）](#心跳数据包答复)
    - [心跳数据包（面板信息）](#心跳数据包面板信息)
  - [输入命令](#输入命令)
    - [上报事件](#上报事件)
    - [执行](#执行)
  - [面板输出](#面板输出)
    - [上报事件](#上报事件-1)
    - [转发→面板](#转发面板)
    - [转发→控制台](#转发控制台)

## 格式一览

| 键            | 类型                | 可能的值                  | 备注       |
| ------------- | ------------------- | ------------------------- | ---------- |
| `type`        | String              | `verify` `notice`         | 类型       |
| `sub_type`    | String              | `request` `console_reply` | 子类型     |
| `data`        | String/Array/Object | `-`                       | 主要数据   |
| `from`        | String              | `host` *`guid`*           | 来源       |
| `target`      | String              | *`guid`*                  | 目标       |
| `custom_name` | String              | *`自定义名称`*            | 自定义名称 |
| `time`        | Number              | `1660074578`              | 时间戳     |

## 验证

1. 连接成功后服务器会发送[验证请求数据包](#服务器请求)，内包含一个随机生成的`guid`
   - 每次生成的`guid`都是唯一随机且不重复的
2. 客户端需要计算出`guid`+*`你的密码`* 的MD5值后构造[答复数据包](#客户端答复)发回服务器
   - 假如收到的`guid`为`8983e04ea8af45ae8b386a743d7cf798`，密码为`pwd`，那么计算所得的MD5就应该是`21403e9d4e3ea1c768d0d5dbc6718bb7`
   - 计算的MD5格式应该是32位小写
3. 服务器会根据发送的值判断是否正确并给出[答复](#服务器答复)
   - MD5不正确或答复子类型不正确均会自动断开连接，你可以重新连接再次发送

### 服务器请求

```jsonc
{
    "type": "verify",
    "sub_type": "request",
    "data": "72620b932d72431eace7da1309b0242c",
    "from": "host",
    "time": 1660074578
}
```

### 客户端答复

```jsonc
{
    "type": "verify",
    "sub_type": "console_reply", // 网页控制台：console_reply  面板：panel_reply
    "data": "b77ca0be609ca15ed6c3c66d5079c272", 
    "custom_name": "Serein v1.3.1", // 会显示在网页版控制台上
}
```

### 服务器答复

#### 验证通过

```jsonc
{
    "type": "notice",
    "sub_type": "verify_success",
    "data": "验证成功:控制台",
    "from": "host",
    "time": 1660073797
}
```

#### 密码错误

```jsonc
{
    "type": "notice",
    "sub_type": "verify_failed",
    "data": "验证失败:错误的MD5值",
    "from": "host",
    "time": 1659869755
}
```

#### 客户端类型错误

```jsoncc
{
    "type": "notice",
    "sub_type": "verify_failed",
    "data": "验证失败:无效的客户端类型",
    "from": "host",
    "time": 1659869928
}
```

#### 验证超时

```jsonc
{
    "type": "notice",
    "sub_type": "verify_timeout",
    "data": "验证超时",
    "from": "host",
    "time": 1659869839
}
```

## 心跳

服务器每5s会发出[心跳数据包](#心跳数据包请求)，面板类型的客户端需要进行[答复](#心跳数据包答复)  
此外，服务器还会给控制台发送[有关所有面板的信息数据包](#心跳数据包面板信息)

### 心跳数据包（请求）

```jsonc
{
    "type": "heartbeat",
    "sub_type": "request",
    "from": "host",
    "time": 1660075907
}
```

### 心跳数据包（答复）

```jsonc
{
    "type": "heartbeat",
    "sub_type": "response",
    "data": {
        "server_status": false, // 服务端状态
        "server_file": "", // 服务端启动文件名称
        "server_cpuperc": "", // 服务端进程CPU占用
        "server_time": "", // 服务端运行时长
        "os": "", // 系统名称
        "cpu": "", // CPU名称
        "ram_total": "", // 总内存
        "ram_used": "", // 已使用内存
        "ram_perc": "", // 内存使用率
        "cpu_perc": ""  // CPU使用率
    }
}
```

### 心跳数据包（面板信息）

```jsonc
{
    "type": "heartbeat",
    "sub_type": "info",
    "data": [ // 所有面板客户端的答复列表
        {
            "guid": "c91ea7ab01894f1eb1b48b9c393b240c",
            "name": "Serein v1.3.1",
            "server_status": false,
            "server_file": "",
            "server_cpuperc": "0.00",
            "server_time": "-",
            "os": "Microsoft Windows 10",
            "cpu": "我不是CPU",
            "ram_total": "7778",
            "ram_used": "5522",
            "ram_perc": "71.0",
            "cpu_perc": "5.8"
        }
    ],
    "from": "host",
    "time": 1660076502
}
```

## 输入命令

### 上报事件

当面板客户端执行输入操作时上报

```jsonc
{
    "type": "input",
    "sub_type": "panel_input", // 网页控制台：console_input  面板：panel_input
    "data": "?",
    "from": "c91ea7ab01894f1eb1b48b9c393b240c", // 输入客户端的guid
    "time": 1660076849
}
```

### 执行

>网页版控制台执行输入操作时会触发上报事件

```jsonc
{
    "type": "input",
    "sub_type": "execute", 
    "data": "?",
    "target": "c91ea7ab01894f1eb1b48b9c393b240c", // 目标面板客户端的guid，为all则表示输出给所有面板客户端
    "time": 1660077188
}
```

## 面板输出

当面板客户端有新的输出时上报

### 上报事件

```jsonc
{
    "type": "output",
    "sub_type": "output",
    "data": "。。。",
}
```

### 转发→面板

```jsonc
{
    "type": "output",
    "sub_type": "origin",
    "data": "。。。",
    "from": "c91ea7ab01894f1eb1b48b9c393b240c",
    "time": 1660078810
}
```

### 转发→控制台

自动进行彩色文本处理和字符转义

>输出格式与`Serin`的混合输出模式一致

```jsonc
{
    "type": "output",
    "sub_type": "colored",
    "data": "<span class=\"noColored\">。。。</span>",
    "from": "c91ea7ab01894f1eb1b48b9c393b240c",
    "time": 1660078607
}
```
