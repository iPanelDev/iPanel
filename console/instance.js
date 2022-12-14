let instanceDict = {};
let inputLines = [];
let waitingToRestart = false;
let currentSelectedKey = "";

/**
 * @description 实例
 */
class Instance {
    constructor(dic) {
        this.name = dic.custom_name;
        this.server_status = false;
        this.server_file = "";
        this.server_cpuperc = "";
        this.server_time = "";
        this.os = "";
        this.cpu = "";
        this.ram_total = "";
        this.ram_used = "";
        this.ram_perc = "";
        this.cpu_perc = "";
        this.update_time = Date.now();
    }
}

/**
 * @description 发送命令
 */
function sendCommand() {
    if (!($("header select").find("option:selected").val() == null || $("header select").find("option:selected").val() == "")) {
        let cmd = $("#input-area>input").val();
        inputLines.push(cmd);
        let tmp = inputLines;
        setTimeout(function () {
            if (tmp == inputLines) {
                send("execute", "input", inputLines);
                inputLines.splice(0, inputLines.length);
            }
        }, 250);
    }
    $("#input-area>input").val("");
}

/**
 * @description 更新字典
 * @param {Array<Instance>} list 
 */
function updateInstanceDic(list) {
    list.forEach(element => {
        if (element.type == "instance")
            instanceDict[element.guid] = new Instance(element);
    });
    let selectedKey = $("header select").find("option:selected").val();
    currentSelectedKey = selectedKey;
    let selectedName = $("header select option:selected").text();
    let selectedIndex = 0;
    $("header select option").remove();
    $("header select").append('<option value="" disabled>选择一个实例</option>');
    let i = 0;
    for (var key in instanceDict) {
        if (Date.now() - instanceDict[key].update_time > 15) {
            if (selectedKey == key) {
                notice(2, "所选的实例\"" + selectedName + "\"已丢失");
                selectedKey = null;
            }
            delete instanceDict[key];
        } else {
            i++;
            if (selectedKey == key)
                selectedIndex = i;
            $("header select").append("<option value='" + key + "'>" + instanceDict[key].name + "(" + key.substring(0, 6) + ")" + "</option>");
        }
    }
    if (count(instanceDict) > 0 && selectedIndex == 0) {
        $("header select").get(0).selectedIndex = 1;
    } else {
        $("header select").get(0).selectedIndex = selectedIndex;
    }
    changeInstance();
}

/**
 * @description 启动服务器
 */
function startServer() {
    send("execute", "start");
}

/**
 * @description 重启服务器
 */
function restartServer() {
    send("execute", "stop");
    waitingToRestart = true;
}

/**
 * @description 关闭服务器
 */
function stopServer() {
    if (waitingToRestart && server_status) {
        appendText("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>重启已取消")
        waitingToRestart = false;
    }
    else
        send("execute", "stop");
}

/**
 * @description 强制结束服务器
 */
function killServer() {
    if (waitingToRestart && server_status) {
        appendText("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>重启已取消")
        waitingToRestart = false;
    }
    else if (confirm("确定结束进程吗？\n此操作可能导致存档损坏等问题")) {
        send("execute", "kill");
    }
}

/**
 * @description 更改实例
 */
function changeInstance() {
    if (currentSelectedKey != $("header select").find("option:selected").val())
        appendText("#clear");

    send("api", "select", $("header select").find("option:selected").val());
}

/**
 * @description 更新详细信息
 * @param {Instance} data 
 */
function updateInfo(data = null) {
    $(".section#info .table .row :last-child").text("-");
    if (!data)
        return;
    server_status = data.server_status;
    $(".section#info .table .row#server_status :last-child").text(data.server_status ? "已启动" : "未启动");
    if (server_status) {
        $(".section#info .table .row#server_file :last-child").text(data.server_file);
        $(".section#info .table .row#server_cpuperc :last-child").text(data.server_cpuperc ? data.server_cpuperc + "%" : "-");
        $(".section#info .table .row#server_time :last-child").text(data.server_time);
    } else {
        $(".section#info .table .row#server_status :last-child").text("未启动");
        $(".section#info .table .row#server_file :last-child").text("-");
        $(".section#info .table .row#server_cpuperc :last-child").text("-");
        $(".section#info .table .row#server_time :last-child").text("-");
    }
    $(".section#info .table .row#os :last-child").text(data.os);
    $(".section#info .table .row#cpu :last-child").text(data.cpu);
    $(".section#info .table .row#cpu_perc :last-child").text(data.cpu_perc ? data.cpu_perc + "%" : "-");
    $(".section#info .table .row#ram :last-child").text(
        data.ram_used && data.ram_total && data.ram_perc ?
            data.ram_used + "MB/" + data.ram_total + "MB(" + data.ram_perc + "%)" : "-");
    if (data != null) {

    } else {
        $(".section#info .table .row :last-child").text("-");
        $(".section#info .table .row#server_status :last-child").text("-");
        $(".section#info .table .row#server_file :last-child").text("-");
        $(".section#info .table .row#server_cpuperc :last-child").text("-");
        $(".section#info .table .row#server_time :last-child").text("-");
        $(".section#info .table .row#os :last-child").text("-");
        $(".section#info .table .row#cpu :last-child").text("-");
        $(".section#info .table .row#cpu_perc :last-child").text("-");
        $(".section#info .table .row#ram :last-child").text("-");
    }
    $(".section#console").height(($(".child-container").height() - 90) + "px");
}

function update(target, content) {
    $(target).text(content);
}