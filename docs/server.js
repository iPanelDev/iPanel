var server_list = [];

class Server {
    constructor(dic) {
        this.guid = dic.guid;
        this.name = dic.name;
        this.server_status = dic.server_status;
        this.server_file = dic.server_file;
        this.server_cpuperc = dic.server_cpuperc;
        this.server_time = dic.server_time;
        this.os = dic.os;
        this.cpu = dic.cpu;
        this.ram_total = dic.ram_total;
        this.ram_used = dic.ram_used;
        this.ram_perc = dic.ram_perc;
        this.cpu_perc = dic.cpu_perc;
        this.update_time = Date.now();
        this.added = false;
    }
}

function send_command() {
    if (!($("header select").find("option:selected").val() == null || $("header select").find("option:selected").val() == "")) {
        var cmd = $("#input-area>input").val();
        last_input = cmd;
        append_text(">" + html2Escape(cmd));
        ws_send("input", "execute", cmd, $("header select").find("option:selected").val());
    }
    $("#input-area>input").val("");
}

function update_server(list) {
    // 更新数组
    for (var j = 0; j < list.length; j++) {
        var added = false;
        for (var i = 0; i < server_list.length; i++) {
            if (server_list[i].guid == list[j].guid) {
                var item_added = server_list[i].added; // 判断是否已经被添加至选择框
                server_list[i] = new Server(list[j]);
                server_list[i].added = item_added;
                added = true;
                break;
            }
        }
        if (!added) {
            server_list.push(new Server(list[j]));
        }
    }

    // 删除失效项并更新列表
    for (var i = 0; i < server_list.length; i++) {
        if (Date.now() - server_list[i].update_time < 10) {
            if (!server_list[i].added) {
                $("header select").append("<option value='" + server_list[i].guid + "'>" + server_list[i].name + "(" + server_list[i].guid.substring(0, 6) + ")" + "</option>");
                server_list[i].added = true;
            }
        }
        else if (server_list[i].added) {
            if ($("header select").find("option:selected").val() == server_list[i].guid) {
                alert(2, "所选的实例\"" + $("header select option:selected").text() + "\"已丢失");
                $("header select option:selected").remove();
                $("header select").get(0).selectedIndex = 0;
            } else {
                $("header select option[value=" + server_list[i].guid + "]").remove();
                $("header select").get(0).selectedIndex = 0;
            }

            server_list = server_list.splice(i, i);
        }
    }
    if (server_list.length > 0) {
        $("header select").get(0).selectedIndex = 1;
    }
}