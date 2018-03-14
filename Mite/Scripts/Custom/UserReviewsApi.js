var UserReviewsApi = {
    add: function (text) {
        return $.post('/api/userreviews', {
            Review: text
        }, function () {
            iziToast.success({
                title: 'Спасибо!',
                message: 'Отзыв отправлен'
            });
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Ошибка при отправке отзыва'
            });
        });
    }
}