var FollowersApi = {
    /**
     * Подписаться на пользователя
     * @param {string} followingUserId на кого подписываются
     * @param {HTMLElement} нажатая кнопка
     */
    add: function (followingUserId, btn) {
        var $btn = $(btn);
        $btn.addClass('loading disabled');

        return $.post('/api/followers', '=' + followingUserId, function () {
            $btn.hide();
            $btn.siblings('.button').show();
            NotificationsApi.add(NotificationTypes.follower, followingUserId, null);
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка сервера'
            });
        }).always(function () {
            $btn.removeClass('loading disabled');
        });
    },
    /**
     * Отписаться
     * @param {string} followingUserId
     * @param {HTMLElement} btn
     */
    delete: function (followingUserId, btn) {
        var $btn = $(btn);
        $btn.addClass('loading disabled');

        $.ajax({
            url: '/api/followers',
            type: 'delete',
            data: '=' + followingUserId,
            success: function (resp) {
                $btn.hide();
                $btn.siblings('.button').show();
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка сервера.'
                });
            },
            complete: function () {
                $btn.removeClass('loading disabled');
            }
        });
    }
}