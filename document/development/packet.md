
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
  - `select` 选择实例对象 *(仅控制台)*
- `event` [事件](event.md)
- `execute` 执行
  - `input`
  - `start`
  - `stop`
  - `kill`
- `response`
  - `verify_request` 请求验证 *(仅iPanel)*
- `heartbeat` [心跳事件](heartbeat.md)

### sender