const API_BASE = 'http://localhost:5182/api/calendar';
let currentUserId = localStorage.getItem('calendarUserId');

if (!currentUserId) {
    window.location.href = 'login.html';
}

// UI Elements
const formContainer = document.getElementById('formContainer');
const formTitle = document.getElementById('formTitle');
const formTypeInput = document.getElementById('formType');
const submitFormBtn = document.getElementById('submitFormBtn');
const toggleAddBtn = document.getElementById('toggleAddBtn');
const toggleGroupBtn = document.getElementById('toggleGroupBtn');

const apptForm = document.getElementById('apptForm');
const calendarGrid = document.getElementById('calendarGrid');
const listTitle = document.getElementById('listTitle');
const sidebarReminders = document.getElementById('sidebarReminders');

const reminderCountDisplay = document.getElementById('reminderCountDisplay');
const addReminderBtn = document.getElementById('addReminderBtn');
const inlineReminderForm = document.getElementById('inlineReminderForm');
const confirmReminderBtn = document.getElementById('confirmReminderBtn');
const formReminderList = document.getElementById('formReminderList');
const reminderTimeInput = document.getElementById('reminderTimeInput');
const reminderTypeInput = document.getElementById('reminderTypeInput');

// Modals
const conflictModal = document.getElementById('conflictModal');
const groupMeetingModal = document.getElementById('groupMeetingModal');

// Temp data
let pendingApptData = null;
let pendingConflictId = null;
let pendingMeetingId = null;
let pendingReminders = [];

// Initialization
async function init() {
    // Fetch user info for greeting (hacky way since we don't store Name in localStorage properly, though we did in login)
    // Wait, let's fetch users and find this user's name
    try {
        const res = await fetch(`${API_BASE}/users`);
        const users = await res.json();
        const me = users.find(u => u.userID === currentUserId);
        if (me) {
            document.getElementById('userGreeting').textContent = `Xin chào, ${me.name}`;
        }
    } catch(e) { console.error(e); }

    loadAppointments();
}

document.getElementById('logoutBtn').addEventListener('click', () => {
    localStorage.removeItem('calendarUserId');
    window.location.href = 'login.html';
});

// Form Toggling
function openForm(type) {
    formContainer.style.display = 'block';
    if (type === 'normal') {
        formTitle.innerHTML = '📄 Thêm Lịch Hẹn';
        submitFormBtn.innerHTML = '✔ Tạo cuộc hẹn';
        submitFormBtn.className = 'btn-primary-large';
        formTypeInput.value = 'normal';
        
        toggleAddBtn.innerHTML = 'X Hủy';
        toggleAddBtn.style.background = '#e5e7eb';
        toggleAddBtn.style.color = '#1f2937';
        
        toggleGroupBtn.innerHTML = '👥 Tạo Cuộc Họp Nhóm';
    } else {
        formTitle.innerHTML = '👥 Tạo Cuộc Họp Nhóm Mới';
        submitFormBtn.innerHTML = '✔ Tạo cuộc họp';
        submitFormBtn.className = 'btn-primary-large'; // Assuming same style, or use green
        submitFormBtn.style.background = 'var(--secondary)';
        formTypeInput.value = 'group';

        toggleGroupBtn.innerHTML = 'X Hủy';
        toggleAddBtn.innerHTML = 'Thêm Lịch Hẹn';
        toggleAddBtn.style.background = 'var(--primary)';
        toggleAddBtn.style.color = 'white';
    }
}

function closeForm() {
    formContainer.style.display = 'none';
    apptForm.reset();
    toggleAddBtn.innerHTML = 'Thêm Lịch Hẹn';
    toggleAddBtn.style.background = 'var(--primary)';
    toggleAddBtn.style.color = 'white';
    
    toggleGroupBtn.innerHTML = '👥 Tạo Cuộc Họp Nhóm';
    
    pendingReminders = [];
    renderPendingReminders();
    inlineReminderForm.style.display = 'none';
    addReminderBtn.textContent = '+ Thêm';
}

toggleAddBtn.addEventListener('click', () => {
    if (formTypeInput.value === 'normal' && formContainer.style.display === 'block') {
        closeForm();
    } else {
        openForm('normal');
    }
});

toggleGroupBtn.addEventListener('click', () => {
    if (formTypeInput.value === 'group' && formContainer.style.display === 'block') {
        closeForm();
    } else {
        openForm('group');
    }
});

document.getElementById('cancelFormBtn').addEventListener('click', closeForm);
document.getElementById('refreshBtn').addEventListener('click', loadAppointments);

// Reminder Logic
addReminderBtn.addEventListener('click', () => {
    if (inlineReminderForm.style.display === 'none') {
        inlineReminderForm.style.display = 'block';
        addReminderBtn.textContent = 'x Đóng';
    } else {
        inlineReminderForm.style.display = 'none';
        addReminderBtn.textContent = '+ Thêm';
    }
});

confirmReminderBtn.addEventListener('click', () => {
    const time = reminderTimeInput.value;
    const type = reminderTypeInput.value;
    
    if (!time) {
        alert("Vui lòng chọn thời gian nhắc!");
        return;
    }
    
    pendingReminders.push({ time, type });
    renderPendingReminders();
    
    reminderTimeInput.value = '';
    inlineReminderForm.style.display = 'none';
    addReminderBtn.textContent = '+ Thêm';
});

function renderPendingReminders() {
    formReminderList.innerHTML = '';
    reminderCountDisplay.textContent = `🔔 Nhắc nhở (${pendingReminders.length})`;
    
    pendingReminders.forEach((rem, index) => {
        const item = document.createElement('div');
        item.className = 'reminder-item';
        
        const dateObj = new Date(rem.time);
        const timeStr = dateObj.toLocaleTimeString('vi-VN', {hour: '2-digit', minute:'2-digit', second:'2-digit'}) + ' ' + dateObj.toLocaleDateString('vi-VN');
        
        item.innerHTML = `
            <div class="reminder-item-left">
                <span style="color: var(--text-muted); margin-right: 4px;">-</span>
                <span class="reminder-badge" style="margin-bottom: 0;">${rem.type.toUpperCase()}</span>
                <span>${timeStr}</span>
            </div>
            <button type="button" class="reminder-item-remove" onclick="removePendingReminder(${index})">x</button>
        `;
        formReminderList.appendChild(item);
    });
}

window.removePendingReminder = function(index) {
    pendingReminders.splice(index, 1);
    renderPendingReminders();
};

// Calculate Duration Helper
function getDurationString(start, end) {
    const s = new Date(start);
    const e = new Date(end);
    const diffMs = e - s;
    const diffMins = Math.round(diffMs / 60000);
    
    if (diffMins < 60) return `${diffMins} phút`;
    const hours = Math.floor(diffMins / 60);
    const mins = diffMins % 60;
    if (mins === 0) return `${hours} giờ`;
    return `${hours} giờ ${mins} phút`;
}

// Load Appointments
async function loadAppointments() {
    try {
        const res = await fetch(`${API_BASE}/${currentUserId}/appointments`);
        if (!res.ok) throw new Error("Failed to load");
        const appts = await res.json();
        
        calendarGrid.innerHTML = '';
        sidebarReminders.innerHTML = '';
        
        listTitle.textContent = `Lịch hẹn của bạn (${appts.length})`;

        if (appts.length === 0) {
            calendarGrid.innerHTML = '<p class="empty-state">Chưa có lịch hẹn nào.</p>';
            sidebarReminders.innerHTML = '<div class="empty-state">Không có thông báo</div>';
            return;
        }

        appts.sort((a,b) => new Date(a.start) - new Date(b.start)).forEach(a => {
            const card = document.createElement('div');
            card.className = `appointment-card ${a.type === 'GroupMeeting' ? 'group-meeting' : ''}`;
            
            const startStr = new Date(a.start).toLocaleString('vi-VN', {hour: '2-digit', minute:'2-digit', day:'2-digit', month:'2-digit', year:'numeric'});
            const endStr = new Date(a.end).toLocaleString('vi-VN', {hour: '2-digit', minute:'2-digit', day:'2-digit', month:'2-digit', year:'numeric'});
            
            let participantHtml = '';
            if (a.type === 'GroupMeeting') {
                const hostText = a.isHost ? 'Bạn (Chủ phòng)' : a.hostName;
                participantHtml = `
                <div class="app-card-detail">
                    <span>👑</span> <span>Người tạo: ${hostText}</span>
                </div>
                <div class="app-card-detail">
                    <span>👥</span> <span>${a.participantCount || 0} người tham gia</span>
                </div>
                `;
            }

            card.innerHTML = `
                <h3 class="app-card-title">${a.name}</h3>
                <div class="app-card-detail">
                    <span>📍</span> <span>${a.location || 'Chưa xác định'}</span>
                </div>
                <div class="app-card-detail">
                    <span>🕒</span> <span>${startStr} - ${endStr}</span>
                </div>
                <div class="app-card-detail">
                    <span>⏱</span> <span>${getDurationString(a.start, a.end)}</span>
                </div>
                ${participantHtml}
            `;
            
            // Render real reminders
            if (a.reminders && a.reminders.length > 0) {
                const remSection = document.createElement('div');
                remSection.className = 'app-card-reminders';
                let remindersHtml = `<div style="font-size: 0.85rem; font-weight: 600; margin-bottom: 8px;">🔔 Nhắc nhở (${a.reminders.length})</div>`;
                
                a.reminders.forEach(r => {
                    const rTime = new Date(r.remindAt).toLocaleString('vi-VN', {hour: '2-digit', minute:'2-digit', day:'2-digit', month:'2-digit', year:'numeric'});
                    remindersHtml += `
                        <div style="display: flex; justify-content: space-between; align-items: center; margin-top: 4px;">
                            <span style="font-size: 0.85rem; color: var(--text-muted);">${rTime}</span>
                            <span class="reminder-badge">${r.type}</span>
                        </div>
                    `;
                    
                    // Add to Sidebar
                    const sideCard = document.createElement('div');
                    sideCard.className = 'sidebar-card';
                    sideCard.innerHTML = `
                        <span class="reminder-badge">${r.type}</span>
                        <div style="font-weight: 600; font-size: 0.9rem; margin-top: 8px; margin-bottom: 4px;">Nhắc nhở cho: ${a.name}</div>
                        <div style="font-size: 0.8rem; color: var(--text-muted);">${rTime}</div>
                    `;
                    sidebarReminders.appendChild(sideCard);
                });
                
                remSection.innerHTML = remindersHtml;
                card.appendChild(remSection);
            }

            calendarGrid.appendChild(card);
        });
    } catch (err) {
        console.error(err);
        calendarGrid.innerHTML = '<p style="color: red;">Lỗi tải dữ liệu.</p>';
    }
}

// Save Appointment
apptForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const start = document.getElementById('apptStart').value;
    const end = document.getElementById('apptEnd').value;
    
    if (new Date(start) >= new Date(end)) {
        alert("Thời gian kết thúc phải sau thời gian bắt đầu!");
        return;
    }

    pendingApptData = {
        name: document.getElementById('apptName').value,
        location: document.getElementById('apptLocation').value,
        start: start,
        end: end,
        type: document.getElementById('formType').value,
        replaceExisting: false,
        ignoreGroupMeeting: false,
        reminders: pendingReminders.map(r => ({ method: r.type, triggerTime: r.time }))
    };

    await attemptAddAppointment();
});

async function attemptAddAppointment() {
    try {
        const res = await fetch(`${API_BASE}/${currentUserId}/appointments`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(pendingApptData)
        });

        if (res.ok) {
            closeForm();
            loadAppointments();
            return;
        }

        if (res.status === 409) {
            const errorData = await res.json();
            if (errorData.code === 'TIME_CONFLICT') {
                pendingConflictId = errorData.conflictingId;
                conflictModal.classList.add('active');
            } else if (errorData.code === 'GROUP_MEETING_MATCH') {
                pendingMeetingId = errorData.meetingId;
                groupMeetingModal.classList.add('active');
            }
        } else {
            const errText = await res.text();
            alert("Lỗi: " + errText);
        }
    } catch (err) {
        console.error(err);
        alert("Lỗi kết nối.");
    }
}

// Conflict Resolution
document.getElementById('changeTimeBtn').addEventListener('click', () => {
    conflictModal.classList.remove('active');
});

document.getElementById('replaceApptBtn').addEventListener('click', async () => {
    pendingApptData.replaceExisting = true;
    conflictModal.classList.remove('active');
    await attemptAddAppointment();
});

// Group Meeting Resolution
document.getElementById('cancelGroupBtn').addEventListener('click', async () => {
    groupMeetingModal.classList.remove('active');
    pendingApptData.ignoreGroupMeeting = true;
    await attemptAddAppointment();
});

document.getElementById('joinGroupBtn').addEventListener('click', async () => {
    try {
        const res = await fetch(`${API_BASE}/groupmeetings/${pendingMeetingId}/join/${currentUserId}`, {
            method: 'POST'
        });
        
        if (res.ok) {
            groupMeetingModal.classList.remove('active');
            closeForm();
            loadAppointments();
        } else {
            alert("Lỗi tham gia nhóm.");
        }
    } catch (err) {
        console.error(err);
    }
});

init();
