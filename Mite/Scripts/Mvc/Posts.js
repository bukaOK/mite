var Posts = {
    comicsItemTmpl: null,
    /**
     * Добавить элемент коллекции
     * @param {HTMLElement} addBtn кнопка добавления
    */
    addCollectionItem: function () {
        return $($('#postItemFormTmpl').html())
            .appendTo('.collection-items').form({
                fields: {
                    Description: {
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Заполните описание элемента'
                            },
                            {
                                type: 'maxLength[300]',
                                prompt: 'Слишком длинное описание'
                            }
                        ]
                    },
                    Content: {
                        rules: [
                            {
                                type: 'empty',
                                prompt: 'Вы не выбрали изображение'
                            }
                        ]
                    }
                }
            });
    },
    /**
     * Добавляет страницу комикса
     * @param {HTMLElement} addBtn кнопка добавления
    */
    addComicsItem: function (addBtn) {
        var self = this;
        if (!self.comicsItemTmpl) {
            self.comicsItemTmpl = $.templates('#comicsItemFormTmpl');
        }
        return $(self.comicsItemTmpl.render({
            Page: $('.p-item-form [name=Page]').length + 1
        })).appendTo('.collection-items').form({
            fields: {
                Page: {
                    rules: [
                        {
                            type: 'empty',
                            prompt: 'Заполните номер страницы'
                        },
                        {
                            type: 'integer',
                            prompt: 'Введите целое число'
                        }
                    ]
                },
                Content: {
                    rules: [
                        {
                            type: 'empty',
                            prompt: 'Вы не выбрали изображение'
                        }
                    ]
                }
            }
        });
    },
    /**
     * Добавляем несколько страниц
     * @param {Event} evt событие загрузки файлов
    */
    addComicsItems: function (evt) {
        var self = Posts,
            btn = evt.target.parentNode;
        for (var i = 0; i < evt.target.files.length; i++) {
            var insertedItem = self.addComicsItem();
            if (!insertedItem)
                return;
            FileReaderHelper.inputDownloadHandler(evt, {
                progress: insertedItem.find('.ui.progress'),
                imgWrapper: insertedItem.find('.img-wrapper'),
                field: insertedItem.find('[name=Content]')
            }, i);
        }
    },
    /**
     * Удаляем форму
     * @param {HTMLElement} target кнопка для закрытия
    */
    removeCollectionItem: function (target) {
        $(target).parents('.form').remove();
    },
    Api: {
        /**
         * @param {object} model модель
         * @param {string} url url запроса
         * @param {HTMLElement} btn кнопка сохранения
        */
        _send: function (model, url, btn) {
            $(btn).addClass('loading disabled');
            return $.ajax({
                type: 'post',
                data: model,
                dataType: 'json',
                url: url,
                success: function (resp) {
                    if (resp.status === undefined) {
                        resp = JSON.parse(resp);
                    }
                    switch (resp.status) {
                        case Settings.apiStatuses.error:
                        case Settings.apiStatuses.validationError:
                            $('.p-form').form('add errors', [resp.data]);
                            break;
                        case Settings.apiStatuses.success:
                            location.reload();
                            break;
                    }
                },
                error: function (jqXhr) {
                    $('.p-form').form('add errors', [jqXhr.responseJSON]);
                },
                complete: function () {
                    $(btn).removeClass('loading disabled');
                }
            });
        },
        /**
         * Сохранить коллекцию
         * @param {HTMLElement} btn
         * @param {('add'|'edit')} saveMode добавить или обновить
         * @param {('comics'|'imagecol')} collectionType тип коллекции
         * @param {boolean} isPublished
        */
        saveCollection: function (btn, saveMode, collectionType, isPublished) {
            var self = Posts.Api,
                saveUrl = saveMode === 'add' ? '/posts/addpost' : '/posts/updatepost',
                $form = $('.p-form'),
                reqSize = 0;
            var itemsValidResult = $('.p-item-form:visible').form('validate form'),
                itemsValid = itemsValidResult === true || itemsValidResult.every(function (isItemValid) {
                    return isItemValid;
                });
            if (!$form.form('validate form') || !itemsValid) {
                iziToast.error({
                    title: 'Ошибка валидации!',
                    message: 'Проверьте правильность заполнения полей.'
                });
                return;
            }
            var $items = $('.p-item-form:visible');
            if ($items.length === 0) {
                var err = collectionType === 'imagecol'
                    ? 'Для сохранения необходим хотя бы один элемент коллекции.'
                    : 'Для сохранения необходима хотя бы одна страница.';
                $form.form('add errors', [err]);
                iziToast.error({
                    title: 'Нет элементов!',
                    message: err
                });
                return;
            }
            var model = {
                Id: $('#Id').val(),
                Header: $('#Header').val(),
                Description: $('#Description').val(),
                Content: $('#Content').val(),
                ContentType: collectionType === 'imagecol' ? 'ImageCollection' : 'Comics',
                Tags: $('[name="Tags"]').val().split(','),
                Collection: [],
                ComicsItems: []
            };
            reqSize += model.Content.length + model.Header.length + model.Description.length;
            switch (collectionType) {
                case 'imagecol':
                    $('.p-item-form:visible').each(function (index, formEl) {
                        var item = {
                            Content: $(formEl).find('[name="Content"]').val(),
                            Description: $(formEl).find('[name="Description"]').val(),
                        };
                        reqSize += item.Content.length + item.Description.length;
                        if (formEl.dataset.id !== '') {
                            item.Id = formEl.dataset.id;
                        }
                        model.Collection.push(item);
                    });
                    break;
                case 'comics':
                    $('.p-item-form:visible').each(function (index, formEl) {
                        var item = {
                            Content: $(formEl).find('[name="Content"]').val(),
                            Page: $(formEl).find('[name="Page"]').val(),
                        };
                        reqSize += item.Content.length + item.Page.length;
                        if (formEl.dataset.id) {
                            item.Id = formEl.dataset.id;
                        }
                        model.ComicsItems.push(item);
                    });
                    break;
                default:
                    throw 'Unknown content type';
            }
            if (reqSize / 1024 / 1024 > 30) {
                $form.form('add errors', ['Суммарный размер контента превышает 30 мбайт, пожалуйста, загрузите изображения с меньшим размером']);
                iziToast.error({
                    title: 'Слишком большой контент',
                    message: 'Подробности в первой форме'
                });
                return;
            }
            if (!isPublished) {
                $('#publishConfirmModal').modal({
                    onApprove: function () {
                        model.PublishDate = new Date().toISOString();
                        model.Type = 'Published';
                        self._send(model, saveUrl, btn);
                    },
                    onDeny: function () {
                        self._send(model, saveUrl, btn);
                    }
                }).modal('show');
            } else {
                self._send(model, saveUrl, btn);
            }
        }
    }
}