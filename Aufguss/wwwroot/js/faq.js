window.initializeSortable = (elementId, dotNetHelper) => {
    const el = document.getElementById(elementId);
    if (!el) return;

    new Sortable(el, {
        animation: 150,
        handle: '.accordion-header',
        onEnd: function (evt) {
            const order = [...el.children].map(child => child.getAttribute('data-id'));
            dotNetHelper.invokeMethodAsync('UpdateOrder', order);
        }
    });
};
