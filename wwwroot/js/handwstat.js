(function () {
    var storageKey = "handwstat.theme";
    var root = document.documentElement;
    var isAndroid = /\bAndroid\b/i.test(navigator.userAgent || "");

    if (isAndroid) {
        root.setAttribute("data-handwstat-platform", "android");
    }

    function normalizeTheme(value) {
        return value === "dark" ? "dark" : "light";
    }

    function applyTheme(value, persist) {
        var theme = normalizeTheme(value);
        root.setAttribute("data-theme", theme);
        root.style.colorScheme = theme;

        if (persist) {
            try {
                localStorage.setItem(storageKey, theme);
            } catch (error) {
                // Ignore persistence failures.
            }
        }

        return theme;
    }

    var storedTheme = null;
    try {
        storedTheme = localStorage.getItem(storageKey);
    } catch (error) {
        storedTheme = null;
    }

    var preferredTheme = storedTheme === "dark" || storedTheme === "light"
        ? storedTheme
        : "dark";

    applyTheme(preferredTheme, false);

    window.handwstatTheme = {
        get: function () {
            return normalizeTheme(root.getAttribute("data-theme"));
        },
        set: function (value) {
            return applyTheme(value, true);
        }
    };

    window.handwstatExports = {
        downloadTextFile: function (fileName, mimeType, content) {
            var blob = new Blob([content], { type: mimeType || "text/plain;charset=utf-8" });
            var url = URL.createObjectURL(blob);
            var link = document.createElement("a");
            link.href = url;
            link.download = fileName || "export.txt";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.setTimeout(function () {
                URL.revokeObjectURL(url);
            }, 0);
        },
        downloadBinaryFile: function (fileName, mimeType, bytes) {
            var blob = new Blob([bytes], { type: mimeType || "application/octet-stream" });
            var url = URL.createObjectURL(blob);
            var link = document.createElement("a");
            link.href = url;
            link.download = fileName || "export.xlsx";
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            window.setTimeout(function () {
                URL.revokeObjectURL(url);
            }, 0);
        },
        copyText: function (content) {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                return navigator.clipboard.writeText(content || "");
            }

            var textarea = document.createElement("textarea");
            textarea.value = content || "";
            textarea.setAttribute("readonly", "readonly");
            textarea.style.position = "fixed";
            textarea.style.left = "-9999px";
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand("copy");
            document.body.removeChild(textarea);
            return Promise.resolve();
        }
    };

    window.handwstatCommands = {
        dotnet: null,
        handler: null,
        register: function (dotnetReference) {
            this.unregister();
            this.dotnet = dotnetReference;
            this.handler = function (event) {
                if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
                    event.preventDefault();
                    if (window.handwstatCommands.dotnet) {
                        window.handwstatCommands.dotnet.invokeMethodAsync("OpenCommandPaletteFromKeyboard");
                    }
                }
            };
            document.addEventListener("keydown", this.handler);
        },
        unregister: function () {
            if (this.handler) {
                document.removeEventListener("keydown", this.handler);
            }
            this.handler = null;
            this.dotnet = null;
        }
    };

    window.handwstatViewport = {
        isMobile: function () {
            return window.matchMedia("(max-width: 980px)").matches;
        }
    };

    function installAndroidScrollFallback() {
        if (!isAndroid || window.__handwstatAndroidScrollFallbackInstalled) {
            return;
        }

        window.__handwstatAndroidScrollFallbackInstalled = true;

        var scroller = null;
        var startX = 0;
        var startY = 0;
        var lastY = 0;
        var direction = null;

        function getMainScroller() {
            return document.querySelector(".studio-workbench")
                || document.querySelector(".studio-stage")
                || document.scrollingElement
                || document.documentElement;
        }

        function shouldIgnoreTouch(target) {
            return !!(target && target.closest && target.closest(
                ".studio-commandbar, .studio-mobile-dock, .studio-domain-rail, .studio-context-lens, .command-palette-backdrop, .drawer-shell, select, input, textarea"
            ));
        }

        document.addEventListener("touchstart", function (event) {
            if (event.touches.length !== 1 || shouldIgnoreTouch(event.target)) {
                scroller = null;
                return;
            }

            scroller = getMainScroller();
            startX = event.touches[0].clientX;
            startY = event.touches[0].clientY;
            lastY = startY;
            direction = null;
        }, { passive: true });

        document.addEventListener("touchmove", function (event) {
            if (!scroller || event.touches.length !== 1) {
                return;
            }

            var touch = event.touches[0];
            var deltaX = touch.clientX - startX;
            var deltaYFromStart = touch.clientY - startY;
            var absX = Math.abs(deltaX);
            var absY = Math.abs(deltaYFromStart);

            if (!direction && (absX > 6 || absY > 6)) {
                direction = absY >= absX ? "vertical" : "horizontal";
            }

            if (direction !== "vertical") {
                return;
            }

            var deltaY = lastY - touch.clientY;
            lastY = touch.clientY;

            if (Math.abs(deltaY) < 0.5) {
                return;
            }

            var before = scroller.scrollTop;
            scroller.scrollTop = before + deltaY;

            if (scroller.scrollTop !== before) {
                event.preventDefault();
            }
        }, { passive: false });

        document.addEventListener("touchend", function () {
            scroller = null;
            direction = null;
        }, { passive: true });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", installAndroidScrollFallback);
    } else {
        installAndroidScrollFallback();
    }
})();
