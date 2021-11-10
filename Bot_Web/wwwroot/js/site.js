// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
"use strict";
var connection = new signalR.HubConnectionBuilder().withUrl("/taskHub").build();

connection.on("TaskUpdate", function (message) {
    var li = document.createElement("li");
    li.textContent = message;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("streamButton").addEventListener("click", function (event) {
    $.ajax({
        url: '/Home/StartStream',
        type: 'POST'
    }).done(function (result) { });
    event.preventDefault();
});

document.getElementById("stopStreamButton").addEventListener("click", function (event) {
    $.ajax({
        url: '/Home/StopStream',
        type: 'POST'
    }).done(function (result) { });
    event.preventDefault();
});

document.getElementById("buyButton").addEventListener("click", function (event) {
    $.ajax({
        url: '/Home/BuyOrder',
        type: 'POST'
    }).done(function (result) { });
    event.preventDefault();
});

document.getElementById("sellButton").addEventListener("click", function (event) {
    $.ajax({
        url: '/Home/SellOrder',
        type: 'POST'
    }).done(function (result) { });
    event.preventDefault();
});

