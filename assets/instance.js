var instance_dic = {};
var input_lines = [];
var waitingToRestart = false;

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
        this.added = false;
    }
}

function send_command() {
    if (!($("header select").find("option:selected").val() == null || $("header select").find("option:selected").val() == "")) {
        var cmd = $("#input-area>input").val();
        input_lines.push(cmd);
        var tmp = input_lines;
        setTimeout(function () {
            if (tmp == input_lines) {
                ws_send("execute", "input", input_lines);
                input_lines.splice(0, input_lines.length);
            }
        }, 250);
    }
    $("#input-area>input").val("");
}

function update_instance_dic(list) {
    // 更新字典
    list.forEach(element => {
        if (instance_dic.hasOwnProperty(element.guid)) {
            var added = instance_dic[element.guid].added;
            instance_dic[element.guid] = new Instance(element);
            instance_dic[element.guid].added = added;
        } else {
            instance_dic[element.guid] = new Instance(element)
        }
    });

    for (var key in instance_dic) {
        if (Date.now() - instance_dic[key].update_time < 10) {
            if (!instance_dic[key].added) {
                $("header select").append("<option value='" + key + "'>" + instance_dic[key].name + "(" + key.substring(0, 6) + ")" + "</option>");
                instance_dic[key].added = true;
            }
        }
        else if (instance_dic[key].added) {
            if ($("header select").find("option:selected").val() == key) {
                notice(2, "所选的实例\"" + $("header select option:selected").text() + "\"已丢失");
                $("header select option:selected").remove();
                $("header select").get(0).selectedIndex = 0;
            } else {
                $("header select option[value=" + key + "]").remove();
                $("header select").get(0).selectedIndex = 0;
            }
            delete instance_dic[key];
        }
    }
    if (dic_count(instance_dic) > 0) {
        $("header select").get(0).selectedIndex = 1;
        change_panel();
    }
}

function start_server() {
    ws_send("execute", "start");
}

function restart_server() {
    ws_send("execute", "stop");
    waitingToRestart = true;
}

function stop_server() {
    if (waitingToRestart&&server_status) {
        append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>重启已取消")
        waitingToRestart = false;
    }
    else
        ws_send("execute", "stop");
}

function kill_server() {
    if (waitingToRestart&&server_status) {
        append_text("<span style=\"color:#4B738D;font-weight: bold;\">[Serein]</span>重启已取消")
        waitingToRestart = false;
    }
    else if (confirm("确定结束进程吗？\n此操作可能导致存档损坏等问题")) {
        ws_send("execute", "kill");
    }
}

function change_panel() {
    ws_send("api", "select", $("header select").find("option:selected").val());
}

function update_info(data = null) {
    if (data) {
        server_status = data.server_status;
        $(".section#info .table .row :last-child").text("-");
        $(".section#info .table .row#server_status :last-child").text(data.server_status ? "已启动" : "未启动");
        if (server_status) {
            $(".section#info .table .row#server_file :last-child").text(data.server_file);
            $(".section#info .table .row#server_cpuperc :last-child").text(data.server_cpuperc + "%");
            $(".section#info .table .row#server_time :last-child").text(data.server_time);
        }
        $(".section#info .table .row#os :last-child").text(data.os);
        $(".section#info .table .row#cpu :last-child").text(data.cpu);
        $(".section#info .table .row#cpu_perc :last-child").text(data.cpu_perc + "%");
        $(".section#info .table .row#ram :last-child").text(data.ram_used + "MB/" + data.ram_total + "MB(" + data.ram_perc + "%)");
    } else if (!server_status) {
        $(".section#info .table .row#server_status :last-child").text("未启动");
        $(".section#info .table .row#server_file :last-child").text("-");
        $(".section#info .table .row#server_cpuperc :last-child").text("-");
        $(".section#info .table .row#server_time :last-child").text("-");
    }
    $(".section#console").height(($(".child-container").height() - 90) + "px");
}