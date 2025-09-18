window.initializeSortable = (elementId, dotNetHelper) => {
    const el = document.getElementById(elementId);
    if (!el) {
        console.warn("Element not found:", elementId);
        return;
    }

    new Sortable(el, {
        animation: 150,
        handle: '.drag-handle',
        onEnd: function () {
            // Find all .accordion-item elements (which contain the correct data-id)
            const order = Array.from(el.querySelectorAll('.accordion-item'))
                .map(item => item.getAttribute("data-id"));
            dotNetHelper.invokeMethodAsync("UpdateOrder", order);
        }
    });
};