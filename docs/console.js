var line = 0;
function AppendText(str) {
    var ConsoleDiv = document.querySelector("#console-child");
    line = line + 1;
    if (str == "#clear") {
        Clear();
    } else {
        var div = document.createElement("div");
        div.innerHTML = str;
        ConsoleDiv.appendChild(div);
    }
    if (line > 250) {
        ConsoleDiv.removeChild(ConsoleDiv.children[0]);
        line = line - 1;
    }
    ConsoleDiv.scrollTop = ConsoleDiv.scrollHeight;
}
function Clear() {
    document.querySelector("#console-child").innerHTML = "";
    line = 0;
}
