let selectedUserId = "";
let selectedGroupId = "";
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

                u.profileImage = u.profileImage || "/images/default-user.png";
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
    selectedGroupId = ""; // IMPORTANT
    document
        .getElementById(
            "groupActions"
        ).style.display = "none";

    document
        .getElementById(
            "groupMemberContainer"
        ).style.display = "none";

    selectedUserId = userId;

    document.getElementById("chatUserName").innerText = userName;

    document.getElementById("selectedUserImg").src = profileImage ?? "/images/default-user.png";

    //document.getElementById("onlineStatusText").innerText = "online" ; // placeholder for now

    loadMessages(userId);
}

function renderMessage(msg) {

    let cls = msg.senderId === currentUserId ? "sent" : "received";

    let content = "";
    let seenIcon = "";
    if (msg.senderId === currentUserId) {
        seenIcon = msg.isRead
            ? "✔✔"
            : msg.isDelivered ? "✔" : "";
    }

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
            <div class="message-time">
                ${seenIcon}
            </div>
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
    if (!msg.trim() && fileData.length <= 0) return;

    // Phase 5 GROUP MODE
    if (selectedGroupId) {

        connection.invoke(
            'SendGroupMessage',
            selectedGroupId,
            msg,
            fileData?.fileUrl || null,
            fileData?.fileType || null,
            fileData?.fileName || null);


        document
            .getElementById(
                "messageInput"
            ).value = "";

        return;
    }

    // let fileData = await uploadFile();

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
    //removeSelectedFile();
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

connection.on("UserStatusChanged", function (userId, isOnline, lastSeenAt) {

    // update user list dot
    updateUserStatus(userId, isOnline);

    // update selected user header
    if (userId === selectedUserId) {
        setSelectedUserStatus(isOnline, lastSeenAt);
    }

    // update current user header
    if (userId === currentUserId) {
        setCurrentUserStatus(isOnline, lastSeen);
    }
});

function updateUserStatus(userId, isOnline) {
    let dot = document.getElementById(`status-${userId}`);

    if (!dot) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");
}

function setSelectedUserStatus(isOnline, lastSeen) {

    let dot = document.getElementById("selectedStatusDot");
    let text = document.getElementById("onlineStatusText");

    if (!dot || !text) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");

    //text.innerText = isOnline ? "Online" : "Offline";
    if (isOnline) {

        text.innerText = "Online";

    } else {

        text.innerText =
            "Last seen "
            + formatLastSeen(lastSeen);
    }
}
function formatLastSeen(date) {

    if (!date)
        return "";

    let d =
        new Date(date);

    return d.toLocaleString();
}
function setCurrentUserStatus(isOnline) {

    let dot = document.getElementById("currentStatusDot");
    let text = document.getElementById("currentStatusText");

    if (!dot || !text) return;

    dot.classList.remove("online", "offline");
    dot.classList.add(isOnline ? "online" : "offline");

    text.innerText = isOnline ? "Online" : "Offline";
}
const fileInput = document.getElementById("fileInput");
const filePreview = document.getElementById("filePreview");
const fileName = document.getElementById("fileName");

// Show selected file name
fileInput.addEventListener("change", function () {

    if (this.files.length > 0) {

        const file = this.files[0];

        fileName.innerText = file.name;

        filePreview.style.display = "block";
    }
});

// Remove selected file
function removeSelectedFile() {

    fileInput.value = "";

    fileName.innerText = "";

    filePreview.style.display = "none";
}

//if (!msg.trim()) return;


//Group Chat Section Started


connection.on(
    "ReceiveGroupMessage",
    function (msg) {

        console.log(
            "GROUP MSG:",
            msg
        );

        renderMessage(msg);

    });

function loadGroups() {

    fetch('/Chat/GetGroups')
        .then(r => r.json())
        .then(groups => {

            let html = "";

            groups.forEach(g => {

                html += `
                    <li onclick="selectGroup(
                    '${g.id}',
                    '${g.name}')">
                    👥 ${g.name}
                    </li>`;

            });

            document
                .getElementById('groupList')
                .innerHTML = html;

        });

}
loadGroups();

function selectGroup(id, name) {

    selectedUserId = "";
    selectedGroupId = id;

    document
        .getElementById(
            "chatUserName"
        ).innerText = name;

    // SHOW GROUP BUTTONS
    document
        .getElementById(
            "groupActions"
        ).style.display = "block";

    connection.invoke(
        "JoinGroup",
        id
    )
        .catch(err => {

            console.error(
                "JoinGroup error:",
                err
            );

        });

    loadGroupMessages(id);
    // NEW
    loadGroupDetails(id);
}
function loadGroupMessages(groupId) {

    fetch(
        `/Chat/GetGroupMessages?groupId=${groupId}`)
        .then(r => r.json())
        .then(messages => {

            let chatBox =
                document.getElementById('chatBox');

            chatBox.innerHTML = '';

            messages.forEach(m => {
                renderMessage(m);
            });

        });

}
async function createGroup() {

    let groupName = prompt("Enter Group Name");

    if (!groupName)
        return;

    let memberIds = [];

    let formData = new FormData();

    formData.append("groupName", groupName);

    // append each member
    memberIds.forEach(id => {
        formData.append("memberIds", id);
    });

    let response = await fetch('/Chat/CreateGroup', {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        alert("Group creation failed");
        return;
    }

    alert("Group created");

    loadGroups();
}
connection.on('GroupTyping', function (userId) {

    const statusEl = document.getElementById('onlineStatusText');

    if (!statusEl)
        return;

    statusEl.innerText = 'Someone typing...';

    setTimeout(() => {
        statusEl.innerText = 'Online';
    }, 2000);

});
let selectedMembers = [];

function openAddMembersModal() {

    if (!selectedGroupId)
        return;

    fetch('/Chat/GetUsers')
        .then(r => r.json())
        .then(users => {

            let html = '';

            users.forEach(u => {

                html += `
 <div>

 <input
 type='checkbox'
 value='${u.id}'
 onchange='toggleMember(this)'/>

 ${u.userName}

 </div>
 `;

            });

            document
                .getElementById(
                    'memberPicker'
                ).innerHTML = html;

            document
                .getElementById(
                    'addMemberModal'
                ).style.display = 'block';

        });

}
function toggleMember(cb) {

    if (cb.checked) {

        selectedMembers.push(
            cb.value
        );

    } else {

        selectedMembers =
            selectedMembers.filter(
                x => x !== cb.value
            );

    }

}
async function saveSelectedMembers() {

    if (!selectedGroupId) {

        alert(
            "Select a group first"
        );

        return;
    }

    if (selectedMembers.length === 0) {

        alert(
            "Select members first"
        );

        return;
    }

    let formData =
        new FormData();

    formData.append(
        "groupId",
        selectedGroupId
    );

    // send each member separately
    selectedMembers.forEach(id => {

        formData.append(
            "memberIds",
            id
        );

    });

    let res = await fetch(

        '/Chat/AddMembers',

        {
            method: 'POST',

            body: formData
        }

    );

    if (!res.ok) {

        alert(
            "Failed to add members"
        );

        return;
    }

    alert(
        "Members added"
    );
    loadGroupDetails(
        selectedGroupId
    );

    closeMemberModal();

}
function closeMemberModal() {

    selectedMembers = [];

    document
        .getElementById(
            'addMemberModal'
        ).style.display = 'none';

}

async function renameCurrentGroup() {

    if (!selectedGroupId) {
        alert("Select a group first");
        return;
    }

    let newName = prompt(
        "Enter new group name:"
    );

    if (!newName || !newName.trim())
        return;

    // Use FormData so MVC binds:
    let formData =
        new FormData();

    formData.append(
        "groupId",
        selectedGroupId
    );

    formData.append(
        "name",
        newName.trim()
    );

    let res = await fetch(
        '/Chat/RenameGroup',
        {
            method: 'POST',
            body: formData
        });

    if (!res.ok) {
        alert("Rename failed");
        return;
    }

    // Update header immediately
    document
        .getElementById(
            "chatUserName"
        ).innerText =
        newName;

    // Reload sidebar groups
    loadGroups();

}

function loadGroupDetails(groupId) {

    fetch(
        `/Chat/GetGroupDetails?groupId=${groupId}`
    )

        .then(r => r.json())

        .then(g => {

            if (!g) return;

            // show container
            document
                .getElementById(
                    "groupMemberContainer"
                ).style.display =
                "inline-block";


            // show count only
            document
                .getElementById(
                    "groupMemberInfo"
                ).innerText =

                g.memberCount +
                " members";


            // build hover tooltip
            let html = '';

            g.members.forEach(m => {

                html +=
                    `<div
                        style='display:flex;
                        justify-content:space-between;
                        align-items:center;'>
                        <span>
                        ${m.userName}
                        </span>
                        <button class="btn btn-sm btn-danger"
                            onclick="removeMember(
                            '${m.userId}')"
                            style='font-size:11px;'>
                            Remove
                          </button>
                      </div>`;

            });


            document
                .getElementById(
                    "groupMembersTooltip"
                ).innerHTML =
                html;

        });

}
const memberContainer =
    document.getElementById(
        "groupMemberContainer"
    );

const memberTooltip =
    document.getElementById(
        "groupMembersTooltip"
    );

memberContainer.addEventListener(
    "mouseenter",
    function () {

        memberTooltip.style.display =
            "block";

    });

memberContainer.addEventListener(
    "mouseleave",
    function () {

        memberTooltip.style.display =
            "none";

    });

async function leaveCurrentGroup() {

    if (!selectedGroupId) {

        alert(
            "Select group first"
        );

        return;
    }

    if (!confirm(
        "Leave this group?"
    ))
        return;


    let formData =
        new FormData();

    formData.append(
        "groupId",
        selectedGroupId
    );

    let res = await fetch(

        '/Chat/LeaveGroup',

        {
            method: 'POST',

            body: formData
        }

    );

    if (!res.ok) {

        alert(
            "Failed to leave group"
        );

        return;
    }


    alert(
        "You left group"
    );


    // clear selected group
    selectedGroupId = "";


    document
        .getElementById(
            "chatBox"
        ).innerHTML = '';


    document
        .getElementById(
            "chatUserName"
        ).innerText =
        "Select User";


    document
        .getElementById(
            "groupActions"
        ).style.display =
        "none";


    document
        .getElementById(
            "groupMemberContainer"
        ).style.display =
        "none";


    loadGroups();

}

async function removeMember(userId) {

    if (!selectedGroupId)
        return;

    if (!confirm(
        "Remove this member?"
    ))
        return;


    let formData =
        new FormData();

    formData.append(
        "groupId",
        selectedGroupId
    );

    formData.append(
        "userId",
        userId
    );


    let res = await fetch(

        '/Chat/RemoveMember',

        {
            method: 'POST',

            body: formData
        }

    );

    if (!res.ok) {

        alert(
            "Remove failed"
        );

        return;
    }


    alert(
        "Member removed"
    );


    // refresh count + tooltip
    loadGroupDetails(
        selectedGroupId
    );

}