<p align="center">
  <img src="src/Sources/logo.png" style="width:30%;min-width:150px; margin:-40px">
  <br>
  <b>iPanel</b>
  <br>
  <br>
  适用于<a href="https://serein.cc/">Serein</a>等软件的网页版控制台插件
  <br>
  <br>
  <a href="https://wakatime.com/badge/github/Zaitonn/iPanel">
    <img src="https://wakatime.com/badge/github/Zaitonn/iPanel.svg" alt="wakatime">
  </a>
  <a href="https://github.com/Zaitonn/iPanel/actions/workflows/build.yml">
    <img alt="GitHub bulid" src="https://img.shields.io/github/actions/workflow/status/Zaitonn/iPanel/build.yml?branch=main&color=blue">
  </a>
  <a href="https://github.com/Zaitonn/iPanel">
    <img alt="GitHub repo file count" src="https://img.shields.io/github/languages/code-size/Zaitonn/iPanel">
  </a>
</p>

## 演示

![网页版控制台](https://ipanel.serein.cc/assets/imgs/webconsole.gif)

## 支持的软件

- [x] [Serein](https://serein.cc/)
- [ ] [MCDReforged](https://github.com/Fallen-Breath/MCDReforged)
- [ ] ...

## 快速上手&使用方法

<https://ipanel.serein.cc/>

## 项目原理

```mermaid
flowchart TB
    subgraph iPanel Host
    ws(["WebSocket服务器"])
    web(["网页服务器"])
    end
    
    subgraph 实例插件
    i1["Serein"]
    i2["MCDReforged"]
    i3["..."]
    end
    
    i1 ---> ws
    i2 ---> ws
    i3 ---> ws

    subgraph 网页控制台
    u1["用户1"]
    u2["用户2"]
    u3["..."]
    website("网页")
    end
    
    web ---> website
    website --- u1
    website --- u2
    website --- u3

    ws --->|"提供实例数据/控制实例"| website
```

## Stars记录

![starchart](https://starchart.cc/Zaitonn/iPanel.svg)
