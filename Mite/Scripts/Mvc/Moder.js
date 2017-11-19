var ModerMvc = {
    _send: function (url, method, data, $form, succCallback) {
        if (succCallback === undefined) {
            succCallback = function (resp) {
                iziToast.success({
                    title: 'Успех.'
                });
            }
        }
        var self = this;
        return $.ajax({
            type: method,
            url: url,
            data: data,
            success: succCallback,
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка.'
                });
            },
            complete: function () {
                $form.removeClass('loading');
            }
        });
    },
    updateTags: function (btn, postId) {
        var $form = $(btn).parents('.form');
        $form.addClass('loading');
        return this._send('/moder/updateposttags', 'post', {
            postId: postId,
            tagsNames: $('#Tags').val().split(',')
        });
    },
    blockPost: function (btn, postId) {
        return this._send('/moder/blockpost/' + postId, 'post');
    },
    unblockPost: function (btn, postId) {
        return this._send('/moder/unblockpost/' + postId, 'post');
    },
}