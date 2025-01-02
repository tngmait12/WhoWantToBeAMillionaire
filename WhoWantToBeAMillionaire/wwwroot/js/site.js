let connection = new signalR.HubConnectionBuilder()
    .withUrl("/onlinecount")
    .build();

connection.on("updateCount", function (count) {
    document.getElementById("onlineUsers").innerText = count;
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
