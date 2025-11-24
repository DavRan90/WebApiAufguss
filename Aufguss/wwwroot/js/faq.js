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

window.blazorCalendar ??= {};

window.blazorCalendar.syncWeekGutter = function (calendarGridId, gutterId) {
    try {
        const grid = document.getElementById(calendarGridId);
        const gutter = document.getElementById(gutterId);
        if (!grid || !gutter) return;

        const calendarWrapper = grid.querySelector(".calendar-container-wrapper");
        if (!calendarWrapper) return;

        // Find ALL monthly day cells (these correspond to calendar boxes)
        let dayCells = calendarWrapper.querySelectorAll(".monthly-day");
        if (!dayCells || dayCells.length === 0) return;

        dayCells = Array.from(dayCells);

        // Group into weeks (7 days each)
        const weeks = [];
        for (let i = 0; i < dayCells.length; i += 7) {
            weeks.push(dayCells.slice(i, i + 7));
        }

        // Create/remove gutter items
        while (gutter.children.length < weeks.length) {
            const div = document.createElement("div");
            div.className = "week-gutter-item";
            gutter.appendChild(div);
        }
        while (gutter.children.length > weeks.length) {
            gutter.removeChild(gutter.lastChild);
        }

        // Calculate height per week
        weeks.forEach((week, i) => {
            // week height = height of the TALLEST day in that week
            const height = Math.max(...week.map(d => d.getBoundingClientRect().height));
            const child = gutter.children[i];
            child.style.height = `${height}px`;
            child.style.lineHeight = `${height}px`;
        });

    } catch (e) {
        console.warn("syncWeekGutter failed", e);
    }
};

window.addEventListener("resize", () => {
    if (window.blazorCalendar?.syncWeekGutterDelayed)
        window.blazorCalendar.syncWeekGutterDelayed();
});

window.blazorCalendar.syncWeekGutterDelayed = function () {
    clearTimeout(window._gutterTimer);
    window._gutterTimer = setTimeout(() => {
        window.blazorCalendar.syncWeekGutter("calendarGrid", "weekGutter");
    }, 150);
};


//window.blazorCalendar ??= {};

//window.blazorCalendar.syncWeekGutter = function (calendarGridId, gutterId) {
//    try {
//        const grid = document.getElementById(calendarGridId);
//        const gutter = document.getElementById(gutterId);
//        if (!grid || !gutter) return;

//        // the calendar wrapper is second child of grid
//        const calendarWrapper = grid.children[1];
//        if (!calendarWrapper) return;

//        // Step 1: find actual week rows inside the calendar
//        let rows =
//            calendarWrapper.querySelectorAll("table tr") ||       // BlazorCalendar uses tables
//            calendarWrapper.querySelectorAll("[role=row]");

//        rows = Array.from(rows).filter(r => r.clientHeight > 2);
//        if (rows.length === 0) return;

//        // Step 2: ensure gutter has same number of items
//        while (gutter.children.length < rows.length) {
//            const div = document.createElement("div");
//            div.className = "week-gutter-item";
//            gutter.appendChild(div);
//        }
//        while (gutter.children.length > rows.length) {
//            gutter.removeChild(gutter.lastChild);
//        }

//        // Step 3: match heights exactly, row-by-row
//        rows.forEach((row, i) => {
//            const h = row.getBoundingClientRect().height;
//            const child = gutter.children[i];
//            child.style.height = `${h}px`;
//            child.style.lineHeight = `${h}px`;
//        });
//    }
//    catch (e) {
//        console.warn("syncWeekGutter failed", e);
//    }
//}


