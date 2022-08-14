var panel_dic = {};
var input_lines = [];

class Panel {
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
                ws_send("api", "input", input_lines);
                input_lines.splice(0, input_lines.length);
            }
        }, 250);
    }
    $("#input-area>input").val("");
}

function update_panel_dic(list) {
    // 更新字典
    list.forEach(element => {
        if (panel_dic.hasOwnProperty(element.guid)) {
            var added = panel_dic[element.guid].added;
            panel_dic[element.guid] = new Panel(element);
            panel_dic[element.guid].added = added;
        } else {
            panel_dic[element.guid] = new Panel(element)
        }
    });

    for (var key in panel_dic) {
        if (Date.now() - panel_dic[key].update_time < 10) {
            if (!panel_dic[key].added) {
                $("header select").append("<option value='" + key + "'>" + panel_dic[key].name + "(" + key.substring(0, 6) + ")" + "</option>");
                panel_dic[key].added = true;
            }
        }
        else if (panel_dic[key].added) {
            if ($("header select").find("option:selected").val() == key) {
                alert(2, "所选的实例\"" + $("header select option:selected").text() + "\"已丢失");
                $("header select option:selected").remove();
                $("header select").get(0).selectedIndex = 0;
            } else {
                $("header select option[value=" + key + "]").remove();
                $("header select").get(0).selectedIndex = 0;
            }
            delete panel_dic[key];
        }
    }
    if (dic_count(panel_dic) > 0) {
        $("header select").get(0).selectedIndex = 1;
        change_panel();
    }
}

function start_server() {
    ws_send("api", "start");
}

function stop_server() {
    ws_send("api", "stop");
}

function kill_server() {
    if (confirm("确定结束进程吗？\n此操作可能导致存档损坏等问题")) {
        ws_send("api", "kill");
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