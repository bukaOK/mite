var CommentsApi = {
    scrollToComment: function () {
        if (location.hash == '') {
            return;
        }
        $(window).scrollTo(location.hash, 2200, {
            easing: 'easeOutExpo',
            offset: 20
        });
    },
    init: function (postId, sort) {
        var self = this;
        return $.getJSON('/api/comments', {
            sort: sort,
            postId: postId
        }, function (data) {
            data.forEach(function (item) {
                var dateIso = item.PublicTime;
                dateIso += dateIso[dateIso.length - 1] == 'Z' ? '' : 'Z';
                item.PublicDateStr = ViewHelper.getPastTense(new Date(dateIso).getTime());
            });
            var tmpl = $.templates('#commentTmpl');
            var html = tmpl.render(data);
            $('#comments-wrapper').html(html);
            self.scrollToComment();
        }).fail(function () {
            iziToast.error({
                title: 'Упс!',
                message: 'Комментарии не загружены.'
            });
        });
    },
    add: function (postId, postAuthorId) {
        var self = this;
        if (!$('#comments-form').form('form validation')) {
            return;
        }
        var commentContent = $('#CommentContent'),
            model = {
                Content: commentContent.val(),
                PostId: postId
            }, replyCommentId = commentContent.data('replyComment');

        if (replyCommentId != null && replyCommentId != '') {
            model.ParentComment = {
                Id: replyCommentId,
                User: {
                    Id: commentContent.data('replyUserId')
                }
            };
        }
        return $.post('/api/comments', model, function (data) {
            NotificationsApi.add(NotificationTypes.postComment, postAuthorId, postId + '#com' + data.Id);
            if (replyCommentId != null && replyCommentId != '') {
                NotificationsApi.add(NotificationTypes.commentReply, model.ParentComment.User.Id, postId + '#com' + data.Id);
            }

            var $commentsCount = $('#comments-count');
            $commentsCount.data('count', $commentsCount.data('count') + 1);
            self.loadCommentsCount();

            var tmpl = $.templates('#commentTmpl'),
                dateIso = data.PublicTime;
            dateIso += dateIso[dateIso.length - 1] == 'Z' ? '' : 'Z';
            data.PublicDateStr = ViewHelper.getPastTense(new Date(dateIso).getTime());

            var html = tmpl.render(data);
            $('#comments-wrapper').prepend(html);
        }).fail(function (jqXhr) {
            var errors = [];
            if (jqXhr.status === 400) {
                var resp = jqXhr.responseJSON;
                for (var key in resp.ModelState) {
                    resp.ModelState[key].forEach(function (val) {
                        errors.push(val);
                    });
                }
            } else {
                errors.push('Внутренняя ошибка');
            }
            $('#comments-form').form('add errors', errors);
        });
    },
    loadCommentsCount: function () {
        var $commentsCount = $('#comments-count'),
            count = $commentsCount.data('count');
        $commentsCount.text(count + ' ' + ViewHelper.getWordCase(count, 'комментарий', 'комментария', 'комментариев'));
    },
    delete: function (elem) {
        var commentId = $(elem).data('commentId'),
            comment = $(elem).parents('.comment'),
            self = this;

        $.ajax({
            url: '/api/comments/' + commentId,
            type: 'delete',
            success: function (data) {
                var $commentsCount = $('#comments-count');
                $commentsCount.data('count', $commentsCount.data('count') - 1);
                self.loadCommentsCount();
                $('#com' + commentId).remove();
            },
            error: function (jqXhr) {
                iziToast.error({
                    title: 'Упс!',
                    message: 'Ошибка при удалении комментария.'
                });
            }
        });
    }
}