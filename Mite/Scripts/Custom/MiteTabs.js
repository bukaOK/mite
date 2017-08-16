function MiteTab(itemsSelector, basePath) {
    this.tabs = [];
    this.basePath = basePath;
    //Приводим к стандартному виду
    this.basePath += basePath[basePath.length - 1] == '/' ? '' : '/';
    var self = this;

    var matches = document.querySelectorAll(itemsSelector);
    matches.forEach(function (match) {
        var tab = {
            name: match.dataset.tab,
            item: match,
            wrapper: document.querySelector('.tab[data-tab="' + match.dataset.tab + '"]')
        };
        self.tabs.push(tab);

        match.addEventListener('click', function (ev) {
            self.loadTab(this.dataset.tab, true);
        });
    });
    this.initFirstTab(basePath);
    window.onpopstate = function (ev) {
        self.loadTab(ev.state, false);
    }
}
MiteTab.prototype.initFirstTab = function (basePath) {
    var self = this;
    var url = new URL(location.href);
    var path = url.pathname;
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
    var self = this;
    this.tabs.forEach(function (tab) {
        if (tab.name == name) {
            tab.item.classList.add('active');
            tab.wrapper.classList.add('active');
        } else {
            tab.item.classList.remove('active');
            tab.wrapper.classList.remove('active');
        }
    });
    if (remember)
        history.pushState(name, '', this.basePath + name);
}