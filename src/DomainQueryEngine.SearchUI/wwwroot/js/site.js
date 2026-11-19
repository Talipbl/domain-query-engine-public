// Site-wide behaviour. No framework: small progressive enhancements over working HTML.

(function () {
    "use strict";

    // Currency switch: the form works without JavaScript (POST + redirect). With JavaScript we save the
    // choice with fetch and let the page re-render in place; pages that can do so listen for
    // "dqe:currency" and call preventDefault(). Otherwise the page reloads to render server-side.
    document.querySelectorAll("form[data-currency-switch]").forEach(function (form) {
        form.addEventListener("submit", function (event) {
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
    });
})();
