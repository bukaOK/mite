/**
 * Табы
 * @param {string} itemsSelector селектор элементов меню табов
 * @param {string} basePath путь, относительно которого будут строиться новые(html5 history)
 * @param {boolean} serverLoad подгружать ли страницы табов с сервера(все запросы будут выполняться через GET)
 */
function MiteTab(itemsSelector, basePath, serverLoad) {
    this.tabs = [];
    this.basePath = basePath;
    this.serverLoad = serverLoad;
    this.menuItemSelector = itemsSelector;
    //Приводим к стандартному виду
    this.basePath += basePath[basePath.length - 1] == '/' ? '' : '/';
    var self = this;

    $(itemsSelector).each(function (index, match) {
        var $match = $(match),
            tabName = $match.data('tab'),
            tab = {
                name: tabName,
                $item: $match,
                $wrapper: $('.tab[data-tab="' + tabName + '"]'),
                url: $match.data('url') ? $match.data('url') : self.basePath + tabName
            };
        self.tabs.push(tab);

        $match.click(function (ev) {
            self.loadTab(this.dataset.tab, true);
        });
    });
    this.initFirstTab(basePath);
    window.onpopstate = function (ev) {
        self.loadTab(ev.state, false);
    }
}
MiteTab.prototype.initFirstTab = function (basePath) {
    var self = this,
        url = new URL(location.href),
        path = url.pathname;
    //Приводим к стандартному виду
    if (path[path.length - 1] == '/')
        path = path.substr(0, path.length - 1);

    var tabName = path.replace(basePath, '').replace('/', '');
    if (tabName == '') {
        self.loadTab(this.tabs[0].name, false);
    } else {
        self.loadTab(tabName, false);
    }
}
MiteTab.prototype.loadTab = function (name, remember) {
    var self = this,
        loadTab;
    this.tabs.forEach(function (tab) {
        if (tab.name == name) {
            tab.$item.addClass('active');
            tab.$wrapper.addClass('active');
            loadTab = tab;
        } else if (tab.$item.hasClass('active') || tab.$wrapper.hasClass('active')) {
            tab.$item.removeClass('active');
            tab.$wrapper.removeClass('active');
        }
    });
    if (remember)
        history.pushState(name, '', loadTab.url);
    if (self.serverLoad && !loadTab.initialized) {
        loadTab.initialized = true;
        return $.get(loadTab.url, function (resp) {
            loadTab.$wrapper.removeClass('loading').html(resp);
        });
    }
}