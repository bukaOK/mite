﻿var Tabs = {
    initTabs: function (initObj) {
        this._initSettings(initObj);
        this.initFirstTab();
        var self = this,
            itemsSelector = this.items.selector;

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
        error: function (jqXhr) {},
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
            itemUrl = dataUrl[0] === '/' ? dataUrl : '/' + dataUrl;
        } else {
            itemUrl += dataTab[0] === '/' ? dataTab : '/' + dataTab;
        }
        window.location.hash = dataTab[0] !== '/' ? dataTab : dataTab.substr(1, dataTab.length - 1);
        
        this.ajaxSettings.url = itemUrl;
        this.ajaxSettings.data = this.ajaxSettings.dynamicData();
        if (ajaxData !== '' && ajaxData !== undefined) {
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
        if (initObj.ajaxSettings !== undefined) {
            for (var prop in this.ajaxSettings) {
                if (initObj.ajaxSettings !== undefined && initObj.ajaxSettings[prop] !== undefined) {
                    this.ajaxSettings[prop] = initObj.ajaxSettings[prop];
                }
            }
        }
        //Ставим настройки tabContent
        if (initObj.tabContent !== undefined) {
            for (var prop in this.tabContent) {
                if (initObj.tabContent !== undefined && initObj.tabContent[prop] !== undefined) {
                    this.tabContent[prop] = initObj.tabContent[prop];
                }
            }
        }
        //Ставим настройки items
        if (initObj.items !== undefined) {
            for (var prop in this.items) {
                if (initObj.items[prop] !== undefined) {
                    this.items[prop] = initObj.items[prop];
                }
            }
        }
    }
}
var MiteMobile = {
    initSwipeListener: function () {
        var xDown;
        var yDown;

        var xDiff;
        var yDiff;
        document.addEventListener('touchstart', function (evt) {
            xDown = evt.touches[0].clientX;
            yDown = evt.touches[0].clientY;
        }, false);
        document.addEventListener('touchmove', function (evt) {
            if (!xDown || !yDown) {
                return;
            }
            var xUp = evt.touches[0].clientX;
            var yUp = evt.touches[0].clientY;

            xDiff = xDown - xUp;
            yDiff = yDown - yUp;
            /* reset values */
            xDown = null;
            yDown = null;
        });
        document.addEventListener('touchend', function (ev) {

            if (Math.abs(xDiff) > Math.abs(yDiff)) {
                if (xDiff > 0) {
                    $('.ui.sidebar').sidebar('show');
                } else {
                    $('.ui.sidebar').sidebar('hide');
                }
            } else {
                if (yDiff > 0) {
                    /* up swipe */
                } else {
                    /* down swipe */
                }
            }
        });
    }
}
var Scrolling = {
    /**
     * @param {string} selector
     * @param {string|HTMLElement} container
    */
    init: function (selector, callback, container) {
        if (container === null || container === undefined) {
            container = window;
        }
        $(container).scroll(function () {
            if (!Scrolling._isBlocked && Scrolling._isInView(selector, container)) {
                Scrolling._isBlocked = true;
                var exeFunc = callback();
                if (exeFunc != undefined) {
                    exeFunc.then(function () {
                        Scrolling._isBlocked = false;
                    });
                } else {
                    Scrolling._isBlocked = false;
                }
            }
        });
        $(document).on('touchmove', function () {
            if (!Scrolling._isBlocked && Scrolling._isInView(selector, container)) {
                Scrolling._isBlocked = true;
                var exeFunc = callback();
                if (exeFunc != undefined) {
                    exeFunc.then(function () {
                        Scrolling._isBlocked = false;
                    });
                } else {
                    Scrolling._isBlocked = false;
                }
            }
        });
    },
    _isBlocked: false,
    _isInView: function (elm, container) {
        var vpH = $(container).height(), // Viewport Height
            st = $(container).scrollTop(), // Scroll Top
            y = $(elm).offset().top,
            elementHeight = $(elm).height();

        return ((y < (vpH + st)) && (y > (st - elementHeight)));
    },
    _touchListener: function () {
        document.addEventListener('touchstart', function (e) {
            lastY = e.touches[0].clientY;
            lastX = e.touches[0].clientX;
        });
        
    },
    scrollTo: function scrollTo(settings) {
        $(window).scrollTo(settings.selector, settings.time, {
            easing: settings.easing,
            offset: settings.offset
        });
        var btnScrollSettings = settings.btnScroll;
        if (btnScrollSettings != undefined && btnScrollSettings != null) {
            var btnSelector = btnScrollSettings.btnSelector;
            window.addEventListener('scroll', function () {
                if (document.body.scrollTop > btnScrollSettings.btnShowScroll
                    || document.documentElement.scrollTop > btnScrollSettings.btnShowScroll) {
                    $(btnSelec)
                }
            })
        }
    }
}
var PostGallery = {
    _postsList: [
        {
            Id: null,
            IsLoaded: false
        }
    ],
    _currentIndex: 0,
    _callbacks: {
        nextItem: function (item) { },
        previousItem: function (item) { }
    },
    init: function (settings) {
        var currentPostId = settings.currentPostId,
            posts = settings.posts;
        this._callbacks.nextItem = settings.nextItemCall;
        this._callbacks.previousItem = settings.previousItemCall;
        var currentPostIndex;
        for (var i = 0; i < posts.length; i++) {
            posts[i].IsLoaded = false;
            if (posts[i].Id == currentPostId) {
                currentPostIndex = i;
                break;
            }
        }
        var currentItem = posts.splice(currentPostIndex, 1)[0];
        posts.unshift(currentItem);

        this._postsList = posts;
    },
    nextItem: function () {
        this._currentIndex++;
        if (this._currentIndex > this._postsList.length - 1) {
            this._currentIndex--;
        }
        this._callbacks.nextItem(this._postsList[this._currentIndex]);
    },
    previousItem: function () {
        this._currentIndex--;
        if (this._currentIndex < 0) {
            this._currentIndex++;
        }
        this._callbacks.previousItem(this._postsList[this._currentIndex]);
    },
    reset: function () {
        this._currentIndex = 0;
        this._callbacks.nextItem(this._postsList[this._currentIndex]);
    },
    getCurrentItem: function () {
        return this._postsList[this._currentIndex];
    }
}
