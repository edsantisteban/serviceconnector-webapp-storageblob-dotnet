(function () {
    "use strict";
    const form = document.getElementById("upload-form");
    const input = document.getElementById("UploadedFile");
    const selectedFile = document.getElementById("selected-file");
    const button = document.getElementById("upload-button");
    const status = document.getElementById("upload-status");
    const maxSize = Number(form.dataset.maxFileSize);

    input.addEventListener("change", function () {
        const file = input.files[0];
        input.setCustomValidity("");
        status.textContent = "";
        if (!file) {
            selectedFile.textContent = "Ningún archivo seleccionado.";
            return;
        }
        const unit = file.size < 1024 ? "bytes" : file.size < 1024 * 1024 ? "KB" : "MB";
        const divisor = unit === "bytes" ? 1 : unit === "KB" ? 1024 : 1024 * 1024;
        const size = (file.size / divisor).toLocaleString("es", { maximumFractionDigits: 2 });
        selectedFile.textContent = file.name + " · " + size + " " + unit;
        if (file.size > maxSize) {
            input.setCustomValidity("El archivo supera el límite de 20 MB. Selecciona uno más pequeño.");
            input.reportValidity();
        }
    });

    form.addEventListener("submit", function () {
        button.disabled = true;
        button.textContent = "Subiendo…";
        status.textContent = "Estamos subiendo tu archivo. Espera a que termine.";
        form.setAttribute("aria-busy", "true");
    });

    window.addEventListener("pageshow", function () {
        button.disabled = false;
        button.textContent = "Subir archivo";
        status.textContent = "";
        form.removeAttribute("aria-busy");
    });
}());
