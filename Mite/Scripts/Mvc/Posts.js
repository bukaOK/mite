var Posts = {
    /**
     * Добавить элемент коллекции
     * @param {HTMLElement} addBtn
    */
    addCollectionItem: function (addBtn) {
        if (!$('.p-form').form('validate form') || !$('.p-item-form').form('validate form')) {
            return;
        }
        $(addBtn).before($('#postItemFormTmpl').html());
        $('.p-item-form:hidden').transition('scale').form({
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
     * Удаляем форму
     * @param {HTMLElement} target кнопка для закрытия
    */
    removeCollectionItem: function (target) {
        $(target).parents('.form').transition({
            animation: 'scale',
            onComplete: function () {
                this.remove();
            }
        });
    },
    Api: {
        /**
         * @param {object} model модель
         * @param {string} url url запроса
         * @param {HTMLElement} btn кнопка сохранения
        */
        _send: function (model, url, btn) {
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
        */
        saveCollection: function (btn, saveMode) {
            var self = Posts.Api,
                saveUrl = saveMode === 'add' ? '/posts/addpost' : '/posts/updatepost',
                $form = $('.p-form'),
                reqSize = 0;

            if (!$form.form('validate form') || !$('.p-item-form:visible').form('validate form')) {
                return;
            }
            var $items = $('.p-item-form:visible');
            if ($items.length === 0) {
                $form.form('add errors', ['Для сохранения необходим хотя бы один элемент коллекции.']);
                iziToast.error({
                    title: 'Нет элементов!',
                    message: 'Для сохранения необходим хотя бы один элемент коллекции.'
                });
                return;
            }
            $(btn).addClass('loading disabled');
            var model = {
                Id: $('#Id').val(),
                Header: $('#Header').val(),
                Description: $('#Description').val(),
                Content: $('#Content').val(),
                ContentType: 'ImageCollection',
                Tags: $('[name="Tags"]').val().split(','),
                Collection: []
            };
            reqSize += model.Content.length + model.Header.length + model.Description.length;
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
            if (reqSize / 1024 / 1024 > 30) {
                $form.form('add errors', ['Суммарный размер контента превышает 30 мбайт, пожалуйста загрузите изображения с меньшим размером']);
                iziToast.error({
                    title: 'Слишком большой контент',
                    message: 'Подробности в первой форме'
                });
                return;
            }
            $('#publishConfirmModal').modal({
                onApprove: function () {
                    model.PublishDate = new Date().toISOString();
                    self._send(model, saveUrl, btn);
                },
                onDeny: function () {
                    self._send(model, saveUrl, btn);
                }
            }).modal('show');
        }
    }
}