(() => {
    'use strict';

    document.addEventListener("DOMContentLoaded", () => {

        // AppUtils.Pagination.init(key, {
        //     defaultPageSize: 25,
        //     pageSizeSelector: "#pageSize",
        //     defaultOrder: { column: 0, dir: 'asc' },
        //     onChange: loadData
        // });

        // AppUtils.Sort.attachHeaderSorting("#weddingsTable", key, loadData);


        // loadData();
        // reloadWeddings = loadData;

        // document.dispatchEvent(new Event("weddings:loaded"));
    });

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
                if(!AppUtils.validateAll()) return;
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