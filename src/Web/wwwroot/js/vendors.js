(() => {
    'use strict';

    const key = 'vendors';

    document.addEventListener('DOMContentLoaded', () => {
        AppUtils.Pagination.init(key, {
            defaultPageSize: 25,
            pageSizeSelector: '#vendorPageSize',
            onChange: loadData
        });
        document.addEventListener('submit', submitModalForm);
        loadData();
    });

    async function loadData() {
        const state = AppUtils.Pagination.getState(key);
        const form = new URLSearchParams({
            draw: '1',
            start: String((state.currentPage - 1) * state.pageSize),
            length: String(state.pageSize)
        });

        try {
            const response = await fetch('/Vendors/InitializeTable', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: form
            });
            if (!response.ok) throw new Error(`Error cargando proveedores (${response.status}).`);

            const result = await response.json();
            AppUtils.Pagination.setRecordsTotal(key, result.recordsFiltered ?? result.recordsTotal ?? 0);
            renderTable(result.data || []);
            AppUtils.Pagination.renderWithState('#vendorPagination', key, loadData);
        } catch (error) {
            console.error('Error cargando proveedores:', error);
            document.querySelector('#vendorsTable tbody').innerHTML =
                '<tr><td colspan="7" class="text-danger text-center">No se pudieron cargar los proveedores.</td></tr>';
            AppUtils.showToast('error', 'No se pudieron cargar los proveedores.');
        }
    }

    function renderTable(vendors) {
        const tbody = document.querySelector('#vendorsTable tbody');
        if (!vendors.length) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center">Todavía no hay proveedores registrados.</td></tr>';
            return;
        }

        const serviceTypes = [
            'Catering', 'Salón o lugar', 'Fotografía', 'Video', 'Florería', 'Decoración',
            'DJ', 'Pastelería', 'Invitaciones', 'Transporte', 'Peluquería y maquillaje',
            'Entretenimiento', 'Otro'
        ];

        tbody.innerHTML = vendors.map(vendor => {
            const id = encodeURIComponent(vendor.id);
            const serviceType = Number(vendor.serviceType);
            const service = Number.isInteger(serviceType)
                ? serviceTypes[serviceType] || 'Otro'
                : AppUtils.escapeHtml(vendor.serviceType);
            const city = vendor.address?.city || 'Sin ciudad';
            const rating = Number(vendor.averageRating || 0).toFixed(1);

            return `<tr>
                <td>${AppUtils.escapeHtml(vendor.businessName)}${vendor.isFavorite ? ' <span aria-label="Favorito" title="Favorito">★</span>' : ''}</td>
                <td>${service}</td>
                <td>${AppUtils.escapeHtml(city)}</td>
                <td>${AppUtils.escapeHtml(vendor.email || vendor.contactPerson || 'Sin contacto')}</td>
                <td>${AppUtils.escapeHtml(rating)} / 5</td>
                <td>${vendor.isActive ? 'Activo' : 'Inactivo'}</td>
                <td class="text-nowrap">
                    <button class="btn btn-sm btn-outline-secondary me-1" data-open-modal data-modal-id="GlobalModal"
                            data-url="/Vendors/${id}" data-title="Detalle del proveedor" type="button">Ver</button>
                    <button class="btn btn-sm btn-primary me-1" data-open-modal data-modal-id="GlobalModal"
                            data-url="/Vendors/Edit/${id}" data-title="Editar proveedor" type="button">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" data-open-modal data-modal-id="GlobalModal"
                            data-url="/Vendors/Delete/${id}" data-title="Eliminar proveedor" type="button">Eliminar</button>
                </td>
            </tr>`;
        }).join('');
    }

    async function submitModalForm(event) {
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
                ModalUtils.close('GlobalModal');
                await loadData();
                return;
            }

            const html = await response.text();
            if (!response.ok) throw new Error(`No se pudo guardar el proveedor (${response.status}).`);
            document.querySelector('#GlobalModal-body').innerHTML = html;
        } catch (error) {
            console.error('Error guardando proveedor:', error);
            AppUtils.showToast('error', error.message);
        } finally {
            if (submitButton?.isConnected) submitButton.disabled = false;
        }
    }
})();
