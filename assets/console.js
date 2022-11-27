/**
 * @description 行数
 */
let line = 0;

/**
 * @description 追加文本到控制台
 * @param {string} str 文本
 */
function appendText(str) {
    let ConsoleDiv = document.querySelector("#console-child");
    line = line + 1;
    if (str == "#clear") {
        document.querySelector("#console-child").innerHTML = "";
        line = 0;
    } else {
        let div = document.createElement("div");
        div.innerHTML = str;
        ConsoleDiv.appendChild(div);
    }
    if (line > 2500) {
        ConsoleDiv.removeChild(ConsoleDiv.children[0]);
        line = line - 1;
    }
    ConsoleDiv.scrollTop = ConsoleDiv.scrollHeight;
}