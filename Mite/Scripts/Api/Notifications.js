var NotificationTypes = {
    postRating: 0,
    follower: 1,
    postComment: 2,
    commentRating: 3,
    commentReply: 4,
    tariffPayment: 5
};
var NotificationsApi = {
    add: function (notificType, targetUserId, sourceValue) {
        return $.ajax({
            url: '/api/notification',
            type: 'post',
            data: {
                NotificationType: notificType,
                User: {
                    Id: targetUserId
                },
                SourceValue: sourceValue
            },
            success: function (data) {
                window.hubReady.done(function () {
                    $.connection.notifyHub.server.newNotification(targetUserId, notificType, sourceValue);
                });
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при отправке уведомления.'
                });
            }
        });
    }
}