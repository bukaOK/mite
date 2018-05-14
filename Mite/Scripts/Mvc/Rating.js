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
        return $.post('/rating/ratecomment', {
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
                NotificationsApi.add(notificationType, userId, postId + '#com' + commentId);
                if (isRate) {
                    commentRateIcon.addClass('outline');
                    commentRating.innerHTML--;
                } else {
                    commentRateIcon.removeClass('outline');
                    commentRating.innerHTML++;
                }
                $(btn).data('isRate', !isRate);
            }
        });
    },
    /**
     * @typedef {{rateValue: number, lastRateValue: number, postRating: number, postAuthorId: string, ratingId: string, postId: string, isTop: boolean, rateObj: HTMLElement|string}} PostRateSettings
     * Оценка поста
     * @param {PostRateSettings} settings новое значение рейтинга
    */
    ratePost: function (settings) {
        return $.post('/rating/ratepost', {
            Id: settings.ratingId,
            Value: settings.rateValue,
            PostId: settings.postId,
            IsTop: settings.isTop === true
        }, function (resp) {
            if (resp.status === undefined)
                resp = JSON.parse(resp);
            if (resp.status === Settings.apiStatuses.validationError) {
                iziToast.error({
                    title: 'Упс!',
                    message: resp.message
                });
                $(settings.rateObj).rating('set rating', settings.lastRateValue);
            } else {
                NotificationsApi.add(NotificationTypes.postRating, settings.postAuthorId, settings.postId);
                var newRating = settings.rateValue - settings.lastRateValue,
                    newPostRating = settings.postRating + newRating;
                if (settings.isTop) {
                    $(settings.rateObj).siblings('.right.floated').find('.rate-value').text(newPostRating);
                    settings.rateObj.dataset.fullRating = newPostRating;
                    
                } else {
                    $("#post-rating").text("(" + newPostRating + ")")[0].dataset = newPostRating;
                }
                settings.rateObj.dataset.rating = settings.rateValue;
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