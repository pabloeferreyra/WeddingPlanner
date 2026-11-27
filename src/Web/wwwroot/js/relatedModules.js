(() => {
    'use strict';

    function updateFormEnhancements(container) {
        container.querySelectorAll('form.planning-form').forEach(form => {
            const recurringToggle = form.querySelector('[data-recurring-toggle]');
            const recurringFields = form.querySelector('[data-recurring-fields]');
            if (recurringToggle && recurringFields) {
                recurringFields.hidden = !recurringToggle.checked;
            }

            updateCompanionFields(form);
        });
    }

    function updateCompanionFields(form) {
        const toggle = form.querySelector('[data-companion-toggle]');
        const fields = form.querySelector('[data-companion-fields]');
        if (!toggle || !fields) return;

        fields.hidden = !toggle.checked;
        toggle.setAttribute('aria-expanded', String(toggle.checked));

        if (!toggle.checked) {
            const status = fields.querySelector('[data-companion-status]');
            if (status) status.value = status.dataset.pendingValue;
        }
    }

    function associateValidationErrors(container) {
        const fields = Array.from(container.querySelectorAll('input, select, textarea'));
        container.querySelectorAll('[data-valmsg-for]').forEach(error => {
            if (!error.id || !error.textContent.trim()) return;

            const fieldName = error.getAttribute('data-valmsg-for');
            const field = fields.find(candidate => candidate.name === fieldName);
            if (!field) return;

            field.setAttribute('aria-invalid', 'true');
            const describedBy = new Set((field.getAttribute('aria-describedby') || '').split(/\s+/).filter(Boolean));
            describedBy.add(error.id);
            field.setAttribute('aria-describedby', Array.from(describedBy).join(' '));
        });
    }

    document.addEventListener('change', event => {
        const toggle = event.target;
        if (!(toggle instanceof HTMLInputElement)) return;

        if (toggle.matches('[data-recurring-toggle]')) {
            const fields = toggle.form?.querySelector('[data-recurring-fields]');
            if (fields) fields.hidden = !toggle.checked;
        }

        if (toggle.matches('[data-companion-toggle]') && toggle.form) {
            updateCompanionFields(toggle.form);
        }
    });

    document.addEventListener('submit', async event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.closest('#GlobalModal-body')) return;

        event.preventDefault();
        const submitButton = event.submitter;
        if (submitButton) submitButton.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: form.method || 'POST',
                body: new FormData(form)
            });
            if (response.redirected) {
                window.location.assign(response.url);
                return;
            }
            if (!response.ok) throw new Error((response.status === 413 || response.status === 400) && form.querySelector('input[type="file"]')
                ? 'No se pudo procesar el formulario. Comprueba que el archivo no supere 10 MiB y vuelve a cargar la página.'
                : `No se pudo guardar la información (${response.status}).`);

            const modalBody = form.closest('#GlobalModal-body');
            modalBody.innerHTML = await response.text();
            updateFormEnhancements(modalBody);
            associateValidationErrors(modalBody);

            const invalidField = modalBody.querySelector(
                'input.input-validation-error, select.input-validation-error, textarea.input-validation-error, ' +
                'input[aria-invalid="true"], select[aria-invalid="true"], textarea[aria-invalid="true"]'
            );
            const errorSummary = modalBody.querySelector('.validation-summary-errors');
            const focusTarget = invalidField || errorSummary;
            if (focusTarget instanceof HTMLElement) {
                if (focusTarget === errorSummary) focusTarget.tabIndex = -1;
                focusTarget.focus({ preventScroll: true });
                focusTarget.scrollIntoView({ block: 'nearest' });
            }
        } catch (error) {
            console.error('Error guardando la información:', error);
            window.AppUtils?.showToast(
                'error',
                error instanceof Error ? error.message : 'No se pudo guardar la información.',
                true
            );
        } finally {
            if (submitButton?.isConnected) submitButton.disabled = false;
        }
    });

    document.addEventListener('submit', event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || form.closest('#GlobalModal-body')) return;
        if (!form.action.includes('/Planning/') && !form.action.includes('/Documents/')) return;

        if (!window.confirm('¿Confirmas esta acción?')) event.preventDefault();
    });
})();
