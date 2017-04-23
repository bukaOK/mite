var Tabs = {
    initTabs: function (initObj) {
        this._initSettings(initObj);
        this.initFirstTab();
        var self = this,
            itemsSelector = this.items.selector;

        console.log(this.ajaxSettings);
        $(itemsSelector + '[data-tab]').click(function (ev) {
            //Если у таба есть потомки, показываем содержимое
            if (!self._haveChildren(this)) {
                self.loadTab(this);
            } else {
                //Т.к. мы загружаем потомков, текущий таб также следует сделать активным
                $(this).siblings(itemsSelector).removeClass('active');
                $(this).addClass('active');
                var tab = $(self.tabContent.selector + '[data-tab="' + $(this).data('tab') + '"]');
                self.loadTab(tab.find(self.items.selector + '.active')[0]);
                console.log(this);
            }
        });
        return self;
    },
    initFirstTab: function(){
        var tabName = this.getCurrentTabName();
        var currentItem = $(this.items.selector + '[data-tab="' + tabName + '"]')[0];
        //console.log(this.tabContent.selector + '[data-tab="' + tabName + '"]');
        if (this._haveChildren(currentItem)) {
            //Внутри таба родителя находим активные табы
            currentItem = $(this.tabContent.selector + ' .active' + this.items.selector)[0];
        }
        this.loadTab(currentItem);
    },
    getCurrentTabName: function () {
        if (window.location.hash !== '') {
            var tabName = window.location.hash;
            tabName = tabName.substr(1, tabName.length - 1);
            if (tabName[0] == '/') {

            }
            return tabName;
        }
        var $activeTabs = $(this.items.wrapperSelector + '>.active' + this.items.selector);
        var count = $activeTabs.length;
        return $activeTabs[$activeTabs.length - 1].dataset['tab'];
    },
    tabContent: {
        selector: '.tab',
        display: 'block'
    },
    items: {
        wrapperSelector: '.tabs',
        selector: '.item'
    },
    ajaxSettings: {
        type: 'get',
        beforeSuccess: function (resp) { return resp; },
        error: function (jqXhr) {
            console.log(jqXhr);
        },
        data: {},
        dynamicData: function () { return this.data },
        afterSuccess: function (item, tabContent) { return false; }
    },
    path: window.location.pathname,
    loadTab: function (item) {
        var $item = $(item),
            dataUrl = $item.data('url'),
            dataTab = $item.data('tab'),
            ajaxData = $item.data('ajax'),
            self = this,
            tabContent = $(this.tabContent.selector + '[data-tab="' + dataTab + '"]'),
            itemUrl = this.path;

        //Удаляем активный класс у соседей
        $item.siblings(this.items.selector).removeClass('active');
        $item.addClass('active');

        $(this.tabContent.selector + '[data-tab]').css('display', 'none');
        tabContent.parents(self.tabContent.selector).css('display', this.tabContent.display);
        tabContent.css('display', this.tabContent.display);

        if (dataUrl !== '' && dataUrl != undefined) {
            itemUrl += dataUrl[0] === '/' ? dataUrl : '/' + dataUrl;
        } else {
            itemUrl += dataTab[0] === '/' ? dataTab : '/' + dataTab;
        }
        window.location.hash = dataTab[0] !== '/' ? dataTab : dataTab.substr(1, dataTab.length - 1);
        
        this.ajaxSettings.url = itemUrl;
        this.ajaxSettings.data = this.ajaxSettings.dynamicData();
        if (ajaxData !== '' && ajaxData != undefined) {
            this.ajaxSettings.data = ajaxData;
        }
        this.ajaxSettings.success = function (resp) {
            var handlingResp = self.ajaxSettings.beforeSuccess(resp);

            tabContent.removeClass('loading');
            tabContent.html(handlingResp);
            tabContent.css('display', self.tabContent.display);

            self.ajaxSettings.afterSuccess(item, tabContent);
        }
        tabContent.addClass('loading');

        return $.ajax(this.ajaxSettings);
    },
    _haveChildren: function (item) {
        //Внутри таба ищем вложенные табы
        if ($(this.tabContent.selector + '[data-tab="' + $(item).data('tab') + '"]').children(this.tabContent.selector).length > 0) {
            return true;
        }
        return false;
    },
    _initSettings: function (initObj) {
        //Ставим настройки ajax
        if (initObj.ajaxSettings != undefined) {
            for (var prop in this.ajaxSettings) {
                if (initObj.ajaxSettings != undefined && initObj.ajaxSettings[prop] != undefined) {
                    this.ajaxSettings[prop] = initObj.ajaxSettings[prop];
                }
            }
        }
        //Ставим настройки tabContent
        if (initObj.tabContent != undefined) {
            for (var prop in this.tabContent) {
                if (initObj.tabContent != undefined && initObj.tabContent[prop] != undefined) {
                    this.tabContent[prop] = initObj.tabContent[prop];
                }
            }
        }
        //Ставим настройки items
        if (initObj.items != undefined) {
            for (var prop in this.items) {
                if (initObj.items[prop] != undefined) {
                    this.items[prop] = initObj.items[prop];
                }
            }
        }
    }
}