var TagsApi = {
    initModerTags: function () {
        return $.post('/moder/tags', function (resp) {
            if (resp.status == undefined) {
                resp = JSON.parse(resp);
            }
            var tagsRowTmpl = $.templates('#tagRowTmpl');
            var confirmedHtml = tagsRowTmpl.render(resp.data.Confirmed);
            var uncheckedHtml = tagsRowTmpl.render(resp.data.Unchecked);
            var checkedHtml = tagsRowTmpl.render(resp.data.Checked);

            $('.tab[data-tab="tagconfirmed"] tbody').html(confirmedHtml);
            $('.tab[data-tab="tagunchecked"] tbody').html(uncheckedHtml);
            $('.tab[data-tab="tagchecked"] tbody').html(checkedHtml);
            $('.tab').removeClass('loading');
        });
    },
    /** 
     * Помечает тег как проверенный
     * @param {HTMLElement} btn 
     */
    checkTag: function (btn) {
        var $tagRow = $(btn).parents('tr'),
            data = {
                Id: $tagRow.data('id'),
                Name: $tagRow.data('name'),
                IsConfirmed: $tagRow.data('confirmed'),
                Checked: true
            };
        return $.ajax({
            url: '/api/tags',
            type: 'put',
            data: data,
            success: function () {
                $tagRow[0].remove();
                var trToInsert = $.templates('#tagRowTmpl').render(data);
                $('table.checked').prepend(trToInsert);
            },
            error: function () {
                alert('Ошибка!');
            }
        });
    },
    /**
     * Привязать один тег к другому
     * @param {string} oldId id старого тега
     * @param {string} newId id нового тега
     */
    bindTag: function (oldId, newId) {
        if (oldId == '' || newId == '') {
            alert('Поля не могут быть пустыми');
            return;
        }
        return $.ajax({
            url: '/moder/bindtag',
            data: {
                fromId: oldId,
                toId: newId
            },
            type: 'post',
            success: function () {
                alert('Привязка успешна');
                $('tr[data-id="' + oldId + '"]')[0].remove();
            },
            error: function (jqXhr) {
                alert('Ошибка сервера');
            }
        });
    },
    /**
     * Подтвердить тег
     * @param {HTMLElement} btn
     */
    confirmTag: function (btn) {
        var $row = $(btn).parents('tr'),
            data = {
            Id: $row.data('id'),
            Name: $row.data('name'),
            IsConfirmed: true,
            Checked: true
        };
        return $.ajax({
            url: '/api/tags',
            type: 'put',
            data: data,
            success: function (resp) {
                $row.remove();
                var tagRowHtml = $.templates('#tagRowTmpl').render(data);
                $('table.checked tbody').prepend(tagRowHtml);
            },
            error: function (jqXhr) {
                alert('Ошибка сервера');
            }
        });
    },
    /**
     * Удалить тег
     * @param {HTMLElement} btn
     */
    deleteTag: function (btn) {
        return $.ajax({
            url: '/api/tags/' + $(btn).parents('tr').data('id'),
            type: 'delete',
            success: function () {
                $(btn).parents('tr').remove();
            },
            error: function () {
                alert('Ошибка во время удаления');
            }
        });
    },
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
    },
    /**
     * Подписаться на тег
     * @param {HTMLElement} tagLabel
     */
    addUserTag: function (tagLabel) {
        var $tagLabel = $(tagLabel),
            tagId = $tagLabel.data('id'),
            self = this;
        return $.post('/api/usertags?tagId=' + tagId, function (resp) {
            $tagLabel.addClass('violet').off('click').attr('title', 'Отписаться')[0].onclick = function () {
                self.removeUserTag(this);
            };
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при подписке'
            });
        });
    },
    /**
     * Отписаться от тега
     * @param {HTMLElement} tagLabel
     */
    removeUserTag: function (tagLabel) {
        var $tagLabel = $(tagLabel),
            tagId = $tagLabel.data('id'),
            self = this;
        return $.ajax({
            url: '/api/usertags?tagId=' + tagId,
            type: 'delete',
            success: function () {
                $tagLabel.removeClass('violet').attr('title', 'Подписаться')[0].onclick = function () {
                    self.addUserTag(this);
                };
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при подписке'
                });
            }
        });
    },

}