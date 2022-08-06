var config = {
    addr:"ws://127.0.0.1:30000",
    pwd:"",
    chatRegex:[
        "\[Chat\]\s<(.+?)>\s(.+?)$"
    ]
};

var ws = new WebSocket(config.addr,"");

ws.onopen = function(){
    serein.log("成功连接至"+config.addr)
};

ws.onclose = function(){
    serein.log("连接已断开")
};

ws.onmessage = function(msg){
    serein.log(msg);
    if(JSON.parse(msg).type=="request"){
        ws.send(JSON.stringify({
            "type":"reply",
            "sub_type":"verify",
            "data":getMD5(JSON.parse(msg).data+"pwd"),
            "from":"server"
        }))
    }
};

serein.setListener("onServerOutput",function(line){
    ws.send(JSON.stringify({
        "type":"msg",
        "sub_type":"output",
        "data":line,
        "from":"server"
    }))
});

serein.setListener("onPluginsReload",function(){
    ws.close()
});
serein.setListener("onSereinClose",function(){
    ws.close()
});
