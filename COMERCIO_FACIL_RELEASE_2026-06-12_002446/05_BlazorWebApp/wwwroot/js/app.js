window.easyLoginBase = window.easyLoginBase || {};

window.easyLoginBase.storage = {
    get: function (key) {
        return window.localStorage.getItem(key);
    },
    set: function (key, value) {
        window.localStorage.setItem(key, value);
    },
    remove: function (key) {
        window.localStorage.removeItem(key);
    }
};

window.easyLoginBase.clipboard = {
    write: async function (text) {
        if (navigator.clipboard) {
            await navigator.clipboard.writeText(text || "");
            return true;
        }

        return false;
    }
};

window.easyLoginBase.download = {
    base64: function (fileName, contentType, base64) {
        const link = document.createElement("a");
        link.href = `data:${contentType || "application/octet-stream"};base64,${base64 || ""}`;
        link.download = fileName || "download";
        document.body.appendChild(link);
        link.click();
        link.remove();
    },
    text: function (fileName, contentType, text) {
        const blob = new Blob([text || ""], { type: contentType || "text/plain;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = fileName || "download.txt";
        document.body.appendChild(link);
        link.click();
        link.remove();
        URL.revokeObjectURL(url);
    }
};

window.easyLoginBase.notifications = {
    requestPermission: async function () {
        if (!("Notification" in window)) {
            return "unsupported";
        }

        if (Notification.permission === "default") {
            return await Notification.requestPermission();
        }

        return Notification.permission;
    },
    show: async function (title, body, url) {
        const permission = await window.easyLoginBase.notifications.requestPermission();
        if (permission !== "granted") {
            return false;
        }

        const notification = new Notification(title || "Comercio Facil", { body: body || "" });
        if (url) {
            notification.onclick = function () {
                window.focus();
                window.location.href = url;
            };
        }

        return true;
    }
};

window.easyLoginBase.scanner = (function () {
    const sessions = new Map();
    const defaultFormats = ["code_128", "qr_code", "itf"];

    function stop(videoId) {
        const session = sessions.get(videoId);
        if (session) {
            session.active = false;

            if (session.frameId) {
                cancelAnimationFrame(session.frameId);
            }

            if (session.stream) {
                session.stream.getTracks().forEach(track => track.stop());
            }

            sessions.delete(videoId);
        }

        const video = document.getElementById(videoId);
        if (video) {
            video.pause();
            video.srcObject = null;
            video.removeAttribute("src");
            video.load();
        }

        return true;
    }

    async function getSupportedFormats(formats) {
        const requested = Array.isArray(formats) && formats.length > 0 ? formats : defaultFormats;

        if (!("BarcodeDetector" in window)) {
            return [];
        }

        if (!BarcodeDetector.getSupportedFormats) {
            return requested;
        }

        const supported = await BarcodeDetector.getSupportedFormats();
        return requested.filter(format => supported.includes(format));
    }

    function normalizeError(error) {
        if (error && (error.name === "NotAllowedError" || error.name === "SecurityError")) {
            return {
                status: "permission-denied",
                message: "Permissao de camera negada."
            };
        }

        if (error && (error.name === "NotFoundError" || error.name === "OverconstrainedError")) {
            return {
                status: "camera-unavailable",
                message: "Nenhuma camera compativel foi encontrada."
            };
        }

        return {
            status: "decode-error",
            message: "Nao foi possivel iniciar a leitura automatica."
        };
    }

    async function start(videoId, dotNetReference, formats) {
        stop(videoId);

        const video = document.getElementById(videoId);
        if (!video) {
            return {
                status: "decode-error",
                message: "Elemento de video nao encontrado."
            };
        }

        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia || !("BarcodeDetector" in window)) {
            return {
                status: "unsupported",
                message: "Camera ou BarcodeDetector indisponivel neste navegador."
            };
        }

        const selectedFormats = await getSupportedFormats(formats);
        if (selectedFormats.length === 0) {
            return {
                status: "unsupported",
                message: "Os formatos de codigo solicitados nao sao suportados neste navegador."
            };
        }

        try {
            const stream = await navigator.mediaDevices.getUserMedia({
                audio: false,
                video: {
                    facingMode: { ideal: "environment" },
                    width: { ideal: 1280 },
                    height: { ideal: 720 }
                }
            });

            video.srcObject = stream;
            video.setAttribute("playsinline", "true");
            video.muted = true;
            await video.play();

            const detector = new BarcodeDetector({ formats: selectedFormats });
            const session = {
                active: true,
                stream,
                frameId: 0
            };

            const scan = async () => {
                if (!session.active) {
                    return;
                }

                try {
                    if (video.readyState >= HTMLMediaElement.HAVE_CURRENT_DATA) {
                        const codes = await detector.detect(video);
                        const value = codes && codes.length > 0 ? codes[0].rawValue : null;

                        if (value) {
                            await dotNetReference.invokeMethodAsync("HandleBarcodeDetected", value);
                            stop(videoId);
                            return;
                        }
                    }
                } catch {
                    // Detection can fail while the camera is warming up; keep the loop alive.
                }

                session.frameId = requestAnimationFrame(scan);
            };

            sessions.set(videoId, session);
            session.frameId = requestAnimationFrame(scan);

            return {
                status: "started",
                message: "Camera iniciada."
            };
        } catch (error) {
            stop(videoId);
            return normalizeError(error);
        }
    }

    return {
        start,
        stop
    };
})();
