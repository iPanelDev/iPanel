var wsclient = new WebSocket("ws://0.0.0.0");

$("#connect").click(function(){
    $("#mask").remove();
    $("#main").css("filter","none")
})




window.onload=function(){
    if(!WebSocket){
        alert("你的浏览器不支持WebSocket");
    }
    else{
        wsclient = new WebSocket("ws://127.0.0.1:30000");
        wsclient.onmessage=Receive;
    }
    
}

window.onunload=function(){
}

function Receive(e){
    console.log(e.data)
    var json = JSON.parse(e.data);
    if(json.type=="request"){
        if(json.sub_type=="verify"){
            wsclient.send(JSON.stringify(
                {
                    "type":"reply",
                    "sub_type":"verify",
                    "data":md5(json.data+"pwd"),
                    "from":"server"
                }
            ))
        }
    }
    if(json.type=="msg"){
        if(json.sub_type=="server_output"){
            AppendText(json.data)
        }
    }
}