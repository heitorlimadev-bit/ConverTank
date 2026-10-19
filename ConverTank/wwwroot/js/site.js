document.addEventListener("DOMContentLoaded", function () {
    function formatCNPJ(value) {
        let digits = value.replace(/\D/g, "");
        
        if (digits.length > 14) {
            digits = digits.slice(0, 14);
        }
        
        let formatted = digits;
        if (digits.length > 2) {
            formatted = digits.slice(0, 2) + "." + digits.slice(2);
        }
        if (digits.length > 5) {
            formatted = formatted.slice(0, 6) + "." + formatted.slice(6);
        }
        if (digits.length > 8) {
            formatted = formatted.slice(0, 10) + "/" + formatted.slice(10);
        }
        if (digits.length > 12) {
            formatted = formatted.slice(0, 15) + "-" + formatted.slice(15);
        }
        return formatted;
    }

    function applyCNPJMaskToElement(input) {
        input.addEventListener("input", function (e) {
            const cursorPosition = input.selectionStart;
            const originalLength = input.value.length;
            
            const formatted = formatCNPJ(input.value);
            input.value = formatted;
            
            const newLength = formatted.length;
            const diff = newLength - originalLength;
            input.setSelectionRange(cursorPosition + diff, cursorPosition + diff);
        });

        if (input.value) {
            input.value = formatCNPJ(input.value);
        }
    }

    const cnpjInputs = document.querySelectorAll('.cnpj-mask, input[name*="cnpj" i], input[id*="cnpj" i], input[id*="Cnpj" i]');
    cnpjInputs.forEach(applyCNPJMaskToElement);

    const passwordInputs = document.querySelectorAll('input[type="password"]');
    passwordInputs.forEach(function (input) {
        const wrapper = document.createElement("div");
        wrapper.className = "password-input-group w-100";
        
        input.parentNode.insertBefore(wrapper, input);
        wrapper.appendChild(input);
        
        const toggleBtn = document.createElement("button");
        toggleBtn.type = "button";
        toggleBtn.className = "password-toggle-btn";
        toggleBtn.innerHTML = '<i class="bi bi-eye"></i>';
        toggleBtn.setAttribute("aria-label", "Mostrar senha");
        wrapper.appendChild(toggleBtn);
        
        toggleBtn.addEventListener("click", function () {
            const isPassword = input.type === "password";
            input.type = isPassword ? "text" : "password";
            toggleBtn.innerHTML = isPassword ? '<i class="bi bi-eye-slash"></i>' : '<i class="bi bi-eye"></i>';
            toggleBtn.setAttribute("aria-label", isPassword ? "Ocultar senha" : "Mostrar senha");
        });
    });
});
