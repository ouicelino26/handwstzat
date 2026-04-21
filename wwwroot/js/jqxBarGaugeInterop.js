window.handwstatBarGaugeRegistry = window.handwstatBarGaugeRegistry || {};

window.handwstatJqxBarGaugeSetOptions = function (id, options) {
    if (!window.jqxBlazor || !window.jqxBlazor.createComponent || !window.jqxBlazor.setOptions) {
        return;
    }

    try {
        if (window.handwstatBarGaugeRegistry[id]) {
            window.jqxBlazor.setOptions(id, options);
            return;
        }

        window.jqxBlazor.createComponent(id, "jqxBarGauge", options);
        window.handwstatBarGaugeRegistry[id] = true;
    } catch {
        try {
            if (window.handwstatBarGaugeRegistry[id]) {
                window.jqxBlazor.setOptions(id, options);
            } else {
                window.jqxBlazor.createComponent(id, "jqxBarGauge", options);
                window.handwstatBarGaugeRegistry[id] = true;
            }
        } catch {
            // Leave the fallback text visible if the widget cannot initialize.
        }
    }
};

window.handwstatJqxBarGaugeEnsure = function (id, options) {
    window.handwstatJqxBarGaugeSetOptions(id, options);
};

window.handwstatJqxBarGaugeUpdateValues = function (id, values) {
    if (!window.jqxBlazor || !window.jqxBlazor.setOptions || !window.handwstatBarGaugeRegistry[id]) {
        return;
    }

    try {
        window.jqxBlazor.setOptions(id, { values: values });
    } catch {
        // Ignore update failures and keep the textual KPI visible.
    }
};

window.handwstatJqxBarGaugeDestroy = function (id) {
    if (!window.jqxBlazor || !window.jqxBlazor.manageMethods || !window.handwstatBarGaugeRegistry[id]) {
        return;
    }

    try {
        window.jqxBlazor.manageMethods(id, "destroy", []);
    } catch {
        // Ignore teardown failures during navigation or app shutdown.
    }

    delete window.handwstatBarGaugeRegistry[id];
};
