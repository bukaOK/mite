var CharacterMvc = {
    /**
     * Сохраняем персонажа
     * @param {string} saveUrl Url для сохранения(добавление|обновление)
     * @param {string} description Контент с описанием персонажа(содержит html)
     * @param {HTMLElement} btn Кнопка для сохранения
     */
    save: function (saveUrl, description, btn) {
        //Делаем валидацию особенностей персонажа
        var $items = $('.char-item-form');
        if (!$items.length) {
            iziToast.error({
                title: 'Добавьте хотя бы одну особенность'
            });
            return;
        }
        var itemsValidResult = $items.form('validate form'),
            itemsValid = !$items.length || itemsValidResult === true || itemsValidResult.every(function (isItemValid) {
                return isItemValid;
            }),
            $form = $('.char-form');

        if (!$form.form('validate form') || !itemsValidResult) {
            iziToast.error({
                title: 'Нет элементов!',
                message: err
            });
        }
        var $btn = $(btn).addClass('loading disabled');
        var model = {
            Id: $('#Id').val(),
            ImageSrc: $('#ImageSrc').val(),
            Name: $('#Name').val(),
            Description: description,
            Original: $('#Original').length ? $('#Original').parent().checkbox('is checked') : true,
            Universe: $('#Universe').val(),
            Features: []
        };
        $items.each(function (index, elem) {
            var $elem = $(elem);
            model.Features.push({
                FeatureName: $elem.find('[name=FeatureName]').val(),
                FeatureDescription: $elem.find('[name=FeatureDescription]').val()
            });
        });
        return $.post(saveUrl, model, function (resp) {
            if (resp.status == Settings.apiStatuses.validationError) {
                $form.form('add errors', resp.data);
                return;
            }
            $form.addClass('m-success').removeClass('error');
            location.reload();
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Внутренняя ошибка'
            });
        }).always(function () {
            $btn.removeClass('loading disabled');
        });
    },
    /**
     * Удаляем персонажа
     * @param {string} id
     * @param {HTMLElement} removeElem
     */
    remove: function (id, removeElem) {
        return $.post('/character/remove/' + id, null, function (resp) {
            removeElem.remove();
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при удалении'
            });
        });
    }
}