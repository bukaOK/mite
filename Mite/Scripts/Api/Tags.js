var TagsApi = {
    /**
     * Загружаем список всех тегов
     * @param {string|HTMLElement} containerSelector
    */
    loadTop: function (containerSelector) {
        var tmplStr = '<a class="ui large label" onclick="loadTag(\'#{{:Name}}\');$(\'.tags.modal\').modal(\'hide\')">' +
            '{{:Name}}<span class="detail">{{:Popularity}}</span></a>',
            tmpl = $.templates(tmplStr);
        return $.getJSON('/api/tags').done(function (resp) {
            $(containerSelector).html(tmpl.render(resp));
        });
    }
}