(() => {
    'use strict';

const AppUtils = window.AppUtils = window.AppUtils || {};
AppUtils.Pagination = AppUtils.Pagination || {};
AppUtils.Date = AppUtils.Date || {};

AppUtils.escapeHtml = function (s) {
    return s == null ? '' : String(s)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
};

AppUtils.TabLoader = {
    /**
     * Initializes a tab with optional pagination and lazy-loaded HTML content.
     *
     * @param {Object} opts
     * @param {string} opts.tabSelector - CSS selector for the tab element
     * @param {string} opts.tableSelector - CSS selector identifying whether its content is loaded
     * @param {Object} opts.paginationOpts - Optional options for AppUtils.Pagination.init
     * @param {Function} opts.loadData - Callback invoked on tab activation and initial load
     * @param {string} opts.tabListSelector - Selector for sibling tabs; defaults to the nearest .nav
     * @param {string} opts.contentSelector - Selector for the HTML content container
     */
    init({
        tabSelector,
        tableSelector,
        paginationOpts,
        loadData,
        tabListSelector,
        contentSelector = '[data-tab-content]',
        activeClass = 'active',
        onError
    }) {
        if (!tabSelector) throw new TypeError('TabLoader.init: tabSelector es obligatorio.');

        if (paginationOpts?.key) {
            AppUtils.Pagination.init(paginationOpts.key, {
                defaultPageSize: paginationOpts.defaultPageSize || 25,
                defaultOrder: paginationOpts.defaultOrder || { column: 0, dir: 'asc' },
                pageSizeSelector: paginationOpts.pageSizeSelector,
                onChange: loadData
            });
        }

        const tab = document.querySelector(tabSelector);
        if (tab) {
            tab.addEventListener('click', async () => {
                const tabList = tabListSelector
                    ? document.querySelector(tabListSelector)
                    : tab.closest('.nav');
                tabList?.querySelectorAll('.nav-link')
                    .forEach(link => link.classList.remove(activeClass));
                tab.classList.add(activeClass);

                const url = tab.dataset.url;
                if (!url) return;

                const container = document.querySelector(contentSelector);
                try {
                    if (container && tableSelector && !container.querySelector(tableSelector)) {
                        const response = await fetch(url);
                        if (!response.ok) {
                            throw new Error(`No se pudo cargar el contenido (${response.status}).`);
                        }
                        container.innerHTML = await response.text();
                    }
                    loadData?.();
                } catch (error) {
                    if (onError) onError(error);
                    else console.error('Error cargando contenido de pestaña:', error);
                }
            });
        }

        loadData?.();
    }
};

AppUtils.ConfirmDelete = {
    /**
     * Sets up inline delete confirmation for a table.
     * @param {Object} opts
     * @param {string} opts.deleteBtnSelector - CSS selector for the delete button
     * @param {string} opts.confirmBtnSelector - CSS selector for confirm button
     * @param {string} opts.cancelBtnSelector - CSS selector for cancel button
     * @param {string} opts.deleteUrlPrefix - URL prefix for the DELETE request
     * @param {string} opts.successMessage - Toast message on success
     * @param {string} opts.errorMessage - Toast message on error
     * @param {Function} opts.loadData - Function to reload data after delete/cancel
     */
    init({
        deleteBtnSelector,
        confirmBtnSelector,
        cancelBtnSelector,
        deleteUrlPrefix,
        successMessage,
        errorMessage,
        loadData,
        confirmLabel = 'Sí',
        cancelLabel = 'No'
    }) {
        if (!deleteBtnSelector || !confirmBtnSelector || !cancelBtnSelector) {
            throw new TypeError('ConfirmDelete.init: se requieren selectores para los tres botones.');
        }

        const selectorClass = selector =>
            /^\.[A-Za-z_][\w-]*$/.test(selector || '') ? selector.slice(1) : '';

        document.addEventListener('click', async ev => {
            const target = ev.target instanceof Element ? ev.target.closest('button, a') : null;
            if (!target) return;

            if (target.matches(deleteBtnSelector)) {
                const id = target.dataset.id;
                const cell = target.closest('td');
                if (!id || !cell) return;
                target.classList.add('btn-fade-out');
                setTimeout(() => {
                    const confirmButton = document.createElement('button');
                    confirmButton.type = 'button';
                    confirmButton.dataset.id = id;
                    confirmButton.className = `btn btn-danger btn-sm me-1 ${selectorClass(confirmBtnSelector)}`;
                    confirmButton.textContent = confirmLabel;

                    const cancelButton = document.createElement('button');
                    cancelButton.type = 'button';
                    cancelButton.className = `btn btn-secondary btn-sm ${selectorClass(cancelBtnSelector)}`;
                    cancelButton.textContent = cancelLabel;

                    cell.replaceChildren(confirmButton, cancelButton);
                }, 200);
                return;
            }

            if (target.matches(cancelBtnSelector)) {
                loadData?.();
                return;
            }

            if (target.matches(confirmBtnSelector)) {
                const id = target.dataset.id;
                if (!id || !deleteUrlPrefix) return;
                try {
                    const response = await fetch(`${deleteUrlPrefix}${encodeURIComponent(id)}`, { method: 'DELETE' });
                    if (!response.ok) throw new Error(`Delete failed (${response.status}).`);
                    AppUtils.showToast('success', successMessage);
                } catch (error) {
                    console.error('Error eliminando el registro:', error);
                    AppUtils.showToast('error', errorMessage);
                }
                loadData?.();
                return;
            }
        });
    }
};

AppUtils.todayString = function () {
    const d = new Date();
    const yyyy = d.getFullYear();
    const mm = (d.getMonth() + 1).toString().padStart(2, '0');
    const dd = d.getDate().toString().padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
};

AppUtils.bindDateValidation = function (input, button, options = {}) {
    input = input?.jquery ? input[0] : input;
    button = button?.jquery ? button[0] : button;
    if (!input) return;

    const cfg = Object.assign({
        minToday: false,
        blockWeekends: false
    }, options);

    const safeDisable = state => {
        if (button) button.disabled = state;
    };

    if (cfg.minToday) input.min = AppUtils.todayString();
    else input.removeAttribute('min');

    input.addEventListener('change', validateDate);
    input.addEventListener('input', validateDate);

    function validateDate() {
        const val = input.value;
        if (!val) return;

        const d = new Date(val + 'T00:00:00');
        const today = new Date(); today.setHours(0, 0, 0, 0);
        if (Number.isNaN(d.getTime())) {
            safeDisable(true);
            return;
        }

        if (cfg.minToday && d < today) {
            AppUtils.showToast('info', cfg.pastMessage || 'No puede seleccionar fechas anteriores a hoy.');
            input.value = AppUtils.todayString();
            safeDisable(false);
            return;
        }

        if (cfg.blockWeekends && (d.getDay() === 0 || d.getDay() === 6)) {
            AppUtils.showToast('info', cfg.weekendMessage || 'No puede seleccionar fines de semana.');
            safeDisable(true);
            return;
        }

        safeDisable(false);
    }
};

AppUtils.paginateTable = function (paginationSelector, recordsTotal, pageSize, currentPage, onPageClick) {
    const pagination = document.querySelector(paginationSelector);
    if (!pagination) return;

    pagination.replaceChildren();
    if (!recordsTotal || pageSize <= 0) return;

    const totalPages = Math.max(1, Math.ceil(recordsTotal / pageSize));
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    const append = (p, text, active) => {
        const li = document.createElement('li');
        li.className = `page-item${active ? ' active' : ''}`;
        const link = document.createElement('a');
        link.href = '#';
        link.className = 'page-link';
        link.textContent = text;
        link.addEventListener('click', e => {
            e.preventDefault();
            if (p >= 1 && typeof onPageClick === 'function') onPageClick(p);
        });
        li.append(link);
        pagination.append(li);
    };

    if (currentPage > 1) append(currentPage - 1, 'Anterior', false);
    for (let p = startPage; p <= endPage; p++) append(p, p, p === currentPage);
    if (currentPage < totalPages) append(currentPage + 1, 'Siguiente', false);
};

AppUtils.reloadTable = function (url, payload, successCallback) {
    if (!url) throw new Error('reloadTable: URL no especificada');
    const body = new URLSearchParams();
    Object.entries(payload || {}).forEach(([key, value]) => {
        if (value !== undefined && value !== null) body.append(key, value);
    });

    return fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' },
        body
    })
        .then(response => {
            if (!response.ok) throw new Error(`Error cargando datos (${response.status}).`);
            return response.json();
        })
        .then(data => {
            if (typeof successCallback === 'function') successCallback(data);
            return data;
        })
        .catch(error => {
            console.error('Error recargando tabla:', error);
            AppUtils.showToast('error', 'Error cargando datos');
            throw error;
        });
};

AppUtils.showToast = function (icon, title, show = false, ms = 1200) {
    if (window.Swal?.fire) {
        return window.Swal.fire({
            position: 'top-end',
            icon,
            title,
            showConfirmButton: show,
            timer: ms
        });
    }

    document.dispatchEvent(new CustomEvent('apputils:toast', {
        detail: { icon, title, show, duration: ms }
    }));
    console.info(`[${icon}] ${title}`);
};

AppUtils.isPastISODate = function (iso) {
    const [Y, M, D] = iso.split("-").map(Number);
    const d = new Date(Y, M - 1, D, 0, 0, 0);

    const now = new Date();
    const t = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 0, 0, 0);

    return d < t;
};

AppUtils.validateField = function (selector) {
    if (typeof window.FormValidationRules === 'object' && window.FormValidationRules !== null) {
        const fn = window.FormValidationRules[selector];
        if (!fn) return true;
        const el = document.querySelector(selector);
        if (!el) return true;
        return fn(el, { err: msg => AppUtils.showToast("info", msg) });
    }
    return true;
};

AppUtils.validateAll = function () {
    if (typeof window.FormValidationRules === 'object' && window.FormValidationRules !== null) {
        for (const selector in window.FormValidationRules) {
            if (!AppUtils.validateField(selector)) return false;
        }
        return true;
    }
    return true;
};

AppUtils.Sort = {
    parseDateString(s) {
        if (!s) return null;
        s = s.trim();
        let iso = /^\d{4}-\d{2}-\d{2}/;
        if (iso.test(s)) {
            let d = new Date(s);
            return isNaN(d) ? null : d;
        }
        let dm = /^(\d{1,2})\/(\d{1,2})\/(\d{4})$/;
        let m = s.match(dm);
        if (m) {
            let day = parseInt(m[1], 10);
            let month = parseInt(m[2], 10) - 1;
            let year = parseInt(m[3], 10);
            let d = new Date(year, month, day);
            return isNaN(d) ? null : d;
        }
        let d = new Date(s);
        return isNaN(d) ? null : d;
    },

    normalizeValue(val) {
        if (val === null || val === undefined) return { type: 'string', value: '' };
        val = String(val).trim();
        if (val === '') return { type: 'string', value: '' };

        let num = val.replace(/\s/g, '').replace(/\./g, '').replace(',', '.');
        if (/^-?\d+(\.\d+)?$/.test(num)) {
            let n = parseFloat(num);
            if (!isNaN(n)) return { type: 'number', value: n };
        }

        const d = this.parseDateString(val);
        if (d) return { type: 'date', value: d.getTime() };

        return { type: 'string', value: val.toLowerCase() };
    },

    compareValues(a, b) {
        if (a.type === b.type) {
            if (a.value < b.value) return -1;
            if (a.value > b.value) return 1;
            return 0;
        }
        return String(a.value).localeCompare(String(b.value));
    },

    sortTableByColumn(tableSelector, columnIndex, ascending) {
        let tbody = document.querySelector(tableSelector + ' tbody');
        if (!tbody) return;
        let rows = Array.from(tbody.querySelectorAll('tr'));
        let stabilized = rows.map((row, idx) => {
            let raw = row.children[columnIndex]?.textContent.trim() ?? '';
            let norm = this.normalizeValue(raw);
            return { row: row, norm: norm, idx: idx };
        });

        stabilized.sort((a, b) => {
            let cmp = this.compareValues(a.norm, b.norm);
            if (cmp === 0) return a.idx - b.idx;
            return ascending ? cmp : -cmp;
        });

        let fragment = document.createDocumentFragment();
        stabilized.forEach(item => fragment.appendChild(item.row));
        tbody.appendChild(fragment);
    },

    attachHeaderSorting(tableSelector) {

        var sortState = { index: null, asc: true };

        function headerSortHandler(e) {
            var btn = e.currentTarget;
            let index = parseInt(btn.getAttribute('data-index'), 10);
            if (isNaN(index)) return;

            if (sortState.index === index) {
                sortState.asc = !sortState.asc;
            } else {
                sortState.index = index;
                sortState.asc = true;
            }

            let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
            buttons.forEach(function (b) {
                b.classList.remove('active');
                b.setAttribute('aria-sort', 'none');
                let ind = b.querySelector('.sort-indicator');
                if (ind) ind.textContent = '';
            });

            btn.classList.add('active');
            btn.setAttribute('aria-sort', sortState.asc ? 'ascending' : 'descending');
            let ind = btn.querySelector('.sort-indicator');
            if (!ind) {
                ind = document.createElement('span');
                ind.className = 'sort-indicator';
                btn.appendChild(ind);
            }
            ind.textContent = sortState.asc ? '▲' : '▼';

            AppUtils.Sort.sortTableByColumn(tableSelector, index, sortState.asc);
        }

        // set listeners
        let buttons = document.querySelectorAll(tableSelector + ' thead .sort-btn');
        buttons.forEach(function (btn) {
            btn.removeEventListener('click', headerSortHandler);
            btn.addEventListener('click', headerSortHandler);
        });
    },

    getOrder(tableSelector, columnsMap) {
        // busca el botón con class .sort-btn.active dentro de thead
        const btn = document.querySelector(`${tableSelector} thead .sort-btn.active`);
        if (!btn) return { index: 0, asc: true }; // default

        const idx = parseInt(btn.getAttribute('data-index'), 10);
        const asc = btn.getAttribute('aria-sort') !== 'descending';

        // por si un botón no coincide con el columnsMap real
        const colIndex = isNaN(idx) ? 0 : idx;
        return { index: colIndex, asc };
    }
};

AppUtils.Pagination.init = function (key, opts = {}) {

    AppUtils._tables = AppUtils._tables || {};
    AppUtils._tables[key] = AppUtils._tables[key] || {
        currentPage: 1,
        pageSize: 25,
        recordsTotal: 0,
        order: { column: 0, dir: 'asc' }
    };

    const st = AppUtils._tables[key];

    if (opts.defaultPageSize != null) {
        st.pageSize = parseInt(opts.defaultPageSize, 10) || st.pageSize;
    }

    if (opts.defaultOrder) {
        st.order = {
            column: Number(opts.defaultOrder.column) || 0,
            dir: (opts.defaultOrder.dir || 'asc') === 'desc' ? 'desc' : 'asc'
        };
    }

    if (opts.pageSizeSelector) {
        const sel = opts.pageSizeSelector;
        document.addEventListener("change", (e) => {
            if (!e.target.matches(sel)) return;
            const v = parseInt(e.target.value, 10);
            st.pageSize = isNaN(v) ? st.pageSize : v;
            st.currentPage = 1;
            opts.onChange?.();
        });
    }
};

AppUtils.Pagination.getState = function (key) {
    AppUtils._tables = AppUtils._tables || {};
    return AppUtils._tables[key];
};

AppUtils.Pagination.getOrder = function (key) {
    const st = AppUtils.Pagination.getState(key);
    return st?.order || { column: 0, dir: 'asc' };
};

AppUtils.Pagination.setRecordsTotal = function (key, total) {
    const st = AppUtils.Pagination.getState(key);
    if (st) {
        st.recordsTotal = Number(total) || 0;
    }
};

AppUtils.Pagination.goTo = function (key, page, onChange) {
    const st = AppUtils.Pagination.getState(key);
    if (!st) return;
    st.currentPage = Math.max(1, Number(page) || 1);
    onChange?.();
};

AppUtils.Pagination.renderWithState = function (selector, key, onPageChange) {

    const container = document.querySelector(selector);
    if (!container) return;

    const st = AppUtils.Pagination.getState(key);
    if (!st) return;

    const totalPages = Math.max(1, Math.ceil(st.recordsTotal / st.pageSize));
    const current = st.currentPage;

    const MAX_VISIBLE = 7;
    const half = Math.floor(MAX_VISIBLE / 2);

    container.innerHTML = "";

    const ul = document.createElement("ul");
    ul.className = "pagination justify-content-center";

    function add(label, page, disabled = false, active = false) {
        const li = document.createElement("li");
        li.className = "page-item"
            + (disabled ? " disabled" : "")
            + (active ? " active" : "");

        const a = document.createElement("a");
        a.className = "page-link";
        a.href = "#";
        a.textContent = label;

        if (!disabled && typeof page === "number") {
            a.addEventListener("click", e => {
                e.preventDefault();
                AppUtils.Pagination.goTo(key, page, onPageChange);
            });
        }

        li.appendChild(a);
        ul.appendChild(li);
    }

    add("«", current - 1, current === 1);

    let start = Math.max(1, current - half);
    let end = Math.min(totalPages, current + half);

    if (end - start + 1 < MAX_VISIBLE) {
        if (start === 1) {
            end = Math.min(totalPages, start + MAX_VISIBLE - 1);
        } else if (end === totalPages) {
            start = Math.max(1, end - MAX_VISIBLE + 1);
        }
    }

    if (start > 1) {
        add(1, 1);
        if (start > 2) add("…", null, true);
    }

    for (let i = start; i <= end; i++) {
        add(i, i, false, i === current);
    }

    if (end < totalPages) {
        if (end < totalPages - 1) add("…", null, true);
        add(totalPages, totalPages);
    }

    // »
    add("»", current + 1, current === totalPages);

    container.appendChild(ul);
};


AppUtils.Pagination.attach = function (
    paginationSelector,
    loadFunc,
    getPageFunc,
    getPageSizeFunc,
    getRecordsTotalFunc,
    setPageFunc
) {
    const pagination = document.querySelector(paginationSelector);
    if (!pagination) return;

    pagination.replaceChildren();

    const currentPage = getPageFunc();
    const pageSize = getPageSizeFunc();
    const recordsTotal = getRecordsTotalFunc();

    if (pageSize <= 0) return;

    const totalPages = Math.max(1, Math.ceil(recordsTotal / pageSize));
    const startPage = Math.max(1, currentPage - 2);
    const endPage = Math.min(totalPages, currentPage + 2);

    function addPage(p, label, active) {
        const li = document.createElement('li');
        li.className = `page-item${active ? ' active' : ''}`;
        const link = document.createElement('a');
        link.href = '#';
        link.className = 'page-link';
        link.textContent = label;
        link.addEventListener('click', e => {
            e.preventDefault();
            if (p >= 1 && p <= totalPages) {
                setPageFunc?.(p);
                loadFunc?.();
            }
        });

        li.append(link);
        pagination.append(li);
    }

    if (currentPage > 1) addPage(currentPage - 1, 'Anterior', false);

    for (let p = startPage; p <= endPage; p++) {
        addPage(p, p, p === currentPage);
    }

    if (currentPage < totalPages) addPage(currentPage + 1, 'Siguiente', false);
};

AppUtils.ddmmyyyy_to_iso = function (s) {
    if (!s) return null;
    if (s.includes("-")) {
        s = s.split("-")[0];
    }
    if (s.includes("-")) {
        return s;
    }
    const [d, m, y] = s.split("/");
    return `${y}-${m}-${d}`;
}

AppUtils.toDisplayDate = function (raw, locale = window.navigator.language) {
    if (!raw) return "";
    const [y, m, d] = raw.split('-').map(Number);
    return new Date(y, m - 1, d).toLocaleDateString(locale);
}

AppUtils.initFlatpickr = function (selector, opts) {

    if (!window.flatpickr) {
        console.warn("flatpickr no está cargado aún");
        return;
    }

    const el = typeof selector === "string"
        ? document.querySelector(selector)
        : selector;

    if (!el || el._flatpickr) return;

    const base = {
        dateFormat: "Y-m-d",
        allowInput: true,
        disableMobile: false,
        altInput: true,
        altFormat: "d/m/Y"
    };
    Object.assign(base, opts?.flatpickrOptions || {});

    if (opts?.maxToday === true) {
        base.maxDate = new Date();
    }

    if (opts?.minToday === true) {
        base.minDate = new Date();
    }

    if (opts?.blockSundays === true) {
        base.disable = [
            date => date.getDay() === 0
        ];
    }

    if (opts?.defaultToday === true) {
        base.defaultDate = new Date();
    }

    window.    window.flatpickr(el, base);
};

AppUtils.FormValidationRules = function (submitSelector, rules) {
    const submit = document.querySelector(submitSelector);

    function evalRule(val, ruleKey, ruleValue) {

        if (ruleKey === 'required') {
            if (ruleValue && (!val || !String(val).trim())) return false;
        }

        if (ruleKey === 'minLength') {
            if (val.length < ruleValue) return false;
        }

        return true;
    }

    function validateOne(id) {
        const el = document.getElementById(id);
        if (!el) return true;

        const val = String(el.value || '').trim();
        const fieldRules = rules[id] || {};

        for (let rk in fieldRules) {
            if (!evalRule(val, rk, fieldRules[rk])) return false;
        }
        return true;
    }

    function validateAll() {
        let ok = true;
        for (let id in rules) {
            if (!validateOne(id)) ok = false;
        }
        if (submit) submit.disabled = !ok;
    }

    Object.keys(rules).forEach(id => {
        const input = document.getElementById(id);
        input?.addEventListener('input', validateAll);
        input?.addEventListener('change', validateAll);
        input?.addEventListener('blur', validateAll);
    });

    validateAll();
};

document.addEventListener("click", (e) => {
    const btn = e.target instanceof Element ? e.target.closest("[data-open-modal]") : null;
    if (!btn) return;

    const detail = {
        modalId: btn.dataset.modalId,
        url: btn.dataset.url,
        title: btn.dataset.title || null
    };
    if (typeof ModalUtils !== 'undefined' && typeof ModalUtils.load === 'function') {
        ModalUtils.load(detail.modalId, detail.url, detail.title);
    }
    else document.dispatchEvent(new CustomEvent('apputils:modal-open', { detail }));
});

document.addEventListener("click", (e) => {
    const btn = e.target instanceof Element ? e.target.closest("[data-close-modal]") : null;
    if (!btn) return;

    const modalId = btn.dataset.modalId
        || (typeof ModalUtils !== 'undefined' ? ModalUtils.options?.defaultModalId : null)
        || 'GlobalModal';
    const detail = { modalId };
    if (detail.modalId && typeof ModalUtils !== 'undefined' && typeof ModalUtils.close === 'function') {
        ModalUtils.close(detail.modalId);
    }
    else document.dispatchEvent(new CustomEvent('apputils:modal-close', { detail }));
});

AppUtils.Export = {

    async post({
        url,
        params,
        filename,
        tokenSelector = 'input[name="__RequestVerificationToken"]',
        tokenFieldName = '__RequestVerificationToken'
    }) {
        if (!url) throw new Error("Export.post: url requerida");

        const token = tokenSelector ? document.querySelector(tokenSelector)?.value : null;

        const form = new URLSearchParams();

        if (token) {
            form.append(tokenFieldName, token);
        }

        const data = (typeof params === "function")
            ? params()
            : (params || {});

        Object.entries(data).forEach(([k, v]) => {
            if (v !== undefined && v !== null && v !== "") {
                form.append(k, v);
            }
        });

        const res = await fetch(url, {
            method: "POST",
            body: form
        });

        if (!res.ok) {
            AppUtils.showToast("error", "Error exportando datos");
            throw new Error(`Error exportando datos (${res.status}).`);
        }

        const blob = await res.blob();
        const blobUrl = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = blobUrl;
        a.download = filename || "export";
        document.body.appendChild(a);
        a.click();
        a.remove();

        setTimeout(() => URL.revokeObjectURL(blobUrl), 0);
    }
};

AppUtils.Date.ddMMyyyy = function (date = new Date()) {
    const d = String(date.getDate()).padStart(2, '0');
    const m = String(date.getMonth() + 1).padStart(2, '0');
    return `${d}${m}${date.getFullYear()}`;
};
})();
