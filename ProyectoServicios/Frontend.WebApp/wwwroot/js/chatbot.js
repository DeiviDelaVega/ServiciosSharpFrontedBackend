(() => {
    const CHAT_API_URL = "/api/chat";
    const STORAGE_KEY = "chat_conversation_id";

    function el(id) { return document.getElementById(id); }

    const toggleBtn = el("chatbot-toggle");
    const chatbot = el("chatbot");
    const closeBtn = el("chatbot-close");
    const list = el("chatbot-messages");
    const input = el("chatbot-text");
    const sendBtn = el("chatbot-send");

    if (!toggleBtn || !chatbot || !closeBtn || !list || !input || !sendBtn) return;

    let conversationId = localStorage.getItem(STORAGE_KEY) || null;

    function openChat() { chatbot.classList.remove("chatbot-hidden"); input.focus(); }
    function closeChat() { chatbot.classList.add("chatbot-hidden"); }

    function escapeHtml(str) {
        return String(str)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function addMessage(role, text) {
        const row = document.createElement("div");
        row.className = "chatbot-row " + role;

        const bubble = document.createElement("div");
        bubble.className = "chatbot-bubble";
        bubble.innerHTML = escapeHtml(text);

        row.appendChild(bubble);
        list.appendChild(row);
        list.scrollTop = list.scrollHeight;

        return { bubble };
    }

    function setLoading(v) {
        sendBtn.disabled = v;
        input.disabled = v;
    }

    function extractReply(data) {
        if (data && typeof data.reply === "string" && data.reply.trim() !== "") {
            const raw = data.reply.trim();

            try {
                const parsed = JSON.parse(raw);

                if (parsed?.output?.response && typeof parsed.output.response === "string") {
                    return parsed.output.response;
                }

                if (typeof parsed === "string") return parsed;

                return raw;
            } catch {
                return raw;
            }
        }

        if (data?.output?.response && typeof data.output.response === "string") {
            return data.output.response;
        }

        if (Array.isArray(data) && data[0]?.output?.response && typeof data[0].output.response === "string") {
            return data[0].output.response;
        }

        if (typeof data === "string") return data;

        return "(sin respuesta)";
    }

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        input.value = "";
        addMessage("user", text);
        const typing = addMessage("bot", "Escribiendo…");

        setLoading(true);

        try {
            const res = await fetch(CHAT_API_URL, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ conversationId, message: text })
            });

            if (!res.ok) {
                const errText = await res.text().catch(() => "");
                throw new Error("HTTP " + res.status + " " + errText);
            }

            const data = await res.json();

            // Guardar conversationId si te lo devuelven
            if (data && data.conversationId) {
                conversationId = data.conversationId;
                localStorage.setItem(STORAGE_KEY, conversationId);
            }

            const replyText = extractReply(data);
            typing.bubble.innerHTML = escapeHtml(replyText);

        } catch (e) {
            typing.bubble.innerHTML = escapeHtml("Error API: " + (e.message || e));
        } finally {
            setLoading(false);
            input.focus();
            list.scrollTop = list.scrollHeight;
        }
    }

    toggleBtn.addEventListener("click", () => {
        chatbot.classList.contains("chatbot-hidden") ? openChat() : closeChat();
    });

    closeBtn.addEventListener("click", closeChat);
    sendBtn.addEventListener("click", sendMessage);
    input.addEventListener("keydown", (e) => { if (e.key === "Enter") sendMessage(); });

    addMessage("bot", "Hola 👋 ¿Qué necesitás hacer hoy?");
})();
