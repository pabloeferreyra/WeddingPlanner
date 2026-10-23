(() => {
    'use strict';

    const ModalUtils = window.ModalUtils = window.ModalUtils || {};

    ModalUtils.options = Object.assign({
        defaultModalId: 'GlobalModal',
        modalOptions: { backdrop: 'static' },
        getBodyElement: id => document.getElementById(`${id}-body`),
        getTitleElement: id => document.getElementById(`${id}-title`),
        getModalInstance: null,
        shownEvent: 'shown.bs.modal',
        tokenSelector: 'input[name="__RequestVerificationToken"]',
        tokenHeader: 'RequestVerificationToken',
        requireToken: true
    }, ModalUtils.options || {});

    ModalUtils.configure = function (options = {}) {
        this.options = {
            ...this.options,
            ...options,
            modalOptions: {
                ...this.options.modalOptions,
                ...(options.modalOptions || {})
            }
        };
        return this;
    };

    ModalUtils.notify = function (type, message, show = false) {
        if (typeof window.AppUtils?.showToast === 'function') {
            window.AppUtils.showToast(type, message, show);
        } else {
            const log = type === 'error' ? console.error : console.info;
            log.call(console, message);
        }
    };

    ModalUtils.load = async function (modalId, url, title = null) {
        if (!modalId || !url) throw new TypeError('ModalUtils.load requiere modalId y url.');
        if (!document.getElementById(modalId)) throw new Error(`Modal no encontrado: ${modalId}`);

        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error(`Error cargando el modal (${response.status}).`);
            this.update(modalId, { html: await response.text(), title });
            this.open(modalId);
            return true;
        } catch (error) {
            console.error('Error cargando modal:', error);
            this.notify('error', error.message);
            return false;
        }
    };

    ModalUtils.getModal = function (modalId) {
        const element = document.getElementById(modalId);
        if (!element) throw new Error(`Modal no encontrado: ${modalId}`);

        if (typeof this.options.getModalInstance === 'function') {
            return this.options.getModalInstance(element, modalId);
        }

        const Modal = window.bootstrap?.Modal;
        if (!Modal) throw new Error('Configura un adaptador de modal o carga Bootstrap Modal.');

        return Modal.getInstance(element)
            || new Modal(element, this.options.modalOptions);
    };

    ModalUtils.open = function (modalId) {
        const element = document.getElementById(modalId);
        const modal = this.getModal(modalId);

        if (!element || typeof modal.show !== 'function') {
            throw new Error(`No se pudo abrir el modal: ${modalId}`);
        }

        if (!element.dataset.modalShownHookAttached) {
            element.addEventListener(this.options.shownEvent, () => {
                document.dispatchEvent(new CustomEvent('modal:shown', { detail: { modalId } }));
            });
            element.dataset.modalShownHookAttached = 'true';
        }

        modal.show();
    };

    ModalUtils.close = function (modalId) {
        const modal = this.getModal(modalId);
        if (typeof modal.hide !== 'function') throw new Error(`No se pudo cerrar el modal: ${modalId}`);
        modal.hide();
    };

    ModalUtils.update = function (modalId, { html = null, title = null } = {}) {
        const body = this.options.getBodyElement(modalId);
        const titleElement = this.options.getTitleElement(modalId);
        if (!body) throw new Error(`No se encontró el contenido del modal: ${modalId}`);

        if (html !== null) body.innerHTML = html;
        if (title !== null && titleElement) titleElement.textContent = title;

        document.dispatchEvent(new CustomEvent('modal:updated', { detail: { modalId } }));
    };

    ModalUtils.submitForm = async function (
        modalId,
        formId,
        url,
        method = 'POST',
        title = null,
        showSuccess = true,
        update = false,
        options = {}
    ) {
        const form = document.getElementById(formId);
        if (!form) throw new Error(`Formulario no encontrado: ${formId}`);

        const requestMethod = method.toUpperCase();
        const formData = new FormData(form);
        form.querySelectorAll('input[type="checkbox"][name]').forEach(checkbox => {
            formData.set(checkbox.name, checkbox.checked ? 'true' : 'false');
        });

        const tokenSelector = options.tokenSelector ?? this.options.tokenSelector;
        const tokenHeader = options.tokenHeader ?? this.options.tokenHeader;
        const requiresToken = options.requireToken ?? this.options.requireToken;
        const isUnsafeMethod = !['GET', 'HEAD', 'OPTIONS'].includes(requestMethod);
        const token = tokenSelector ? form.querySelector(tokenSelector)?.value : null;

        try {
            if (isUnsafeMethod && requiresToken && !token) {
                throw new Error(`No se encontró el token antifalsificación (${tokenSelector}).`);
            }

            const headers = new Headers(options.headers || {});
            if (isUnsafeMethod && token) headers.set(tokenHeader, token);

            const response = await fetch(url, {
                method: requestMethod,
                headers,
                body: isUnsafeMethod ? formData : undefined
            });

            if (!response.ok) {
                const responseText = await response.text();
                let message = responseText || `Error guardando los datos (${response.status}).`;
                try {
                    const errorData = JSON.parse(responseText);
                    message = errorData.error || errorData.title || message;
                } catch {
                    // Keep the response text when the server does not return JSON.
                }
                throw new Error(message);
            }

            const html = await response.text();
            if (update) this.update(modalId, { html, title });
            if (showSuccess) this.notify('success', options.successMessage || 'Guardado correctamente');
            if (!update) this.close(modalId);
            return true;
        } catch (error) {
            console.error('Error guardando el formulario:', error);
            this.notify('error', error.message, true);
            return false;
        }
    };
})();
