# 数据包格式

- [数据包格式](#数据包格式)
  - [验证](#验证)
    - [服务器请求](#服务器请求)
    - [客户端答复](#客户端答复)
    - [服务器反馈](#服务器反馈)
      - [验证通过](#验证通过)
      - [密码错误](#密码错误)
      - [客户端类型错误](#客户端类型错误)
      - [验证超时](#验证超时)

```jsonc
{
    "type":"",
    "sub_type":"",
    "data":"",
    "from":"",
    "target":"",
    "time":0
}
```




## 验证

### 服务器请求

```json
{"type":"verify","sub_type":"request","data":"789716628507424f81c1b0b5ba9c06db","from":"host","target":"127.0.0.1:52846","time":1659869755}
```

### 客户端答复
```json
{"type": "verify","sub_type": "console_reply","data": "b77ca0be609ca15ed6c3c66d5079c272","from":"自定义名称","time":1659869755}
```

### 服务器反馈

#### 验证通过
```json
{"type":"notice","sub_type":"verify_success","data":"验证成功:控制台","from":"host","target":"127.0.0.1:52846","time":1659869755}
```

#### 密码错误
```json
{"type":"notice","sub_type":"verify_failed","data":"验证失败:错误的MD5值","from":"host","target":"127.0.0.1:52846","time":1659869755}
```

#### 客户端类型错误
```json
{"type":"notice","sub_type":"verify_failed","data":"验证失败:无效的客户端类型","from":"host","target":"127.0.0.1:53138","time":1659869928}
```

#### 验证超时
```json
{"type":"notice","sub_type":"verify_timeout","data":"验证超时","from":"host","target":"127.0.0.1:52980","time":1659869839}
```