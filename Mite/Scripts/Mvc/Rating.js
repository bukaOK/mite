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
    _send: function (action, method, data, succCallback, errMsg) {
        if (succCallback === undefined) {
            succCallback = function (resp) { };
        }
        if (errMsg === undefined) {
            errMsg = 'Ошибка при оценке';
        }
        var self = this;
        return $.ajax({
            type: method,
            url: self.baseUrl + action,
            data: data,
            success: succCallback,
            error: function () {
                iziToast.error({
                    title: 'Упс!',
                    message: errMsg
                });
            }
        });
    },
    /**
     * Оцениваем коммент
     * @param {HTMLElement} btn "нравится" кнопка(иконка)
     * @param {string} commentId
     * @param {string} userId
    */
    rateComment: function (btn, commentId, userId, notificationType, postId) {
        var isRate = $(btn).data('isRate');
        var commentRating = $(btn).find('.rating')[0];
        var rateValue = isRate ? 0 : 1;
        return RatingMvc._send('ratecomment', 'post', {
            CommentId: commentId,
            UserId: userId,
            Value: rateValue
        }, function (resp) {
            var commentRateIcon = $(btn).find('.thumbs');
            if (isRate) {
                commentRateIcon.addClass('outline');
                commentRating.innerHTML--;
            } else {
                commentRateIcon.removeClass('outline');
                commentRating.innerHTML++;
            }
            $(btn).data('isRate', !isRate);
            sendNotification(notificationType, userId, postId + '#com' + commentId);
        });
    },
    /**
     * Оценка поста
     * @param {string} notificType тип уведомления
     * @param {string} userId id пользователя
     * @param {string} postId id поста
    */
    ratePost: function (rateValue, notificType, userId, postId) {
        return RatingMvc._send('ratepost', 'post', {
            Id: $("#CurrentRating_Id").val(),
            Value: rateValue,
            PostId: $("#CurrentRating_PostId").val()
        }, function (resp) {
            var currentPostRating = +$("#post-rating").data('rating');
            var newRating = rateValue - lastRateValue;

            var newPostRating = currentPostRating + newRating;
            $("#post-rating").text("(" + newPostRating + ")").data('rating', currentPostRating + newRating);

            lastRateValue = rateValue;
            sendNotification(notificType, userId, postId);
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