function zeroExtend(num, size) {
    var s = num + "";
    while (s.length < size) s = "0" + s;
    return s;
}

function formatCurrencyForDisplay(value) {
    if (!value)
        return '';
    var number = value.toString(),
        dollars = number.split('.')[0],
        cents = (number.split('.')[1] || '') + '00';
    dollars = dollars.split('').reverse().join('')
        .replace(/(\d{3}(?!$))/g, '$1,')
        .split('').reverse().join('');
    return '$' + dollars + '.' + cents.slice(0, 2);
}

window.ko.bindingHandlers.textMoney = {
    init: function () {
    },
    // ReSharper disable UnusedParameter
    update: function (element, valueAccessor, allBindingsAccessor) {
        // ReSharper restore UnusedParameter
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).text(formatCurrencyForDisplay(value));
    }
};

function formatDateForDisplay(value) {
    if (!value)
        return '';
    // Chrome appears to handle this type of date string wrong.
    var matches = /([0-9]+)-([0-9]+)-([0-9]+)T.*/.exec(value);
    if (matches !== null)
        return zeroExtend(matches[2], 2) + '/' + zeroExtend(matches[3], 2) + '/' + zeroExtend(matches[1], 4);
    else
        return '';
}

window.ko.bindingHandlers.textDate = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        $(element).attr('data-text', 'textDate');

        var value = ko.utils.unwrapObservable(valueAccessor());
        if (!value) {
            $(element).text('');

            return;
        }

        // Chrome appears to handle this type of date string wrong.
        var matches = /([0-9]+)-([0-9]+)-([0-9]+)T.*/.exec(value);
        if (matches !== null) {
            $(element).text(zeroExtend(matches[2], 2) + '/' + zeroExtend(matches[3], 2) + '/' + zeroExtend(matches[1], 4));
            var title = value.replace('T', ' ');
            if (title.indexOf('.') >= 0) {
                title = title.substring(0, title.indexOf('.'));
            }
            $(element).attr('title', title);
        }
        else
            $(element).text('');

    },
    // ReSharper disable UnusedParameter
    update: function (element, valueAccessor, allBindingsAccessor) {
        // ReSharper restore UnusedParameter
        var value = ko.utils.unwrapObservable(valueAccessor());
        $(element).text(formatDateForDisplay(value));
    }
};
window.ko.bindingHandlers.displayText = { // a custom binder so that the displayed value for selects can be shown as filter criteria
    // ReSharper disable UnusedParameter 
    init: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) { // ReSharper restore UnusedParameter
        if (element.nodeName == 'SELECT') {
            var newText = $(':selected', element).text();
            valueAccessor()(newText);
        }
    },// ReSharper disable UnusedParameter
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) { // ReSharper restore UnusedParameter
        if (element.nodeName == 'SELECT') {
            var newText = $(':selected', element).text();
            valueAccessor()(newText);
        }
    }
};
window.ko.safeObservable = function (initialValue) {
    var result = ko.observable(initialValue);
    result.safe = ko.dependentObservable(function () {
        return result() || {};
    });

    return result;
};
window.ko.observable.fn.beforeAndAfterSubscribe = function (callback, target) {
    var oldValue;
    this.subscribe(function (currentValue) {
        oldValue = currentValue;
    }, null, 'beforeChange');

    this.subscribe(function (newValue) {
        callback.call(target, oldValue, newValue);
    });
};