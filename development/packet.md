
## 示例

如下是一个可能的数据包示例

```json
{
    "type": "heartbeat",
    "sub_type": "info",
    "sender": {
        "guid": "",
        "type": "host"
    },
    "time": 1669401686
}
```

## 结构

| 字段       | 名称       | 类型          |
| -------- | ---------- | ------------- |
| type     | 类型       | string        |
| sub_type | 子类型     | string        |
| data     | 数据主体   | string / 列表 |
| sender   | 发送者信息 | object        |
| time     | 发送时间戳 | long          |

### type & sub_type

- `api` 应用接口，详见[API](api.md)
  - `instance_verify` 客户端验证 *(仅实例)*
  - `console_verify` 客户端验证 *(仅控制台)*
  - `list` 获取当前在线的所有客户端
  - `select` 选择实例对象
- `event` [事件](event.md)
- `execute` 执行
  - `input` 输入
  - `start` 启动服务器
  - `stop` 关闭服务器
  - `kill` 强制结束服务器
  - `restart` 重启服务器
- `response`
  - `verify_request` 请求验证 *(仅iPanel)*
  - `invalid` 请求出错
- `heartbeat` [心跳事件](heartbeat.md)

### sender

- `guid` 客户端的唯一识别码
  - 32位16进制数字
- `type` 类型
  - `console` 网页控制台
  - `instance` 实例
  - `iPanel` *顾名思义*

### time

时间戳

```csharp
Time = (long)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
// 一个整数，表示从1970.1.1 00:00 到此时刻的总秒数
```
