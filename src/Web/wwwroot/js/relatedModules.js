(() => {
    'use strict';

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
            if (!response.ok) throw new Error(`No se pudo guardar la información (${response.status}).`);

            document.querySelector('#GlobalModal-body').innerHTML = await response.text();
        } catch (error) {
            console.error('Error guardando la información:', error);
            window.AppUtils?.showToast('error', error.message, true);
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
