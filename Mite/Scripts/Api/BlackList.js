var BlackListApi = {
    url: '/api/blacklist',
    /**
     * Добавить в черный список
     * @param {string} userId Id пользователя
     * @param {HTMLElement} item элемент на который нажали
    */
    add: function (userId, item) {
        return $.post(BlackListApi.url, '=' + userId, function () {
            iziToast.success({
                title: 'Успех!',
                message: 'Пользователь добавлен в черный список'
            });
            $(item).hide().siblings('.blist').show();
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Внутреняя ошибка'
            });
        });
    },
    /**
     * Удалить из черного списка
     * @param {string} userId Id пользователя
     * @param {HTMLElement} item элемент на который нажали
    */
    remove: function (userId, item) {
        return $.ajax({
            type: 'delete',
            url: BlackListApi.url + '?targetId=' + userId,
            success: function () {
                iziToast.success({
                    title: 'Успех!',
                    message: 'Пользователь убран из черного списка'
                });
                $(item).hide().siblings('.blist').show();
            },
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутреняя ошибка'
                });
            }
        });
    }
}