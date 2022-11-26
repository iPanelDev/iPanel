
## 请求验证

连接成功后由`iPanel`发送请求数据包

```json
// iPanel -> Client
{
    "type": "response",
    "sub_type": "verify_request",
    "data": "58b3ceb62b404b61983a373cbf000df5",
    "sender": {
        "guid": "",
        "type": "host"
        },
    "time": 1669416642
}
```

其中`data`为随机生成的32位16进制数

## 客户端回复

客户端需要在5s内发送验证数据包

```json
// Client -> iPanel
{
    "type": "api",
    "sub_type": "instance_verify",
    "data": "1b659b5b178277218a82ea16f11db6e4",
    "custom_name": "Serein v1.3.2"
}
```

### 客户端类型

>[!NOTE]该项将影响客户端的权限

- `instance_verify`
  - 实例，如Serein或服务器
    - 需要定期向`iPanel`发送信息数据
    - 可以添加`custom_name`字段，用于区分实例名称，建议改为 实例类型+版本号
- `console_verify`
  - 控制台
    - 可以控制实例的输入和输出

### data项计算过程

1. 将请求验证过程中获得的`guid`和密码拼接
2. 计算32位MD5值
3. 构造数据包发送至`iPanel`

>[!TIP]  
>
>举个例子（密码为`123`，`guid`[如上](#客户端回复)）：
>
>1. 拼接字符串，得到`58b3ceb62b404b61983a373cbf000df5123`
>2. 计算`58b3ceb62b404b61983a373cbf000df5123`的32位MD5值，得到`1b659b5b178277218a82ea16f11db6e4`
>3. 发送至`iPanel`
>
>**Javascript实现方法**
>
>```js
>ws.send(JSON.stringify({
>   "type": "api",
>   "sub_type": "instance_verify",
>   "data": getMD5(data + config.pwd),
>   "custom_name": config.name
>}));
>```

## 验证回复

### 成功

```json
// iPanel -> Client
{
    "type": "response",
    "sub_type": "verify_success",
    "data": "验证成功",
    "sender": {
        "guid": "",
        "type": "host"
    },
    "time": 1669417556
}
```

### 错误

```json
// iPanel -> Client
{
    "type": "response",
    "sub_type": "verify_failed",
    "data": "验证失败:错误的MD5值",
    "sender": {
        "guid": "",
        "type": "host"
    },
    "time": 1669417622
}
```

### 超时

```json
// iPanel -> Client
{
    "type": "response",
    "sub_type": "verify_timeout",
    "data": "验证超时",
    "sender": {
        "guid": "",
        "type": "host"
    },
    "time": 1669417715
}
```
