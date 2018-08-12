var TariffMvc = {
    /**
     * Добавить или обновить тариф для подписки
     * @param {string} saveUrl url для добавления/обновления
     * @param {HTMLElement} saveBtn кнопка сохранения
     **/
    save: function (saveUrl, descrData, saveBtn) {
        var $form = $('#tariffEditForm');
        if (!$form.form('validate form')) {
            return false;
        }
        $(saveBtn).addClass('loading disabled');
        var model = {
            Header: $('#Header').val(),
            Description: descrData,
            Price: $('#Price').val(),
            Id: $('#Id').val()
        };
        return $.post(saveUrl, model, function (resp) {
            if (resp.status == Settings.apiStatuses.validationError) {
                $form.form('add errors', resp.data);
            } else {
                $form.addClass('m-success').removeClass('error');
                location.reload();
            }
        }).fail(function () {
            $form.form('add errors', ['Внутренняя ошибка']);
        }).always(function () {
            $(saveBtn).removeClass('loading disabled');
        });
    },
    /**
     * Удалить тариф
     * @param {string} id id для удаления
     * @param {HTMLElement} item элемент DOM
     **/
    remove: function (id, item) {
        return $.post('/tariff/remove/' + id, function () {
            item.remove();
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при удалении тарифа'
            });
        });
    },
    /**
     * Оформляем подписку
     * @param {string} tariffId
     * @param {HTMLElement} кнопка продолжить
     * @param {string} targetUserId пользователь, которого уведомляем о том что на него оформили платную подписку
     */
    tariffBill: function (tariffId, btn, targetUserId) {
        $(btn).addClass('loading disabled');
        return $.post('/tariff/bill/' + tariffId, null, function (resp) {
            if (resp.status === Settings.apiStatuses.validationError) {
                iziToast.error({
                    title: 'Упс!',
                    message: resp.data[0]
                });
            } else {
                iziToast.success({
                    title: 'Вы успешно стали спонсором!'
                });
                NotificationsApi.add(NotificationTypes.tariffPayment, targetUserId);
            }
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при оформлении подписки'
            });
        }).always(function () {
            $(btn).removeClass('loading disabled');
        });
    },
    /**
     * Отменить платную подписку
     * @param {string} clientId id клиента
     * @param {HTMLElement} removeElem
     */
    removeForClient: function (tariffId, removeElem) {
        return $.post('/tariff/clientremove', {
            tariffId: tariffId
        }, function (resp) {
            if (resp.status == Settings.apiStatuses.validationError) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при удалении'
                });
            } else {
                removeElem.remove();
                location.reload();
            }
        })
    }
}