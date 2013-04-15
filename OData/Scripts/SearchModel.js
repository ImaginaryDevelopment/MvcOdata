var addSearch = function (ko, self, odataUrl, wcfUrl) {
    self.makeLinkJson = ko.observable(false);
    self.hasError = ko.observable(false);
    self.errorDetail = ko.observable();
    self.totalServerItems = ko.observable();
    self.odataBase = odataUrl + '?';
    self.wcfDataBase = wcfUrl + '?';
    self.message = ko.observable('KO initialized');
    self.items = ko.observableArray();
    self.initialized = false;
    self.pagesize = ko.observable(10);
    self.page = ko.observable(1);
    self.sortField = ko.observable('');
    self.descending = ko.observable(false);
    
    for (var i in self.columns) {
        self[self.columns[i].Name] = ko.observable();
    }
    //self['@p.PropertyName'] = ko.observable();
    self.filter = ko.computed(function () {
        var filter = '';
        var filters = [];
        var addFilter = '&$filter=';

        var buildFilter = function (name, operator, prefix, surround) {

            if (self[name] && self[name]()) {

                var newFilter = name + ' ' + operator + ' ';
                if (self[name]() === -1 || self[name]() === "-1") {
                    if (operator !== "eq")
                        console.warn('null comparison on != eq');
                    newFilter += 'null';
                } else {
                    newFilter += prefix + surround + self[name]() + surround;
                }

                filters.push(newFilter);
            }
        };
        for (var j in self.columns) {
            var n = self.columns[j];
            if (window.filterOverride && window.filterOverride[n.Name]) {
                window.filterOverride[n.Name](self, filters, buildFilter);
            } else if (n.IsDateRange) {
                var startName = n.Name + '.StartDate';
                var endName = n.Name + '.EndDate';
                buildFilter(startName, 'ge', 'datetime', '\'');
                buildFilter(endName, 'lt', 'datetime', '\'');
            } else {
                var useDateTimePrefix = n.ModelType === "System.DateTime" || n.ModelType === "System.Nullable`1[System.DateTime]";
                buildFilter(n.Name, 'eq', useDateTimePrefix ? 'datetime' : '', n.IsValueType ? '' : '\'');
            }
        }
        if (filters.length > 0)
            filter = addFilter + filters.join(" and ");
        return filter;
    });
   

    self.searchOptions = ko.observableArray([

        { text: 'exact match', value: 'eq' },
        { text: 'indexof contains', value: 'indexof' },
        { text: 'contains', value: 'substringof' }
    ]);
    self.sortOptions = ko.observableArray([
        { text: '...', value: '' },
        { text: 'UniverseID', value: 'UniverseID' }
    ]);
    self.htmlMapSearchType = ko.observable('eq');

    self.ordering = ko.computed(function () {
        var order = '';
        var orderPrefix = "&$orderby=";

        if (self.sortField() && self.sortField() !== '') {
            order += orderPrefix + self.sortField();
            if (self.descending())
                order += " desc";
        }
        return order;
    });
    self.sortField.beforeAndAfterSubscribe(function (oldvalue, newvalue) {
        if (oldvalue != newvalue)
            self.fetch();
    });
    self.paging = ko.computed(function () {
        var pagingQuery = "&$top=" + self.pagesize();

        if (self.page() > 1)
            pagingQuery += "&$skip=" + ((self.page() - 1) * self.pagesize());
        pagingQuery += "&$inlinecount=allpages";
        return pagingQuery;
    });
    self.queryUrl = ko.computed(function () {
        var query = self.filter() + self.ordering() + self.paging();


        return query;
    });

    self.odataJson = ko.computed(function () {
        if (self.makeLinkJson())
            return '&json=true';
        return '';
    });
    self.uri = ko.computed(function () {
        var base = self.odataBase;
        var query = base + self.queryUrl();

        return query + self.odataJson();
    });

    self.orderingAlternate = ko.computed(function () {
        return !self.filter();
    });
    self.pagingAlternate = ko.computed(function () {
        //return (self.filter() && self.ordering() || (!self.filter() && !self.ordering()));
        return !(!self.ordering() ^ !self.filter());
    });
    self.jsonAlternate = ko.computed(function () {
        return !self.pagingAlternate(); //only works because paging is ALWAYS present in this context
    });
    self.wcfJson = ko.computed(function () {
        if (self.makeLinkJson())
            return "&$format=json";
        return '';
    });
    self.WcfDataUrl = ko.computed(function () {

        var query = self.wcfDataBase + self.queryUrl();

        return query;
    });

    self.fetch = function () {
        $.ajax(self.uri()).success(function (d) {
            self.hasError(false);
            self.message('fetched');
            self.items.removeAll();
            self.items(d.value);
            self.totalServerItems(d.count);
            if (!self.initialized) {
                self.initialized = true;
                self.message("initialized");
                window.ko.applyBindings(komodel);
                self.message("ko bound");
            }

        }).error(function (error, status, jqXhr) {
            self.message('query failed:' + status); //":"+JSON.stringify(error));
            self.hasError(true);
        });
    };
};

var addGridOptions = function (ko, self) {
    self.pagingOptions = {
        currentPage: self.page,
        pageSize: self.pagesize,
        pageSizes: ko.observableArray([10, 2, 25]),
        totalServerItems: self.totalServerItems
    };
    self.gridOptions = {
        data: self.items,
        enablePaging: true,
        pagingOptions: self.pagingOptions,
        afterSelectionChange: function () { }
    };
};