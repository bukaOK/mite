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
     * @returns {JQuery<HTMLElement>}
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
         * 
         */
        saveImage: function (isPublished, url, postId) {
            var $saveBtn = $('#save-btn'),
                $form = $saveBtn.parents('.form'),
                $resultMsg = $('#result-msg'),
                postType = isPublished ? 'Published' : 'Drafts',
                model = {
                    WatermarkId: $('#WatermarkId').val(),
                    ProductId: $('#ProductId').val(),
                    Header: $('#Header').val(),
                    Content: $('#Content').val(),
                    Description: $('#Description').val(),
                    ContentType: 'Image',
                    Type: postType,
                    Id: postId,
                    Tags: $('#Tags').val().split(',')
                }, reqSize = model.Content.length;
            if (isPublished) {
                model.PublishDate = new Date().toISOString();
            }
            if ($('#WmNeedCheck').checkbox('is checked')) {
                model.Watermark = {
                    WmPath: $('#WmPath').val(),
                    Gravity: $('#Gravity').val(),
                    FontSize: $('#FontSize').val(),
                    WmText: $('#WmText').val(),
                    Inverted: $('#invertCheck').checkbox('is checked'),
                    UseCustomImage: $('#UseCustomImage').val()
                };
                if (model.Watermark.WmPath)
                    reqSize += model.Watermark.WmPath.length;
            }
            if ($('#ProdNeedCheck').checkbox('is checked')) {
                model.Product = {
                    Id: $('#ProductId').val(),
                    Price: $('#Price').val(),
                    BonusBase64: $('#BonusBase64').val(),
                    BonusDescription: $('#BonusDescription').val(),
                    BonusFormat: $('#BonusFormat').val(),
                    ForAuthors: $('#ForAuthors').parent().checkbox('is checked')
                };
            }
            if (reqSize / 1024 / 1024 > 50) {
                $form.form('add errors', ['Слишком большой размер запроса, добавьте файлы с меньшим размером.']);
                return;
            }
            $saveBtn.addClass('loading disabled');
            return $.post(url, model, function (resp) {
                if (resp.status === Settings.apiStatuses.validationError) {
                    if (resp.message) {
                        var errorStr = 'Ошибка при сохранении: ' + resp.message;
                        $('#imgPostForm').form('add errors', [errorStr]);
                    } else {
                        $('#imgPostForm').form('add errors', resp.data);
                    }
                } else {
                    iziToast.success({
                        title: 'Успешно!',
                        message: 'Работа сохранена.'
                    });
                    location.reload();
                }
            }).fail(function () {
                $('#imgPostForm').form('add errors', ['Ошибка во время добавления работы']);
            }).always(function () {
                $saveBtn.removeClass('loading disabled');
                $resultMsg.removeClass('hidden');
            });
        },
        /**
         * Отправить запрос для коллекции
         * @param {object} model модель
         * @param {string} url url запроса
         * @param {HTMLElement} btn кнопка сохранения
        */
        _sendCol: function (model, url, btn) {
            var $btn = $(btn).addClass('loading disabled');
            return $.post(url, model, function (resp) {
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
            }).fail(function () {
                $('.p-form').form('add errors', [jqXhr.responseJSON]);
            }).always(function () {
                $btn.removeClass('loading disabled');
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
                    $items.each(function (index, formEl) {
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
                    $items.each(function (index, formEl) {
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
                        self._sendCol(model, saveUrl, btn);
                    },
                    onDeny: function () {
                        self._sendCol(model, saveUrl, btn);
                    }
                }).modal('show');
            } else {
                self._sendCol(model, saveUrl, btn);
            }
        }
    }
}