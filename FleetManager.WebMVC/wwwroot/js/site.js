
document.getElementById('themeToggle')?.addEventListener('click', () => {
    document.body.classList.toggle('light-theme');
    localStorage.setItem('theme', document.body.classList.contains('light-theme') ? 'light' : 'dark');
});

if (localStorage.getItem('theme') === 'light') {
    document.body.classList.add('light-theme');
}

let timerInterval, startTime;

function updateTimer() {
    const diff = new Date(new Date() - startTime);
    const h = String(diff.getUTCHours()).padStart(2, '0');
    const m = String(diff.getUTCMinutes()).padStart(2, '0');
    const s = String(diff.getUTCSeconds()).padStart(2, '0');
    document.getElementById('recordTime').innerText = `${h}:${m}:${s}`;
}

document.getElementById('btnStart')?.addEventListener('click', () => {
    startTime = new Date();
    timerInterval = setInterval(updateTimer, 1000);
    document.getElementById('recordStatus').classList.add('active');
    document.getElementById('btnStart').disabled = true;
    document.getElementById('btnSave').disabled = false;
    document.getElementById('btnDiscard').disabled = false;
    console.log("[SYSTEM] Data acquisition started...");
});

function stopRecording(actionMessage) {
    clearInterval(timerInterval);
    document.getElementById('recordStatus').classList.remove('active');
    document.getElementById('recordTime').innerText = "00:00:00";
    document.getElementById('btnStart').disabled = false;
    document.getElementById('btnSave').disabled = true;
    document.getElementById('btnDiscard').disabled = true;
    console.log(`[SYSTEM] Data acquisition ${actionMessage}.`);
}

document.getElementById('btnSave')?.addEventListener('click', () => stopRecording('saved'));
document.getElementById('btnDiscard')?.addEventListener('click', () => stopRecording('discarded'));


function saveWorkspaceLayout() {
    let grid = document.getElementById('workspace-grid');
    if (!grid) return;

    let layout = [];
    grid.querySelectorAll('.workspace-wrapper').forEach(card => {
        layout.push(card.getAttribute('data-ws-type'));
    });
    localStorage.setItem('rlp_layout', JSON.stringify(layout));
}

function initSpecialWorkspaces(wrapper) {
    if (wrapper.getAttribute('data-ws-type') === 'camera') {
        let video = wrapper.querySelector('.local-camera');
        let status = wrapper.querySelector('.cam-status');
        if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
            navigator.mediaDevices.getUserMedia({ video: true, audio: false })
                .then(stream => {
                    video.srcObject = stream;
                    status.innerText = '[ SIGNAL: OK ]';
                    status.style.color = 'var(--accent-color)';
                })
                .catch(err => {
                    status.innerText = '[ SIGNAL: CONNECTION FAILED ]';
                    status.style.color = '#ff4444';
                });
        }
    }
}

function stopCamera(wrapper) {
    let video = wrapper.querySelector('.local-camera');
    if (video && video.srcObject) {
        video.srcObject.getTracks().forEach(track => track.stop());
    }
}

function loadWorkspaces() {
    let grid = document.getElementById('workspace-grid');
    if (!grid) return;

    let savedLayout = localStorage.getItem('rlp_layout');
    let layout = savedLayout ? JSON.parse(savedLayout) : ['robots', 'logs', 'camera'];

    grid.innerHTML = '';

    layout.forEach(type => {
        let template = document.getElementById('tpl-' + type);
        if (template) {
            let newNode = template.cloneNode(true);
            grid.appendChild(newNode);
            initSpecialWorkspaces(newNode);
        }
    });
}

function showLimitWarning() {
    let warning = document.createElement('div');
    warning.className = 'alert alert-danger position-fixed fw-bold';
    warning.style.cssText = 'top: 20px; left: 50%; transform: translateX(-50%); z-index: 9999; border: 1px solid #ff0000; background-color: #111; color: #ff0000;';
    warning.innerText = '[ SYSTEM ERROR ] MAX WORKSPACES LIMIT REACHED (6)';
    document.body.appendChild(warning);
    setTimeout(() => warning.remove(), 3000);
}

document.addEventListener('click', function (e) {
    if (e.target.classList.contains('delete-btn')) {
        let wrapper = e.target.closest('.workspace-wrapper');
        stopCamera(wrapper);
        wrapper.remove();
        saveWorkspaceLayout();
    }

    if (e.target.classList.contains('select-source-btn')) {
        let type = e.target.getAttribute('data-target');
        let template = document.getElementById('tpl-' + type);
        let wrapper = e.target.closest('.workspace-wrapper');

        if (template && wrapper) {
            let newNode = template.cloneNode(true);
            wrapper.replaceWith(newNode);
            initSpecialWorkspaces(newNode);
            saveWorkspaceLayout();
        }
    }
});

document.getElementById('btnAddWorkspace')?.addEventListener('click', function () {
    let grid = document.getElementById('workspace-grid');
    if (!grid) return;

    let currentCount = grid.querySelectorAll('.workspace-wrapper').length;
    if (currentCount >= 6) {
        showLimitWarning();
        return;
    }

    let template = document.getElementById('tpl-empty');
    if (template) {
        let newNode = template.cloneNode(true);
        grid.appendChild(newNode);
        saveWorkspaceLayout();
    }
});

document.getElementById('btnEditWorkspaces')?.addEventListener('click', function () {
    document.body.classList.toggle('edit-mode');
    this.innerText = document.body.classList.contains('edit-mode') ? 'DONE' : 'EDIT';
    document.getElementById('btnAddWorkspace').style.display = document.body.classList.contains('edit-mode') ? 'block' : 'none';
});

function fetchLiveLogs() {
    const logsWorkspace = document.querySelector('#workspace-grid .workspace-wrapper[data-ws-type="logs"]');

    if (!logsWorkspace) return;

    fetch('/HardwareLogs/GetRecentLogs')
        .then(response => response.json())
        .then(data => {
            const tbody = logsWorkspace.querySelector('tbody');
            if (tbody) {
                tbody.innerHTML = '';

                data.forEach(log => {
                    let msgStyle = log.message.includes('[AUTO]') ? 'color: var(--accent-color);' : '';

                    tbody.innerHTML += `<tr>
                        <td style="opacity: 0.7;">${log.time}</td>
                        <td style="color: #00bcd4;">${log.robot}</td>
                        <td style="${msgStyle}">${log.message}</td>
                    </tr>`;
                });
            }
        })
        .catch(err => console.error("[SYSTEM] Live feed error:", err));
}

setInterval(fetchLiveLogs, 3000);

document.addEventListener("DOMContentLoaded", loadWorkspaces);