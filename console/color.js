/**
 * @description 颜色数码
 */
const color_nums =
    [
        "30",
        "31",
        "32",
        "33",
        "34",
        "35",
        "36",
        "37",
        "40",
        "41",
        "42",
        "43",
        "44",
        "45",
        "46",
        "47",
        "90",
        "91",
        "92",
        "93",
        "94",
        "95",
        "96",
        "97",
        "100",
        "101",
        "102",
        "103",
        "104",
        "105",
        "106",
        "107"
    ];

/**
 * @description 正则表达式
 */
const patten = /\[(.+?)m(.*)/;

/**
 * @description 颜色代码转义
 * @param {string} line 
 * @returns html文本
 */
function colorEscape(line = "") {
    if (!line || line.search("\x1b") < 0) {
        return line;
    }
    let output = "";
    let group = line.trimStart("\x1b").split("\x1b");
    for (var i = 0; i < group.length; i++) {
        let match = patten.exec(group[i]);
        if (match == null) {
            continue;
        }
        let arg_group = match[1].split(";");
        let style = "";
        let classes = "";
        for (var arg_index = 0; arg_index < arg_group.length; arg_index++) {
            let child_arg = arg_group[arg_index];
            if (child_arg == "1") {
                style += "font-weight:bold;";
            }
            else if (child_arg == "3") {
                style += "font-style: italic;";
            }
            else if (child_arg == "4") {
                style += "text-decoration: underline;";
            }
            else if (child_arg == "38" && arg_group[arg_index + 1] == "2" && arg_index + 4 <= arg_group.length) {
                style += "color:rgb(" + arg_group[arg_index + 2] + "," + arg_group[arg_index + 3] + "," + arg_group[arg_index + 4] + ");";
                colored = true;
            }
            else if (child_arg == "48" && arg_group[arg_index + 1] == "2" && arg_index + 4 <= arg_group.length) {
                style += "background-color:rgb(" + arg_group[arg_index + 2] + "," + arg_group[arg_index + 3] + "," + arg_group[arg_index + 4] + ");";
                colored = true;
            }
            else if (color_nums.indexOf(child_arg) >= 0) {
                classes += "vanillaColor" + child_arg + " ";
            }
        }
        output += "<span style=\"" + style + "\" class=\"" + classes + "\">" + match[2] + "</span>";
    }
    return output;
}