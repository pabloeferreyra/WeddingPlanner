(() => {
    'use strict';
    const key = "weddings";
    let reloadWeddings = null;

    document.addEventListener("DOMContentLoaded", () => {

        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: "#pageSize",
            defaultOrder: { column: 0, dir: 'asc' },
            onChange: AppUtils.Pagination.init
        });

        AppUtils.Sort.attachHeaderSorting("#weddingsTable", key, loadData);


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

        const columnsMap = ['Title', 'WeddingDate', 'Location', 'ExpectedGuestCount', 'InitialBudget'];
        columnsMap.forEach((name, i) => form.append(`columns[${i}][name]`, name));

        try {
            const res = await fetch("/Weddings/InitializeTable", {
                method: "POST",
                body: form
            });

            const json = await res.json();
            const data = json?.data || [];
            const total = json?.recordsTotal || json?.recordsFiltered || data.length;

            AppUtils.Pagination.setRecordsTotal(key, total);
            renderTable(data);
            AppUtils.Pagination.renderWithState("#weddingPagination", key, loadData);

        } catch (err) {
            console.error("Error cargando celebraciones:", err);
            const tbody = document.querySelector("#weddingsTable tbody");
            if (tbody) {
                tbody.innerHTML =
                    '<tr><td colspan="9" class="text-danger text-center">Error cargando celebraciones.</td></tr>';
            }
        }
    }

    function renderTable(rows) {
        const tbody = document.querySelector("#weddingsTable tbody");
        if (!tbody) return;

        tbody.innerHTML = "";

        if (!rows.length) {
            tbody.innerHTML = '<div class="empty-state">' +
                '<h2>Aún no hay bodas registradas</h2>' +
                '<p>Crea una celebración para empezar a organizarla.</p>' +
                '<button class="btn btn-primary mt-3" data-open-modal ' +
                'data-modal-id="GlobalModal" ' +
                'data-url="/Weddings/Create" ' +
                'data-title="Nueva celebración" ' +
                'type="button">Crear primera boda</button>' +
                '</div>';
            return;
        }

        rows.forEach(d => {

            let actions = "";
            const location = d.location
                ? [d.location.city, [d.location.state].filter(Boolean).join(" ")]
                    .filter(Boolean)
                    .join(", ") || "Por confirmar"
                : "Por confirmar";

            actions = `
                    <button
                        data-open-modal
                        data-modal-id="GlobalModal"
                        data-url="/Weddings/${d.id}/Edit"
                        data-title="Editar celebración"
                        class="btn btn-sm btn-primary me-1">
                        Editar
                    </button>
                    <button data-id="${d.id}" class="btn btn-sm btn-outline-danger btn-delete">Eliminar</button>
                `;

            tbody.insertAdjacentHTML("beforeend", `
                <tr>
                    <td>${escapeHTML(d.title)}</td>
                    <td>${escapeHTML(d.weddingDate)}</td>
                    <td>${escapeHTML(location)}</td>
                    <td>${escapeHTML(d.expectedGuestCount)}</td>
                    <td>${escapeHTML(d.initialBudget)}</td>
                    <td>${actions}</td>
                </tr>
            `);
        });
    }

    function escapeHTML(t) {
        const div = document.createElement("div");
        div.textContent = t ?? "";
        return div.innerHTML;
    }

    function initCreateWeddingModal() {
        const createWeddingModal = document.querySelector("#createWeddingModal");
        if (createWeddingModal) {
            createWeddingModal.addEventListener("show.bs.modal", () => {
                const form = createWeddingModal.querySelector("form");
                if (form) {
                    form.reset();
                }
            });
            const btn = document.querySelector("#createWeddingBtn");
            if (!btn) return;
            btn.addEventListener("click", async () => {
                e.preventDefault();
                if (!AppUtils.validateAll()) return;
                await ModalUtils.submitForm(
                    "GlobalModal",
                    "createWeddingForm",
                    "POST",
                    "/Weddings/Create",
                    "Nueva boda",
                    true,
                    false
                )
            });
        }
    }

    initCreateWeddingModal();
})();