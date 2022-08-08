class Server {
    constructor(dic) {
        this.guid = dic.guid;
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
    }
}

function send_command() {
    var cmd = $("#input-area>input").val();
    last_input = cmd;
    append_text(">" + html2Escape(cmd));
    send("input","console",cmd,"all")
    $("#input-area>input").val("");
}
