var FavoritePostsApi = {
    url: '/api/favoriteposts',
    onError: function (jqXhr) {
        var message;
        switch (jqXhr.status) {
            case 403:
                message = 'Действие запрещено';
                break;
            case 404:
                message = 'Страница не найдена';
                break;
            default:
                message = 'Внутренняя ошибка сервера';
                break;
        }
        iziToast.error({
            title: 'Упс!',
            message: message
        });
    },
    /**
     * Добавить работу в избранное
     * @param {HTMLElement} btn
     * @param {string} postId
     * @param {string} userId
    */
    add: function (btn, postId, userId) {
        $(btn).addClass('loading disabled');
        var self = this;
        return $.ajax({
            url: self.url,
            type: 'post',
            data: {
                PostId: postId,
                UserId: userId
            },
            success: function () {
                $(btn).siblings('.label')[0].innerHTML++;
                $(btn).children('.content').text('Из избранных');
                iziToast.success({
                    title: 'Успех!',
                    message: 'Работа успешно добавлена в избранное'
                });
            },
            error: self.onError,
            complete: function () {
                $(btn).removeClass('loading disabled');
            }
        });
    },
    /**
     * Удалить работу из избранных
     * @param {HTMLElement} btn
     * @param {string} postId
     * @param {string} userId
     * @param {boolean} fromGrid
    */
    remove: function (btn, postId, userId, fromGrid) {
        if (!fromGrid) {
            $(btn).addClass('loading disabled');
        }
        var self = this,
            $btn = $(btn);
        return $.ajax({
            url: self.url + '?postId=' + postId + '&userId=' + userId,
            type: 'delete',
            error: self.onError,
            success: function () {
                if (fromGrid) {
                    var $col = $btn.parents('.column[data-post-id]'),
                        $grid = $btn.parents('.tab.grid[data-tab]');
                    $grid.masonry('remove', $col[0]);
                    if ($grid.children('.column').length === 0) {
                        $grid.html('#emptyPostTmpl');
                    }
                    $grid.masonry('layout');
                } else {
                    $btn.siblings('.label')[0].innerHTML--;
                    $btn.children('.content').text('В избранное');
                }
                iziToast.success({
                    title: 'Успех!',
                    message: 'Работа успешно удалена из избранных.'
                });
            },
            complete: function () {
                if (!fromGrid) {
                    $(btn).removeClass('loading disabled');
                }
            }
        });
    }
}