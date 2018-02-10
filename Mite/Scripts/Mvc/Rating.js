/**
 * API для работы с рейтингами
 */
var RatingMvc = {
    baseUrl: '/rating/',
    /**
     * Отправка запроса
     * @param {string} action
     * @param {string} method
     * @param {Object} data
     * @param {function(resp)} succCallback
    */
    _send: function (action, method, data, succCallback) {
        if (succCallback === undefined) {
            succCallback = function (resp) { };
        }
        var self = this;
        return $.ajax({
            type: method,
            url: self.baseUrl + action,
            data: data,
            success: function (resp) {
                
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Внутренняя ошибка'
                });
            }
        });
    },
    /**
     * Оцениваем коммент
     * @param {HTMLElement} btn "нравится" кнопка(иконка)
     * @param {string} commentId id коммента 
     * @param {string} userId id пользователя
    */
    rateComment: function (btn, commentId, userId, notificationType, postId) {
        var isRate = $(btn).data('isRate'),
            commentRating = $(btn).find('.rating')[0],
            rateValue = isRate ? 0 : 1;
        return $.post('ratecomment', {
            CommentId: commentId,
            UserId: userId,
            Value: rateValue
        }, function (resp) {
            var commentRateIcon = $(btn).find('.thumbs');
            if (resp.status === undefined)
                resp = JSON.parse(resp);
            if (resp.status === Settings.apiStatuses.validationError) {
                iziToast.error({
                    title: 'Упс!',
                    message: resp.message
                });
            } else {
                if (isRate) {
                    commentRateIcon.addClass('outline');
                    commentRating.innerHTML--;
                } else {
                    commentRateIcon.removeClass('outline');
                    commentRating.innerHTML++;
                }
                $(btn).data('isRate', !isRate);
                sendNotification(notificationType, userId, postId + '#com' + commentId);
            }
        });
    },
    /**
     * Оценка поста
     * @param {string} notificType тип уведомления
     * @param {string} userId id пользователя
     * @param {string} postId id поста
    */
    ratePost: function (rateValue, notificType, userId, postId) {
        var currentPostRating = $('#post-rating').data('rating');
        return $.post('ratepost', {
            Id: $("#CurrentRating_Id").val(),
            Value: rateValue,
            PostId: $("#CurrentRating_PostId").val()
        }, function (resp) {
            if (resp.status === undefined)
                resp = JSON.parse(resp);
            if (resp.status === Settings.apiStatuses.validationError) {
                iziToast.error({
                    title: 'Упс!',
                    message: resp.message
                });
                $('.ui.rating').rating('set rating', currentPostRating);
            } else {
                var newRating = rateValue - lastRateValue,
                    newPostRating = currentPostRating + newRating;
                $("#post-rating").text("(" + newPostRating + ")").data('rating', currentPostRating + newRating);

                lastRateValue = rateValue;
                sendNotification(notificType, userId, postId);
            }
        });
    },
    recount: function (itemId, recountType, btn) {
        var $form = $(btn).parents('.form');
        $form.addClass('loading')
        return $.ajax({
            url: '/rating/recount/' + itemId,
            type: 'post',
            data: 'recountType=' + recountType,
            success: function () {
                iziToast.success({
                    title: 'Успех'
                });
            },
            error: function () {
                $form.form('add errors', ['Ошибка при пересчете']);
            },
            complete: function () {
                $form.removeClass('loading')
            }
        });
    }
}