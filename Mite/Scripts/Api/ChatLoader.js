function ChatLoader(chatId) {
    this._page = 1;
    this.ended = false;
    this.chat = $('.chat.feed[data-id="' + chatId + '"]');
    this.msgPs = new PerfectScrollbar(this.chat[0].parentNode);
    this.loader = $('.chat.feed[data-id="' + chatId + '"]').siblings('.dot-loader-wrapper');
    this.loading = false;
    var self = this;
    Scrolling.init(self.loader[0], function () {
        if (!self.ended && !self.loading) {
            self.loadNext();
        }
    }, self.chat.parent()[0]);
}
ChatLoader.prototype.loadNext = function () {
    if (this.ended)
        return;
    var self = this,
        initialized = self._page > 1,
        statuses = ChatMessages.Api.statuses;

    self.loading = true;
    return $.ajax({
        dataType: 'json',
        data: {
            page: self._page,
            chatId: ChatMessages.chatId
        },
        url: '/api/message',
        success: function (resp) {
            self.ended = resp.ended;

            resp.data.forEach(function (msg, i, messages) {
                msg.images = [];
                msg.documents = [];
                if (msg.Attachments.length > 0) {
                    msg.Attachments.forEach(function (att, j) {
                        att.Index = j;
                        if (att.Type === 0) {
                            msg.images.push(att);
                        } else {
                            msg.documents.push(att);
                        }
                    });
                }
                var dateIso = msg.SendDate,
                    now = new Date();
                dateIso += dateIso[dateIso.length - 1] === 'Z' ? '' : 'Z';

                var date = new Date(dateIso);
                msg.Time = DateTimeHelper.toTimeString(date);
                msg.Date = DateTimeHelper.toDateString(date, 'short');
                msg.IsoDate = date;
                msg.Status = msg.Readed ? statuses.readed : statuses.sended;

                if (i === 0) {
                    var withYear = date.getFullYear() !== now.getFullYear();
                    msg.DividerDate = DateTimeHelper.toDateString(date, 'long', withYear);
                } else if (initialized && i === messages.length - 1) {
                    var firstOldMsgDate = $('.chat .event').first()[0].dataset.date;
                    if (msg.Date === firstOldMsgDate) {
                        $('.chat .divider').first().remove();

                    }
                } else if (i > 0) {
                    var prevMsg = messages[i - 1];
                    if (prevMsg.Date !== msg.Date) {
                        msg.DividerDate = DateTimeHelper.toDateString(now, 'short') === msg.Date
                            ? 'Сегодня' : DateTimeHelper.toDateString(msg.IsoDate, 'long');
                    }
                }
            });
            var tmpl = $.templates('#messageTmpl'),
                html = tmpl.render(resp.data);
            self.chat.prepend(html);
            self.chat.find('.event .extra.images:not(.initialized)').click(function (ev) {
                ev.stopPropagation();
            }).addClass('initialized').lightGallery({
                nextHtml: '<i class="angle big right icon"></i>',
                prevHtml: '<i class="angle big left icon"></i>'
                });
            self.chat.find('.event .meta:not(.initialized)').click(function (ev) {
                ev.stopPropagation();
            }).addClass('initialized');
            if (!initialized) {
                setTimeout(function () {
                    self.msgPs.update();
                    self.chat.parent().scrollTop(self.chat[0].offsetHeight);
                }, 50);
            }
            self._page++;
        },
        error: function (jqXhr) {
            var errMsg = 'Внутренняя ошибка';
            if (jqXhr.status === 400) {
                errMsg = jqXhr.responseText;
            }
            $('#chat-form').form('add errors', [errMsg]);
        },
        complete: function () {
            self.loading = false;
            if (self.ended) {
                self.loader.removeClass('active');
            }
        }
    });
}