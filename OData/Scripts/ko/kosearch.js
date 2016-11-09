var KoSearchModel = function (self, uri,descendingdefault) {
    self.defaultUri = uri + '?';
    self.displaySearch = ko.observable(true);
    self.selectAllQueryChecked = ko.observable(false);
    self.selected = ko.observableArray();//holds selected from page to page
    self.pagesize = ko.observable(10);
    self.page = ko.observable(1);
    self.pagesize.beforeAndAfterSubscribe(function (oldValue, newValue) {
        if (oldValue != newValue) {
            self.page(1);
        }
    });
    self.makeLinkJson = ko.observable(true);
    self.sortField = ko.observable('');
    self.descending = ko.observable(descendingdefault===true);

    self.toggleSearch = function () {
        self.displaySearch(!self.displaySearch());
    };

    self.selectedCount = ko.computed(function () {
        if (self.selectAllQueryChecked())
            return self.resultModel().totalCount;
        return self.selected().length;
    });

    self.clearSelected = function () {
        self.selected.removeAll();
        self.fetch();
    };

    self.selectAllQuery = function () {
        self.selectAllQueryChecked(!self.selectAllQueryChecked());
        if (self.selectAllQueryChecked()) {
            // change mode to ALL everything is selected

        } else {
            //change mode back to user individual selects
        }
        return true;
    };

    self.sortText = ko.computed(function () {
        if (self.descending()) {
            return 'Z..A';
        }
        return 'A..Z';
    });

    self.resultModel = ko.safeObservable(new UserSearchResultModel([], 0, self));

    self.selectAll = function () { //handle selecting all on a page as a proxy for result model
        if (self.resultModel()) {
            self.resultModel().selectAll();
        }
    };
    self.loading = ko.observable(false);
    self.loading.subscribe(function (newvalue) {
        if (newvalue) {
            $.blockUI();
        } else {
            $.unblockUI();
        }
    });
    self.initialized = ko.observable(false);
    self.status = ko.computed(function () {
        if (!self.initialized()) {
            return 'Set filters and click search to view data...';
        }
        if (self.error()) {
            return '<div class="error">' + self.error() + '</div>';
        }
        if (self.resultModel().displayCount == 0) {
            return 'No items were found matching your search.';
        }

        return undefined;

    });
};

var KoSearchPaging = function (self,updateImmediate) {
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
    self.formvalid = ko.observable(false); //TODO: DEV-865 helpful?
    self.exportQueryUri = ko.computed(function () {
        var base = '';
        base += self.filter();

        if (!self.selectAllQueryChecked() && self.selected().length > 0) {
            var filters = [];
            var exp = new RegExp("^\\d+$");
            var buildOrFilter = function (name, items) {
                var orFilters = [];
                var surround = '';
                if (ko.utils.arrayFirst(items, function (item) {
                    return !exp.test(item);
                })) {
                    surround = "'";
                }
                for (var i in items) {
                    var item = items[i];
                    orFilters.push(name + " eq " + surround + item + surround);
                }

                var newFilter = '(' + orFilters.join(' or ') + ')';

                if (base.indexOf('&$filter=') < 0) {
                    return "&$filter=" + newFilter;
                } else {
                    return " and (" + newFilter + ")";
                }
            };
            base += buildOrFilter(window.primaryKey, self.selected());
        }

        base += self.ordering();
        return base;
    });
    self.uri = ko.computed(function () {
        var base = self.defaultUri;
        base += self.filter();
        base += self.ordering();
        base += self.paging();
        return base;
    });
    self.toggleSort = function () {
        self.descending(!self.descending());
        self.fetch();
    };
    self.search = function () {

        if (self.page() != 1)
            self.page(1); //should auto-fire a fetch
        else
            self.fetch();
    };
    self.error = ko.observable();
    self.fetch = function () {
        var valid = $('form').data("validator").numberOfInvalids();
        if (valid !== 0) {
            console.warn('invalid form');
            return;
        }
        self.initialized(true);
        self.loading(true);
        self.displaySearch(false);
        $.getJSON(self.uri(), null, self.loadData).error(function (jqXhr, status, error) {
            console.error(jqXhr.status + ':failed to query:' + error);
            self.error(jqXhr.status + ':failed to query');
            if (jqXhr.status == "401") location.reload(true);
        }).success(function () {
            self.error(null);
        });


        self.setFilterUi();
    };
    if (updateImmediate) {
        self.page.beforeAndAfterSubscribe(function (prev, current) {
            if (prev != current)
                self.fetch();
        });
        self.pagesize.beforeAndAfterSubscribe(function (prev, current) {
            if (prev != current)
                self.fetch();
        });
    }
    //Navigation
    self.gofirst = function () {
        if (self.resultModel()) {
            self.page(1);
            if (!updateImmediate)
                self.fetch();
        }

    };
    self.golast = function () {
        if (self.resultModel()) {
            self.page(self.resultModel().totalPages);
            if (!updateImmediate)
                self.fetch();
        }

    };
    self.next = function () {
        if (self.resultModel() && self.page() < self.resultModel().totalPages) {
            self.page(+self.page() + 1);
            if (!updateImmediate)
                self.fetch();
        }

    };
    self.previous = function () {
        if (self.resultModel()) {
            if (self.page() > 1) {
                self.page(+self.page() - 1);
                if (!updateImmediate)
                    self.fetch();
            }
        }

    };
};