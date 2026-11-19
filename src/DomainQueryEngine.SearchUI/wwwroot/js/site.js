// Site-wide behaviour. No framework: small progressive enhancements over working HTML.
// Listeners are delegated to the document so they keep working after htmx swaps parts of the page.

(function () {
    "use strict";

    // Currency switch: the form works without JavaScript (POST + redirect). With JavaScript we save the
    // choice with fetch and let the page re-render in place; pages that can do so listen for
    // "dqe:currency" and call preventDefault(). Otherwise the page reloads to render server-side.
    document.addEventListener("submit", function (event) {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.matches("[data-currency-switch]")) return;

        const button = event.submitter;
        if (!button || button.name !== "currency") return;
        event.preventDefault();

        const data = new FormData(form);
        data.set("currency", button.value);

        fetch(form.action, { method: "POST", body: data, headers: { "X-Requested-With": "fetch" } })
            .then(function (response) {
                if (!response.ok) throw new Error("Currency could not be saved");

                form.querySelectorAll("button[name=currency]").forEach(function (b) {
                    b.setAttribute("aria-pressed", b === button ? "true" : "false");
                });

                const notHandled = document.dispatchEvent(new CustomEvent("dqe:currency", {
                    detail: { currency: button.value },
                    cancelable: true
                }));
                if (notHandled) window.location.reload();
            })
            .catch(function () { form.submit(); });
    });

    // Theme switch: flips between light and dark from the theme currently on screen (an explicit choice
    // or the system setting), applies it at once and saves it. Without JavaScript the form posts instead.
    const systemDark = window.matchMedia("(prefers-color-scheme: dark)");
    function effectiveTheme() {
        return document.documentElement.dataset.theme || (systemDark.matches ? "dark" : "light");
    }

    // The server only knows an explicit choice, so the button label is set here for the theme on screen.
    function syncThemeButtons() {
        const theme = effectiveTheme();
        document.querySelectorAll("form[data-theme-switch]").forEach(function (form) {
            const button = form.querySelector("button[name=theme]");
            if (!button) return;
            const label = theme === "dark" ? form.dataset.labelLight : form.dataset.labelDark;
            button.value = theme === "dark" ? "light" : "dark";
            button.setAttribute("aria-label", label);
            button.title = label;
        });
    }
    syncThemeButtons();
    systemDark.addEventListener("change", syncThemeButtons);

    document.addEventListener("submit", function (event) {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.matches("[data-theme-switch]")) return;
        event.preventDefault();

        const next = effectiveTheme() === "dark" ? "light" : "dark";
        document.documentElement.dataset.theme = next;
        syncThemeButtons();

        const data = new FormData(form);
        data.set("theme", next);
        fetch(form.action, { method: "POST", body: data, headers: { "X-Requested-With": "fetch" } })
            .catch(function () { /* the theme still applies for this page */ });
    });

    // Selects marked data-autosubmit submit their form when changed (sorting on list pages).
    document.addEventListener("change", function (event) {
        const select = event.target;
        if (select instanceof HTMLSelectElement && select.hasAttribute("data-autosubmit") && select.form) {
            select.form.requestSubmit();
        }
    });
})();
