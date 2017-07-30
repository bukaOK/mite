function Tab(settings) {
    this.name = settings.name;
    this.item = settings.$item;
    this.content = settings.$content;
    this.parentTab = settings.parentTab;
    this.childrenTabs = [];
    this.isActive = settings.isActive;
}
Tab.prototype.activate = function (activateChild) {
    this.content.show();
    this.item.addClass('active');

    if (this.isActive) {
        return;
    }
    this.isActive = true;
    if (this.parentTab != null && !this.parentTab.isActive) {
        this.parentTab.activate();
    }
    if (activateChild == true && this.childrenTabs.length > 0) {
        this.childrenTabs[0].activate();
    }
}
Tab.prototype.deActivate = function () {
    this.item.removeClass('active');
    this.content.hide();

    if (!this.isActive) {
        return;
    }
    this.isActive = false;
    if (this.childrenTabs.length > 0) {
        this.childrenTabs.forEach(function (tab) {
            tab.deActivate();
        });
    }
}

var TabFilter = {
    Tabs: {
        items: [],
        //Строим путь табов с помощью рекурсии
        buildPath: function (path, parentTab) {
            var self = this,
                tabs;
            path = path[path.length - 1] == '/' ? path.substr(0, path.length - 1) : path;

            if (parentTab == null || parentTab == undefined) {
                tabs = this.items;
            } else {
                tabs = parentTab.childrenTabs;
            }
            tabs.forEach(function (tab) {
                if (tab.isActive) {
                    if(tab.parentTab == null || tab.childrenTabs.length == 0){
                        path += '/' + tab.name;
                    }
                    if (tab.childrenTabs.length > 0) {
                        self.buildPath(path, tab);
                    }
                }
            });
            return path;
        },
        //Получаем самый младший активированный таб (для вставки результата ajax)
        getLastActiveTab: function () {
            var tab;
            for (var i = 0; i < this.items.length; i++) {
                tab = this.items[i];
                if (tab.isActive && tab.childrenTabs.length == 0) {
                    return tab;
                }
            }
        },
        add: function (settings) {
            this.items.push(new Tab(settings));
        },
        deActivateSiblings: function (tab) {
            var parentTab = tab.parentTab,
                tabs;
            if (parentTab == null) {
                tabs = this.items;
            } else {
                tabs = parentTab.childrenTabs;
            }
            tabs.forEach(function (siblingTab) {
                siblingTab.deActivate();
            });
        },
        refresh: function () {
            TabFilter.refresh(false);
        },
        updateState: function (basePath) {
            var tabsPath = location.pathname.replace(basePath, '');
            tabsPath = tabsPath.substr(1, tabsPath.length - 1);

            if (tabsPath != '' && tabsPath != null) {
                this.items.forEach(function (tab) {
                    if (tabsPath.search(tab.name) !== -1) {
                        tab.activate();
                    } else {
                        tab.deActivate();
                    }
                });
            } else {
                this.items.forEach(function (tab) {
                    if (tab.isActive) {
                        tab.activate();
                    }
                });
            }
        },
        initEventListeners: function () {
            var self = this;
            this.items.forEach(function (tab) {
                //Инициализируем обработчики нажатий
                tab.item.click(function (ev) {
                    self.deActivateSiblings(tab);
                    tab.activate(true);
                    self.refresh();
                });
            });
        },
        init: function (basePath) {
            var self = this;

            console.log(this.items);
            //Добавляем к родительским табам дочерние
            self.items.forEach(function (parentTab) {
                self.items.forEach(function (childTab) {
                    if (childTab.parentTab != null && childTab.parentTab != undefined && childTab.parentTab.name == parentTab.name) {
                        parentTab.childrenTabs.push(childTab);
                    }
                });
            });
            this.updateState(basePath);
            this.initEventListeners();
        }
    },
    Filters: {
        items: [],
        add: function (settings) {
            this.items.push({
                name: settings.name,
                elem: settings.elem,
                getVal: settings.getVal,
                updateState: settings.updateState
            });
        },
        //Получаем параметры фильтров в виде: fName1=fName1Val&fname2=...
        getStringParams: function () {
            var str = '';
            var filters = this.items;

            this.items.forEach(function (elem, index) {
                str += elem.name + '=' + elem.getVal() + '&';
            });
            return str.substr(0, str.length - 1);
        },
        init: function () {
            var params = location.search.substr(1, location.search.length - 1);
            this.updateFiltersState(params);
        },
        //Обновляем состояние фильтра из строки(fName1=fName1Val&fname2=...)
        updateFiltersState: function (paramsStr) {
            var self = this;
            var pairs = paramsStr.split('&');
            pairs.forEach(function (pair) {
                var name = pair.split('=')[0];
                var val = pair.split('=')[1];

                self.items.forEach(function (elem) {
                    if (elem.name == name && elem.getVal() != val) {
                        elem.updateState(val);
                    }
                });
            });
        },
    },
    basePath: '',
    page: 0,
    pagesEnded: false,
    loadNextPage: false,
    initialized: false,
    loading: false,

    init: function(basePath, settings){
        var self = this;
        this.basePath = basePath;

        if (settings.beforeLoad != undefined) {
            this.ajaxCallbacks.beforeLoad = settings.beforeLoad;
        }
        if (settings.onSuccess != undefined) {
            this.ajaxCallbacks.onSuccess = settings.onSuccess;
        }
        if (settings.onError != undefined) {
            this.ajaxCallbacks.onError = settings.onError;
        }
        if (settings.onComplete != undefined) {
            this.ajaxCallbacks.onComplete = settings.onComplete;
        }
        if (settings.ajax != undefined && settings.ajax.type != undefined)
            this.ajax.type = settings.ajax.type;
        //this.ajax.checkResponseLength = settings.ajax.checkResponseLength;

        this.Tabs.init(basePath);
        this.Filters.init();
        this.initialized = true;

        window.onpopstate = function (ev) {
            self.Tabs.updateState(basePath);
            self.Filters.updateFiltersState();
            self.refresh(true);
        }
        this.loadNext();
    },
    ajaxCallbacks: {
        beforeLoad: function () { },
        onSuccess: function (resp, tab) { },
        onError: function (jqXhr) { return false; },
        onComplete: function (jqXhr) { return false; }
    },
    ajax: {
        type: 'get',
        url: '',
        success: function (resp) {
            if (resp.status == undefined) {
                resp = JSON.parse(resp);
            }
            if (resp.data.length == 0) {
                TabFilter.pagesEnded = true;
            }
            TabFilter.ajaxCallbacks.onSuccess(resp, TabFilter.Tabs.getLastActiveTab());
            TabFilter.loading = false;
        },
        error: function (jqXhr) {
            TabFilter.ajaxCallbacks.onError(resp);
        },
        complete: function (jqXhr) {
            SearchFilters.callbacks.onComplete(jqXhr);
        },
        checkResponseLength: function (resp) {
            //Если длина ответа 0
            //Возвращает false
        }
    },
    load: function () {
        this.ajaxCallbacks.beforeLoad();
        //Переводим в строку
        this.ajax.url = this.Tabs.buildPath(this.basePath);
        this.ajax.data = this.Filters.getStringParams();
        this.ajax.data += '&page=' + this.page;
        this.loading = true;
        return $.ajax(this.ajax);
    },
    //Получаем URL из текущего таба и фильтра(не зависит от URL страницы)
    buildUrl: function(){
        var path = this.Tabs.buildPath(this.basePath);
        var params = '?' + this.Filters.getStringParams();
        return path + params;
    },
    refresh: function (toBack) {
        this.pagesEnded = false;
        this.loadNextPage = false;
        this.page = 1;

        var url = this.buildUrl();
        this.ajax.url = url;
        if (!toBack)
            history.pushState(url, '', url);

        return this.load();
    },
    loadNext: function () {
        this.loadNextPage = true;
        if (this.pagesEnded) {
            return;
        }
        this.page++;
        return this.load();
    }
}