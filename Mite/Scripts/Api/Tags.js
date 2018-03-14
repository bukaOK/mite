var TagsApi = {
    /**
     * Загрузить список тегов для выпадающего списка
     * @param {string} selector селектор списка
     * @param {Array<string>} selected список выбранных элементов
    */
    loadDropdown: function (selector, selected) {
        var tagsMenu = $(selector).addClass('loading'),
            items = selected.length ? selected.map(function (sel) {
                return {
                    name: sel,
                    value: sel,
                    selected: true
                }
            }) : [];
        return $.getJSON('/api/tags', function (data) {
            data.forEach(function (item) {
                var selectedContains = selected.some(function (val) {
                    return item.Name === val;
                });
                if (!selectedContains) {
                    items.push({
                        name: item.Name,
                        value: item.Name
                    });
                }
            });
            tagsMenu.dropdown({
                values: items,
                allowAdditions: true,
                fullTextSearch: true,
                keys: {
                    delimiter: 13
                },
                match: 'text',
                forceSelection: false
            });
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Теги не загружены.'
            });
        }).always(function () {
            tagsMenu.removeClass('loading');
        });
    },
    /**
     * Загружаем список всех тегов
     * @param {string|HTMLElement} containerSelector
    */
    loadTop: function (containerSelector) {
        var tmplStr = '<a class="ui large label" onclick="loadTag(\'#{{:Name}}\');$(\'.tags.modal\').modal(\'hide\')">' +
            '{{:Name}}<span class="detail">{{:Popularity}}</span></a>',
            tmpl = $.templates(tmplStr);
        return $.getJSON('/api/tags').done(function (resp) {
            $(containerSelector).html(tmpl.render(resp)).removeClass('loading');
        });
    },
    inputHandle: function (inputVal) {
        if (!inputVal) {
            $('.tags.modal .label:hidden').show();
            return;
        }
        $('.tags.modal .label').each(function (index, elem) {
            var $elem = $(elem);
            if (elem.text.indexOf(inputVal) !== -1)
                $elem.show();
            else
                $elem.hide();
        });
    }
}