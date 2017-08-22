var SearchFilters = {
    initialized: false,
    _loadNextPage: false,
    _pagesEnded: false,
    _page: 0,
    _initialDate: new Date().toUTCString(),
    filters: [
        {
            name: 'name',
            val: 'val',
            selector: 'selector',
            getVal: function () {
                return this.val;
            },
            updateState: function () {
                //Вызывается только один раз, при первой загрузке страницы
                //Обновляет состояние каждого фильтра
            }
        }
    ],
    callbacks: {
        beforeLoad: function () { },
        onSuccess: function (resp) { },
        onError: function (jqXhr) { return false; },
        onComplete: function (jqXhr) { return false; }
    },
    ajax: {
        type: 'post',
        url: '',
        success: function (resp) {
            if (resp.status === undefined) {
                resp = JSON.parse(resp);
            }
            if (resp.data.length === 0) {
                SearchFilters._pagesEnded = true;
            }
            SearchFilters.callbacks.onSuccess(resp.data);
        },
        error: function (jqXhr) {
            SearchFilters.callbacks.onError(resp);
        },
        complete: function (jqXhr) {
            SearchFilters.callbacks.onComplete(jqXhr);
        }
    },
    init: function (settings) {
        if (settings.beforeLoad != undefined) {
            this.callbacks.beforeLoad = settings.beforeLoad;
        }
        if (settings.onSuccess != undefined) {
            this.callbacks.onSuccess = settings.onSuccess;
        }
        if (settings.onError != undefined) {
            this.callbacks.onError = settings.onError;
        }
        if (settings.onComplete != undefined) {
            this.callbacks.onComplete = settings.onComplete;
        }
        this.ajax.url = settings.ajax.url;

        this.filters = settings.filters;
        this._updateFiltersState(decodeURIComponent(location.search.substr(1)));
        this.initialized = true;

        var self = this;
        window.onpopstate = function (ev) {
            self._updateFiltersState(ev.state);
            return self.changeFilter(true);
        }
        self.loadNext();
    },
    changeFilter: function (toBack) {
        this._page = 1;
        this._loadNextPage = false;
        this._pagesEnded = false;
        this._initialDate = new Date().toUTCString();
        var stringParams = this._getStringParams();
        if (!toBack)
            history.pushState(stringParams, '', '?' + stringParams);
        return this.load();
    },
    loadNext: function () {
        this._loadNextPage = true;
        if (this._pagesEnded) {
            return;
        }
        this._page++;
        return this.load();
    },
    load: function () {
        this.callbacks.beforeLoad();
        //Переводим в строку
        var stringParams = this._getStringParams();
        this.ajax.data = stringParams + '&page=' + this._page + '&initialDate=' + this._initialDate;
        return $.ajax(this.ajax);
    },
    _getStringParams: function () {
        var str = '';
        var filters = this.filters;

        filters.forEach(function (elem, index) {
            str += elem.name + '=' + elem.getVal() + '&';
        });
        return str.substr(0, str.length - 1);
    },
    _updateFiltersState: function (paramsStr) {
        var self = this;
        var pairs = paramsStr.split('&');
        pairs.forEach(function (param) {
            var name = param.split('=')[0];
            var val = param.split('=')[1];

            self.filters.forEach(function (elem) {
                if (elem.name == name && elem.getVal() != val) {
                    elem.updateState(val);
                }
            });
        });
    }
}