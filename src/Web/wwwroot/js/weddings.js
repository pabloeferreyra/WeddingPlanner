(() => {
    'use strict';
    const key = "weddings";
    let searchTimer = null;
    let loadController = null;
    let dateFilter = "";
    let statusFilter = "";

    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: 'asc' },
            onChange: loadData
        });

        document.addEventListener("submit", submitWeddingForm);
        document.addEventListener("click", deleteWedding);
        document.addEventListener("change", toggleWeddingFields);
        bindFilters();

        loadData();

        document.dispatchEvent(new Event("weddings:loaded"));
    });

    async function loadData() {
        loadController?.abort();
        const controller = new AbortController();
        loadController = controller;

        const st = AppUtils.Pagination.getState(key);
        const order = AppUtils.Pagination.getOrder(key);

        const start = (st.currentPage - 1) * st.pageSize;
        const length = st.pageSize;

        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || "";

        const form = new URLSearchParams();

        form.append("__RequestVerificationToken", token);
        form.append("draw", 1);
        form.append("start", start);
        form.append("length", length);
        form.append("order[0][column]", order.column);
        form.append("order[0][dir]", order.dir);
        form.append("search", document.querySelector("#weddingSearch")?.value.trim() || "");
        form.append("status", statusFilter);
        form.append("date", dateFilter);

        const columnsMap = ['Title', 'WeddingDate', 'Status'];
        columnsMap.forEach((name, i) => form.append(`columns[${i}][name]`, name));

        const table = document.querySelector("#weddingsTable");
        table?.setAttribute("aria-busy", "true");
        const resultCount = document.querySelector("#weddingsResultCount");
        if (resultCount) resultCount.textContent = "Cargando celebraciones...";
        const tbody = table?.querySelector("tbody");
        if (tbody && !tbody.children.length) {
            tbody.innerHTML = '<tr><td colspan="3"><div class="empty-state" role="status">Cargando celebraciones...</div></td></tr>';
        }

        try {
            const res = await fetch("/Weddings/InitializeTable", {
                method: "POST",
                body: form,
                signal: controller.signal
            });
            if (!res.ok) throw new Error(`Error del servidor (${res.status}).`);

            const json = await res.json();
            const recordsTotal = Number(json?.recordsTotal);
            const filtered = Number(json?.recordsFiltered);
            if (!Array.isArray(json?.data) ||
                !Number.isInteger(recordsTotal) || recordsTotal < 0 ||
                !Number.isInteger(filtered) || filtered < 0 ||
                !json.summary) {
                throw new Error("El servidor devolvió datos inesperados.");
            }
            if (controller !== loadController) return;

            AppUtils.Pagination.setRecordsTotal(key, filtered);
            updateSummary(json?.summary, filtered);
            renderTable(json.data);
            AppUtils.Pagination.renderWithState("#weddingPagination", key, loadData);
            updatePaginationSummary(filtered);

        } catch (err) {
            if (err instanceof Error && err.name === "AbortError") return;
            if (controller !== loadController) return;

            console.error("Error cargando celebraciones:", err);
            if (resultCount) resultCount.textContent = "No se pudieron cargar las celebraciones.";
            const currentTbody = table?.querySelector("tbody");
            if (currentTbody) {
                currentTbody.innerHTML =
                    '<tr><td colspan="3"><div class="empty-state" role="alert">' +
                    '<h3>No se pudieron cargar las celebraciones</h3>' +
                    '<p>Revisa tu conexión e inténtalo nuevamente.</p>' +
                    '<button type="button" class="btn btn-primary mt-3" data-retry-weddings>Reintentar</button>' +
                    '</div></td></tr>';
                currentTbody.querySelector("[data-retry-weddings]")?.addEventListener("click", loadData);
            }
            document.querySelector("#weddingPagination")?.replaceChildren();
            const paginationSummary = document.querySelector("#weddingPaginationSummary");
            if (paginationSummary) paginationSummary.textContent = "Resultados no disponibles.";
        } finally {
            if (controller === loadController) {
                loadController = null;
                table?.setAttribute("aria-busy", "false");
            }
        }
    }

    function renderTable(rows) {
        const tbody = document.querySelector("#weddingsTable tbody");
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!rows.length) {
            const hasFilters = Boolean(
                document.querySelector("#weddingSearch")?.value.trim() ||
                statusFilter ||
                dateFilter
            );
            const title = hasFilters ? "No encontramos celebraciones" : "Aún no hay celebraciones";
            const description = hasFilters
                ? "Prueba a cambiar o quitar los filtros."
                : "Crea una celebración para empezar a organizarla.";
            const button = hasFilters
                ? '<button type="button" class="btn btn-outline-secondary mt-3" data-clear-wedding-filters>Quitar filtros</button>'
                : '<button class="btn btn-primary mt-3" data-open-modal data-modal-id="GlobalModal" data-url="/Weddings/Create" data-title="Nueva celebración" type="button">Crear primer evento</button>';
            tbody.innerHTML = `<tr><td colspan="3"><div class="empty-state"><h3>${title}</h3><p>${description}</p>${button}</div></td></tr>`;
            tbody.querySelector("[data-clear-wedding-filters]")?.addEventListener("click", () => {
                statusFilter = "";
                dateFilter = "";
                const search = document.querySelector("#weddingSearch");
                if (search) search.value = "";
                document.querySelector("#weddingDateFilterLabel").textContent = "Fecha";
                document.querySelector("#weddingStatusFilterLabel").textContent = "Estado";
                const state = AppUtils.Pagination.getState(key);
                if (state) state.currentPage = 1;
                loadData();
            });
            return;
        }

        rows.forEach(d => {
            const status = statusDetails(d.status);
            tbody.insertAdjacentHTML("beforeend", `
                <tr class="celebration-card-row">
                    <th scope="row" class="celebration-card-info" data-label="Evento">
                        <strong>${escapeHTML(d.title)}</strong>
                        <span class="table-row-subtitle">${d.isWedding ? 'Celebración de boda' : 'Celebración especial'}</span>
                        <span class="celebration-card-meta">
                            <span>
                                <svg aria-hidden="true" viewBox="0 0 24 24" width="16" height="16">
                                    <rect x="3" y="5" width="18" height="16" rx="2" fill="none" stroke="currentColor" stroke-width="1.7"></rect>
                                    <path d="M7 3v4M17 3v4M3 10h18" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"></path>
                                </svg>
                                ${escapeHTML(formatDate(d.weddingDate))}
                            </span>
                            <span>
                                <svg aria-hidden="true" viewBox="0 0 24 24" width="16" height="16">
                                    <circle cx="9" cy="8" r="3" fill="none" stroke="currentColor" stroke-width="1.7"></circle>
                                    <path d="M3.5 19v-1.5a5.5 5.5 0 0 1 11 0V19M16 5.5a3 3 0 0 1 0 5.8M18 14a4.5 4.5 0 0 1 2.5 4v1" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round"></path>
                                </svg>
                                ${escapeHTML(d.expectedGuestCount)} invitados
                            </span>
                        </span>
                    </th>
                    <td class="celebration-card-status" data-label="Estado">
                        <span class="status-badge status-${status.className}">${status.label}</span>
                    </td>
                    <td class="celebration-card-actions" data-label="Acciones">
                        <div class="celebration-card-action-list">
                            <a href="/Weddings/${encodeURIComponent(d.id)}" class="btn btn-primary wedding-card-view">Ver evento</a>
                            <button class="wedding-icon-button"
                                    type="button"
                                    data-open-modal
                                    data-modal-id="GlobalModal"
                                    data-url="/Weddings/${encodeURIComponent(d.id)}/Edit"
                                    data-title="Editar celebración"
                                    aria-label="Editar ${escapeHTML(d.title)}"
                                    title="Editar celebración">
                                <svg aria-hidden="true" viewBox="0 0 24 24" width="17" height="17">
                                    <path d="m14 5 5 5M4 20l4.2-.8L19 8.4a2.1 2.1 0 0 0-3-3L5.2 16.2 4 20Z" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" stroke-linejoin="round"></path>
                                </svg>
                            </button>
                            <div class="dropdown">
                                <button class="wedding-icon-button"
                                        type="button"
                                        data-bs-toggle="dropdown"
                                        aria-expanded="false"
                                        aria-label="Más opciones para ${escapeHTML(d.title)}"
                                        title="Más opciones">
                                    <svg aria-hidden="true" viewBox="0 0 24 24" width="17" height="17">
                                        <circle cx="5" cy="12" r="1.2" fill="currentColor"></circle>
                                        <circle cx="12" cy="12" r="1.2" fill="currentColor"></circle>
                                        <circle cx="19" cy="12" r="1.2" fill="currentColor"></circle>
                                    </svg>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><a class="dropdown-item" href="/Weddings/${encodeURIComponent(d.id)}/Planning">Administrar planificación</a></li>
                                    <li><button type="button" data-id="${encodeURIComponent(d.id)}" class="dropdown-item btn-delete">Eliminar celebración</button></li>
                                </ul>
                            </div>
                        </div>
                    </td>
                </tr>
            `);
        });
    }

    function statusDetails(status) {
        const statuses = {
            1: { label: "En planificación", className: "planning" },
            2: { label: "Confirmada", className: "confirmed" },
            3: { label: "Finalizada", className: "completed" },
            4: { label: "Cancelada", className: "pending" }
        };
        return statuses[Number(status)] || statuses[1];
    }

    function updateSummary(summary, filtered) {
        if (!summary) return;
        const total = document.querySelector("#weddingsTotalMetric");
        const upcoming = document.querySelector("#weddingsUpcomingMetric");
        const completed = document.querySelector("#weddingsCompletedMetric");
        const resultCount = document.querySelector("#weddingsResultCount");
        if (total) total.textContent = summary.total;
        if (upcoming) upcoming.textContent = summary.upcoming;
        if (completed) completed.textContent = summary.completed;
        if (resultCount) {
            resultCount.textContent =
                `Mostrando ${filtered} celebración${filtered === 1 ? "" : "es"}`;
        }
    }

    function updatePaginationSummary(filtered) {
        const st = AppUtils.Pagination.getState(key);
        if (!st) return;
        const start = filtered === 0 ? 0 : (st.currentPage - 1) * st.pageSize + 1;
        const end = Math.min(st.currentPage * st.pageSize, filtered);
        const paginationSummary = document.querySelector("#weddingPaginationSummary");
        if (paginationSummary) {
            paginationSummary.textContent = `${start}-${end} de ${filtered} resultados`;
        }
    }

    function bindFilters() {
        document.addEventListener("click", event => {
            if (!event.target.closest(".filter-button, .filter-menu")) {
                document.querySelectorAll(".filter-menu").forEach(menu => menu.remove());
            }
        });

        const search = document.querySelector("#weddingSearch");
        search?.addEventListener("input", () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => {
                const st = AppUtils.Pagination.getState(key);
                if (st) st.currentPage = 1;
                loadData();
            }, 250);
        });

        setupFilterMenu("#weddingDateFilter", "#weddingDateFilterLabel", [
            { value: "", label: "Todas las fechas" },
            { value: "upcoming", label: "Próximas" },
            { value: "past", label: "Pasadas" }
        ], (value, label) => {
            dateFilter = value;
            document.querySelector("#weddingDateFilterLabel").textContent =
                value ? label : "Fecha";
        });

        setupFilterMenu("#weddingStatusFilter", "#weddingStatusFilterLabel", [
            { value: "", label: "Todos los estados" },
            { value: "Planning", label: "En planificación" },
            { value: "Confirmed", label: "Confirmada" },
            { value: "Completed", label: "Finalizada" },
            { value: "Cancelled", label: "Cancelada" }
        ], (value, label) => {
            statusFilter = value;
            document.querySelector("#weddingStatusFilterLabel").textContent =
                value ? label : "Estado";
        });

        document.querySelector("#weddingSortButton")?.addEventListener("click", () => {
            const order = AppUtils.Pagination.getOrder(key);
            AppUtils.Pagination.getState(key).order = {
                column: 1,
                dir: order.dir === "asc" ? "desc" : "asc"
            };
            loadData();
        });
    }

    function setupFilterMenu(buttonSelector, labelSelector, options, onSelect) {
        const button = document.querySelector(buttonSelector);
        if (!button) return;

        button.addEventListener("click", event => {
            event.stopPropagation();
            document.querySelectorAll(".filter-menu").forEach(menu => {
                if (menu !== button.nextElementSibling) menu.remove();
            });
            if (button.nextElementSibling?.classList.contains("filter-menu")) {
                button.nextElementSibling.remove();
                return;
            }

            const menu = document.createElement("div");
            menu.className = "filter-menu";
            menu.setAttribute("role", "menu");
            options.forEach(option => {
                const item = document.createElement("button");
                item.type = "button";
                item.role = "menuitem";
                item.textContent = option.label;
                item.addEventListener("click", () => {
                    onSelect(option.value, option.label);
                    const st = AppUtils.Pagination.getState(key);
                    if (st) st.currentPage = 1;
                    menu.remove();
                    loadData();
                });
                menu.appendChild(item);
            });
            button.insertAdjacentElement("afterend", menu);
        });
    }

    function toggleWeddingFields(event) {
        const toggle = event.target;
        if (!(toggle instanceof HTMLInputElement) || !toggle.matches("[data-wedding-type-toggle]")) return;

        const form = toggle.closest("form");
        const weddingFields = form?.querySelector(".person-wedding");
        const celebratedFields = form?.querySelector(".person-celebrated");
        if (!weddingFields || !celebratedFields) return;

        weddingFields.classList.toggle("d-none", !toggle.checked);
        weddingFields.disabled = !toggle.checked;
        celebratedFields.classList.toggle("d-none", toggle.checked);
        celebratedFields.disabled = toggle.checked;
    }

    function escapeHTML(t) {
        const div = document.createElement("div");
        div.textContent = t ?? "";
        return div.innerHTML;
    }

    function formatDate(value) {
        const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value ?? "");
        return match ? `${match[3]}/${match[2]}/${match[1]}` : value;
    }

    async function submitWeddingForm(event) {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.closest("#GlobalModal-body")) return;

        event.preventDefault();
        if (form.dataset.submitting === "true") return;
        form.dataset.submitting = "true";

        const submitButton = event.submitter;
        const originalButtonText = submitButton?.textContent;
        if (submitButton) {
            submitButton.disabled = true;
            submitButton.setAttribute("aria-busy", "true");
            submitButton.textContent = "Guardando...";
        }

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: new FormData(form)
            });

            if (response.redirected) {
                const redirectUrl = new URL(response.url);
                if (redirectUrl.origin === window.location.origin &&
                    redirectUrl.pathname.startsWith("/Weddings/")) {
                    window.ModalUtils.close("GlobalModal");
                    await loadData();
                    AppUtils.showToast("success", "Celebración guardada");
                } else {
                    window.location.assign(response.url);
                }
                return;
            }

            const html = await response.text();
            if (!response.ok) throw new Error(`No se pudo guardar la celebración (${response.status}).`);
            const modalBody = document.querySelector("#GlobalModal-body");
            if (!modalBody) throw new Error("No se encontró el formulario de celebración.");
            modalBody.innerHTML = html;

            const invalidField = modalBody.querySelector(
                'input.input-validation-error, select.input-validation-error, textarea.input-validation-error, ' +
                'input[aria-invalid="true"], select[aria-invalid="true"], textarea[aria-invalid="true"]'
            );
            const errorSummary = modalBody.querySelector(".validation-summary-errors");
            const focusTarget = invalidField || errorSummary;
            if (focusTarget instanceof HTMLElement) {
                if (focusTarget === errorSummary) focusTarget.tabIndex = -1;
                focusTarget.focus({ preventScroll: true });
                focusTarget.scrollIntoView({ block: "nearest" });
            }
        } catch (error) {
            console.error("Error guardando la celebración:", error);
            AppUtils.showToast("error", error.message);
        } finally {
            if (form.isConnected) form.dataset.submitting = "false";
            if (submitButton?.isConnected) {
                submitButton.disabled = false;
                submitButton.removeAttribute("aria-busy");
                if (originalButtonText !== null) submitButton.textContent = originalButtonText;
            }
        }
    }

    async function deleteWedding(event) {
        const button = event.target instanceof Element ? event.target.closest(".btn-delete") : null;
        if (!button) return;

        const confirmed = window.Swal
            ? (await window.Swal.fire({
                title: "¿Eliminar esta celebración?",
                text: "Esta acción no se puede deshacer.",
                icon: "warning",
                showCancelButton: true,
                confirmButtonText: "Sí, eliminar",
                cancelButtonText: "Cancelar",
                reverseButtons: true
            })).isConfirmed
            : window.confirm("¿Eliminar esta celebración? Esta acción no se puede deshacer.");
        if (!confirmed) return;

        button.disabled = true;
        try {
            const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
            if (!token) throw new Error("No se encontró el token antifalsificación.");

            const response = await fetch(`/Weddings/${encodeURIComponent(button.dataset.id)}/Delete`, {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: new URLSearchParams({ __RequestVerificationToken: token })
            });
            if (!response.ok) throw new Error(`No se pudo eliminar la celebración (${response.status}).`);

            await loadData();
            AppUtils.showToast("success", "Celebración eliminada");
        } catch (error) {
            console.error("Error eliminando la celebración:", error);
            AppUtils.showToast("error", error.message, true);
        } finally {
            if (button.isConnected) button.disabled = false;
        }
    }
})();