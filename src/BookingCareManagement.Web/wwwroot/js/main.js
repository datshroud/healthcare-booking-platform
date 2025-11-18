const CHAT_WIDGET_OPEN_CLASS = "chat-widget--open";
const CHAT_WIDGET_API_BASE = "/api/support-chat";
const CHAT_WIDGET_POLL_INTERVAL_MS = 10000;
document.addEventListener("DOMContentLoaded", function () {
    const currentPath = window.location.pathname.toLowerCase();
    const sidebarLinks = document.querySelectorAll('.sidebar a');

    sidebarLinks.forEach(link => {
        const linkPath = link.getAttribute('href').toLowerCase();

        // Nếu đường dẫn trang hiện tại BẮT ĐẦU BẰNG href của link
        // (ví dụ: /doctors/edit/123 cũng sẽ highlight link /doctors)
        if (currentPath.startsWith(linkPath) && linkPath !== "/") {
            link.classList.add('active');
        } else {
            link.classList.remove('active');
        }
    });

    const dashboardLink = document.querySelector('.sidebar a[data-nav="dashboard"]');
    if (dashboardLink) {
        const dashPath = dashboardLink.getAttribute('href').toLowerCase();
        if (currentPath === dashPath || (dashPath === "/dashboard" && currentPath === "/")) {
            dashboardLink.classList.add('active');
        }
    }

    // Sao chép clipboard
    const copyBtn = document.querySelector('.dropdown-menu button.btn-outline-secondary');
    if (copyBtn) {
        copyBtn.addEventListener('click', function () {
            const input = this.previousElementSibling;
            navigator.clipboard.writeText(input.value).then(() => {
                this.innerHTML = '<i class="bi bi-clipboard-check text-success"></i>';
                setTimeout(() => {
                    this.innerHTML = '<i class="bi bi-clipboard"></i>';
                }, 1500);
            });
        });
    }

    // Giữ panel chuông mở khi chuyển tab
    const notifWrapper = document.getElementById('notifDropdownWrapper');
    const notifMenu = notifWrapper?.querySelector('.dropdown-menu');

    if (notifMenu) {
        notifMenu.addEventListener('click', function (e) {
            // Ngăn dropdown tự đóng khi click trong vùng menu
            e.stopPropagation();
        });

        // Ngăn auto-close khi click tab
        const tabs = notifMenu.querySelectorAll('[data-bs-toggle="tab"]');
        tabs.forEach(tab => {
            tab.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                const target = document.querySelector(this.dataset.bsTarget);
                notifMenu.querySelectorAll('.tab-pane').forEach(p => p.classList.remove('show', 'active'));
                target.classList.add('show', 'active');
                notifMenu.querySelectorAll('.nav-link').forEach(t => t.classList.remove('active'));
                this.classList.add('active');
            });
        });
    }

    initializeNotificationsModule();
    initializeChatWidget();
});

// dang xuat
const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn){
    logoutBtn.addEventListener('click', async () => {
        try {
            const resp = await fetch('api/account/auth/logout', { method: 'POST', credentials: 'include' });
            if (resp.ok){
                // reload to update UI (cookies cleared server-side)
                window.location.href = '/';
                return;
            }
            // try to read error
            let txt = '';
            try { txt = await resp.text(); } catch {}
            alert('Đăng xuất thất bại: ' + (txt || resp.status));
        } catch (err) {
            console.error(err); 
            alert('Có lỗi khi đăng xuất.');
        }
    });
}

const NOTIF_API_BASE = '/api/AdminNotification';
const NOTIF_POLL_INTERVAL_MS = 60000;

function initializeNotificationsModule() {
    const elements = {
        wrapper: document.getElementById('notifDropdownWrapper'),
        dropdown: document.getElementById('notifDropdown'),
        badge: document.getElementById('notifUnreadBadge'),
        list: document.getElementById('notifAlertsContent'),
        empty: document.getElementById('notifEmptyState'),
        loading: document.getElementById('notifLoadingState'),
        error: document.getElementById('notifErrorState'),
        markAll: document.getElementById('notifMarkAllBtn')
    };

    if (!elements.wrapper || !elements.list) {
        return;
    }

    setupNotificationDropdown(elements);
}

function setupNotificationDropdown(elements) {
    const state = {
        pollTimer: null,
        isLoading: false,
        hasFetched: false
    };

    const refresh = async (showSpinner = false) => {
        if (state.isLoading) {
            return;
        }
        state.isLoading = true;
        if (showSpinner) {
            setNotifLoading(elements, true);
        }
        hideNotifError(elements);
        try {
            const [items, unreadCount] = await Promise.all([
                fetchJson(`${NOTIF_API_BASE}?take=15`),
                fetchJson(`${NOTIF_API_BASE}/unread-count`)
            ]);
            renderNotificationList(elements, Array.isArray(items) ? items : []);
            updateNotificationBadge(elements, Number(unreadCount) || 0);
            toggleMarkAllButton(elements, Number(unreadCount) > 0);
            state.hasFetched = true;
        } catch (error) {
            console.error('Không thể tải thông báo', error);
            showNotifError(elements, 'Không thể tải danh sách thông báo. Vui lòng thử lại.');
        } finally {
            state.isLoading = false;
            if (showSpinner) {
                setNotifLoading(elements, false);
            }
        }
    };

    const schedulePolling = () => {
        if (state.pollTimer) {
            window.clearInterval(state.pollTimer);
        }
        state.pollTimer = window.setInterval(() => {
            refresh(false);
        }, NOTIF_POLL_INTERVAL_MS);
    };

    elements.list.addEventListener('click', async (event) => {
        const target = event.target.closest('[data-action="mark-read"]');
        if (!target) {
            return;
        }
        const id = target.dataset.notifId;
        if (!id) {
            return;
        }
        target.disabled = true;
        try {
            await fetchJson(`${NOTIF_API_BASE}/${id}/read`, { method: 'POST' });
            await refresh(false);
        } catch (error) {
            console.error('Không thể cập nhật thông báo', error);
            target.disabled = false;
            showNotifError(elements, 'Không thể đánh dấu thông báo là đã đọc.');
        }
    });

    if (elements.markAll) {
        elements.markAll.addEventListener('click', async () => {
            elements.markAll.disabled = true;
            try {
                await fetchJson(`${NOTIF_API_BASE}/mark-all-read`, { method: 'POST' });
                await refresh(false);
            } catch (error) {
                console.error('Không thể đánh dấu tất cả thông báo', error);
                showNotifError(elements, 'Không thể đánh dấu tất cả thông báo là đã đọc.');
            } finally {
                elements.markAll.disabled = false;
            }
        });
    }

    if (elements.dropdown) {
        elements.dropdown.addEventListener('show.bs.dropdown', () => {
            if (!state.hasFetched) {
                refresh(true);
            } else {
                refresh(false);
            }
        });
    }

    document.addEventListener('visibilitychange', () => {
        if (document.visibilityState === 'visible') {
            refresh(false);
        }
    });

    schedulePolling();
    refresh(true);
}

function renderNotificationList(elements, items) {
    const list = elements.list;
    list.innerHTML = '';

    if (!items.length) {
        list.classList.add('d-none');
        toggleNotificationEmptyState(elements, true);
        return;
    }

    toggleNotificationEmptyState(elements, false);
    list.classList.remove('d-none');

    const fragment = document.createDocumentFragment();
    items.forEach(item => {
        fragment.appendChild(buildNotificationEntry(item));
    });
    list.appendChild(fragment);
}

function buildNotificationEntry(item) {
    const wrapper = document.createElement('div');
    wrapper.className = `notif-entry ${item.isRead ? '' : 'notif-entry--unread'}`;

    const body = document.createElement('div');
    body.className = 'flex-grow-1';

    const title = document.createElement('p');
    title.className = 'notif-entry__title mb-0';
    title.textContent = item.title || 'Thông báo';

    const message = document.createElement('p');
    message.className = 'notif-entry__message mb-0';
    message.textContent = item.message || '';

    const meta = document.createElement('p');
    meta.className = 'notif-entry__meta mb-0';
    meta.textContent = formatRelativeTime(item.createdAtUtc);

    body.appendChild(title);
    if (item.message) {
        body.appendChild(message);
    }
    body.appendChild(meta);

    const markButton = document.createElement('button');
    markButton.type = 'button';
    markButton.className = 'btn btn-link btn-sm p-0 notif-entry__mark';
    markButton.dataset.action = 'mark-read';
    markButton.dataset.notifId = item.id;
    markButton.textContent = item.isRead ? 'Đã đọc' : 'Đánh dấu đã đọc';
    markButton.disabled = !!item.isRead;

    wrapper.appendChild(body);
    wrapper.appendChild(markButton);

    return wrapper;
}

function updateNotificationBadge(elements, count) {
    const badge = elements.badge;
    if (!badge) {
        return;
    }
    if (count > 0) {
        badge.textContent = count > 99 ? '99+' : String(count);
        badge.classList.remove('d-none');
    } else {
        badge.classList.add('d-none');
    }
}

function toggleNotificationEmptyState(elements, shouldShow) {
    if (!elements.empty) {
        return;
    }
    elements.empty.classList.toggle('d-none', !shouldShow);
}

function toggleMarkAllButton(elements, shouldShow) {
    if (!elements.markAll) {
        return;
    }
    elements.markAll.classList.toggle('d-none', !shouldShow);
}

function setNotifLoading(elements, isLoading) {
    if (!elements.loading) {
        return;
    }
    elements.loading.classList.toggle('d-none', !isLoading);
}

function showNotifError(elements, message) {
    if (!elements.error) {
        return;
    }
    elements.error.textContent = message;
    elements.error.classList.remove('d-none');
}

function hideNotifError(elements) {
    if (!elements.error) {
        return;
    }
    elements.error.classList.add('d-none');
}

async function fetchJson(url, options = {}) {
    const requestInit = { ...options };
    requestInit.credentials = 'include';
    requestInit.headers = {
        Accept: 'application/json',
        ...(options.headers || {})
    };

    const response = await fetch(url, requestInit);
    if (!response.ok) {
        const errorBody = await safeReadBody(response);
        throw new Error(errorBody || `Request failed: ${response.status}`);
    }
    if (response.status === 204) {
        return null;
    }
    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
        return await response.json();
    }
    return await response.text();
}

async function safeReadBody(response) {
    try {
        return await response.text();
    } catch {
        return '';
    }
}

function formatRelativeTime(value) {
    if (!value) {
        return '';
    }
    const target = new Date(value);
    if (Number.isNaN(target.getTime())) {
        return '';
    }

    const now = new Date();
    const diffMs = now - target;
    const diffMinutes = Math.floor(diffMs / 60000);

    if (diffMinutes < 1) {
        return 'Vừa xong';
    }
    if (diffMinutes < 60) {
        return `${diffMinutes} phút trước`;
    }
    const diffHours = Math.floor(diffMinutes / 60);
    if (diffHours < 24) {
        return `${diffHours} giờ trước`;
    }
    const diffDays = Math.floor(diffHours / 24);
    if (diffDays < 7) {
        return `${diffDays} ngày trước`;
    }
    return target.toLocaleDateString('vi-VN');
}

function initializeChatWidget() {
    const widget = document.querySelector('[data-chat-widget]');
    if (!widget) {
        return;
    }

    const elements = {
        toggle: widget.querySelector('[data-chat-toggle]'),
        close: widget.querySelector('[data-chat-close]'),
        panel: widget.querySelector('[data-chat-panel]'),
        badge: widget.querySelector('[data-chat-badge]'),
        messages: widget.querySelector('[data-chat-messages]'),
        loading: widget.querySelector('[data-chat-loading]'),
        empty: widget.querySelector('[data-chat-empty]'),
        recipientContainer: widget.querySelector('[data-chat-recipient-container]'),
        recipientSelect: widget.querySelector('[data-chat-recipient]'),
        recipientMeta: widget.querySelector('[data-chat-recipient-meta]'),
        recipientAvatar: widget.querySelector('[data-chat-recipient-avatar]'),
        recipientName: widget.querySelector('[data-chat-recipient-name]'),
        recipientRole: widget.querySelector('[data-chat-recipient-role]'),
        recipientEmpty: widget.querySelector('[data-chat-recipient-empty]'),
        form: widget.querySelector('[data-chat-form]'),
        input: widget.querySelector('[data-chat-input]'),
        send: widget.querySelector('[data-chat-send]'),
        error: widget.querySelector('[data-chat-error]')
    };

    if (!elements.toggle || !elements.panel || !elements.messages) {
        return;
    }

    const state = {
        conversationId: null,
        isOpen: false,
        isSending: false,
        lastMessageUtc: null,
        pollTimerId: null,
        recipients: [],
        selectedRecipientId: null
    };

    elements.recipientSelect?.addEventListener('change', async (event) => {
        const staffId = event.target.value;
        const recipient = state.recipients.find((item) => item.staffId === staffId);
        await selectRecipient(recipient);
    });

    const setPanelState = (shouldOpen) => {
        state.isOpen = shouldOpen;
        widget.classList.toggle(CHAT_WIDGET_OPEN_CLASS, shouldOpen);
        elements.panel.setAttribute('aria-hidden', shouldOpen ? 'false' : 'true');
        elements.toggle.setAttribute('aria-expanded', shouldOpen ? 'true' : 'false');

        if (shouldOpen) {
            hideChatBadge(elements.badge);
            elements.input?.focus();
            scrollMessagesToBottom(elements.messages, true);
        }
    };

    const toggleWidget = () => setPanelState(!state.isOpen);

    const schedulePolling = () => {
        if (state.pollTimerId) {
            window.clearInterval(state.pollTimerId);
        }

        if (!state.conversationId) {
            return;
        }

        state.pollTimerId = window.setInterval(async () => {
            await fetchNewMessages();
        }, CHAT_WIDGET_POLL_INTERVAL_MS);
    };

    const setLoading = (isLoading) => {
        elements.loading?.classList.toggle('d-none', !isLoading);
        elements.messages?.classList.toggle('opacity-50', isLoading);
    };

    const updateEmptyState = () => {
        if (!elements.empty) {
            return;
        }
        const hasMessages = Boolean(elements.messages?.children.length);
        elements.empty.classList.toggle('d-none', hasMessages);
    };

    const showError = (message) => {
        if (!elements.error) {
            return;
        }
        elements.error.textContent = message;
        elements.error.classList.remove('d-none');
    };

    const clearError = () => {
        if (!elements.error) {
            return;
        }
        elements.error.classList.add('d-none');
        elements.error.textContent = '';
    };

    const getSelectedRecipient = () => state.recipients.find(r => r.staffId === state.selectedRecipientId) ?? null;

    const formatRecipientRole = (recipient) => {
        if (!recipient) {
            return '';
        }
        return recipient.staffRole === 0 ? 'Bác sĩ phụ trách' : 'Đội ngũ hỗ trợ';
    };

    const applyRecipientMeta = (recipient) => {
        if (!elements.recipientMeta) {
            return;
        }

        if (!recipient) {
            elements.recipientMeta.classList.add('d-none');
            elements.recipientEmpty?.classList.remove('d-none');
            return;
        }

        elements.recipientMeta.classList.remove('d-none');
        elements.recipientEmpty?.classList.add('d-none');

        if (elements.recipientName) {
            elements.recipientName.textContent = recipient.displayName || '---';
        }

        if (elements.recipientRole) {
            elements.recipientRole.textContent = formatRecipientRole(recipient);
        }

        if (elements.recipientAvatar) {
            if (recipient.avatarUrl) {
                elements.recipientAvatar.innerHTML = '';
                const img = document.createElement('img');
                img.src = recipient.avatarUrl;
                img.alt = recipient.displayName || 'avatar';
                elements.recipientAvatar.appendChild(img);
            } else {
                const initials = (recipient.displayName || '?').trim().charAt(0).toUpperCase();
                elements.recipientAvatar.textContent = initials || '?';
            }
        }

        if (elements.recipientSelect) {
            elements.recipientSelect.value = recipient.staffId;
            elements.recipientSelect.disabled = state.recipients.length <= 1;
        }
    };

    const populateRecipientOptions = () => {
        if (!elements.recipientSelect) {
            return;
        }

        elements.recipientSelect.innerHTML = '';
        state.recipients.forEach((recipient) => {
            const option = document.createElement('option');
            option.value = recipient.staffId;
            option.textContent = recipient.displayName || 'Không xác định';
            option.dataset.role = String(recipient.staffRole ?? '');
            option.dataset.doctorId = recipient.doctorId ?? '';
            elements.recipientSelect.appendChild(option);
        });

        elements.recipientSelect.disabled = state.recipients.length <= 1;
    };

    const getAgentLabel = () => {
        const recipient = getSelectedRecipient();
        if (recipient?.staffRole === 0) {
            return recipient.displayName || 'Bác sĩ phụ trách';
        }
        return recipient?.displayName || 'Trợ lý BookingCare';
    };

    const ensureConversation = async (recipient) => {
        if (!recipient) {
            return;
        }

        setLoading(true);
        clearError();

        try {
            const payload = {
                staffId: recipient.staffId,
                staffRole: recipient.staffRole,
                doctorId: recipient.doctorId
            };

            const conversation = await chatWidgetRequest(`${CHAT_WIDGET_API_BASE}/conversations`, {
                method: 'POST',
                body: JSON.stringify(payload)
            });

            state.conversationId = conversation?.id || null;
            state.lastMessageUtc = getLastMessageTimestamp(conversation?.messages);
            applyRecipientMeta(recipient);
            renderMessages(conversation?.messages ?? [], true);
            schedulePolling();
        } catch (error) {
            handleChatWidgetError(error, 'Không thể mở cuộc trò chuyện.');
        } finally {
            setLoading(false);
        }
    };

    const selectRecipient = async (recipient) => {
        if (!recipient || recipient.staffId === state.selectedRecipientId) {
            return;
        }

        state.selectedRecipientId = recipient.staffId;
        state.lastMessageUtc = null;
        await ensureConversation(recipient);
    };

    const loadRecipients = async () => {
        setLoading(true);
        clearError();
        try {
            const recipients = await chatWidgetRequest(`${CHAT_WIDGET_API_BASE}/targets`);
            const previousRecipientId = state.selectedRecipientId;
            state.recipients = Array.isArray(recipients) ? recipients : [];
            populateRecipientOptions();

            if (!state.recipients.length) {
                state.selectedRecipientId = null;
                applyRecipientMeta(null);
                showError('Bạn chưa có bác sĩ nào để trao đổi. Hãy đặt lịch để mở kênh trò chuyện.');
                elements.input?.setAttribute('disabled', 'disabled');
                elements.send?.setAttribute('disabled', 'disabled');
                return;
            }

            elements.input?.removeAttribute('disabled');
            elements.send?.removeAttribute('disabled');
            const nextRecipient = state.recipients.find((item) => item.staffId === previousRecipientId)
                ?? state.recipients[0];
            applyRecipientMeta(nextRecipient);
            await selectRecipient(nextRecipient);
        } catch (error) {
            handleChatWidgetError(error, 'Không thể tải danh sách người nhận.');
        } finally {
            setLoading(false);
        }
    };

    const renderMessages = (messages, replace = false) => {
        if (!elements.messages) {
            return;
        }

        if (replace) {
            elements.messages.innerHTML = '';
        }

        if (!Array.isArray(messages) || messages.length === 0) {
            updateEmptyState();
            return;
        }

        const fragment = document.createDocumentFragment();
        messages.forEach((message) => {
            fragment.appendChild(buildChatWidgetMessageElement(message, getAgentLabel()));
            state.lastMessageUtc = message.createdAtUtc;
            state.conversationId = message.conversationId || state.conversationId;
        });

        elements.messages.appendChild(fragment);
        updateEmptyState();
        scrollMessagesToBottom(elements.messages, !state.isOpen);
    };

    const fetchNewMessages = async () => {
        if (!state.conversationId) {
            return;
        }

        try {
            const params = new URLSearchParams({ conversationId: state.conversationId });
            if (state.lastMessageUtc) {
                params.set('afterUtc', state.lastMessageUtc);
            }

            const url = `${CHAT_WIDGET_API_BASE}/messages?${params.toString()}`;
            const messages = await chatWidgetRequest(url);
            if (Array.isArray(messages) && messages.length) {
                renderMessages(messages, false);
                const inboundCount = messages.filter((msg) => chatWidgetIsInbound(msg)).length;
                if (inboundCount && !state.isOpen) {
                    bumpChatBadge(elements.badge, inboundCount);
                }
            }
        } catch (error) {
            console.error('Không thể tải tin nhắn mới', error);
        }
    };

    const sendMessage = async () => {
        if (!elements.input || state.isSending) {
            return;
        }

        const content = elements.input.value.trim();
        if (!content) {
            elements.input.focus();
            return;
        }

        if (!state.conversationId) {
            showError('Vui lòng chọn người nhận trước khi gửi.');
            return;
        }

        state.isSending = true;
        elements.send?.setAttribute('disabled', 'disabled');
        clearError();

        try {
            const payload = {
                conversationId: state.conversationId,
                content
            };

            const message = await chatWidgetRequest(`${CHAT_WIDGET_API_BASE}/messages`, {
                method: 'POST',
                body: JSON.stringify(payload)
            });

            elements.input.value = '';
            if (message) {
                renderMessages([message], false);
            }
            await fetchNewMessages();
        } catch (error) {
            handleChatWidgetError(error, 'Gửi tin nhắn thất bại. Vui lòng thử lại.');
        } finally {
            state.isSending = false;
            elements.send?.removeAttribute('disabled');
        }
    };

    const handleChatWidgetError = (error, fallbackMessage) => {
        console.error('Support chat error', error);
        const message = error?.message || fallbackMessage;
        showError(message);

        if (error?.status === 401) {
            widget.classList.add('d-none');
            if (state.pollTimerId) {
                window.clearInterval(state.pollTimerId);
            }
        }
    };

    elements.toggle.addEventListener('click', toggleWidget);
    elements.close?.addEventListener('click', () => setPanelState(false));

    document.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && state.isOpen) {
            setPanelState(false);
        }
    });

    elements.form?.addEventListener('submit', (event) => {
        event.preventDefault();
        sendMessage();
    });

    elements.input?.addEventListener('keydown', (event) => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            sendMessage();
        }
    });

    elements.send?.addEventListener('click', (event) => {
        event.preventDefault();
        sendMessage();
    });

    loadRecipients();
}

function chatWidgetRequest(url, options = {}) {
    const requestInit = {
        credentials: 'include',
        headers: {
            Accept: 'application/json',
            'Content-Type': 'application/json',
            ...(options.headers || {})
        },
        ...options
    };

    return fetch(url, requestInit).then(async (response) => {
        if (!response.ok) {
            const error = new Error((await safeReadBody(response)) || 'Yêu cầu thất bại');
            error.status = response.status;
            throw error;
        }

        if (response.status === 204) {
            return null;
        }

        const contentType = response.headers.get('content-type') || '';
        if (contentType.includes('application/json')) {
            return response.json();
        }

        return response.text();
    });
}

function buildChatWidgetMessageElement(message, agentLabel) {
    const wrapper = document.createElement('div');
    const author = (message?.author || '').toString().toLowerCase();
    const variant = author === 'user' ? 'user' : 'agent';
    wrapper.className = `chat-widget__message chat-widget__message--${variant}`;

    if (variant === 'agent') {
        const authorLabel = document.createElement('div');
        authorLabel.className = 'chat-widget__message-author';
        authorLabel.textContent = author === 'system'
            ? 'Hệ thống BookingCare'
            : (agentLabel || 'Trợ lý BookingCare');
        wrapper.appendChild(authorLabel);
    }

    const bubble = document.createElement('div');
    bubble.className = 'chat-widget__bubble';
    bubble.textContent = message?.content ?? '';
    wrapper.appendChild(bubble);

    const timestamp = document.createElement('div');
    timestamp.className = 'chat-widget__timestamp';
    timestamp.textContent = formatChatWidgetTimestamp(message?.createdAtUtc);
    wrapper.appendChild(timestamp);

    return wrapper;
}

function formatChatWidgetTimestamp(value) {
    if (!value) {
        return '';
    }

    try {
        return new Date(value).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    } catch {
        return '';
    }
}

function scrollMessagesToBottom(container, smooth) {
    if (!container) {
        return;
    }

    const behavior = smooth ? 'smooth' : 'auto';
    container.scrollTo({ top: container.scrollHeight, behavior });
}

function hideChatBadge(badge) {
    if (!badge) {
        return;
    }
    badge.classList.add('d-none');
    badge.textContent = '0';
}

function bumpChatBadge(badge, amount) {
    if (!badge) {
        return;
    }

    const current = parseInt(badge.textContent || '0', 10) || 0;
    const next = current + amount;
    badge.textContent = next > 99 ? '99+' : String(next);
    badge.classList.remove('d-none');
}

function chatWidgetIsInbound(message) {
    const author = (message?.author || '').toString().toLowerCase();
    return author !== 'user';
}

function getLastMessageTimestamp(messages) {
    if (!Array.isArray(messages) || messages.length === 0) {
        return null;
    }
    const last = messages[messages.length - 1];
    return last?.createdAtUtc ?? null;
}