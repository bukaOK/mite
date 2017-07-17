function Tab(path, $item, $content, innerTabs) {
    this.path = path;
    this.item = $item;
    this.content = $content;
    if (innerTabs == null || innerTabs == undefined)
        this.innerTabs = [];
    else
        this.innerTabs = innerTabs;
}
Tab.prototype.activate = function () {
    this.item.addClass('active');
    this.content.show();
    if (this.innerTabs.length > 0) {
        var innerTab;
        for (var i = 0; i < this.innerTabs.length; i++) {
            innerTab = innerTabs[i];
            if (innerTab.item.hasClass('active') || innerTab.content.hasClass('active')) {
                innerTab.activate();
                break;
            }
        }
    }
}
Tab.prototype.deActivate = function () {
    this.item.removeClass('active');
    this.content.hide();
}

var TabFilter = {
    tabs: [],
    filters: [],
    currentTab: {},
    basePath: '',

    init: function(basePath){
        this.basePath = basePath;

        var url = location.href;
        url = url.replace(basePath, '');
        var urlParts = url.split('?');
        var tabPath = urlParts[0];
        var params = urlParts[1];

    },
    _updateFiltersState: function (paramsStr) {
        var self = this;
        var pairs = paramsStr.split('&');
        pairs.forEach(function (pair) {
            var name = pair.split('=')[0];
            var val = pair.split('=')[1];

            self.filters.forEach(function (elem) {
                if (elem.name == name && elem.getVal() != val) {
                    elem.updateState(val);
                }
            });
        });
    },
    _getStringParams: function () {
        var str = '';
        var filters = this.filters;

        filters.forEach(function (elem, index) {
            str += elem.name + '=' + elem.getVal() + '&';
        });
        return str.substr(0, str.length - 1);
    },
    //Получаем URL из текущего таба и фильтра(не зависит от URL страницы)
    _getCurrentUrl: function(){
        var path = this.currentTab.path[0] == '/' || this.basePath[this.basePath.length - 1] == '/'
            ? this.currentTab.path : '/' + this.currentTab.path;
        var params = '?' + currentFilter.name + this._getStringParams();
        return this.basePath + path + params;
    },
    addTab: function (path, $item, $content, innerTabs) {
        tabs.push(new Tab(path, $item, $content, innerTabs));
    },
    addFilter: function (name, $filter, filterValFunc, updateStateFunc) {
        filters.push({
            name: name,
            elem: $filter,
            getVal: filterValFunc,
            updateState: updateStateFunc
        });
    },
    changeTab: function (path) {
        var tab;
        //Находим нужный таб
        for (var i = 0; i < this.tabs.length; i++) {
            tab = this.tabs[i];
            if (tab.path == path) {
                this.currentTab = tab;
                tab.activate();
            } else {
                tab.deActivate();
            }
        }
        history.pushState()
    },
}