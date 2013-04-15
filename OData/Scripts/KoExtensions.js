window.ko.observable.fn.beforeAndAfterSubscribe = function (callback, target) {
           var oldValue;
           this.subscribe(function (currentValue) {
               oldValue = currentValue;
           }, null, 'beforeChange');

           this.subscribe(function (newValue) {
               callback.call(target, oldValue, newValue);
           });
       };