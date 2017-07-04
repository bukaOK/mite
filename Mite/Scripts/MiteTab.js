function MiteTab(itemsSelector, basePath) {
    this.tabs = [];
    this.basePath = basePath;
    //Приводим к стандартному виду
    this.basePath += basePath[basePath.length - 1] == '/' ? '' : '/';
    var self = this;

    var matches = document.querySelectorAll(itemsSelector);
    for (var match of matches) {
        var tab = {
            name: match.dataset.tab,
            item: match,
            wrapper: document.querySelector('.tab[data-tab="' + match.dataset.tab + '"]')
        };
        this.tabs.push(tab);

        match.addEventListener('click', function (ev) {
            self.loadTab(this.dataset.tab, true);
        });
    }
    this.initFirstTab(basePath);
    window.onpopstate = function (ev) {
        self.loadTab(ev.state, false);
    }
}
MiteTab.prototype.initFirstTab = function(basePath) {
    var url = new URL(location.href);
    var path = url.pathname;
    //Приводим к стандартному виду
    path += path[path.length - 1] == '/' ? '' : '/';

    var tabName = path.replace(basePath, '').replace('/', '');
    console.log(tabName);
    console.log(basePath);
    if (tabName == '') {
        this.loadTab(this.tabs[0].name, false);
    } else {
        this.loadTab(tabName, false);
    }
}
MiteTab.prototype.loadTab = function(name, remember){
    for (var tab of this.tabs) {
        if (tab.name == name) {
            tab.item.classList.add('active');
            tab.wrapper.style.display = 'block';
        } else {
            tab.item.classList.remove('active');
            tab.wrapper.style.display = 'none';
        }
    }
    if (remember)
        history.pushState(name, '', this.basePath + name);
}