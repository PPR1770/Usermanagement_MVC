let selectedUserId = "";
let currentUserId = "";

fetch('/Account/GetCurrentUserId')
    .then(res => res.text())
    .then(id => currentUserId = id);

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.start().then(() => {

    console.log("SignalR Connected");

    // 👇 CALL HERE (IMPORTANT)
    loadInitialUserStatus();

});

function loadInitialUserStatus() {

    fetch('/Chat/GetUsers')
        .then(res => res.json())
        .then(users => {

            users.forEach(u => {
                updateUserStatus(u.id, u.isOnline);
            });

        });
}
function loadUsers() {
    fetch('/Chat/GetUsers')
        .then(res => res.json())
        .then(users => {

            let html = "";

            users.forEach(u => {

                html += `
                        <li onclick="selectUser('${u.id}', '${u.userName}', '${u.profileImage}')"
                            id="user-${u.id}">

                            <img src="${u.profileImage}" />

                            <span>${u.userName}</span>

                            <span class="status-dot offline" id="status-${u.id}"></span>

                        </li>`;
            });

            document.getElementById("userList").innerHTML = html;
        });
}

loadUsers();


// 🔹 Select User
function selectUser(userId, userName, profileImage) {

    selectedUserId = userId;

    document.getElementById("chatUserName").innerText = userName;

    document.getElementById("selectedUserImg").src = profileImage;

    //document.getElementById("onlineStatusText").innerText = "online" ; // placeholder for now

    loadMessages(userId);
}

function renderMessage(msg) {

    let cls = msg.senderId === currentUserId ? "sent" : "received";

    let content = "";

    // TEXT
    if (msg.message) {
        content += `<div>${msg.message}</div>`;
    }

    // IMAGE
    if (msg.fileType && msg.fileType.includes("image")) {
        content += `<img src="${msg.fileUrl}" style="max-width:200px;border-radius:10px;" />`;
    }

    // PDF / FILE
    if (msg.fileType && !msg.fileType.includes("image")) {
        content += `
            <a href="${msg.fileUrl}" target="_blank">
                📎 ${msg.fileName}
            </a>
        `;
    }

    let html = `
        <div class="message ${cls}">
            ${content}
        </div>
    `;

    document.getElementById("chatBox").innerHTML += html;

    scrollToBottom();
}

// 🔹 Load Messages
function loadMessages(userId) {
    fetch(`/Chat/GetMessages?userId=${userId}`)
        .then(res => res.json())
        .then(messages => {

            document.getElementById("chatBox").innerHTML = "";

            messages.forEach(m => {
                renderMessage(m);

                if (m.receiverId === currentUserId && !m.isRead) {
                    connection.invoke("MarkAsRead", m.id);
                }
            });
        });
}
// 🔹 Send Message
async function sendMessage() {

    let msg = document.getElementById("messageInput").value;

    let fileData = await uploadFile();

    connection.invoke(
        "SendMessage",
        selectedUserId,
        msg,
        fileData?.fileUrl || null,
        fileData?.fileType || null,
        fileData?.fileName || null
    );

    document.getElementById("messageInput").value = "";
    document.getElementById("fileInput").value = "";
}

// 🔹 Receive Message
connection.on("ReceiveMessage", msg => {
    renderMessage(msg);
});

function scrollToBottom() {
    let chatBox = document.getElementById("chatBox");
    chatBox.scrollTop = chatBox.scrollHeight;
}

document.getElementById("messageInput").addEventListener("input", function () {
    if (selectedUserId) {
        connection.invoke("Typing", selectedUserId);
    }
});

connection.on("UserTyping", function (userId) {
    document.getElementById("typingStatus").innerText = "Typing...";

    setTimeout(() => {
        document.getElementById("typingStatus").innerText = "";
    }, 1000);
});

connection.on("MessageSeen", function (messageId) {
    console.log("Message seen:", messageId);
});


async function uploadFile() {

    let fileInput = document.getElementById("fileInput");
    let file = fileInput.files[0];

    if (!file) return null;

    let formData = new FormData();
    formData.append("file", file);

    let res = await fetch('/Chat/UploadFile', {
        method: 'POST',
        body: formData
    });

    return await res.json();
}

connection.on("UserStatusChanged", function (userId, isOnline) {

    // update user list dot
    updateUserStatus(userId, isOnline);

    // update selected user header
    if (userId === selectedUserId) {
        setSelectedUserStatus(isOnline);
    }

    // update current user header
    if (userId === currentUserId) {
        setCurrentUserStatus(isOnline);
    }
});

function updateUserStatus(userId, isOnline) {
    let dot = document.getElementById(`status-${userId}`);

    if (!dot) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");
}

function setSelectedUserStatus(isOnline) {

    let dot = document.getElementById("selectedStatusDot");
    let text = document.getElementById("onlineStatusText");

    if (!dot || !text) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");

    text.innerText = isOnline ? "Online" : "Offline";
}

function setCurrentUserStatus(isOnline) {

    let dot = document.getElementById("currentStatusDot");
    let text = document.getElementById("currentStatusText");

    if (!dot || !text) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");

    text.innerText = isOnline ? "Online" : "Offline";
}

//if (!msg.trim()) return;