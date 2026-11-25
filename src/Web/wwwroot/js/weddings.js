(() => {
    'use strict';
    const key = "weddings";
    let reloadWeddings = null;
    let searchTimer = null;
    let dateFilter = "";
    let statusFilter = "";

    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: 'asc' },
            onChange: AppUtils.Pagination.init
        });

        AppUtils.Sort.attachHeaderSorting("#weddingsTable", key, loadData);

        document.addEventListener("submit", submitWeddingForm);
        document.addEventListener("click", deleteWedding);
        bindFilters();

        loadData();
        reloadWeddings = loadData;

        document.dispatchEvent(new Event("weddings:loaded"));
    });

    async function loadData() {

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

        try {
            const res = await fetch("/Weddings/InitializeTable", {
                method: "POST",
                body: form
            });

            const json = await res.json();
            const data = json?.data || [];
            const total = json?.recordsTotal || json?.recordsFiltered || data.length;
            const filtered = json?.recordsFiltered ?? total;

            AppUtils.Pagination.setRecordsTotal(key, filtered);
            updateSummary(json?.summary, filtered);
            renderTable(data);
            AppUtils.Pagination.renderWithState("#weddingPagination", key, loadData);
            updatePaginationSummary(filtered);

        } catch (err) {
            console.error("Error cargando celebraciones:", err);
            const tbody = document.querySelector("#weddingsTable tbody");
            if (tbody) {
                tbody.innerHTML =
                    '<tr><td colspan="3" class="text-danger text-center">Error cargando celebraciones.</td></tr>';
            }
        }
    }

    function renderTable(rows) {
        const tbody = document.querySelector("#weddingsTable tbody");
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!rows.length) {
            tbody.innerHTML = '<tr><td colspan="3">' +
                '<div class="empty-state">' +
                '<h3>Aún no hay celebraciones</h3>' +
                '<p>Crea una celebración para empezar a organizarla.</p>' +
                '<button class="btn btn-primary mt-3" data-open-modal ' +
                'data-modal-id="GlobalModal" data-url="/Weddings/Create" ' +
                'data-title="Nueva celebración" type="button">Crear primer evento</button>' +
                '</div></td></tr>';
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
        document.querySelector("#weddingsTotalMetric").textContent = summary.total;
        document.querySelector("#weddingsUpcomingMetric").textContent = summary.upcoming;
        document.querySelector("#weddingsCompletedMetric").textContent = summary.completed;
        document.querySelector("#weddingsResultCount").textContent =
            `Mostrando ${filtered} celebración${filtered === 1 ? "" : "es"}`;
    }

    function updatePaginationSummary(filtered) {
        const st = AppUtils.Pagination.getState(key);
        if (!st) return;
        const start = filtered === 0 ? 0 : (st.currentPage - 1) * st.pageSize + 1;
        const end = Math.min(st.currentPage * st.pageSize, filtered);
        document.querySelector("#weddingPaginationSummary").textContent =
            `${start}-${end} de ${filtered} resultados`;
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
        const submitButton = event.submitter;
        if (submitButton) submitButton.disabled = true;

        try {
            const response = await fetch(form.action, {
                method: form.method || "POST",
                body: new FormData(form)
            });

            if (response.redirected) {
                window.location.assign(response.url);
                return;
            }

            const html = await response.text();
            if (!response.ok) throw new Error(`No se pudo guardar la celebración (${response.status}).`);
            document.querySelector("#GlobalModal-body").innerHTML = html;
        } catch (error) {
            console.error("Error guardando la celebración:", error);
            AppUtils.showToast("error", error.message);
        } finally {
            if (submitButton?.isConnected) submitButton.disabled = false;
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